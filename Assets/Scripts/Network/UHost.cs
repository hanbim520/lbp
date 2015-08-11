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
	private int reliableChannelId;
	private int unreliableChannelId;

	private int numOfConnecting = 0;
	private Dictionary<int, int> allConnections = new Dictionary<int, int>();
	private ServerLogic serverLogic;
	private Timer timerConnectClients;

	public Dictionary<int, int> AllConnections
	{
		get { return allConnections; }
	}

	void Start()
	{
		serverLogic = GetComponent<ServerLogic>();
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
		reliableChannelId = config.AddChannel(QosType.ReliableSequenced);
		unreliableChannelId = config.AddChannel(QosType.UnreliableSequenced);
		
		HostTopology topology = new HostTopology(config, GameData.GetInstance().MaxNumOfPlayers);
		hostId = NetworkTransport.AddHost(topology, port);

		StartConnectClients();
	}

	private void HandleConnectEvent(int connectionId, int channelId)
	{
		Debug.Log("Connect event. connectionId: " + connectionId + ", channelId: " + channelId);
		allConnections.Add(connectionId, channelId);
		serverLogic.clientBets.Add(connectionId, 0);
		++numOfConnecting;
		if (numOfConnecting == GameData.GetInstance().MaxNumOfPlayers)
		{
			StopBroadcast();
			GameEventManager.TriggerGameStart();
		}
	}

	private void HandleDisconnectEvent(int connectionId)
	{
		Debug.Log("Disconnect event. connectionId: " + connectionId);
		if (allConnections.ContainsKey(connectionId))
			allConnections.Remove(connectionId);
		if (serverLogic.clientBets.ContainsKey(connectionId))
			serverLogic.clientBets.Remove(connectionId);
		--numOfConnecting;
		if (numOfConnecting < GameData.GetInstance().MaxNumOfPlayers)
			StartBroadcast();
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
		int connectionId, channelId;
		foreach (var client in allConnections)
		{
			connectionId = client.Key;
			channelId = client.Value;
			byte error;
			NetworkTransport.Send(hostId, connectionId, channelId, buffer, buffer.Length, out error);
		}
	}

	public void StartBroadcast()
	{
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
			NetworkTransport.StopBroadcastDiscovery();
	}

}
