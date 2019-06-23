using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Networking
{
	public abstract class MonoNetwork : MonoBehaviour
	{
		protected enum MessageType : byte { Undefined, Initialization, Gameplay }

		#region Network Messages
		[Serializable]
		protected abstract class NetworkMessage
		{
			internal byte ClientID;
			internal int MillisecondTimestamp;
			internal string DebugString;

			internal byte[] ToArray()
			{
				try
				{
					using (var memoryStream = new MemoryStream())
					{
						var binaryFormatter = new BinaryFormatter();
						binaryFormatter.Serialize(memoryStream, this);
						

						return memoryStream.ToArray();
					}
				}
				catch(Exception e)
				{
					Debug.LogException(e);
					return null;
				}
			}

			internal static T Parse<T>(byte[] bytes) where T : NetworkMessage
			{
				try
				{
					using (var memoryStream = new MemoryStream())
					{
						var binaryFormatter = new BinaryFormatter();

						memoryStream.Write(bytes, 0, bytes.Length);
						memoryStream.Seek(0, SeekOrigin.Begin);

						var message = binaryFormatter.Deserialize(memoryStream) as T;
						return message;
					}
				}
				catch(Exception e)
				{
					Debug.LogException(e);
					return null;
				}
			}
		}

		[Serializable]
		protected class TcpMessage : NetworkMessage
		{
			internal TcpMessage() { }

			internal byte MessageType;
		}

		[Serializable]
		protected class InputDataMessageUdp : NetworkMessage
		{
			private readonly float[] inputData;

			internal InputDataMessageUdp()
			{
				inputData = new float[4];
			}

			internal Vector2 MovementInput
			{
				get => new Vector2(inputData[0], inputData[1]);
				set
				{
					inputData[0] = value.x;
					inputData[1] = value.y;
				}
			}

			internal Vector2 AimInput
			{
				get => new Vector2(inputData[2], inputData[3]);
				set
				{
					inputData[2] = value.x;
					inputData[3] = value.y;
				}
			}
		}

		[Serializable]
		protected class BoardDataMessage : NetworkMessage
		{
			private int playerCount;
			private float[] boardData;

			internal BoardDataMessage(int playerCount)
			{
				this.playerCount = playerCount;
				boardData = new float[playerCount * 4];
			}

			internal Vector2[] Positions
			{
				get
				{
					var positions = new Vector2[playerCount];
					for(int i = 0; i < playerCount; i++)
					{
						positions[i] = new Vector2(i, i + 1);
					}
					return positions;
				}
			}
		}
		#endregion

		private class ConnectionState
		{
			public NetworkStream Stream;
			public byte[] Message;
		}

		[SerializeField] private int PORT = 18_000;
		[SerializeField] protected double tickrateMS;
		private const int TCP_DATAGRAM_SIZE_MAX = 2048;

		private Stopwatch stopwatch;
		protected int GetTime { get => stopwatch.Elapsed.Milliseconds; }
		protected Timer tickTimer;
		private bool destroyIssued;

		private UdpClient udpClient;

		private TcpClient tcpClient;
		private TcpListener tcpListener;

		protected virtual void OnDestroy()
		{
			CloseAll();
		}

		private void Awake()
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
			tickTimer = new Timer(OnTimerTick, stopwatch, TimeSpan.Zero, TimeSpan.FromMilliseconds(tickrateMS));
		}

		protected void SetupAsServer(bool setupUdp, bool setupTcp)
		{
			if (setupUdp)
			{
				udpClient = new UdpClient(PORT);
				udpClient.BeginReceive(OnUdpMessageReceive, null);
			}

			if (setupTcp)
			{
				tcpClient = new TcpClient();
				tcpListener = TcpListener.Create(PORT);
				tcpListener.Start();
				tcpListener.BeginAcceptTcpClient(OnTcpClientAccept, null);
			}
		}

		protected void SetupAsClient(bool setupUdp, bool setupTcp, IPAddress ipAdress)
		{
			if (setupUdp)
			{
				udpClient = new UdpClient();
				udpClient.Connect(ipAdress, PORT);
				udpClient.BeginReceive(OnUdpMessageReceive, null);
			}

			if (setupTcp)
			{
				tcpClient = new TcpClient();
				tcpClient.BeginConnect(ipAdress, PORT, OnTcpConnect, ipAdress);
			}
		}

		protected void CloseAll()
		{
			tcpListener?.Stop();
			tcpClient?.Close();

			udpClient?.Close();


			tickTimer?.Dispose();

			destroyIssued = true;
		}

		protected abstract void OnTimerTick(object obj);

		#region TCP
		private void OnTcpClientAccept(IAsyncResult ar)
		{
			try
			{
				var senderClient = tcpListener.EndAcceptTcpClient(ar);
				var stream = senderClient.GetStream();
				byte[] bytes = new byte[TCP_DATAGRAM_SIZE_MAX];
				TcpConnectionEstablished(stream);

				// wait for messages on stream
				stream.BeginRead(bytes, 0, bytes.Length, OnTcpMessageReceive, new ConnectionState() { Stream = stream, Message = bytes });

				// listen for more clients
				tcpListener.BeginAcceptTcpClient(OnTcpClientAccept, null);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
		}

		private void OnTcpConnect(IAsyncResult ar)
		{
			try
			{
				tcpClient.EndConnect(ar);
				var stream = tcpClient.GetStream();

				// callback
				TcpConnectionEstablished(stream);

				// begin reading from now established stream
				byte[] bytes = new byte[TCP_DATAGRAM_SIZE_MAX];
				var connection = new ConnectionState() { Message = bytes, Stream = stream };
				stream.BeginRead(bytes, 0, bytes.Length, OnTcpMessageReceive, connection);
			}
			catch (Exception e)
			{
				var ip = (IPAddress)ar.AsyncState;
				tcpClient.BeginConnect(ip, PORT, OnTcpConnect, ip);
				Debug.Log(e);
			}
		}

		private void OnTcpMessageReceive(IAsyncResult ar)
		{
			try
			{
				ConnectionState connection = (ConnectionState)ar.AsyncState;
				connection.Stream.EndRead(ar);

				if (destroyIssued) connection.Stream.Dispose();

				// callback
				TcpMessageReceived(connection.Stream, connection.Message);

				// wait for more messages
				connection.Stream.BeginRead(connection.Message, 0, connection.Message.Length, OnTcpMessageReceive, connection);
			}
			catch(ObjectDisposedException)
			{
				return;
			}
			catch (IOException)
			{
				return;
			}
		}

		private void OnTcpMessageSend(IAsyncResult ar)
		{
			try
			{
				((NetworkStream)ar.AsyncState).EndWrite(ar);
			}
			catch (SocketException)
			{
				return;
			}
		}

		protected void SendTcpMessage(NetworkStream stream, byte[] message)
		{
			stream.BeginWrite(message, 0, message.Length, OnTcpMessageSend, stream);
		}

		protected abstract void TcpConnectionEstablished(NetworkStream stream);

		protected abstract void TcpMessageReceived(NetworkStream sender, byte[] message);
		#endregion

		#region UDP
		private void OnUdpMessageReceive(IAsyncResult ar)
		{
			try
			{
				IPEndPoint sender = new IPEndPoint(IPAddress.Any, PORT);
				byte[] messageBytes = udpClient.EndReceive(ar, ref sender);

				UdpMessageReceived(sender, messageBytes);

				udpClient.BeginReceive(OnUdpMessageReceive, null);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
		}

		private void OnUdpMessageSend(IAsyncResult ar)
		{
			udpClient.EndSend(ar);
		}

		protected void SendUdpMessage(byte[] message)
		{
			udpClient.BeginSend(message, message.Length, OnUdpMessageSend, null);
		}

		protected void SendUdpMessage(IPEndPoint target, byte[] message)
		{
			udpClient.BeginSend(message, message.Length, target, OnUdpMessageSend, null);
		}

		protected abstract void UdpMessageReceived(IPEndPoint sender, byte[] message);
		#endregion
	}
}
