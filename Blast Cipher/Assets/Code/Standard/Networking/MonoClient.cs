using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Networking
{
	public sealed class MonoClient : MonoNetwork
	{
		[SerializeField] private string targetIP = "127.0.0.1";

		[Header("Latency Display"), SerializeField] private float latencyUpdateInterval = .1f;
		[SerializeField] private TextMeshProUGUI tcpLatencyText;
		[SerializeField] private TextMeshProUGUI udpLatencyText;
		private StringBuilder stringBuilder = new StringBuilder();
		private int tcpLatency = 0;
		private int udpLatency = 0;

		//private PlayerCharacter[] players;

		private Timer messageFactory;
		private NetworkStream stream;
		private byte clientID = byte.MaxValue;
		private bool roundStarted = false;

		private int DEBUGCOUNT = 0;

		private void Start()
		{
			ConnectToServer(IPAddress.Parse(targetIP)); // DEBUG CALL
		}

		private void OnDestroy()
		{
			KillConnection();
		}

		public void ConnectToServer(IPAddress targetIP)
		{
			SetupAsClient(targetIP);
			messageFactory = new Timer(TimerTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(2D));
		}

		private void TimerTick(object obj)
		{
			//if (DEBUGCOUNT >= 1000)
			//{
			//	Debug.Log("clocking in");
			//	DEBUGCOUNT = 0;
			//}
			//else DEBUGCOUNT++;
		}

		public void KillConnection()
		{
			messageFactory.Dispose();
		}

		public void StartRound()
		{
			//StartCoroutine(UpdateAndSendTcp());
			//StartCoroutine(UpdateAndSendUdp());
			//StartCoroutine(UpdateLatencyDisplays());
		}

		private void PlayerAction(PlayerCharacter.ActionType action)
		{
			Debug.Log(action + " performed");
		}

		//private IEnumerator UpdateAndSendTcp()
		//{
		//	var message = new TcpMessage()
		//	{
		//		MessageType = (byte)MessageType.Gameplay,
		//		ClientID = clientID
		//	};

		//	var waiter = new WaitForSecondsRealtime(positionUpdateInterval);

		//	while(true)
		//	{
		//		//message.MillisecondTimestamp = GetTime;
		//		//message.PlayerPosition = localPlayer.transform.position;

		//		//SendTcpMessage(stream, message.ToArray());

		//		yield return waiter;
		//	}
		//}

		//private IEnumerator UpdateAndSendUdp()
		//{
		//	var message = new UdpMessage()
		//	{
		//		ClientID = clientID
		//	};

		//	var waiter = new WaitForSecondsRealtime(0f);
		//	while (true)
		//	{
		//		message.MillisecondTimestamp = GetTime;
		//		message.MovementInput = localPlayer.MovementInput;
		//		message.AimInput = localPlayer.AimInput;

		//		SendUdpMessage(message.ToArray());

		//		yield return waiter;
		//	}
		//}

		private IEnumerator UpdateLatencyDisplays()
		{
			var waiter = new WaitForSecondsRealtime(latencyUpdateInterval);
			while (true)
			{
				stringBuilder.Append("TCP: ").Append(Mathf.Max(0, tcpLatency)).Append(" ms");
				tcpLatencyText.text = stringBuilder.ToString();
				stringBuilder.Clear();

				stringBuilder.Append("UDP: ").Append(Mathf.Max(0, udpLatency)).Append(" ms");
				udpLatencyText.text = stringBuilder.ToString();
				stringBuilder.Clear();
				yield return waiter;
			}
		}

		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			this.stream = stream;
		}

		protected override void TcpMessageReceived(NetworkStream sender, byte[] messageBytes)
		{
			var message = NetworkMessage.Parse<TcpMessage>(messageBytes);
			tcpLatency = GetTime - message.MillisecondTimestamp;

			switch ((MessageType)message.MessageType)
			{
				case MessageType.Initialization:

					break;

				case MessageType.Gameplay:

					break;

				case MessageType.Undefined:
				default:
					Debug.Log("Err");
					break;
			}
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] messageBytes)
		{
			var message = NetworkMessage.Parse<UdpMessage>(messageBytes);
			udpLatency = GetTime - message.MillisecondTimestamp;


		}
	}
}
