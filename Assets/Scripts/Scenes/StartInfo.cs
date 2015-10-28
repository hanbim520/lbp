using UnityEngine;
using System.Collections;

public class StartInfo : MonoBehaviour
{
	void Start()
	{
		GameData.GetInstance().ReadDataFromDisk();
		LoadUpdateUtils();
		LoadInputDevice();
		LoadHIDUtils();
		if (GameData.GetInstance().deviceIndex > 0)
		{
			Application.LoadLevel(Scenes.Main);
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
}
