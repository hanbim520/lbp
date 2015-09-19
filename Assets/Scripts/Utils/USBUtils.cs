using UnityEngine;
using System.Collections;

public class USBUtils : MonoBehaviour
{
	private AndroidJavaClass jc;
	private AndroidJavaObject jo;
	void Start()
	{
		InitData();
	}
	
	void Update()
	{
	
	}

	private void InitData()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
			jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		}
	}

	private void OpenUSB()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			DebugConsole.Log("cs openUsb");
			jo.Call("openUsb");
		}
	}

	private void CloseUSB()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			DebugConsole.Log("cs closeUsb");
			jo.Call("closeUsb");
		}
	}

	public void DebugLog(string msg)
	{
		DebugConsole.Log(msg);
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(200, 10 , 200, 150), "open usb"))
		{
			OpenUSB();
		}
		if (GUI.Button(new Rect(200, 200 , 200, 150), "close usb"))
		{
			CloseUSB();
		}
		if (GUI.Button(new Rect(420, 10 , 200, 150), "open gate"))
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				DebugConsole.Log("cs open gate");
				jo.Call("openGate");
			}
		}
		if (GUI.Button(new Rect(200, 400, 200, 150), "Quit"))
		{
			Application.Quit();
		}
	}
}
