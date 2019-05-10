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
		[SerializeField] private float positionUpdateInternal = .1f;
		[SerializeField] private float velocityUpdateInternal = .1f;
		[SerializeField] private TextMeshProUGUI tcpLatencyText;
		[SerializeField] private TextMeshProUGUI udpLatencyText;
		[SerializeField] private Vector3[] playerSpawnPos;
		[SerializeField] private GameObject playerPrefab;
		[SerializeField] private GameObject networkPlayerPrefab;

		private readonly Queue<PlayerCharacter.ActionType> queuedActions = new Queue<PlayerCharacter.ActionType>();
		private StringBuilder stringBuilder = new StringBuilder();
		private int tcpLatency = 0;
		private int udpLatency = 0;

		private List<Transform>[] transformsPerClient;
		private PlayerCharacter localPlayer;
		private Vector3 bufferedPosition;

		private NetworkStream stream;
		[SerializeField] private byte clientID = byte.MaxValue;

		private void Start()
		{
			SetupAsClient(IPAddress.Parse(targetIP));

			StartCoroutine(UpdateLatencyDisplays());
			StartCoroutine(ShareEntityPositions());
		}

		private void Update()
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
				ClientID = clientID,
				EntityType = new byte[] { (byte)MessageType.EntityPositions },
				xPositions = new float[] { localPlayer.transform.position.x },
				yPositions = new float[] { localPlayer.transform.position.y },
				zPositions = new float[] { localPlayer.transform.position.z }
			};

			var waiter = new WaitForSecondsRealtime(positionUpdateInternal);
			while (true)
			{
				message.MillisecondTimestamp = GetTime;

				message.EntityType = new byte[] { (byte)MessageType.EntityPositions };
				message.xPositions = new float[] { localPlayer.transform.position.x };
				message.yPositions = new float[] { localPlayer.transform.position.y };
				message.zPositions = new float[] { localPlayer.transform.position.z };

				SendTcpMessage(stream, message.ToArray());
				yield return waiter;
			}
		}

		private void UpateEntityTransforms()
		{
			if (transformsPerClient == null)
			{
				if (clientID == byte.MaxValue) return;
				else SpawnPlayers();
			}
			else
			{
				for (int i = 0; i < transformsPerClient.Length; i++)
				{
					if (i == clientID) continue;
					for (int k = 0; k < transformsPerClient[i].Count; k++)
					{
						//Debug.Log(i + " " + bufferedPosition);
						transformsPerClient[i][k].position = bufferedPosition;
					}
				}
			}
		}

		private void SpawnPlayers()
		{
			transformsPerClient = new List<Transform>[] { new List<Transform>(), new List<Transform>() };
			
			for(int i = 0; i < transformsPerClient.Length; i++)
			{
				PlayerCharacter player = null;
				if(i == clientID) // own player
				{
					player = Instantiate(playerPrefab).GetComponent<PlayerCharacter>();
					player.transform.position = playerSpawnPos[i];
					player.RegisterNetworkHook(PlayerActionCallback);
					player.DebugKBControlsActive = true;
					localPlayer = player;
				}
				else // network controlled player
				{
					player = Instantiate(networkPlayerPrefab).GetComponent<PlayerCharacter>();
					player.transform.position = playerSpawnPos[i];
					player.DebugKBControlsActive = false;

					// DEBUG
					bufferedPosition = player.transform.position;
				}

				transformsPerClient[i].Add(player.transform);
			}
		}

		private void PlayerActionCallback(PlayerCharacter.ActionType action)
		{
			queuedActions.Enqueue(action);
		}

		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			this.stream = stream;
		}

		protected override void TcpMessageReceived(NetworkStream sender, byte[] message)
		{
			var messageInst = NetworkMessage.Parse(message);
			tcpLatency = GetTime - messageInst.MillisecondTimestamp;
			HandleServerMessage(messageInst);
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] message)
		{
			var messageInst = NetworkMessage.Parse(message);
			udpLatency = GetTime - messageInst.MillisecondTimestamp;
			HandleServerMessage(messageInst);
		}

		private void HandleServerMessage(NetworkMessage message)
		{
			switch ((MessageType)message.Type)
			{
				default:
				case MessageType.Undefined: return;

				case MessageType.Initialization:
					clientID = message.ClientID;
					Debug.Log(clientID);
					break;

				case MessageType.EntityPositions:

					bufferedPosition = new Vector3(message.xPositions[0], message.yPositions[0], message.zPositions[0]);
					//Debug.Log(clientID + " " + bufferedPosition);
					break;
			}
		}
	}
}
