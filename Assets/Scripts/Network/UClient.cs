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

	void Start()
	{
		SetupClient();
	}

	void OnDestroy()
	{
		NetworkTransport.RemoveHost(hostId);
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
		Debug.Log("From server: " + Utils.BytesToString(recBuffer));
		string msg = Utils.BytesToString(recBuffer);
		char[] delimiterChars = {':'};
		string[] words = msg.Split(delimiterChars);
		if (words.Length > 0)
		{
			int instr;
			if (!int.TryParse(words[0], out instr))
				return;

			if (instr == NetInstr.SynData && words.Length >= 8)
			{
				SynData(ref words);
			}
		}
	}

	private void SynData(ref string[] words)
	{
     	float yanseOdds;
		float shuangOdds;
		float danOdds;
		float daOdds;
		float xiaoOdds;
		float duOdds;
		int betTimeLimit;
		if(float.TryParse(words[1], out yanseOdds))
		   GameData.GetInstance().yanseOdds = yanseOdds;
		if(float.TryParse(words[2], out shuangOdds))
			GameData.GetInstance().shuangOdds = shuangOdds;
		if(float.TryParse(words[3], out danOdds))
			GameData.GetInstance().danOdds = danOdds;
		if(float.TryParse(words[4], out daOdds))
			GameData.GetInstance().daOdds = daOdds;
		if(float.TryParse(words[5], out xiaoOdds))
			GameData.GetInstance().xiaoOdds = xiaoOdds;
		if(float.TryParse(words[6], out duOdds))
			GameData.GetInstance().duOdds = duOdds;
		if(int.TryParse(words[7], out betTimeLimit))
			GameData.GetInstance().betTimeLimit = betTimeLimit;
		DebugConsole.Log("SynData:"+ yanseOdds + ", " + betTimeLimit);
	}

	private void HandleDisconnectEvent()
	{
		connState = ConnectionState.Disconnected;
		DebugConsole.Log("HandleDisconnectEvent");
	}

	public void SendToServer(string msg)
	{
		byte[] buffer = Utils.StringToBytes(msg);
		byte error;
		NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 50, 100, 50), "To test"))
		{
			Application.LoadLevel("test");
		}
	}
}
