using UnityEngine;
using System;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
/*
 * case 	0x425a:	报账       
 * case 	0x4a4d:	解码
 * case 	0x595a:	验证
 */
public class TestEncryChip : MonoBehaviour
{
	protected AndroidJavaClass jc;
	protected AndroidJavaObject jo;

	protected void InitJNI()
	{
		jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
	}

	void Start()
	{
		InitJNI();
	}

	int lineId = 586;
	int machineId = 88111127;
	int maxProfit = 0;
	int profit = 0;
	int checkCount  = 0;
	int crc;
	int userInput = 44579057;
	List<byte> sendToChip = new List<byte>();

	private const float getDataTime = 0.1f;
	private float getDataTimeDelta = 0;
	private const int kReadDataLength = 61;

	void OnGUI()
	{
		if (GUI.Button(new Rect(200, 10, 200, 100), "GetPWCheckValue4"))
		{
			string strCrc = jo.Call<string>("GetPWCheckValue4", (long)lineId, (long)machineId, (long)maxProfit, (long)profit, (long)checkCount);
			int value;
			if (int.TryParse(strCrc, out value))
			{
				crc = value;
			}
			DebugConsole.Log("校验码=" + crc.ToString());
		}
		
		if (GUI.Button(new Rect(200, 150, 200, 100), "CreateCheckPWString"))
		{
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>("CreateCheckPWString", 
			                                                   (long)lineId, (long)machineId, (long)maxProfit, (long)profit, (long)checkCount, (long)crc, (long)userInput);
			byte[] buf = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(rev.GetRawObject());
			List<int> data = new List<int>();
			data.Add(0x42);
			data.Add(0x5a);
			foreach(byte b in buf)
				data.Add((int)b);
			while (data.Count < 64)
				data.Add(0);
			WriteData(data.ToArray());
		}

		if (GUI.Button(new Rect(200, 300, 200, 100), "GetCheckPWStringValue"))
		{
			byte[] data = sendToChip.ToArray();
			IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
			jvalue[] blah = new jvalue[1];
			blah[0].l = pArr;
			
			IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "GetCheckPWStringValue");
			DebugConsole.Log(AndroidJNI.CallStringMethod(jo.GetRawObject(), methodId, blah));
		}
	}

	public int WriteData(int[] data)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
			jvalue[] blah = new jvalue[1];
			blah[0].l = pArr;
			
			IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "writeUsbPort");
			return AndroidJNI.CallIntMethod(jo.GetRawObject(), methodId, blah);
		}
		return -1;
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
			if (data[0] == -1 || data.Length < 2)
			{ 
				return;
			}
			if (data[0] == 0x42 && data[1] == 0x5a)
			{

			}
		}
	}
}
