using Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
	public class MonoClient : MonoBehaviour
	{
		[SerializeField] private TMPro.TextMeshProUGUI RTT_Text;
		[SerializeField] private TMPro.TextMeshProUGUI ClientIdDebugDisplay;
		[SerializeField] private string playerName;

		[SerializeField] private CharacterController[] players;
		private readonly Tuple<Vector3, Vector3>[] bufferedTransforms = new Tuple<Vector3, Vector3>[2];

		private UdpClientInterface client;
		private UdpServerInterface server;

		private MessageType bitmask;
		private float RTT;
		private byte clientID = 255;

		private readonly System.Diagnostics.Stopwatch stopwatch_RTT = new System.Diagnostics.Stopwatch();
		private readonly System.Diagnostics.Stopwatch methodSpecificStopwatch = new System.Diagnostics.Stopwatch();


		private void Start()
		{
			client = UdpClientInterface.ConnectTo("127.0.0.1", 32123);

			//wait for reply messages from server and send them to console
			Task.Factory.StartNew(async () => {
				while (true)
				{
					try
					{
						var received = await client.Receive();

						DecipherNetworkMessage(received.MessageBytes);

						stopwatch_RTT.Stop();
						RTT = stopwatch_RTT.ElapsedMilliseconds;
						stopwatch_RTT.Reset();
					}
					catch (Exception ex)
					{
						Debug.Log(ex);
					}
				}
			});

			StartCoroutine(HandleNetworkOps());
		} // fire-and-forget task factory to receive server messages

		private void Update()
		{
			RTT_Text.text = RTT.ToString() + " ms";
			ClientIdDebugDisplay.text = "ID: " + clientID;

			Transform cachedTransform;
			for(int i = 0; i < bufferedTransforms.Length; i++)
			{
				if(bufferedTransforms[i] != null)
				{
					cachedTransform = players[i].transform;
					cachedTransform.position = bufferedTransforms[i].Item1;
					cachedTransform.rotation = Quaternion.Euler(bufferedTransforms[i].Item2);
				}
			}
		}

		private IEnumerator HandleNetworkOps()
		{
			// connecting to server and setting up own client ID
			bitmask = MessageType.ConnectionSetup;
			while(clientID == 255) // 255 is the client id for "unassigned"
			{
				yield return SetupMessageSending(bitmask);
			}

			// ready check across all connected clients TODO
			//bitmask = MessageType.ReadyCheck;
			//while (true)
			//{
			//	yield return SetupMessageSending(bitmask);
			//}

			// main game loop
			bitmask = MessageType.PlayerPosition;
			while (true)
			{
				yield return SetupMessageSending(bitmask);
			}
		}

		private WaitForSeconds SetupMessageSending(MessageType bitmask)
		{
			stopwatch_RTT.Start(); // stopwatch to measure full round trip time (RTT/latency)

			client.Send(ConstructMessage(bitmask));

			return null;
		}

		private byte[] ConstructMessage(MessageType bitmask)
		{
			var messageBytes = new List<byte>();
			var messageSegmentLengths = new List<short>();
			var plannedMessageTypes = NetworkUtilities.DecipherBitmask(bitmask);

			// add bitmask to message
			messageBytes.AddRange(BitConverter.GetBytes((int)bitmask));

			// fill message with content
			for(int i = 0; i < plannedMessageTypes.Count; i++)
			{
				switch (plannedMessageTypes[i])
				{
					case MessageType.ConnectionSetup:
						byte[] nameBytes = Encoding.ASCII.GetBytes(playerName);
						messageBytes.AddRange(nameBytes);
						messageSegmentLengths.Add((short)nameBytes.Length);
						break;

					case MessageType.ReadyCheck:
						messageBytes.Add(BitConverter.GetBytes(true)[0]);
						messageSegmentLengths.Add(1);
						break;

					case MessageType.PlayerPosition:
						messageBytes.Add(clientID);

						Vector3 playerPos = players[clientID].transform.position;
						messageBytes.AddRange(BitConverter.GetBytes(playerPos.x));
						messageBytes.AddRange(BitConverter.GetBytes(playerPos.y));
						messageBytes.AddRange(BitConverter.GetBytes(playerPos.z));

						Vector3 eulerRot = players[clientID].transform.rotation.eulerAngles;
						messageBytes.AddRange(BitConverter.GetBytes(eulerRot.x));
						messageBytes.AddRange(BitConverter.GetBytes(eulerRot.y));
						messageBytes.AddRange(BitConverter.GetBytes(eulerRot.z));

						messageSegmentLengths.Add(4 * 6 + 1); // sizeof(float) * amt of floats + clientID byte
						break;

					default: throw new Exception("Bitmask Error");
				}
			}

			// specify message segment lengths
			for(int i = 0; i < messageSegmentLengths.Count; i++)
			{
				messageBytes.InsertRange(4 + i * 2, BitConverter.GetBytes(messageSegmentLengths[i]));
			}

			return messageBytes.ToArray();
		}

		private void DecipherNetworkMessage(byte[] message)
		{
			// get message content types
			var matchingTypes = NetworkUtilities.DecipherBitmask(message);

			// get length of each segment
			short[] segmentLengths = new short[matchingTypes.Count];
			for(int i = 0; i < segmentLengths.Length; i++)
			{
				segmentLengths[i] = BitConverter.ToInt16(message, 4 + i * 2);
			}

			// iterate through message segments
			int currentIndex = 4 + matchingTypes.Count * 2;
			for (int i = 0; i < matchingTypes.Count; i++)
			{
				switch (matchingTypes[i])
				{
					case MessageType.ConnectionSetup:
						clientID = message[currentIndex];
						break;

					case MessageType.ReadyCheck:
						bool recoveredReadyCheck = BitConverter.ToBoolean(message, currentIndex);
						break;

					case MessageType.PlayerPosition:
						// buffer position and rotation into the trans buffer, which will be applied on update
						bufferedTransforms[message[currentIndex]] = new Tuple<Vector3, Vector3>
							(new Vector3
								(BitConverter.ToSingle(message, currentIndex + 1),
								BitConverter.ToSingle(message, currentIndex + 5),
								BitConverter.ToSingle(message, currentIndex + 9)),

							new Vector3
								(BitConverter.ToSingle(message, currentIndex + 13),
								BitConverter.ToSingle(message, currentIndex + 17),
								BitConverter.ToSingle(message, currentIndex + 21)));
						break;

					default: throw new Exception("Bitmask Error");
				}
				currentIndex += segmentLengths[i];
			}
		}
	}
}
