using UnityEngine;
using System;
using System.Collections;
using System.Threading;

public class SerialUtils : MonoBehaviour
{
	private AndroidJavaClass jc;
	private AndroidJavaObject jo;

	private Thread readThread;

	void Start()
	{
//		readThread = new Thread();
//		readThread.Start();

		InitData();
        RegisterEvent();
		Test();
	}

	void Test()
	{


		//writeSerialPort0
		byte[] data = new byte[]{3, 4, 5};

		IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
		jvalue[] blah = new jvalue[1];
		blah[0].l = pArr;
		
		IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "writeSerialPort0");
		bool rev = AndroidJNI.CallBooleanMethod(jo.GetRawObject(), methodId, blah);

		if (rev)
		{
			print("write ok");
		}
		else
		{
			print("can't write");
		}

		AndroidJavaObject methord = jo.Call<AndroidJavaObject>("getBytes");
		if (methord == null)
		{
			print ("read methord is null");
		}
		else if (methord.GetRawObject() == IntPtr.Zero)
		{
			print ("read GetRawObject is null");
		}
		else
		{
			byte[] b = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(methord.GetRawObject());
			print("read data:");
			for (int i = 0; i < b.Length; ++i)
			{
				print(b[i]);
			}
		}
	}

	// Note: java side should be return boolean
	private bool WriteData(byte[] data, string methodName)
	{
		IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
		jvalue[] blah = new jvalue[1];
		blah[0].l = pArr;
		
		IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), methodName);
		return AndroidJNI.CallBooleanMethod(jo.GetRawObject(), methodId, blah);
	}

	// Note: Java side should be return byte[]
	private byte[] ReadData(string methodName)
	{
		AndroidJavaObject rev = jo.Call<AndroidJavaObject>("methodName");
		return AndroidJNIHelper.ConvertFromJNIArray<byte[]>(rev.GetRawObject());
	}

    void OnDestroy()
    {
        UnregisterEvent();
    }

	private void InitData()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			print("InitData1");
			jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
			print("InitData2");
			jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			print("InitData3");
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
    {}

    private void CloseSerial()
    {}
	
	void Update()
	{
	
	}
}
