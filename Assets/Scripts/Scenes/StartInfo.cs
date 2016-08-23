using UnityEngine;
using System.Collections;

public class StartInfo : MonoBehaviour
{
	void Start()
	{
		GameData.GetInstance().ReadDataFromDisk();
		Screen.SetResolution(GameData.GetInstance().resolutionWidth, GameData.GetInstance().resolutionHeight, true);
		LoadUpdateUtils();
		LoadInputDevice();
		LoadHIDUtils();
		LoadNetwork();
//		TextDB.LoadFile();
		if (GameData.GetInstance().deviceIndex > 0)
		{
//			Application.LoadLevel(Scenes.Main);
			GameData.GetInstance().NextLevelName = Scenes.Main;
			Application.LoadLevel(Scenes.Loading);
		}
		else
		{
			Application.LoadLevel(Scenes.Backend);
		}
	}

	private void LoadUpdateUtils()
	{
		if (GameObject.Find("UpdateUtils") == null)
		{
			Object prefab = (Object)Resources.Load("Update/UpdateUtils");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "UpdateUtils";
			prefab = null;
		}
	}

	private void LoadInputDevice()
	{
		if (GameObject.Find("InputDevice") == null)
		{
			Object prefab = (Object)Resources.Load("Input/InputDevice");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "InputDevice";
			prefab = null;
		}
	}

	private void LoadHIDUtils()
	{
		if (GameObject.Find("HIDUtils") == null)
		{
			Object prefab = (Object)Resources.Load("Input/HIDUtils");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "HIDUtils";
			prefab = null;
		}
	}

	private void LoadNetwork()
	{
		if (GameObject.Find("NetworkObject") == null)
		{
			Object prefab = (Object)Resources.Load("Network/NetworkObject");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "NetworkObject";
			prefab = null;
		}
	}
}
