using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Networking
{
	public sealed class MonoServer : MonoNetwork
	{
		[SerializeField] private int maxClients;

		private List<NetworkStream> tcpConnections;

		private void Start()
		{
			SetupAsServer();

			tcpConnections = new List<NetworkStream>(maxClients);
	}

		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			var message = new TcpMessage()
			{
				MessageType = (byte)MessageType.Initialization,
				ClientID = (byte)tcpConnections.Count
			};
			tcpConnections.Add(stream);
			SendTcpMessage(stream, message.ToArray());
		}

		protected override void TcpMessageReceived(NetworkStream sender, byte[] messageBytes)
		{
			TcpMessage message = NetworkMessage.Parse<TcpMessage>(messageBytes);


			//SendTcpMessage(sender, messageToSend.ToArray());
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] messageBytes)
		{
			UdpMessage message = NetworkMessage.Parse<UdpMessage>(messageBytes);
			
			//SendUdpMessage(sender, messageToSend.ToArray());
		}
	}
}
