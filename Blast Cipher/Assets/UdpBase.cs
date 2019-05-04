using System.Net;

namespace Networking
{
	public struct Received
	{
		public IPEndPoint Sender;
		public byte[] MessageBytes;
	}

	public struct NetworkMessage
	{
		public IPEndPoint Sender;
		public byte[] MessageBytes;
	}
}