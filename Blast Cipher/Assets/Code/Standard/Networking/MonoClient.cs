using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Networking
{
	public sealed class MonoClient : MonoNetwork
	{
		[SerializeField] private string targetIP = "127.0.0.1";
		NetworkStream stream;

		private void Start()
		{
			udpClient = new UdpClient(targetIP, port);

			tcpClient = new TcpClient();
			tcpClient.BeginConnect(targetIP, port, OnTcpConnect, null);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				byte[] bytes = Encoding.ASCII.GetBytes("urmomgay lol");

				//// send message over udp
				//udpClient.BeginSend(bytes, bytes.Length, OnUdpMessageSend, null);

				//// send message over tcp
				//if (stream != null)
				//{
				//	stream.BeginWrite(bytes, 0, bytes.Length, OnTcpMessageSend, null);
				//}
			}
		}

		private void OnTcpConnect(IAsyncResult ar)
		{
			tcpClient.EndConnect(ar);
			stream = tcpClient.GetStream();
			Debug.Log("CLIENT: TCP connection established");
		}

		private void OnTcpMessageSend(IAsyncResult ar)
		{
			stream.EndWrite(ar);
		}
		private void OnUdpMessageSend(IAsyncResult ar)
		{
			int i = udpClient.EndSend(ar);
		}
	}
}
