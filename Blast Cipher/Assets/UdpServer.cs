using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
	class UdpServer
	{
		private UdpClient Client;
		private IPEndPoint _listenOn;

		public UdpServer() : this(new IPEndPoint(IPAddress.Any, 32123)) { }

		public UdpServer(IPEndPoint endpoint)
		{
			_listenOn = endpoint;
			Client = new UdpClient(_listenOn);
		}

		public void Reply(byte[] messageBytes, IPEndPoint endpoint)
		{
			Client.Send(messageBytes, messageBytes.Length, endpoint);
		}

		public async Task<Received> Receive()
		{
			var result = await Client.ReceiveAsync();

			return new Received()
			{
				MessageBytes = result.Buffer,
				Sender = result.RemoteEndPoint
			};
		}
	}
}
