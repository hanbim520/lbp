using UnityEngine;
using System;
using System.Collections;
using System.Threading;

public class SerialUtils : MonoBehaviour
{
	public int baudrate = 115200;

	private AndroidJavaClass jc;
	private AndroidJavaObject jo;
	private Timer timerRead;
	private Timer timerSend;

	void Start()
	{
		DontDestroyOnLoad(this);
		InitData();
        RegisterEvent();
		Test();
	}

	void Test()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			OpenSerial();
		}
	}

	// Note: java side should be return boolean
	private bool WriteData(byte[] data, string methodName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
			jvalue[] blah = new jvalue[1];
			blah[0].l = pArr;
			
			IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), methodName);
			return AndroidJNI.CallBooleanMethod(jo.GetRawObject(), methodId, blah);
		}
		return false;
	}

	// Note: Java side should be return byte[]
	private byte[] ReadData(string methodName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>(methodName);
			return AndroidJNIHelper.ConvertFromJNIArray<byte[]>(rev.GetRawObject());
		}
		return null;
	}

    void OnDestroy()
    {
        UnregisterEvent();
    }

	private void InitData()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
			jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		}
	}
	
    private void RegisterEvent()
    {
		if (Application.platform == RuntimePlatform.Android)
		{
			GameEventManager.OpenSerial += OpenSerial;
			GameEventManager.CloseSerial += CloseSerial;
		}
    }

    private void UnregisterEvent()
    {
		if (Application.platform == RuntimePlatform.Android)
		{
			GameEventManager.OpenSerial -= OpenSerial;
			GameEventManager.CloseSerial -= CloseSerial;
		}
    }

    private void OpenSerial()
    {
		if (Application.platform == RuntimePlatform.Android)
		{
			jo.Call("openSerialPort", baudrate);
//			timerRead = TimerManager.GetInstance().CreateTimer(100, TimerType.Loop);
//			timerRead.Tick += ;
		}
	}

    private void CloseSerial()
    {
		if (Application.platform == RuntimePlatform.Android)
		{
			jo.Call("closeSerialPort");
		}
	}

	private void ReadSerialPort()
	{
//		byte[] data = ReadData();
//		if (data)
	}
	
	void Update()
	{
	
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 50, 200, 150), "Open"))
		{
			OpenSerial();
		}

		if (GUI.Button(new Rect(10, 250, 200, 150), "Close"))
		{
			CloseSerial();
		}
	}
}
