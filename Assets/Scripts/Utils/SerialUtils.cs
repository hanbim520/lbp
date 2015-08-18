using UnityEngine;
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
		AndroidJavaObject sendMethord = jo.Call<AndroidJavaObject>("writeSerialPort0", data);
		bool rev = AndroidJNIHelper.ConvertFromJNIArray<bool>(sendMethord.GetRawObject());
		if (rev)
		{
			print("write ok");
		}
		else
		{
			print("can't write");
		}

		AndroidJavaObject methord = jo.Call<AndroidJavaObject>("getBytes");
		byte[] b = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(methord.GetRawObject());
		if (b == null)
		{
			print("read failed");
		}
		else if (b.Length > 0)
		{
			print("read data:");
			for (int i = 0; i < b.Length; ++i)
			{
				print(b[i]);
			}
		}
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
