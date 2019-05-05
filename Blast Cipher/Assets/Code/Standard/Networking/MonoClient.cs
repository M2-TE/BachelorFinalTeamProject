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
	public enum MessageType
	{
		ConnectionSetup = 0b_1, // [string] Player Name + [Custom Mesh] player mesh (+ SERVER ONLY assignment of playerID)
		ClientID = 0b_10, // [byte] player ID
		ReadyCheck = 0b_100, // [bool] player ready
		//PlayerMovement = 0b_10_000 // [float2] vertical and horizontal input requests CLIENT: own pos only, SERVER: both positions
		PlayerMovement = 0b_10_000, // [float3] player pos CLIENT: own pos only, SERVER: both positions
		ErrorCode = 0b_100_000 // [byte] error num
	}

	// 1. message byte form: [int] = bitmask e.g. (0b_0100_1010)
	//
	// 2. lengths of data segments in bytes: [char]  corresponding to bitmask segments e.g. (23 => 23 bytes of data for segment X)
	//
	// 3. message contents: [TYPE] actual messages
	//
	// 4. ParamString TODO

	public struct NetworkMessage
	{
		public IPEndPoint Sender;
		public byte[] MessageBytes;
	}

	public static class NetworkUtilities
	{
		private readonly static Array arr = Enum.GetValues(typeof(MessageType));

		public static List<MessageType> DecipherBitmask(byte[] message)
		{
			List<MessageType> bitmaskList = new List<MessageType>();
			MessageType messageBitmask = (MessageType)BitConverter.ToInt32(message, 0);

			//var arr = Enum.GetValues(typeof(MessageType));
			MessageType buffer;
			for (int i = 0; i < arr.Length; i++)
			{
				buffer = (MessageType)arr.GetValue(i);

				if ((messageBitmask & buffer) != 0) // check if enum is included in bitmask
				{
					bitmaskList.Add(buffer);
				}
			}

			return bitmaskList;
		}
		public static List<MessageType> DecipherBitmask(MessageType messageBitmask)
		{
			List<MessageType> bitmaskList = new List<MessageType>();

			//var arr = Enum.GetValues(typeof(MessageType));
			MessageType buffer;
			for (int i = 0; i < arr.Length; i++)
			{
				buffer = (MessageType)arr.GetValue(i);

				if ((messageBitmask & buffer) != 0) // check if enum is included in bitmask
				{
					bitmaskList.Add(buffer);
				}
			}

			return bitmaskList;
		}
	}
}

namespace Networking
{
	public class MonoClient : MonoBehaviour
	{
		[SerializeField] private TMPro.TextMeshProUGUI RTT_Text;
		[SerializeField] private string playerName;

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
						Debug.LogException(ex);
					}
				}
			});

			StartCoroutine(HandleNetworkOps());
		}

		private void Update()
		{
			RTT_Text.text = RTT.ToString();
		}

		private IEnumerator HandleNetworkOps()
		{
			bitmask = MessageType.ConnectionSetup | MessageType.ReadyCheck;
			while(clientID == 255)
			{
				stopwatch_RTT.Start();

				client.Send(ConstructMessage(bitmask));

				yield return null;
			}
			yield return null;
		}

		private byte[] ConstructMessage(MessageType bitmask)
		{
			var messageBytes = new List<byte>();
			var messageSegmentLengths = new List<short>();
			var plannedMessageTypes = NetworkUtilities.DecipherBitmask(bitmask);

			// add bitmask to message
			messageBytes.AddRange(BitConverter.GetBytes((int)bitmask));

			// fill message with content
			MessageType cachedType;
			for(int i = 0; i < plannedMessageTypes.Count; i++)
			{
				cachedType = plannedMessageTypes[i];
				switch (cachedType)
				{
					case MessageType.ConnectionSetup:
						byte[] nameBytes = Encoding.ASCII.GetBytes(playerName);
						messageBytes.AddRange(nameBytes);
						messageSegmentLengths.Add((short)nameBytes.Length);
						break;

					case MessageType.ClientID:

						break;

					case MessageType.ReadyCheck:
						messageBytes.Add(BitConverter.GetBytes(true)[0]);
						messageSegmentLengths.Add(1);
						break;

					case MessageType.PlayerMovement:

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
			MessageType cachedType;
			for (int i = 0; i < matchingTypes.Count; i++)
			{
				cachedType = matchingTypes[i];
				switch (cachedType)
				{
					case MessageType.ConnectionSetup:
						string recoveredString = Encoding.ASCII.GetString(message, currentIndex, segmentLengths[i]);
						break;

					case MessageType.ClientID:

						break;

					case MessageType.ReadyCheck:
						bool recoveredReadyCheck = BitConverter.ToBoolean(message, currentIndex);
						break;

					case MessageType.PlayerMovement:

						break;

					default: throw new Exception("Bitmask Error");
				}
				currentIndex += segmentLengths[i];
			}
		}
	}
}
