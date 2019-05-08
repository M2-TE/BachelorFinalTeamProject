using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Networking
{

	public abstract class MonoNetwork : MonoBehaviour
	{
		private class TcpConnection
		{
			public NetworkStream Stream;
			public byte[] Message;
		}

		protected const int PORT = 18_000;
		protected const int TCP_DATAGRAM_SIZE_MAX = 1024;

		private UdpClient udpClient;

		private TcpClient tcpClient;
		private TcpListener tcpListener;

		private void OnDestroy()
		{
			tcpListener.Stop();

			tcpClient.Dispose();
			udpClient.Dispose();
		}

		protected void SetupAsServer()
		{
			udpClient = new UdpClient(PORT);
			
			tcpClient = new TcpClient();
			tcpListener = TcpListener.Create(PORT);
			tcpListener.Start();


			udpClient.BeginReceive(OnUdpMessageReceive, null);
			tcpListener.BeginAcceptTcpClient(OnTcpClientAccept, null);
		}

		protected void SetupAsClient(IPAddress ipAdress)
		{
			udpClient = new UdpClient();
			udpClient.Connect(ipAdress, PORT);

			tcpClient = new TcpClient();
			tcpClient.BeginConnect(ipAdress, PORT, OnTcpConnect, null);
		}
		
		#region TCP
		private void OnTcpClientAccept(IAsyncResult ar)
		{
			var senderClient = tcpListener.EndAcceptTcpClient(ar);
			var stream = senderClient.GetStream();
			byte[] bytes = new byte[TCP_DATAGRAM_SIZE_MAX];
			TcpConnectionEstablished(stream);

			// wait for messages on stream
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpMessageReceive, new TcpConnection() { Stream = stream, Message = bytes });
			
			// listen for more clients
			tcpListener.BeginAcceptTcpClient(OnTcpClientAccept, null); 
		}

		private void OnTcpConnect(IAsyncResult ar)
		{
			tcpClient.EndConnect(ar);

			TcpConnectionEstablished(tcpClient.GetStream());
		}

		private void OnTcpMessageReceive(IAsyncResult ar)
		{
			TcpConnection tcpStuff = (TcpConnection)ar.AsyncState;
			tcpStuff.Stream.EndRead(ar);

			TcpMessageReceived(tcpStuff.Message);

			tcpStuff.Stream.BeginRead(tcpStuff.Message, 0, tcpStuff.Message.Length, OnTcpMessageReceive, tcpStuff); // wait for more messages
		}

		private void OnTcpMessageSend(IAsyncResult ar)
		{
			((NetworkStream)ar).EndWrite(ar);
		}

		protected void SendTcpMessage(NetworkStream stream, byte[] message)
		{
			stream.BeginWrite(message, 0, message.Length, OnTcpMessageSend, stream);
		}

		protected virtual void TcpConnectionEstablished(NetworkStream stream)
		{

		}

		protected virtual void TcpMessageReceived(byte[] message)
		{

		}
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

		protected void SendUdpMessage(byte[] message, IPEndPoint target = null)
		{
			if(target == null) udpClient.BeginSend(message, message.Length, OnUdpMessageSend, null);
			else udpClient.BeginSend(message, message.Length, target, OnUdpMessageSend, null);
		}

		protected virtual void UdpMessageReceived(IPEndPoint sender, byte[] message)
		{
		
		}
		#endregion
	}
}
