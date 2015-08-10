using UnityEngine;
using System.Collections;

public class StartInfo : MonoBehaviour
{
	void Awake()
	{
		if (Config.Host)
			GameData.GetInstance().NextLevelName = "Server";
		else
			GameData.GetInstance().NextLevelName = "Client";
		GameData.GetInstance().ReadDataFromDisk();
	}

}
