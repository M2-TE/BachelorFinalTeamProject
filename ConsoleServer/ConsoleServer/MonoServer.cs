using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
	public class MonoServer
	{
		private UdpServerInterface server;
		private readonly Dictionary<IPEndPoint, Tuple<byte, string>> clients = new Dictionary<IPEndPoint, Tuple<byte, string>>(2); // max 2 players

		readonly byte[] emptyMessage = new byte[0];
		readonly byte[][] clientMessages = new byte[2][];

		public void Start()
		{
			server = new UdpServerInterface();

			//start listening for messages and copy the messages back to the client
			Task.Factory.StartNew(async () => {
				while (true)
				{
					try
					{
						var received = await server.Receive();

						var outgoingBitmask = DecipherNetworkMessage(received.MessageBytes, received.Sender);
						var newMessage = ConstructMessage(outgoingBitmask, received.Sender);
						server.Reply(newMessage, received.Sender);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			});
		}

		private MessageType DecipherNetworkMessage(byte[] message, IPEndPoint clientEP)
		{
			// get message content types
			var matchingTypes = NetworkUtilities.DecipherBitmask(message);

			// get length of each segment
			short[] segmentLengths = new short[matchingTypes.Count];
			for (int i = 0; i < segmentLengths.Length; i++)
			{
				segmentLengths[i] = BitConverter.ToInt16(message, 4 + i * 2);
			}

			// iterate through message segments
			int currentIndex = 4 + matchingTypes.Count * 2;
			MessageType outgoingBitmask = 0b_0;
			for (int i = 0; i < matchingTypes.Count; i++)
			{
				switch (matchingTypes[i])
				{
					case MessageType.ConnectionSetup:
						string clientName = Encoding.ASCII.GetString(message, currentIndex, segmentLengths[i]);
						if (!clients.ContainsKey(clientEP))
						{
							//Debug.Log(clients.Count);
							clients.Add(clientEP, new Tuple<byte, string>((byte)clients.Count, clientName));
						}

						// assign client ID
						outgoingBitmask |= MessageType.ConnectionSetup;
						break;

					case MessageType.ReadyCheck:

						break;

					case MessageType.PlayerPosition:
						outgoingBitmask |= MessageType.PlayerPosition;

						clientMessages[clients[clientEP].Item1] = message; // save message with entity positions for other clients
						break;

					default: throw new Exception("Bitmask Error");
				}
				currentIndex += segmentLengths[i];
			}

			return outgoingBitmask;
		}

		private byte[] ConstructMessage(MessageType bitmask, IPEndPoint clientEP)
		{
			if (!clients.ContainsKey(clientEP)) return emptyMessage;
			
			// if the entity position exchange loop is active, relay other client messages
			if ((bitmask & MessageType.PlayerPosition) != 0)
			{
				var newMessage = clientMessages[clients[clientEP].Item1 == 0 ? 1 : 0];
				if (newMessage == null) return new byte[] { 0, 0, 0, 0 }; // return empty bitmask
				else return newMessage; // relay other client's message
			}
			
			var messageBytes = new List<byte>();
			var messageSegmentLengths = new List<short>();
			var plannedMessageTypes = NetworkUtilities.DecipherBitmask(bitmask);

			// add bitmask to message
			messageBytes.AddRange(BitConverter.GetBytes((int)bitmask));

			// fill message with content
			for (int i = 0; i < plannedMessageTypes.Count; i++)
			{
				switch (plannedMessageTypes[i])
				{
					case MessageType.ConnectionSetup:
						messageBytes.Add(clients[clientEP].Item1);
						messageSegmentLengths.Add(1);
						break;

					case MessageType.ReadyCheck:

						break;

					case MessageType.PlayerPosition:

						break;

					default: throw new Exception("Bitmask Error");
				}
			}

			// specify message segment lengths
			for (int i = 0; i < messageSegmentLengths.Count; i++)
			{
				messageBytes.InsertRange(4 + i * 2, BitConverter.GetBytes(messageSegmentLengths[i]));
			}

			return messageBytes.ToArray();
		}
	}
}
