﻿using UnityEngine;

using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Networking.Transport.Utilities;


public class ServerBehaviour : MonoBehaviour
{
	public UdpNetworkDriver m_Driver;
	private NetworkPipeline m_Pipeline;
	private NativeList<NetworkConnection> m_Connections;

	private void Start()
	{
		m_Driver = new UdpNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
		m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

		if (m_Driver.Bind(NetworkEndPoint.Parse("127.0.0.1", 9000)) != 0)
		{
			Debug.Log("Failed to bind to port 9000");
		}
		else
		{
			m_Driver.Listen();
		}

		m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
	}

	private void OnDestroy()
	{
		m_Driver.Dispose();
		m_Connections.Dispose();
	}

	void Update()
	{
		m_Driver.ScheduleUpdate().Complete();

		// CleanUpConnections
		for (int i = 0; i < m_Connections.Length; i++)
		{
			if (!m_Connections[i].IsCreated)
			{
				m_Connections.RemoveAtSwapBack(i);
				--i;
			}
		}

		// AcceptNewConnections
		NetworkConnection c;
		while ((c = m_Driver.Accept()) != default(NetworkConnection))
		{
			m_Connections.Add(c);
			Debug.Log("Accepted a connection");
		}

		DataStreamReader stream;
		for (int i = 0; i < m_Connections.Length; i++)
		{
			if (!m_Connections[i].IsCreated)
				continue;

			NetworkEvent.Type cmd;
			while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) !=
				   NetworkEvent.Type.Empty)
			{
				if (cmd == NetworkEvent.Type.Data)
				{
					var readerCtx = default(DataStreamReader.Context);
					uint number = stream.ReadUInt(ref readerCtx);

					Debug.Log("Got " + number + " from the Client adding + 2 to it.");
					number += 2;

					using (var writer = new DataStreamWriter(4, Allocator.Temp))
					{
						writer.Write(number);
						m_Driver.Send(m_Pipeline, m_Connections[i], writer);
					}
				}
				else if (cmd == NetworkEvent.Type.Disconnect)
				{
					Debug.Log("Client disconnected from server");
					m_Connections[i] = default(NetworkConnection);
				}
			}
		}
	}
}
