using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Networking
{
	public sealed class MonoServer : MonoNetwork
	{
		[SerializeField] private int maxClients;

		private PlayerCharacter[] players;
		private int connectedClients;
		private bool gameStarted;

		private void Start()
		{
			SetupAsServer(true, true);
		}

		private void Update()
		{
			if(connectedClients == maxClients)
			{
				StartGame();
			}
		}

		public void StartGame()
		{
			players = FindObjectsOfType<PlayerCharacter>();
			gameStarted = true;
		}

		protected override void OnTimerTick(object obj)
		{
			//Debug.Log("server");
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] messageBytes)
		{
			if (!gameStarted) return;

			InputDataMessageUdp message = NetworkMessage.Parse<InputDataMessageUdp>(messageBytes);

			players[message.ClientID].MovementInput = message.MovementInput;
			players[message.ClientID].AimInput = message.AimInput;

			//SendUdpMessage(sender, messageToSend.ToArray());
		}

		#region TCP
		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			if (gameStarted) return;

			var message = new TcpMessage()
			{
				MessageType = (byte)MessageType.Initialization,
				ClientID = (byte)connectedClients
			};

			connectedClients++;
			SendTcpMessage(stream, message.ToArray());

			if (connectedClients == maxClients) StartGame();
			//else if (connectedClients > maxClients)
			//{
			//	Debug.LogException(new System.Exception("Player Overflow"));
			//}
		}

		protected override void TcpMessageReceived(NetworkStream sender, byte[] messageBytes)
		{
			if (!gameStarted) return;

			//TcpMessage message = NetworkMessage.Parse<TcpMessage>(messageBytes);


			//SendTcpMessage(sender, messageToSend.ToArray());
		}
		#endregion
	}
}
