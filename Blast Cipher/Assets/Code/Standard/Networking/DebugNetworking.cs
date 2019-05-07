using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

class DebugNetworking : MonoBehaviour
{
	static readonly int serverPort = 20000;
	UdpClient server;

	private void Start()
	{
		// Start async receiving
		server = new UdpClient(serverPort);
		server.BeginReceive(DataReceived, server); 
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			// Send some test messages
			using (UdpClient sender1 = new UdpClient(19999))
				sender1.Send(Encoding.ASCII.GetBytes("Hi!"), 3, "localhost", serverPort);
		}
	}

	private void DataReceived(IAsyncResult ar)
	{
		IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
		byte[] receivedBytes = server.EndReceive(ar, ref receivedIpEndPoint);

		// Convert data to ASCII and print in console
		string receivedText = Encoding.ASCII.GetString(receivedBytes);
		Debug.Log(receivedIpEndPoint + ": " + receivedText + Environment.NewLine);

		// Restart listening for udp data packages
		server.BeginReceive(DataReceived, ar.AsyncState);
	}
}