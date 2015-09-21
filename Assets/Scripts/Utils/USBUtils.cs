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
	
	void FixedUpdate()
	{
		ReadUsbPort();
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

	private int[] ReadData(string methodName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>(methodName);
			return AndroidJNIHelper.ConvertFromJNIArray<int[]>(rev.GetRawObject());
		}
		return null;
	}

	private void ReadUsbPort()
	{
		int[] data = ReadData("readUsb0");
		if (data.Length > 0)
		{
			if (data[0] == -1)
			{
				return;
			}
			log = "";
			for (int i = 0; i < data.Length; ++i)
			{

				log += string.Format("{0:X}", data[i]) + ", ";
			}
			DebugConsole.Log(log);
		}
	}

	private string log = "";

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
		if (GUI.Button(new Rect(420, 200 , 200, 150), "blow ball"))
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				DebugConsole.Log("cs open gate");
				jo.Call("blowBall");
			}
		}
		if (GUI.Button(new Rect(200, 400, 200, 150), "Quit"))
		{
			Application.Quit();
		}
	}
}
