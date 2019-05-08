using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Networking
{
	public sealed class MonoClient : MonoNetwork
	{
		[SerializeField] private string targetIP = "127.0.0.1";
		NetworkStream stream;

		private void Start()
		{
			SetupAsClient(IPAddress.Parse(targetIP));
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				byte[] bytes = Encoding.ASCII.GetBytes("urmomgay lol");
				
				// send message over tcp
				if (stream != null) SendTcpMessage(stream, bytes);

				// send message over udp
				SendUdpMessage(bytes);
			}
		}

		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			this.stream = stream;
			Debug.Log("Client Connection Established");
		}

		protected override void TcpMessageReceived(byte[] message)
		{
			Debug.Log("Client TCP received.");
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] message)
		{
			Debug.Log("Client UDP received.");
		}
	}
}
