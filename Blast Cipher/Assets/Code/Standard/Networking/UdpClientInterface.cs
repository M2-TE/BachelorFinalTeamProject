using System.Net.Sockets;
using System.Threading.Tasks;

namespace Networking
{
	class UdpClientInterface
	{
		private UdpClient Client;

		private UdpClientInterface()
		{
			Client = new UdpClient();
		}

		public static UdpClientInterface ConnectTo(string hostname, int port)
		{
			var connection = new UdpClientInterface();
			connection.Client.Connect(hostname, port);
			return connection;
		}

		public void Send(byte[] bytes)
		{
			Client.Send(bytes, bytes.Length);
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
