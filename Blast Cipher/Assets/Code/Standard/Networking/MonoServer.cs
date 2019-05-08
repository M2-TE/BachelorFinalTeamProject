using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Networking
{
	public sealed class MonoServer : MonoNetwork
	{

		private void Start()
		{
			SetupAsServer();
		}

		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			Debug.Log("Server Connection Established");
		}

		protected override void TcpMessageReceived(NetworkStream sender, byte[] message)
		{
			Debug.Log("Server TCP received.");
			SendTcpMessage(sender, message); // reply
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] message)
		{
			Debug.Log("Server UDP received.");
			SendUdpMessage(sender, message); // reply
		}
	}
}
