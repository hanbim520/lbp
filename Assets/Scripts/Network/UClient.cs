using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class UClient : MonoBehaviour
{
	public int port = 8888;
	private int hostId;
	private int connectionId;
	private int broadcastKey = 1000;
	private int broadcastVersion = 1;
	private int broadcastSubversion = 1;
	private const int kMaxBroadcastMsgSize = 1024;
	private ConnectionState state = ConnectionState.Disconnected;

	void Start()
	{
		SetupClient();
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
			break;
		case NetworkEventType.DataEvent:       
			HandleDataEvent(ref recBuffer);
			break;
		case NetworkEventType.DisconnectEvent: 
			HandleDisconnectEvent();
			break;
		case NetworkEventType.BroadcastEvent:
			HandleBroadcast();
			break;
		}

	}

	private void SetupClient()
	{
		// global config
		GlobalConfig gconfig = new GlobalConfig();
		gconfig.ReactorModel = ReactorModel.FixRateReactor;
		gconfig.ThreadAwakeTimeout = 1;
		
		// build ourselves a config with a couple of channels
		ConnectionConfig config = new ConnectionConfig();
		config.AddChannel(QosType.ReliableSequenced);
		config.AddChannel(QosType.UnreliableSequenced);
		
		// create a host topology from the config
		HostTopology hostconfig = new HostTopology(config, 1);
		
		// initialise the transport layer
		NetworkTransport.Init(gconfig);
		hostId = NetworkTransport.AddHost(hostconfig, port);
		byte error;
		NetworkTransport.SetBroadcastCredentials(hostId, broadcastKey, broadcastVersion, broadcastSubversion, out error);
	}

	private void HandleBroadcast()
	{
		if (state != ConnectionState.Disconnected)
			return;
		state = ConnectionState.Connecting;

		string serverAddress;
		int port;
		byte error;
		NetworkTransport.GetBroadcastConnectionInfo(hostId, out serverAddress, out port, out error);
		if (!string.IsNullOrEmpty(serverAddress))
		{
			StartCoroutine(ConnectServer(serverAddress, port));
		}
	}

	private IEnumerator ConnectServer(string serverAddress, int port)
	{
		byte connError;
		connectionId = NetworkTransport.Connect(hostId, serverAddress, port, 0, out connError);
		if (connectionId <= 0)
		{
			yield return new WaitForSeconds(1.0f);
			StartCoroutine(ConnectServer(serverAddress, port));
		}
		yield return null;
	}

	private void HandleDataEvent(ref byte[] recBuffer)
	{
		print ("From server: " + Utils.BytesToString(recBuffer));
	}

	private void HandleDisconnectEvent()
	{
		state = ConnectionState.Disconnected;
	}
}
