using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class UHost : MonoBehaviour
{
	public int port = 8888;

	private int hostId;
	private int broadcastKey = 1000;
	private int broadcastVersion = 1;
	private int broadcastSubversion = 1;
	private const int kMaxBroadcastMsgSize = 1024;
	private const int kMaxReceiveMsgSize = 1024;
	private int reliableChannelId;
	private int unreliableChannelId;

	private int numOfConnecting = 0;
	private List<int> allConnections = new List<int>();
	private ServerLogic serverLogic;
	private Timer timerConnectClients;
	private bool isBroadcasting = false;

	public List<int> AllConnections
	{
		get { return allConnections; }
	}

	void Start()
	{
		serverLogic = GetComponent<ServerLogic>();
		SetupServer();
	}

	void OnDestroy()
	{
		StopBroadcast();
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
			HandleConnectEvent(connectionId);
			break;
		case NetworkEventType.DataEvent: 
            if (dataSize > 0)
            {
                HandleDataEvent(ref recBuffer, connectionId);
            }
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
		reliableChannelId = config.AddChannel(QosType.ReliableSequenced);
		unreliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
		
		HostTopology topology = new HostTopology(config, GameData.GetInstance().MaxNumOfPlayers);
		hostId = NetworkTransport.AddHost(topology, port);

		StartConnectClients();
	}

	private void HandleConnectEvent(int connectionId)
	{
		Debug.Log("Connect event. connectionId: " + connectionId);
		if (numOfConnecting >= GameData.GetInstance().MaxNumOfPlayers)
        {
			return;
        }

		allConnections.Add(connectionId);
		++numOfConnecting;
		if (numOfConnecting >= GameData.GetInstance().MaxNumOfPlayers)
		{
			StopBroadcast();
			GameEventManager.TriggerGameStart();
		}
		// Synchronize backend data
		SynData(connectionId);
	}

	private void SynData(int connectionId)
	{
		GameData gd = GameData.GetInstance();
		string msg = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}", 
		                           NetInstr.SynData, 
                                   gd.yanseOdds, 
                                   gd.shuangOdds, 
                                   gd.danOdds, 
                                   gd.daOdds, 
                                   gd.xiaoOdds, 
                                   gd.duOdds, 
                                   gd.betTimeLimit);
		SendToPeer(msg, connectionId);
	}

	private void HandleDisconnectEvent(int connectionId)
	{
		DebugConsole.Log("Disconnect event. connectionId: " + connectionId);
		if (allConnections.Contains(connectionId))
		{
			allConnections.Remove(connectionId);
		}
		--numOfConnecting;
		if (numOfConnecting < GameData.GetInstance().MaxNumOfPlayers)
		{
			StartBroadcast();
		}
	}


	private void StartConnectClients()
	{
		StartBroadcast();
		timerConnectClients = TimerManager.GetInstance().CreateTimer(GameData.GetInstance().ConnectClientsTime);
		timerConnectClients.Tick += StopConnectClients;
		timerConnectClients.Start();
	}

	private void StopConnectClients()
	{
		if (timerConnectClients.IsStarted())
			timerConnectClients.Stop();
		GameEventManager.TriggerGameStart();
	}

	public void SendToAll(string msg)
	{
		byte[] buffer = Utils.StringToBytes(msg);
		foreach (int connectionId in allConnections)
		{
			byte error;
			NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
		}
	}

	public void SendToPeer(string msg, int connectionId)
	{
		byte[] buffer = Utils.StringToBytes(msg);
		byte error;
		NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
	}

	public void StartBroadcast()
	{
		if (isBroadcasting)
		{
			return;
		}

		isBroadcasting = true;
		byte[] msgOutBuffer = Utils.StringToBytes("");
		byte err;
		if (!NetworkTransport.StartBroadcastDiscovery(hostId, port, broadcastKey, broadcastVersion, broadcastSubversion, msgOutBuffer, msgOutBuffer.Length, 1000, out err))
		{
			Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + err);
		}
	}

	public void StopBroadcast()
	{
		if (NetworkTransport.IsBroadcastDiscoveryRunning())
		{
			NetworkTransport.StopBroadcastDiscovery();
			isBroadcasting = false;
		}
	}

    private void HandleDataEvent(ref byte[] recBuffer, int connectionId)
    {
        string msg = Utils.BytesToString(recBuffer);
        Debug.Log("Server HandleDataEvent:" + msg);
        char[] delimiterChars = {':'};
        string[] words = msg.Split(delimiterChars);
        if (words.Length > 0)
        {
            serverLogic.HandleRecData(ref words, connectionId);
        }
    }

//	void OnGUI()
//	{
//		if (GUI.Button(new Rect(10, 50, 100, 50), "To test"))
//		{
//			Application.LoadLevel("test");
//		}
//	}
}
