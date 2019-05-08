using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Networking
{

	public abstract class MonoNetwork : MonoBehaviour
	{
		private class TcpConnectionThingy
		{
			public NetworkStream Stream;
			public byte[] Message;
		}

		[SerializeField] private int PORT = 18_000;
		[SerializeField] private int TCP_DATAGRAM_SIZE_MAX = 1024;

		private UdpClient udpClient;

		private TcpClient tcpClient;
		private TcpListener tcpListener;

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
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpMessageReceive, new TcpConnectionThingy() { Stream = stream, Message = bytes });
			
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
			var connection = new TcpConnectionThingy() { Message = bytes, Stream = stream };
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpMessageReceive, connection);
		}

		private void OnTcpMessageReceive(IAsyncResult ar)
		{
			TcpConnectionThingy connection = (TcpConnectionThingy)ar.AsyncState;
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
