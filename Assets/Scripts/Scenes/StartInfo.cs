using UnityEngine;
using System.Collections;

public class StartInfo : MonoBehaviour
{
	void Start()
	{
		GameData.GetInstance().ReadDataFromDisk();
		LoadUpdateUtils();
		if (GameData.GetInstance().deviceIndex > 0 && 
		    GameData.GetInstance().deviceIndex < GameData.GetInstance().monitorDeviceIndex)
		{
			Screen.SetResolution(GameData.GetInstance().resolutionWidth, GameData.GetInstance().resolutionHeight, true);
//			LoadInputDevice();
			LoadHIDUtils();
			LoadNetwork();
//			TextDB.LoadFile();

			GameData.GetInstance().NextLevelName = Scenes.Main;
			Application.LoadLevel(Scenes.Loading);
		}
		else if (GameData.GetInstance().deviceIndex >= GameData.GetInstance().monitorDeviceIndex)
		{
			GameData.GetInstance().NextLevelName = Scenes.TopStatistics;
			Application.LoadLevel(Scenes.Loading);
		}
		else
		{
			Screen.SetResolution(GameData.GetInstance().resolutionWidth, GameData.GetInstance().resolutionHeight, true);
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
#if UNITY_STANDALONE_LINUX
		if (GameObject.Find("HIDUtils") == null)
		{
			Object prefab = (Object)Resources.Load("Input/HIDUtils");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "HIDUtils";
			prefab = null;
		}
#endif
#if UNITY_ANDROID
		if (GameObject.Find("AndroidHIDUtils") == null)
		{
			Object prefab = (Object)Resources.Load("Input/AndroidHIDUtils");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "AndroidHIDUtils";
			prefab = null;
		}
#endif
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
