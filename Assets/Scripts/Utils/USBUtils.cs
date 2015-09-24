using UnityEngine;
using System;
using System.Collections;

public class USBUtils : MonoBehaviour
{
	private AndroidJavaClass jc;
	private AndroidJavaObject jo;
	// In seconds
	private const float getDataTime = 0.1f;
	private float getDataTimeDelta = 0;
	void Start()
	{
		print(getDataTime);
		InitData();
	}
	
	void Update()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			getDataTimeDelta += Time.deltaTime;
			if (getDataTimeDelta >= getDataTime)
			{
				getDataTimeDelta = 0;
				GetData();
				ReadUsbPort();
			}
		}
	}

	private void InitData()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
			jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		}
	}

	public void OpenUSB()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			DebugConsole.Log("cs openUsb");
			jo.Call("openUsb");
		}
	}

	public void CloseUSB()
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

	public int[] ReadData(string methodName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>(methodName);
			return AndroidJNIHelper.ConvertFromJNIArray<int[]>(rev.GetRawObject());
		}
		return null;
	}

	public void ReadUsbPort()
	{
//		int[] data = ReadData("readUsb0");
//		if (data != null && data.Length > 2)
//		{
//			if (data[0] == -1 || data[0] != 0x58 || data[1] != 0x57)
//			{ 
//				return;
//			}
//			log = "data.Length:" + data.Length + "--";
//			for (int i = 0; i < data.Length; ++i)
//			{
//				log += string.Format("{0:X}", data[i]) + ", ";
//			}
//			DebugConsole.Log(log);
//		}
		jo.Call("test");
	}

	public void GetData()
	{
		int[] data = new int[]{0x58, 0x57, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0};
		WriteData(data, "writeUsbPort");
	}

	public int WriteData(int[] data, string methodName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
			jvalue[] blah = new jvalue[1];
			blah[0].l = pArr;
			
			IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), methodName);
			return AndroidJNI.CallIntMethod(jo.GetRawObject(), methodId, blah);
		}
		return -1;
	}

	public void OpenGate()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			int[] data = new int[]{0x58, 0x57, 0x02, 0x09, 0xC4, 0, 0, 0, 
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0};
			WriteData(data, "writeUsbPort");
		}
	}

	public void BlowBall(int blowTime)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			int hight = blowTime >> 8 & 0xff;
			int low = blowTime & 0xff;

			int[] data = new int[]{0x58, 0x57, 0x03, hight, low, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0};
			WriteData(data, "writeUsbPort");
		}
	}

	private string log = "";
	private bool bopen = false;
	void OnGUI()
	{
		if (GUI.Button(new Rect(200, 10 , 200, 150), "open usb"))
		{
			if (!bopen)
			{
				bopen = true;
				OpenUSB();
			}
		}
		if (GUI.Button(new Rect(200, 200 , 200, 150), "close usb"))
		{
			CloseUSB();
		}
		if (GUI.Button(new Rect(420, 10 , 200, 150), "open gate"))
		{
			OpenGate();
		}
		if (GUI.Button(new Rect(420, 200 , 200, 150), "blow ball"))
		{
			BlowBall(1500);
		}
		if (GUI.Button(new Rect(200, 400, 200, 150), "Quit"))
		{
			Application.Quit();
		}
	}
}
