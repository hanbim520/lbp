using UnityEngine;
using System.Collections;

public class StartInfo : MonoBehaviour
{
	void Start()
	{
		GameData.GetInstance().ReadDataFromDisk();
		if (GameData.GetInstance().deviceIndex > 0)
		{
			Application.LoadLevel(Scenes.Main);
		}
		else
		{
			Application.LoadLevel(Scenes.Backend);
		}
	}
}
