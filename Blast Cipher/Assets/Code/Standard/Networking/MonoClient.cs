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
		[SerializeField] private float latencyUpdateInterval = .1f;
		[SerializeField] private float positionUpdateInterval = .1f;
		[SerializeField] private float velocityUpdateInterval = .1f;
		[SerializeField] private TextMeshProUGUI tcpLatencyText;
		[SerializeField] private TextMeshProUGUI udpLatencyText;
		[SerializeField] private Vector3[] playerSpawnPos;
		[SerializeField] private GameObject playerPrefab;
		[SerializeField] private GameObject networkPlayerPrefab;

		private readonly Queue<PlayerCharacter.ActionType> outgoingQueuedActions = new Queue<PlayerCharacter.ActionType>();
		private readonly Queue<PlayerCharacter.ActionType> incomingQueuedActions = new Queue<PlayerCharacter.ActionType>();
		private StringBuilder stringBuilder = new StringBuilder();
		private int tcpLatency = 0;
		private int udpLatency = 0;

		private bool roundStarted = false;
		private PlayerCharacter networkPlayer;
		private PlayerCharacter localPlayer;
		private Vector3 bufferedPosition = default;
		private Vector3 bufferedRotation = default;

		private NetworkStream stream;
		[SerializeField] private byte clientID = byte.MaxValue;

		private void Start()
		{
			SetupAsClient(IPAddress.Parse(targetIP));

			StartCoroutine(UpdateLatencyDisplays());
			StartCoroutine(ShareEntityPositions());
			StartCoroutine(SharePlayerInputs());
		}

		private void LateUpdate()
		{
			UpateEntityTransforms();
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

		private IEnumerator ShareEntityPositions()
		{
			while(stream == null || localPlayer == null) { yield return null; }

			NetworkMessage message = new NetworkMessage()
			{
				Type = (byte)MessageType.EntityPositions,
				ClientID = clientID
			};

			var waiter = new WaitForSecondsRealtime(positionUpdateInterval);
			while (localPlayer != null)
			{
				message.MillisecondTimestamp = GetTime;

				message.playerPosition = new float[] 
				{
					localPlayer.transform.position.x,
					localPlayer.transform.position.y,
					localPlayer.transform.position.z
				};

				var eulerRot = localPlayer.transform.rotation.eulerAngles;
				message.playerRotation = new float[] 
				{
					eulerRot.x,
					eulerRot.y,
					eulerRot.z
				};

				SendTcpMessage(stream, message.ToArray());
				yield return waiter;
			}
		}

		private IEnumerator SharePlayerInputs()
		{
			while (stream == null || localPlayer == null) { yield return null; }

			FrequentMessage message = new FrequentMessage()
			{
				ClientID = clientID
			};

			var waiter = new WaitForSecondsRealtime(0f);
			while (localPlayer != null)
			{
				message.MillisecondTimestamp = GetTime;

				message.movementInput = new float[] { localPlayer.movementInput.x, localPlayer.movementInput.y };
				message.rotationInput = new float[] { localPlayer.aimInput.x, localPlayer.aimInput.y };

				SendUdpMessage(message.ToArray());
				yield return waiter;
			}
		}

		private void UpateEntityTransforms()
		{
			if (!roundStarted)
			{
				if (clientID == byte.MaxValue) return;
				else SpawnPlayers();
			}
			else if(networkPlayer != null && localPlayer != null)
			{
				//networkPlayer.transform.position = bufferedPosition;
				//networkPlayer.transform.rotation = Quaternion.Euler(bufferedRotation);
				while (incomingQueuedActions.Count > 0)
				{
					networkPlayer.PerformAction(incomingQueuedActions.Dequeue());
				}
			}
		}

		private void SpawnPlayers()
		{
			for(int i = 0; i < playerSpawnPos.Length; i++)
			{
				if(i == clientID)
				{
					localPlayer = Instantiate(playerPrefab).GetComponent<PlayerCharacter>();
					localPlayer.transform.position = playerSpawnPos[i];
					localPlayer.RegisterNetworkHook(PlayerActionCallback);
					localPlayer.DebugKBControlsActive = true;
				}
				else
				{
					networkPlayer = Instantiate(networkPlayerPrefab).GetComponent<PlayerCharacter>();
					networkPlayer.transform.position = playerSpawnPos[i];
					networkPlayer.DebugKBControlsActive = false;
				}
			}
			roundStarted = true;
		}

		private void PlayerActionCallback(PlayerCharacter.ActionType action)
		{
			outgoingQueuedActions.Enqueue(action);
		}

		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			this.stream = stream;
		}

		protected override void TcpMessageReceived(NetworkStream sender, byte[] message)
		{
			var messageInst = NetworkMessage.Parse(message);
			tcpLatency = GetTime - messageInst.MillisecondTimestamp;
			HandleTcpMessage(messageInst);
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] message)
		{
			var messageInst = FrequentMessage.Parse(message);
			udpLatency = GetTime - messageInst.MillisecondTimestamp;
			HandleUdpMessage(messageInst);
		}

		private void HandleTcpMessage(NetworkMessage message)
		{
			switch ((MessageType)message.Type)
			{
				default:
				case MessageType.Undefined: return;

				case MessageType.Initialization:
					clientID = message.ClientID;
					break;

				case MessageType.EntityPositions:
					bufferedPosition = new Vector3
						(message.playerPosition[0], 
						message.playerPosition[1], 
						message.playerPosition[2]);

					bufferedRotation = new Vector3
						(message.playerRotation[0],
						message.playerRotation[1],
						message.playerRotation[2]);
					break;
			}
		}

		private void HandleUdpMessage(FrequentMessage message)
		{
			try
			{
				networkPlayer.movementInput = new Vector2(message.movementInput[0], message.movementInput[1]);
				networkPlayer.aimInput = new Vector2(message.rotationInput[0], message.rotationInput[1]);
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
