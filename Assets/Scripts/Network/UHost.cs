using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class UHost : MonoBehaviour
{
	public int port = 8888;
	// Fixed hostAddress
	public string hostAddress = "127.0.0.1";

	private int hostId;
	private int reiliableChannelId;
	private int unreliableChannelId;
		
	void Start()
	{
		SetupServer();
	}
	
	void Update()
	{
	
	}

	private void SetupServer()
	{
		ConnectionConfig config = new ConnectionConfig();
		reiliableChannelId  = config.AddChannel(QosType.Reliable);
		unreliableChannelId = config.AddChannel(QosType.Unreliable);
		
		HostTopology topology = new HostTopology(config, GameData.GetInstance().MaxNumOfPlayers);
		hostId = NetworkTransport.AddHost(topology, port, hostAddress);
	}
}
