using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Networking
{
	public sealed class MonoServer : MonoNetwork
	{
		[SerializeField] private int maxClients;

		private List<NetworkStream> connectedClients;
		private NetworkMessage[] storedMessages;

		private void Start()
		{
			connectedClients = new List<NetworkStream>(maxClients);
			storedMessages = new NetworkMessage[maxClients];
			storedMessages[0] = new NetworkMessage() { ClientID = 0 };
			storedMessages[1] = new NetworkMessage() { ClientID = 1 };

			SetupAsServer();
		}

		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			connectedClients.Add(stream);
			NetworkMessage message = new NetworkMessage()
			{
				Type = (byte)MessageType.Initialization,
				ClientID = (byte)connectedClients.IndexOf(stream)
			};

			SendTcpMessage(stream, message.ToArray());
		}

		protected override void TcpMessageReceived(NetworkStream sender, byte[] message)
		{
			var netMessage = NetworkMessage.Parse(message);
			HandleClientMessage(netMessage, sender);
			for (int i = 0; i < storedMessages.Length; i++)
			{
				if (storedMessages[i].ClientID == netMessage.ClientID) continue;
				storedMessages[i].MillisecondTimestamp = netMessage.MillisecondTimestamp;
				SendTcpMessage(sender, storedMessages[i].ToArray());
			}
			//try
			//{
			//}
			//catch (Exception e)
			//{
			//	Debug.LogException(e);
			//}
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] message)
		{
			//SendUdpMessage(sender, message); // reply
		}

		private void HandleClientMessage(NetworkMessage message, NetworkStream stream)
		{
			switch ((MessageType)message.Type)
			{
				default:
				case MessageType.Initialization:
				case MessageType.Undefined: return;

				case MessageType.EntityPositions:
					storedMessages[message.ClientID] = message;
					break;
			}
		}
	}
}
