using System.Net;
using UnityEngine;

using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine.Assertions;
using Unity.Networking.Transport.Utilities;

public class ClientBehaviour : MonoBehaviour
{
    public UdpNetworkDriver m_Driver;
	public NetworkConnection m_Connection;
	public bool m_Done;

	private NetworkPipeline m_Pipeline;

	void Start()
	{
		m_Driver = new UdpNetworkDriver(new ReliableUtility.Parameters { WindowSize = 32 });
		m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
		m_Connection = default(NetworkConnection);
		
		m_Connection = m_Driver.Connect(NetworkEndPoint.Parse("127.0.0.1", 9000));
	}
	
	public void OnDestroy()
	{
		m_Driver.Dispose();
	}
	
	void Update()
	{
		m_Driver.ScheduleUpdate().Complete();
	
		if (!m_Connection.IsCreated)
		{
			if (!m_Done)
				Debug.Log("Something went wrong during connect");
			return;
		}
	
		DataStreamReader stream;
		NetworkEvent.Type cmd;
	
		while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) !=
			   NetworkEvent.Type.Empty)
		{
			if (cmd == NetworkEvent.Type.Connect)
			{
				Debug.Log("We are now connected to the server");
	
				var value = 1;
				using (var writer = new DataStreamWriter(4, Allocator.Temp))
				{
					writer.Write(value);
					m_Connection.Send(m_Driver, writer);
				}
			}
			else if (cmd == NetworkEvent.Type.Data)
			{
				var readerCtx = default(DataStreamReader.Context);
				uint value = stream.ReadUInt(ref readerCtx);
				Debug.Log("Got the value = " + value + " back from the server");
				m_Done = true;
				m_Connection.Disconnect(m_Driver);
				m_Connection = default(NetworkConnection);
			}
			else if (cmd == NetworkEvent.Type.Disconnect)
			{
				Debug.Log("Client got disconnected from server");
				m_Connection = default(NetworkConnection);
			}
		}
	}
}
