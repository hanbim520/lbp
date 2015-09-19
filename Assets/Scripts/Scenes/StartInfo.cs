using UnityEngine;
using System.Collections;

public class StartInfo : MonoBehaviour
{
	void Awake()
	{
		GameData.GetInstance().NextLevelName = "Main";
		GameData.GetInstance().ReadDataFromDisk();
	}

}
