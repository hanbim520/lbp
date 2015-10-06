using UnityEngine;
using System;
using System.Collections;

public class HIDUtils : MonoBehaviour
{
	public const int kPhaseStartBlowBall = 1;
	public const int kPhaseEndBlowBall = 2;
	public const int kPhaseOpenGate = 3;
	public const int kPhaseCloseGate = 4;
	public const int kPhaseDetectBallValue = 5;

	private AndroidJavaClass jc;
	private AndroidJavaObject jo;
	// In seconds
	private const float getDataTime = 0.1f;
	private float getDataTimeDelta = 0;
	private const int kReadDataLength = 61;
	private const int kHeader1 = 0x58;
	private const int kHeader2 = 0x57;
	private const int kBlowBall = 0x55;
	private const int kOpenGate = 0x55;
	private const int kPreviousValue = 0x55;

	private bool bBlowedBall = false;
	private bool bOpenGate = false;
	private int phase = 0; 
	private bool isOpen = false;
	public bool IsOpen
	{
		get { return isOpen; }
		set { isOpen = value; }
	}

	void Start()
	{
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

	public void SetState(string value)
	{
		bool state;
		if (bool.TryParse(value, out state))
		{
			isOpen = state;
			if (isOpen)
				GameEventManager.OnHIDConnected();
			else
				GameEventManager.OnHIDDisconnected();
		}
	}

	public void OpenUSB()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			jo.Call("openUsb");
		}
	}

	public void CloseUSB()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
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
		int[] data = ReadData("readHID");
		if (data != null && data.Length == kReadDataLength)
		{
			if (data[0] == -1 || data[0] != kHeader1 || data[1] != kHeader2)
			{ 
				return;
			}
//			string log = "data.Length:" + data.Length + "--";
//			for (int i = 0; i < data.Length; ++i)
//			{
//				log += string.Format("{0:X}", data[i]) + ", ";
//			}
//			DebugConsole.Log(log);

			// 吹风
			if (data[2] == kBlowBall)
			{
				if (!bBlowedBall)
				{
					phase = kPhaseStartBlowBall;
					bBlowedBall = true;
				}
			}
			else
			{
				if (bBlowedBall && phase == kPhaseStartBlowBall)
				{
					phase = kPhaseEndBlowBall;
					bBlowedBall = false;
					GameEventManager.OnEBlowBall();
				}
			}
			// 开门
			if (data[3] == kOpenGate)
			{
				if (!bOpenGate)
				{
					phase = kPhaseOpenGate;
					bOpenGate = true;
				}
			}
			else
			{
				if (bOpenGate && phase == kPhaseOpenGate)
				{
					phase = kPhaseCloseGate;
					bOpenGate = false;
					GameEventManager.OnCloseGate();
				}
			}
			// 不是上轮结果
			if (data[5] != kPreviousValue && phase == kPhaseEndBlowBall)
			{
				// 结果
				int idx = data[4];
				if (idx == 0)
					return;
				else
					idx -= 1;
				phase = kPhaseDetectBallValue;
				if (GameData.GetInstance().maxNumberOfFields == 38)
					GameEventManager.OnBallValue(GameData.GetInstance().ballValue38[idx]);
				else if (GameData.GetInstance().maxNumberOfFields == 37)
					GameEventManager.OnBallValue(GameData.GetInstance().ballValue37[idx]);
			}
		}
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
			int[] data = new int[]{0x58, 0x57, 0x02, 0x0E, 0xA6, 0, 0, 0, 
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

//	private string log = "";
//	void OnGUI()
//	{
//		if (GUI.Button(new Rect(200, 10 , 200, 150), "open usb"))
//		{
//			OpenUSB();
//		}
//		if (GUI.Button(new Rect(200, 200 , 200, 150), "close usb"))
//		{
//			CloseUSB();
//		}
//		if (GUI.Button(new Rect(420, 10 , 200, 150), "open gate"))
//		{
//			OpenGate();
//		}
//		if (GUI.Button(new Rect(420, 200 , 200, 150), "blow ball"))
//		{
//			BlowBall(1500);
//		}
//		if (GUI.Button(new Rect(200, 400, 200, 150), "Quit"))
//		{
//			Application.Quit();
//		}
//	}
}
