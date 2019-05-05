using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
	public class MonoServer : MonoBehaviour
	{
		UdpServerInterface server;

		byte[] messageBytes = new byte[1];

		void Start()
		{
			server = new UdpServerInterface();

			//start listening for messages and copy the messages back to the client
			Task.Factory.StartNew(async () => {
				while (true)
				{
					try
					{
						var received = await server.Receive();
						DecipherNetworkMessage(received.MessageBytes);

						//for (int i = 0; i < 500000; i++) { } // intentional lag
						server.Reply(messageBytes, received.Sender);
					}
					catch (Exception e)
					{
						Debug.LogException(e);
					}
				}
			});
		}

		private void DecipherNetworkMessage(byte[] message)
		{
			//// get message content types
			//var matchingTypes = NetworkUtilities.DecipherBitmask(message);

			//// get length of each segment
			//short[] segmentLengths = new short[matchingTypes.Count];
			//for (int i = 0; i < segmentLengths.Length; i++)
			//{
			//	segmentLengths[i] = BitConverter.ToInt16(message, 4 + i * 2);
			//}

			//// iterate through message segments
			//int currentIndex = 4 + matchingTypes.Count * 2;
			//MessageType cachedType;
			//for (int i = 0; i < matchingTypes.Count; i++)
			//{
			//	cachedType = matchingTypes[i];
			//	switch (cachedType)
			//	{
			//		case MessageType.ConnectionSetup:

			//			break;

			//		case MessageType.ClientID:

			//			break;

			//		case MessageType.ReadyCheck:

			//			break;

			//		case MessageType.PlayerMovement:

			//			break;

			//		default: throw new Exception("Bitmask Error");
			//	}
			//	currentIndex += segmentLengths[i];
			//}


			lock (messageBytes)
			{
				messageBytes = message;
			}
		}

		private void UpdateMessage()
		{
			lock (messageBytes)
			{

			}
		}
	}
}
