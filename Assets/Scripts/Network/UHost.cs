using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UHost : MonoBehaviour
{
	public int port = 8888;

	private int hostId;
	private int broadcastKey = 1000;
	private int broadcastVersion = 1;
	private int broadcastSubversion = 1;
	private const int kMaxBroadcastMsgSize = 1024;
	private const int kMaxReceiveMsgSize = 4096;
	private int reliableChannelId;
	private int unreliableChannelId;
	private byte[] recBuffer = new byte[kMaxReceiveMsgSize];

	private int maxConnectedCount;
	private int numOfConnecting = 0;
	private List<int> allConnections = new List<int>();
	private ServerLogic serverLogic;
	private BackendLogic backLogic;
	private bool isBroadcasting = false;

	float heartbeatElapsed;
	const float kHeartbeatInterver = 3.0f;

	public List<int> AllConnections
	{
		get { return allConnections; }
	}

	void OnSceneLoaded(Scene scence, LoadSceneMode mod)
	{
		if (serverLogic == null && 
			string.Compare(scence.name, Scenes.Main) == 0)
		{
			serverLogic = GameObject.Find("ServerLogic").GetComponent<ServerLogic>();
		}
		else if (backLogic == null &&
			string.Compare(scence.name, Scenes.Backend) == 0)
		{
			backLogic = GameObject.Find("BackendLogic").GetComponent<BackendLogic>();
		}
	}

	void Start()
	{
		maxConnectedCount = GameData.GetInstance().MaxNumOfPlayers + 1;
		SceneManager.sceneLoaded += OnSceneLoaded;
		GameEventManager.SyncData += SyncDataToClients;
		GameEventManager.SyncInputDevice += SyncInputDevice;
		SetupServer();
	}

	void OnDestroy()
	{
        try
        {
			SceneManager.sceneLoaded -= OnSceneLoaded;
			GameEventManager.SyncData -= SyncDataToClients;
			GameEventManager.SyncInputDevice -= SyncInputDevice;
            StopBroadcast();
            NetworkTransport.RemoveHost(hostId);
        }
        catch(UnityException e)
        {
            Debug.Log(e.ToString());
        }
	}
		
	void Keepalive()
	{
		heartbeatElapsed += Time.deltaTime;
		if (heartbeatElapsed > kHeartbeatInterver)
		{
			heartbeatElapsed = 0;
			SendToAll("hb");
		}
	}

	void Update()
	{
		try
		{
//			Keepalive();

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
		catch(System.Exception ex)
		{
			Debug.Log("UHost Update exception:" + ex.ToString());
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
//		config.PacketSize = 2000;
		config.DisconnectTimeout = 5000;

		HostTopology topology = new HostTopology(config, maxConnectedCount);
		hostId = NetworkTransport.AddHost(topology, port);

		StartBroadcast();
	}

	private void HandleConnectEvent(int connectionId)
	{
		Debug.Log("Connect event. connectionId: " + connectionId);
		if (numOfConnecting >= maxConnectedCount)
        {
			return;
        }

		allConnections.Add(connectionId);
		++numOfConnecting;
		if (numOfConnecting >= maxConnectedCount)
		{
//			StopBroadcast();
			GameEventManager.TriggerGameStart();
		}
		// Synchronize backend data
		SyncData(connectionId);
	}

	public void SyncData(int connectionId)
	{
		string epiredticks = GameData.controlCode ? PlayerPrefs.GetString("ExpiredDate", "0") : "0";
		GameData gd = GameData.GetInstance();
        string msg = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:" +
		                           "{11}:{12}:{13}:{14}:{15}:{16}:{17}:{18}:{19}:{20}:{21}:{22}:{23}:{24}:{25}:{26}:{27}:{28}:{29}:{30}:" +
								   "{31}:{32}:{33}:{34}:{35}:{36}:{37}:{38}:{39}:{40}", 
		                           NetInstr.SyncData, 
                                   gd.betTimeLimit, gd.coinToScore, gd.baoji,
                                   gd.betChipValues[0], gd.betChipValues[1], gd.betChipValues[2],
                                   gd.betChipValues[3], gd.betChipValues[4], gd.betChipValues[5],
                                   gd.max36Value, gd.max18Value, gd.max12Value, gd.max9Value, 
                                   gd.max6Value, gd.max3Value, gd.max2Value, 
                                   gd.couponsStart, gd.couponsKeyinRatio, gd.couponsKeoutRatio, 
                                   gd.maxNumberOfFields,
                                   gd.lineId, gd.machineId,
		                           gd.lotteryCondition, gd.lotteryBase, gd.lotteryRate, gd.lotteryAllocation, gd.inputDevice, 
                                   gd.lotteryDigit,
		                           gd.powerOffCompensate, gd.topScreenLanguage,
		                           gd.allMax36Val, gd.allMax18Val, gd.allMax12Val, gd.allMax9Val, 
		                           gd.allMax6Val, gd.allMax3Val, gd.allMax2Val,
								   gd.lotteryLv, gd.billAcceptorType, epiredticks);
		SendToPeer(msg, connectionId);
	}

	private void HandleDisconnectEvent(int connectionId)
	{
		Debug.Log("Disconnect event. connectionId: " + connectionId);
		if (allConnections.Contains(connectionId))
		{
			allConnections.Remove(connectionId);
		}
		--numOfConnecting;
		if (numOfConnecting < GameData.GetInstance().MaxNumOfPlayers)
		{
//			StartBroadcast();
		}
	}

	public void SendToAll(string msg)
	{
		try
		{
			byte[] buffer = Utils.StringToBytes(msg);
			foreach (int connectionId in allConnections)
			{
				byte error;
				NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
			}
		}
		catch(System.Exception ex)
		{
			Debug.Log("UHost SendToAll exception:" + ex.ToString());
		}
	}

	public void SendToPeer(string msg, int connectionId)
	{
		try
		{
			byte[] buffer = Utils.StringToBytes(msg);
			byte error;
			NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
		}
		catch(System.Exception ex)
		{
			Debug.Log("UHost SendToPeer exception:" + ex.ToString());
		}
	}

	public void StartBroadcast()
	{
		try
		{
			if (isBroadcasting)
			{
				return;
			}
			isBroadcasting = true;
			byte[] msgOutBuffer = Utils.StringToBytes("1");
			byte err;
			if (!NetworkTransport.StartBroadcastDiscovery(hostId, port, broadcastKey, broadcastVersion, broadcastSubversion, msgOutBuffer, msgOutBuffer.Length, 1000, out err))
			{
				Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + err);
			}
			else
			{
				Debug.Log("NetworkDiscovery StartBroadcast");
			}
		}
		catch(System.Exception ex)
		{
			print(ex.ToString());
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

    private void HandleDataEvent(ref byte[] _recBuffer, int connectionId)
    {
		string msg = Utils.BytesToString(_recBuffer);
//        Debug.Log("Server HandleDataEvent:" + msg);
        char[] delimiterChars = {':'};
        string[] words = msg.Split(delimiterChars);
        if (words.Length > 0)
        {
			if (serverLogic != null)
            	serverLogic.HandleRecData(ref words, connectionId);
			if (backLogic != null)
				backLogic.HandleRecData(ref words, connectionId);
        }
    }

	// 主动跟分机同步设置
	private void SyncDataToClients()
	{
        foreach (int connectionId in allConnections)
        {
            SyncData(connectionId);
        }
	}

	private void SyncInputDevice()
	{
		string msg = string.Format("{0}:{1}", NetInstr.SyncInputDevice, GameData.GetInstance().inputDevice);
		foreach (int connectionId in allConnections)
		{
			SendToPeer(msg, connectionId);
		}
	}
}
