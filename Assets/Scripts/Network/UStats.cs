using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

// 顶部路单的网路类
public class UStats : MonoBehaviour
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
	private const int kMaxReceiveMsgSize = 4096;
	private const float reconnServerInterval = 1.0f;
	private byte[] recBuffer = new byte[kMaxReceiveMsgSize];
	private ConnectionState connState = ConnectionState.Disconnected;
	private TopStatistics gameLogic;

	float heartbeatElapsed;
	const float kHeartbeatInterver = 3.0f;

	void Start()
	{
		gameLogic = GetComponent<TopStatistics>();
		SetupClient();
	}

	void Keepalive()
	{
		heartbeatElapsed += Time.deltaTime;
		if (heartbeatElapsed > kHeartbeatInterver)
		{
			heartbeatElapsed = 0;
			SendToServer("hb");
		}
	}
	
	void Update()
	{
		Keepalive();

		int connectionId; 
		int channelId; 
		System.Array.Clear(recBuffer, 0, recBuffer.Length);
		int dataSize;
		byte error;
		NetworkEventType recData = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, recBuffer, recBuffer.Length, out dataSize, out error);
		switch (recData)
		{
			case NetworkEventType.Nothing:         
				break;
			case NetworkEventType.ConnectEvent:    
				print("uclient connected!");
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
		gconfig.ThreadAwakeTimeout = 10;
		NetworkTransport.Init(gconfig);

		// build ourselves a config with a couple of channels
		ConnectionConfig config = new ConnectionConfig();
		reliableChannelId = config.AddChannel(QosType.ReliableFragmented);
		unreliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
		config.PacketSize = 2000;
		config.DisconnectTimeout = 5000;
		
		// create a host topology from the config
		HostTopology hostconfig = new HostTopology(config, 1);
		hostId = NetworkTransport.AddHost(hostconfig, port);
		byte error;
		NetworkTransport.SetBroadcastCredentials(hostId, broadcastKey, broadcastVersion, broadcastSubversion, out error);
		print("Broadcast err:" + error);
	}

	private void HandleDataEvent(ref byte[] _recBuffer)
	{
		string msg = Utils.BytesToString(_recBuffer);
		Debug.Log("UStats HandleDataEvent: " + msg);
		char[] delimiterChars = {':'};
		string[] words = msg.Split(delimiterChars);
		if (words.Length > 0)
		{
			int instr;
			if (!int.TryParse(words[0], out instr))
			{
				return;
			}
			gameLogic.HandleRecData(instr, ref words);
		}
	}

	private void HandleDisconnectEvent()
	{
		print("ustats HandleDisconnectEvent");
		connectionId = 0;
		connState = ConnectionState.Disconnected;
		GameEventManager.OnClientDisconnect();
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
		print("ustats ConnectServer: " + serverAddress + ", port: " + port);
		byte connError;
		connectionId = NetworkTransport.Connect(hostId, serverAddress, port, 0, out connError);
		print("ustats connError:" + connError);
		if (connectionId <= 0)
		{
			yield return new WaitForSeconds(reconnServerInterval);
			ConnectServer(serverAddress, port);
		}
		connState = ConnectionState.Connected;
		yield return null;
	}

	public void SendToServer(string msg)
	{
		try
		{
			if (connState == ConnectionState.Disconnected ||
				connectionId  == 0)
				return;
			
			byte[] buffer = Utils.StringToBytes(msg);
			byte error;
			print("connectionId:" + connectionId);
			NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
		}
		catch(System.Exception ex)
		{
			Debug.Log("SendToServer exception:" + ex.ToString());
		}
	}
}
