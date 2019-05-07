using Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
	public class MonoClientDepr : MonoBehaviour
	{
		[SerializeField] private TMPro.TextMeshProUGUI RTT_Text;
		[SerializeField] private TMPro.TextMeshProUGUI FPS_Text;
		[SerializeField] private TMPro.TextMeshProUGUI ClientIdDebugDisplay;
		[SerializeField] private string playerName;

		[SerializeField] private CharacterController[] players;
		private readonly Tuple<Vector3, Vector3>[] bufferedTransforms = new Tuple<Vector3, Vector3>[2];

		private UdpClientInterface client;
		private Task networkTask;
		private CancellationTokenSource tokenSource;
		private CancellationToken token;

		private MessageType bitmask;
		private float RTT;
		private byte clientID = 255;

		private readonly System.Diagnostics.Stopwatch stopwatch_RTT = new System.Diagnostics.Stopwatch();
		private readonly System.Diagnostics.Stopwatch methodSpecificStopwatch = new System.Diagnostics.Stopwatch();

		// cached vars
		List<byte> messageBytes = new List<byte>();
		List<short> messageSegmentLengths = new List<short>();
		List<MessageType> plannedMessageTypes;

		/***************************************/

		private void Start()
		{
			client = UdpClientInterface.ConnectTo("127.0.0.1", 32123);

			tokenSource = new CancellationTokenSource();
			token = tokenSource.Token;
			networkTask = Task.Run(async () =>
			{
				try
				{
					token.ThrowIfCancellationRequested();
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
			}, tokenSource.Token);

			StartCoroutine(HandleNetworkOps());
			StartCoroutine(FpsUpdater());
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

		private void OnDestroy()
		{
			tokenSource.Cancel();
			networkTask.Wait();
			networkTask.Dispose();
			client.ShutDown();
		}

		private IEnumerator FpsUpdater()
		{
			while (true)
			{
				FPS_Text.text = (int)(1f / Time.deltaTime) + " fps";
				yield return new WaitForSecondsRealtime(.1f);
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

		private WaitForEndOfFrame SetupMessageSending(MessageType bitmask)
		{
			stopwatch_RTT.Start(); // stopwatch to measure full round trip time (RTT/latency)

			client.Send(ConstructMessage(bitmask));

			return new WaitForEndOfFrame();
		}

		private byte[] ConstructMessage(MessageType bitmask)
		{
			messageBytes.Clear();
			messageSegmentLengths.Clear();
			plannedMessageTypes = NetworkUtilities.DecipherBitmask(bitmask);

			// add bitmask to message
			messageBytes = messageBytes.Concat(BitConverter.GetBytes((int)bitmask)).ToList();

			// fill message with content
			for(int i = 0; i < plannedMessageTypes.Count; i++)
			{
				switch (plannedMessageTypes[i])
				{
					case MessageType.ConnectionSetup:
						byte[] nameBytes = Encoding.ASCII.GetBytes(playerName);
						messageBytes = messageBytes.Concat(nameBytes).ToList();
						messageSegmentLengths.Add((short)nameBytes.Length);
						break;

					case MessageType.ReadyCheck:
						messageBytes.Add(BitConverter.GetBytes(true)[0]);
						messageSegmentLengths.Add(1);
						break;

					case MessageType.PlayerPosition:
						messageBytes.Add(clientID);

						Vector3 playerPos = players[clientID].transform.position;
						messageBytes = messageBytes.Concat(BitConverter.GetBytes(playerPos.x)).ToList();
						messageBytes = messageBytes.Concat(BitConverter.GetBytes(playerPos.y)).ToList();
						messageBytes = messageBytes.Concat(BitConverter.GetBytes(playerPos.z)).ToList();

						Vector3 eulerRot = players[clientID].transform.rotation.eulerAngles;
						messageBytes = messageBytes.Concat(BitConverter.GetBytes(eulerRot.x)).ToList();
						messageBytes = messageBytes.Concat(BitConverter.GetBytes(eulerRot.y)).ToList();
						messageBytes = messageBytes.Concat(BitConverter.GetBytes(eulerRot.z)).ToList();

						messageSegmentLengths.Add(1 + 4 * 6); // sizeof(float) * amt of floats + clientID byte
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
			plannedMessageTypes = NetworkUtilities.DecipherBitmask(message);

			// get length of each segment
			short[] segmentLengths = new short[plannedMessageTypes.Count];
			for(int i = 0; i < segmentLengths.Length; i++)
			{
				segmentLengths[i] = BitConverter.ToInt16(message, 4 + i * 2);
			}

			// iterate through message segments
			int currentIndex = 4 + plannedMessageTypes.Count * 2;
			for (int i = 0; i < plannedMessageTypes.Count; i++)
			{
				switch (plannedMessageTypes[i])
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
