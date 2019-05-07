using System;
using System.Collections.Generic;
using System.Net;

namespace Networking
{
	// 1. message byte form: [int] = bitmask e.g. (0b_0100_1010)
	//
	// 2. lengths of data segments in bytes: [char]  corresponding to bitmask segments e.g. (23 => 23 bytes of data for segment X)
	//
	// 3. message contents: [TYPE] actual messages

	public enum MessageType
	{
		PLACEHOLDER = 0b_1, // placeholder
		ConnectionSetup = 0b_10, // [string] Player Name + [Custom Mesh] player mesh (+ SERVER ONLY assignment of playerID)
		ReadyCheck = 0b_100, // [bool] player ready
		//PlayerMovement = 0b_10_000 // [float2] vertical and horizontal input requests CLIENT: own pos only, SERVER: both positions
		PlayerPosition = 0b_10_000, // [float3] player pos CLIENT: own pos only, SERVER: both positions
		EntityPositions = 0b_100_000, // [float3] entity positions of projectiles and similar player-spawned objects
		ErrorCode = 0b_100_000 // [byte] error num TODO
	}

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