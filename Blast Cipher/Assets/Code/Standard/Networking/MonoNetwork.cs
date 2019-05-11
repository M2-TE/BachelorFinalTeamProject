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
		protected enum MessageType : byte { Undefined, Initialization, EntityPositions }
		protected enum EntityType : byte { Undefined, Player, Projectile }

		[Serializable]
		protected class NetworkMessage
		{
			internal byte Type;
			internal byte ClientID;
			internal int MillisecondTimestamp;

			internal float[] playerPosition;
			internal float[] playerRotation;

			internal byte[] ToArray()
			{
				try
				{
					using (var memoryStream = new MemoryStream())
					{
						var binaryFormatter = new BinaryFormatter();
						binaryFormatter.Serialize(memoryStream, this);
						binaryFormatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
						

						return memoryStream.ToArray();
					}
				}
				catch(Exception e)
				{
					Debug.LogException(e);
					return null;
				}
			}

			internal static NetworkMessage Parse(byte[] bytes)
			{
				try
				{
					using (var memoryStream = new MemoryStream())
					{
						var binaryFormatter = new BinaryFormatter();
						binaryFormatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;

						memoryStream.Write(bytes, 0, bytes.Length);
						memoryStream.Seek(0, SeekOrigin.Begin);

						var message = binaryFormatter.Deserialize(memoryStream) as NetworkMessage;
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

		protected class BufferTuple
		{
			public Transform transform;
			public Vector3 bufferedPos;
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
			var senderClient = tcpListener.EndAcceptTcpClient(ar);
			var stream = senderClient.GetStream();
			byte[] bytes = new byte[TCP_DATAGRAM_SIZE_MAX];
			TcpConnectionEstablished(stream);

			// wait for messages on stream
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpMessageReceive, new ConnectionState() { Stream = stream, Message = bytes });
			
			// listen for more clients
			tcpListener.BeginAcceptTcpClient(OnTcpClientAccept, null); 
		}

		private void OnTcpConnect(IAsyncResult ar)
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

		private void OnTcpMessageReceive(IAsyncResult ar)
		{
			ConnectionState connection = (ConnectionState)ar.AsyncState;
			connection.Stream.EndRead(ar);

			// callback
			TcpMessageReceived(connection.Stream, connection.Message);

			// wait for more messages
			connection.Stream.BeginRead(connection.Message, 0, connection.Message.Length, OnTcpMessageReceive, connection);
		}

		private void OnTcpMessageSend(IAsyncResult ar)
		{
			((NetworkStream)ar.AsyncState).EndWrite(ar);
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
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, PORT);
			byte[] messageBytes = udpClient.EndReceive(ar, ref sender);

			UdpMessageReceived(sender, messageBytes);

			udpClient.BeginReceive(OnUdpMessageReceive, null);
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
