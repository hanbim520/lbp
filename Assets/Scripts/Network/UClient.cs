﻿using UnityEngine;
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
	private const int kMaxReceiveMsgSize = 4096;
	private const float reconnServerInterval = 1.0f;
	private byte[] recBuffer = new byte[kMaxReceiveMsgSize];
	private ConnectionState connState = ConnectionState.Disconnected;
    private ClientLogic clientLogic;

	void OnLevelWasLoaded(int level)
	{
		if (clientLogic == null &&
		    string.Compare(Application.loadedLevelName, Scenes.Main) == 0)
		{
			clientLogic = GameObject.Find("ClientLogic").GetComponent<ClientLogic>();
		}
	}

	void Start()
	{
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
		System.Array.Clear(recBuffer, 0, recBuffer.Length);
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
			ConnectServer(serverAddress, port);
		}
		connState = ConnectionState.Connected;
		yield return null;
	}

	private void HandleDataEvent(ref byte[] _recBuffer)
	{
        string msg = Utils.BytesToString(_recBuffer);
        Debug.Log("Client HandleDataEvent: " + msg);
		char[] delimiterChars = {':'};
		string[] words = msg.Split(delimiterChars);
		if (words.Length > 0)
		{
			int instr;
			if (!int.TryParse(words[0], out instr))
			{
				return;
			}
			if (instr == NetInstr.SyncData)
			{
				SyncData(ref words);
			}
			else if (instr == NetInstr.CheckAccount)
			{
				SendAccountToHost();
			}
			else if (instr == NetInstr.ClearAccount)
			{
				ClearAccount();
			}
			else if (instr == NetInstr.ClearCurrentWin)
			{
				ClearCurrentWin();
			}
			else if (instr == NetInstr.SyncInputDevice)
			{
				SyncInputDevice(ref words);
			}
			else
			{
				clientLogic.HandleRecData(instr, ref words);
			}
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
		try
		{
			byte[] buffer = Utils.StringToBytes(msg);
			byte error;
			NetworkTransport.Send(hostId, connectionId, reliableChannelId, buffer, buffer.Length, out error);
		}
		catch(System.Exception ex)
		{
			Debug.Log("SendToServer exception:" + ex.ToString());
		}
	}

	private void SyncData(ref string[] words)
	{
		int betTimeLimit, coinToScore, baoji;
		int max36Value, max18Value, max12Value, max9Value, max6Value, max3Value, max2Value;
		int betChipValue0, betChipValue1, betChipValue2, betChipValue3, betChipValue4, betChipValue5;
		int couponsStart, couponsKeyinRatio, couponsKeoutRatio;
		int maxNumberOfFields;
		int lineId, machineId;
		int lotteryCondition, lotteryBase, lotteryRate, lotteryAlloc, lotteryDigit;
		int inputDevice;
		int powerOffCompensate;
		
		if(int.TryParse(words[1], out betTimeLimit))
			GameData.GetInstance().betTimeLimit = betTimeLimit;
		if(int.TryParse(words[2], out coinToScore))
			GameData.GetInstance().coinToScore = coinToScore;
		if(int.TryParse(words[3], out baoji))
			GameData.GetInstance().baoji = baoji;
		
		if(int.TryParse(words[4], out betChipValue0))
			GameData.GetInstance().betChipValues[0] = betChipValue0;
		if(int.TryParse(words[5], out betChipValue1))
			GameData.GetInstance().betChipValues[1] = betChipValue1;
		if(int.TryParse(words[6], out betChipValue2))
			GameData.GetInstance().betChipValues[2] = betChipValue2;
		if(int.TryParse(words[7], out betChipValue3))
			GameData.GetInstance().betChipValues[3] = betChipValue3;
		if(int.TryParse(words[8], out betChipValue4))
			GameData.GetInstance().betChipValues[4] = betChipValue4;
		if(int.TryParse(words[9], out betChipValue5))
			GameData.GetInstance().betChipValues[5] = betChipValue5;
		
		if(int.TryParse(words[10], out max36Value))
			GameData.GetInstance().max36Value = max36Value;
		if(int.TryParse(words[11], out max18Value))
			GameData.GetInstance().max18Value = max18Value;
		if(int.TryParse(words[12], out max12Value))
			GameData.GetInstance().max12Value = max12Value;
		if(int.TryParse(words[13], out max9Value))
			GameData.GetInstance().max9Value = max9Value;
		if(int.TryParse(words[14], out max6Value))
			GameData.GetInstance().max6Value = max6Value;
		if(int.TryParse(words[15], out max3Value))
			GameData.GetInstance().max3Value = max3Value;
		if(int.TryParse(words[16], out max2Value))
			GameData.GetInstance().max2Value = max2Value;
		
		if(int.TryParse(words[17], out couponsStart))
			GameData.GetInstance().couponsStart = couponsStart;
		if(int.TryParse(words[18], out couponsKeyinRatio))
			GameData.GetInstance().couponsKeyinRatio = couponsKeyinRatio;
		if(int.TryParse(words[19], out couponsKeoutRatio))
			GameData.GetInstance().couponsKeoutRatio = couponsKeoutRatio;
		
		if(int.TryParse(words[20], out maxNumberOfFields))
			GameData.GetInstance().maxNumberOfFields = maxNumberOfFields;
		
		if(int.TryParse(words[21], out lineId))
			GameData.GetInstance().lineId = lineId;
		if(int.TryParse(words[22], out machineId))
			GameData.GetInstance().machineId = machineId;
		if(int.TryParse(words[23], out lotteryCondition))
			GameData.GetInstance().lotteryCondition = lotteryCondition;
		if(int.TryParse(words[24], out lotteryBase))
			GameData.GetInstance().lotteryBase= lotteryBase;
		if(int.TryParse(words[25], out lotteryRate))
			GameData.GetInstance().lotteryRate= lotteryRate;
		if(int.TryParse(words[26], out lotteryAlloc))
			GameData.GetInstance().lotteryAllocation= lotteryAlloc;
		if(int.TryParse(words[27], out inputDevice))
		{
			if (inputDevice != GameData.GetInstance().inputDevice)
			{
				GameData.GetInstance().inputDevice = inputDevice;
				GameData.GetInstance().SaveInputDevice();
				StartCoroutine(ChangeInputHanlde());
			}
		}
		if(int.TryParse(words[28], out lotteryDigit))
		{
			GameData.GetInstance().lotteryDigit = lotteryDigit;
			GameEventManager.OnLotteryChange(lotteryDigit);
		}
		if (int.TryParse(words[29], out powerOffCompensate))
			GameData.GetInstance().powerOffCompensate = powerOffCompensate;
		
		GameData.GetInstance().SaveSetting();
		GameEventManager.OnSyncUI();
	}

	private void SendAccountToHost()
	{
		string msg = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", 
		                           NetInstr.CheckAccount, GameData.GetInstance().deviceIndex, 
		                           GameData.GetInstance().zongShang, GameData.GetInstance().zongXia,
		                           GameData.GetInstance().zongTou, GameData.GetInstance().zongTui,
		                           GameData.GetInstance().currentWin, GameData.GetInstance().totalWin,
		                           GameData.GetInstance().cardCredits);
		SendToServer(msg);
	}

	private void ClearAccount()
	{
		GameData.GetInstance().ClearAccount();
		SendAccountToHost();
	}

	private void ClearCurrentWin()
	{
		GameData.GetInstance().currentWin = 0;
		GameData.GetInstance().currentZongShang = 0;
		GameData.GetInstance().currentZongXia = 0;
		GameData.GetInstance().SaveAccount();
		SendAccountToHost();
	}

	private void SyncInputDevice(ref string[] words)
	{
		int inputDevice;
		if(int.TryParse(words[1], out inputDevice))
		{
			if (inputDevice != GameData.GetInstance().inputDevice)
			{
				GameData.GetInstance().inputDevice = inputDevice;
				GameData.GetInstance().SaveInputDevice();
				StartCoroutine(ChangeInputHanlde());
			}
		}
	}

	private IEnumerator ChangeInputHanlde()
	{
		GameObject go = GameObject.Find("InputDevice");
		if (go != null)
		{
			SerialMousePort mouse = go.GetComponent<SerialMousePort>();
			TouchScreenPort touchScreen = go.GetComponent<TouchScreenPort>();
			if (GameData.GetInstance().inputDevice == 0)
			{
				if (mouse != null)
				{
					mouse.Close();
					Destroy(mouse);
				}
				yield return new WaitForSeconds(2.0f);
				if (touchScreen == null)
				{
					go.AddComponent<TouchScreenPort>();
				}
			}
			else
			{
				if (touchScreen != null)
				{
					touchScreen.Close();
					Destroy(touchScreen);
				}
				yield return new WaitForSeconds(2.0f);
				if (mouse == null)
				{
					go.AddComponent<SerialMousePort>();
				}
			}
		}
	}
}
