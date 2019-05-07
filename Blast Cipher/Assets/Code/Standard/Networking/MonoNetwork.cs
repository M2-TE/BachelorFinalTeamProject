using System.Net.Sockets;
using UnityEngine;

namespace Networking
{
	public abstract class MonoNetwork : MonoBehaviour
	{
		[SerializeField] protected int port = 20_000;
		protected UdpClient udpClient;
		protected TcpClient tcpClient;
		protected TcpListener tcpListener;
	}
}
