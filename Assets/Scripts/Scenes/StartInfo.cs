using UnityEngine;
using System.Collections;

public class StartInfo : MonoBehaviour
{
	void Awake()
	{
		Debug.Log("StartInfo1");
		GameData.GetInstance().NextLevelName = "Main";
		Debug.Log("StartInfo2");
		GameData.GetInstance().ReadDataFromDisk();
		Debug.Log("StartInfo3");
	}

}
