using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class UClient : MonoBehaviour
{
	public int port = 8888;
	private int hostId;
	private int channelReliable;
	private int channelUnreliable;

	void Start()
	{
	
	}
	
	void Update()
	{
	
	}

	private void SetupClient()
	{
		// global config
		GlobalConfig gconfig = new GlobalConfig();
		gconfig.ReactorModel = ReactorModel.FixRateReactor;
		gconfig.ThreadAwakeTimeout = 1;
		
		// build ourselves a config with a couple of channels
		ConnectionConfig config = new ConnectionConfig();
		channelReliable = config.AddChannel(QosType.ReliableSequenced);
		channelUnreliable = config.AddChannel(QosType.UnreliableSequenced);
		
		// create a host topology from the config
		HostTopology hostconfig = new HostTopology(config, 1);
		
		// initialise the transport layer
		NetworkTransport.Init(gconfig);
		hostId = NetworkTransport.AddHost(hostconfig, port);
	}
}
