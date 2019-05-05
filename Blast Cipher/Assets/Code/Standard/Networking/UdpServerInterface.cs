using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Networking
{
	class UdpServerInterface
	{
		private UdpClient Client;
		private IPEndPoint _listenOn;

		public UdpServerInterface() : this(new IPEndPoint(IPAddress.Any, 32123)) { }

		public UdpServerInterface(IPEndPoint endpoint)
		{
			_listenOn = endpoint;
			Client = new UdpClient(_listenOn);
		}

		public void Reply(byte[] messageBytes, IPEndPoint endpoint)
		{
			Client.Send(messageBytes, messageBytes.Length, endpoint);
		}

		public async Task<NetworkMessage> Receive()
		{
			var result = await Client.ReceiveAsync();

			return new NetworkMessage()
			{
				MessageBytes = result.Buffer,
				Sender = result.RemoteEndPoint
			};
		}
	}
}
