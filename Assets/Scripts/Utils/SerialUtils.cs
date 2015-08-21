using UnityEngine;
using System;
using System.Collections;
using System.Threading;

public class SerialUtils : MonoBehaviour
{
	public int baudrate = 9600;

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

	void OnDestroy()
	{
		if (timerRead != null && timerRead.IsStarted())
		{
			timerRead.Stop();
		}
		UnregisterEvent();
	}

	void Test()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			OpenSerial();
		}
	}

	// Note: java side should be return boolean
	private bool WriteData(int[] data, string methodName)
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

	// Note: Java side should be return int[]
	private int[] ReadData(string methodName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>(methodName);
			return AndroidJNIHelper.ConvertFromJNIArray<int[]>(rev.GetRawObject());
		}
		return null;
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
			DebugConsole.Log("cs OpenSerial");
			jo.Call("openSerialPort", baudrate);
			timerRead = TimerManager.GetInstance().CreateTimer(0.1f, TimerType.Loop);
			timerRead.Tick += ReadSerialPort;
			timerRead.Start();

//			timerRead = TimerManager.GetInstance().CreateTimer(1.0f, TimerType.Loop);
//			timerRead.Tick += WriteSerialPort;
//			timerRead.Start();
		}
	}

    private void CloseSerial()
    {
		if (Application.platform == RuntimePlatform.Android)
		{
			jo.Call("closeSerialPort");
		}
	}

	private void WriteSerialPort()
	{
		WriteData(new int[]{0x55, 0x55}, "writeSerialPort3");
	}

	private void ReadSerialPort()
	{
		int[] data = ReadData("readSerialPort3");
		if (data.Length > 0)
		{
			if (data[0] == -1)
			{
				DebugConsole.Log("ReadSerialPort get -1.");
				return;
			}
			log = "";
			for (int i = 0; i < data.Length; ++i)
			{
//				DebugConsole.Log(data[i].ToString());
				log += data[i].ToString() + ", ";
			}
			++receiveCount;
			DebugConsole.Log(log + receiveCount);
		}
	}

	string log = "";
	int receiveCount = 0;

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
