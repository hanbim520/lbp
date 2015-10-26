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
	private int reliableChannelId;
	private int unreliableChannelId;
	private const int kMaxBroadcastMsgSize = 1024;
	private const int kMaxReceiveMsgSize = 1024;
	private const float reconnServerInterval = 1.0f;
	private ConnectionState connState = ConnectionState.Disconnected;
    private ClientLogic clientLogic;

	void Start()
	{
        clientLogic = GetComponent<ClientLogic>();
		SetupClient();
	}

	void OnDestroy()
	{
        try
        {
		    NetworkTransport.RemoveHost(hostId);
        }
        catch(UnityException e)
        {
            Debug.Log(e.ToString());
        }
	}
	
	void Update()
	{		
		int connectionId; 
		int channelId; 
		byte[] recBuffer = new byte[kMaxReceiveMsgSize]; 
		int dataSize;
		byte error;
		NetworkEventType recData = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, recBuffer, recBuffer.Length, out dataSize, out error);
		switch (recData)
		{
		case NetworkEventType.Nothing:         
			break;
		case NetworkEventType.ConnectEvent:    
			break;
		case NetworkEventType.DataEvent:       
			if (dataSize > 0)
			{
				HandleDataEvent(ref recBuffer);
			}
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
		reliableChannelId = config.AddChannel(QosType.ReliableSequenced);
		unreliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
		
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
		if (connState != ConnectionState.Disconnected)
		{
			return;
		}

		connState = ConnectionState.Connecting;
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
			yield return new WaitForSeconds(reconnServerInterval);
			StartCoroutine(ConnectServer(serverAddress, port));
		}
		yield return null;
	}

	private void HandleDataEvent(ref byte[] recBuffer)
	{
        string msg = Utils.BytesToString(recBuffer);
        Debug.Log("Client HandleDataEvent: " + msg);
		char[] delimiterChars = {':'};
		string[] words = msg.Split(delimiterChars);
		if (words.Length > 0)
		{
            clientLogic.HandleRecData(ref words);
		}
	}

	private void HandleDisconnectEvent()
	{
		Debug.Log("HandleDisconnectEvent");
		connState = ConnectionState.Disconnected;
		GameEventManager.OnClientDisconnect();
	}

	public void SendToServer(string msg)
	{
		byte[] buffer = Utils.StringToBytes(msg);
		byte error;
		NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
	}
}
