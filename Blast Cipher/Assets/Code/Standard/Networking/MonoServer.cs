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
		private TcpMessage[] storedTcpMessages;
		private UdpMessage[] storedUdpMessages;

		private void Start()
		{
			SetupAsServer();

			tcpConnections = new List<NetworkStream>(maxClients);
			storedTcpMessages = new TcpMessage[maxClients];
			storedUdpMessages = new UdpMessage[maxClients];
			for (int i = 0; i < maxClients; i++)
			{
				storedTcpMessages[i] = new TcpMessage();
				storedUdpMessages[i] = new UdpMessage();
			}
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
			storedTcpMessages[message.ClientID] = message;

			var messageToSend = storedTcpMessages[message.ClientID == 0 ? 1 : 0];
			messageToSend.MillisecondTimestamp = message.MillisecondTimestamp;
			SendTcpMessage(sender, messageToSend.ToArray());
			//try
			//{
			//}
			//catch (Exception e)
			//{
			//	Debug.LogException(e);
			//}
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] messageBytes)
		{
			UdpMessage message = NetworkMessage.Parse<UdpMessage>(messageBytes);
			storedUdpMessages[message.ClientID] = message;

			var messageToSend = storedUdpMessages[message.ClientID == 0 ? 1 : 0];
			messageToSend.MillisecondTimestamp = message.MillisecondTimestamp;
			SendUdpMessage(sender, messageToSend.ToArray());
		}
	}
}
