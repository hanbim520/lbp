using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class UHost : MonoBehaviour
{
	public int port = 8888;

	private int hostId;
	private int broadcastKey = 1000;
	private int broadcastVersion = 1;
	private int broadcastSubversion = 1;
	const int kMaxBroadcastMsgSize = 1024;

	private byte[] msgOutBuffer = null;
		
	void Start()
	{
		SetupServer();
	}
	
	void Update()
	{
		int connectionId; 
		int channelId; 
		byte[] recBuffer = new byte[1024]; 
		int bufferSize = 1024;
		int dataSize;
		byte error;
		NetworkEventType recData = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
		switch (recData)
		{
		case NetworkEventType.Nothing:         
			break;
		case NetworkEventType.ConnectEvent:    
			HandleConnectEvent(connectionId, channelId);
			break;
		case NetworkEventType.DataEvent:       
			break;
		case NetworkEventType.DisconnectEvent: 
			HandleDisconnectEvent(connectionId);
			break;
		}
	}

	private void SetupServer()
	{
		// global config
		GlobalConfig gconfig = new GlobalConfig();
		gconfig.ReactorModel = ReactorModel.FixRateReactor;
		gconfig.ThreadAwakeTimeout = 10;
		NetworkTransport.Init(gconfig);

		ConnectionConfig config = new ConnectionConfig();
		config.AddChannel(QosType.ReliableSequenced);
		config.AddChannel(QosType.UnreliableSequenced);
		
		HostTopology topology = new HostTopology(config, GameData.GetInstance().MaxNumOfPlayers);
		hostId = NetworkTransport.AddHost(topology, port);

		msgOutBuffer = Utils.StringToBytes("");
		byte err;
		if (!NetworkTransport.StartBroadcastDiscovery(hostId, port, broadcastKey, broadcastVersion, broadcastSubversion, msgOutBuffer, msgOutBuffer.Length, 1000, out err))
		{
			Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + err);
		}
	}

	private void HandleConnectEvent(int connectionId, int channelId)
	{
		byte error;
		byte[] buffer = Utils.StringToBytes("From chenxi server: Connect successfully.");
		NetworkTransport.Send(hostId, connectionId, channelId, buffer, buffer.Length, out error);
	}

	private void HandleDisconnectEvent(int connectionId)
	{
		Debug.Log("Disconnect event. connection Id: " + connectionId);
	}
}
