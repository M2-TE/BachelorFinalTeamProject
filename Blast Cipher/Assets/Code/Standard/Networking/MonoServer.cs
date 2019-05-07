using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Networking
{
	public sealed class MonoServer : MonoNetwork
	{
		private class TcpConnection
		{
			public NetworkStream Stream;
			public byte[] Message;
		}

		private void Start()
		{
			udpClient = new UdpClient(port);

			tcpClient = new TcpClient();
			tcpListener = TcpListener.Create(port);
			tcpListener.Start();
			tcpListener.BeginAcceptTcpClient(OnTcpClientAccept, null);


			udpClient.BeginReceive(OnUdpMessageReceive, null);
		}

		private void OnTcpClientAccept(IAsyncResult ar)
		{
			var newClient = tcpListener.EndAcceptTcpClient(ar);
			var stream = newClient.GetStream();
			byte[] bytes = new byte[1024];
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpMessageReceive, new TcpConnection() { Stream = stream, Message = bytes });

			tcpListener.BeginAcceptTcpClient(OnTcpClientAccept, null); // listen for more clients
		}

		private void OnTcpMessageReceive(IAsyncResult ar)
		{
			TcpConnection tcpStuff = (TcpConnection)ar.AsyncState;
			tcpStuff.Stream.EndRead(ar);

			//Debug.Log(Encoding.ASCII.GetString(tcpStuff.Message));

			tcpStuff.Stream.BeginRead(tcpStuff.Message, 0, tcpStuff.Message.Length, OnTcpMessageReceive, tcpStuff); // wait for more messages
		}

		private void OnUdpMessageReceive(IAsyncResult ar)
		{
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, port);
			byte[] messageBytes = udpClient.EndReceive(ar, ref sender);

			//Debug.Log(Encoding.ASCII.GetString(messageBytes));

			udpClient.BeginReceive(OnUdpMessageReceive, null);
		}
	}
}
