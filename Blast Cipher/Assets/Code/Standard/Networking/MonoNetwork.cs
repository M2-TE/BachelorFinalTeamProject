using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Networking
{
	public abstract class MonoNetwork : MonoBehaviour
	{
		protected enum MessageType : byte { Undefined, Initialization, Gameplay }

		[Serializable]
		protected abstract class NetworkMessage
		{
			internal byte ClientID;
			internal int MillisecondTimestamp;

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
			internal TcpMessage()
			{
				playerPosition = new float[3];
			}

			internal byte MessageType;

			private readonly float[] playerPosition;
			internal Vector3 PlayerPosition
			{
				get => new Vector3(playerPosition[0], playerPosition[1], playerPosition[2]);
				set
				{
					playerPosition[0] = value.x;
					playerPosition[1] = value.y;
					playerPosition[2] = value.z;
				}
			}
		}

		[Serializable]
		protected class UdpMessage : NetworkMessage
		{
			internal UdpMessage()
			{
				movementInput = new float[2];
				aimInput = new float[2];
			}

			private readonly float[] movementInput;
			internal Vector2 MovementInput
			{
				get => new Vector2(movementInput[0], movementInput[1]);
				set
				{
					movementInput[0] = value.x;
					movementInput[1] = value.y;
				}
			}

			private readonly float[] aimInput;
			internal Vector2 AimInput
			{
				get => new Vector2(aimInput[0], aimInput[1]);
				set
				{
					aimInput[0] = value.x;
					aimInput[1] = value.y;
				}
			}
		}

		private class ConnectionState
		{
			public NetworkStream Stream;
			public byte[] Message;
		}

		[SerializeField] private int PORT = 18_000;
		private const int TCP_DATAGRAM_SIZE_MAX = 2048;

		private Stopwatch stopwatch;
		protected int GetTime { get => stopwatch.Elapsed.Milliseconds; }

		private UdpClient udpClient;

		private TcpClient tcpClient;
		private TcpListener tcpListener;

		private void OnDestroy()
		{
			tcpListener?.Stop();
			tcpClient.Close();

			udpClient.Close();
		}

		private void Awake()
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
		}

		protected void SetupAsServer()
		{
			udpClient = new UdpClient(PORT);
			udpClient.BeginReceive(OnUdpMessageReceive, null);

			tcpClient = new TcpClient();
			tcpListener = TcpListener.Create(PORT);
			tcpListener.Start();
			tcpListener.BeginAcceptTcpClient(OnTcpClientAccept, null);
		}

		protected void SetupAsClient(IPAddress ipAdress)
		{
			udpClient = new UdpClient();
			udpClient.Connect(ipAdress, PORT);
			udpClient.BeginReceive(OnUdpMessageReceive, null);

			tcpClient = new TcpClient();
			tcpClient.BeginConnect(ipAdress, PORT, OnTcpConnect, ipAdress);
		}

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

		protected virtual void TcpConnectionEstablished(NetworkStream stream) { }

		protected virtual void TcpMessageReceived(NetworkStream sender, byte[] message) { }
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

		protected virtual void UdpMessageReceived(IPEndPoint sender, byte[] message) { }
		#endregion
	}
}
