using Networking;
using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
	private UdpClientInterface client;
	private UdpServer server;

	private readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
	private readonly System.Diagnostics.Stopwatch permanentStopwatch = new System.Diagnostics.Stopwatch();

	float currentTime = 0f;

	private void Awake()
	{
		StartCoroutine(TimeUpdater());

		permanentStopwatch.Start();
	}

	private void Start()
	{
		server = new UdpServer();

		//start listening for messages and copy the messages back to the client
		Task.Factory.StartNew(async () => {
			while (true)
			{
				try
				{
					var received = await server.Receive();
					for (int i = 0; i < 500000; i++) { }
					server.Reply(received.MessageBytes, received.Sender);
				}
				catch(Exception e)
				{
					Debug.LogException(e);
				}
			}
		});

		client = UdpClientInterface.ConnectTo("127.0.0.1", 32123);

		//wait for reply messages from server and send them to console
		Task.Factory.StartNew(async () => {
			while (true)
			{
				try
				{
					var received = await client.Receive();
					
					Debug.Log(/*(float)permanentStopwatch.ElapsedMilliseconds - */BitConverter.ToSingle(received.MessageBytes, 0));

					stopwatch.Stop();
					Debug.Log(stopwatch.ElapsedMilliseconds);
					stopwatch.Reset();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		});
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			stopwatch.Start();
			client.Send(BitConverter.GetBytes((float)permanentStopwatch.ElapsedMilliseconds));
		}
	}

	private IEnumerator TimeUpdater()
	{
		while (true)
		{
			currentTime = Time.time;
			yield return null;
		}
	}
}
