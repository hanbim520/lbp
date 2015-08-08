using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class UHost : MonoBehaviour
{
	public int port = 8888;
	public string hostAddress = "127.0.0.1";

	private int hostId;
	private int reiliableChannelId;
	private int unreliableChannelId;
	private int broadcastKey = 1000;
	private int broadcastVersion = 1;
	private int broadcastSubversion = 1;

	private byte[] msgOutBuffer = null;
	private byte[] msgInBuffer = null;
		
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
		case NetworkEventType.Nothing:         //1
			break;
		case NetworkEventType.ConnectEvent:    //2
			break;
		case NetworkEventType.DataEvent:       //3
			break;
		case NetworkEventType.DisconnectEvent: //4
			break;
		}
	}

	private void SetupServer()
	{
		ConnectionConfig config = new ConnectionConfig();
		reiliableChannelId  = config.AddChannel(QosType.Reliable);
		unreliableChannelId = config.AddChannel(QosType.Unreliable);
		
		HostTopology topology = new HostTopology(config, GameData.GetInstance().MaxNumOfPlayers);
		hostId = NetworkTransport.AddHost(topology, port);

		msgOutBuffer = Utils.StringToBytes();
		byte err;
		if (!NetworkTransport.StartBroadcastDiscovery(hostId, port, broadcastKey, broadcastVersion, broadcastSubversion, msgOutBuffer, msgOutBuffer.Length, 1000, out err))
		{
			Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + err);
			return false;
		}
	}

	private void HandleBroadcast()
	{
		string clientAddress;
		int port;
		byte error;
		NetworkTransport.GetBroadcastConnectionInfo(hostId, out clientAddress, out port, out error);
		if (!string.IsNullOrEmpty(clientAddress))
		{

		}
	}
}
