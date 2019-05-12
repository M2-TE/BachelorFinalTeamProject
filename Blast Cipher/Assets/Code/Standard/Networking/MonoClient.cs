using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TMPro;
using UnityEngine;

namespace Networking
{
	public sealed class MonoClient : MonoNetwork
	{
		[SerializeField] private string targetIP = "127.0.0.1";
		[SerializeField] private float positionUpdateInterval = .1f;
		[SerializeField] private Vector3[] playerSpawnPos;
		[SerializeField] private GameObject playerPrefab;

		#region Latency Display
		[SerializeField] private float latencyUpdateInterval = .1f;
		[SerializeField] private TextMeshProUGUI tcpLatencyText;
		[SerializeField] private TextMeshProUGUI udpLatencyText;
		private StringBuilder stringBuilder = new StringBuilder();
		private int tcpLatency = 0;
		private int udpLatency = 0;
		#endregion

		private PlayerCharacter localPlayer;
		private PlayerCharacter networkPlayer;
		private Vector3 bufferedPosition = default;
		private bool newPosBuffered = false;

		private NetworkStream stream;
		private byte clientID = byte.MaxValue;
		private bool roundStarted = false;

		private void Start()
		{
			ConnectToServer(IPAddress.Parse(targetIP)); // DEBUG CALL
			StartCoroutine(StartRoundDelayed()); // DEBUG CALL

			StartCoroutine(UpdateLatencyDisplays());
		}

		private void LateUpdate()
		{
			if (newPosBuffered)
			{
				//networkPlayer.transform.position = Vector3.MoveTowards(networkPlayer.transform.position, bufferedPosition, .1f);
				networkPlayer.transform.position = Vector3.Lerp(networkPlayer.transform.position, bufferedPosition, .1f);
				//networkPlayer.transform.position = bufferedPosition;
				newPosBuffered = false;
			}
		}

		private void PlayerAction(PlayerCharacter.ActionType action)
		{

		}

		public void ConnectToServer(IPAddress targetIP)
		{
			SetupAsClient(targetIP);
		}

		public void StartRound()
		{
			SpawnPlayers();

			StartCoroutine(UpdateAndSendTcp());
			StartCoroutine(UpdateAndSendUdp());
		}

		public IEnumerator StartRoundDelayed()
		{
			while (!roundStarted) yield return null;
			StartRound();
		}

		private void SpawnPlayers()
		{
			for(int i = 0; i < playerSpawnPos.Length; i++)
			{
				if (i == clientID)
				{
					localPlayer = Instantiate(playerPrefab, playerSpawnPos[i], Quaternion.identity).GetComponent<PlayerCharacter>();
					localPlayer.NetworkControlled = false;
					localPlayer.RegisterNetworkHook(PlayerAction);
				}
				else
				{
					networkPlayer = Instantiate(playerPrefab, playerSpawnPos[i], Quaternion.identity).GetComponent<PlayerCharacter>();
					networkPlayer.NetworkControlled = true;
					networkPlayer.DebugKBControlsActive = false;

					bufferedPosition = playerSpawnPos[i];
				}
			}
		}

		private IEnumerator UpdateAndSendTcp()
		{
			var message = new TcpMessage()
			{
				MessageType = (byte)MessageType.Gameplay,
				ClientID = clientID
			};

			var waiter = new WaitForSecondsRealtime(positionUpdateInterval);
			while(localPlayer != null && networkPlayer != null)
			{
				message.MillisecondTimestamp = GetTime;
				message.PlayerPosition = localPlayer.transform.position;

				SendTcpMessage(stream, message.ToArray());

				yield return waiter;
			}
		}

		private IEnumerator UpdateAndSendUdp()
		{
			var message = new UdpMessage()
			{
				ClientID = clientID
			};

			var waiter = new WaitForSecondsRealtime(0f);
			while (localPlayer != null && networkPlayer != null)
			{
				message.MillisecondTimestamp = GetTime;
				message.MovementInput = localPlayer.MovementInput;
				message.AimInput = localPlayer.AimInput;

				SendUdpMessage(message.ToArray());

				yield return waiter;
			}
		}

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
			switch ((MessageType)message.MessageType)
			{
				case MessageType.Initialization:
					clientID = message.ClientID;
					roundStarted = true;
					break;

				case MessageType.Gameplay:
					bufferedPosition = message.PlayerPosition;
					newPosBuffered = true;
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

			networkPlayer.MovementInput = message.MovementInput;
			networkPlayer.AimInput = message.AimInput;
		}
	}
}
