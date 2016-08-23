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
#if UNITY_ANDROID
	protected AndroidJavaClass jc;
	protected AndroidJavaObject jo;
    protected HIDUtils hidUtils;

	protected void InitJNI()
	{
		jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
	}

	void Start()
	{
		InitJNI();
        hidUtils = gameObject.GetComponent<HIDUtils>();
	}

	int lineId = 586;
	int machineId = 88111127;
	int maxProfit = 0;
	int profit = 0;
	int checkCount  = 0;
	int crc;
	int userInput = 44579057;
	List<byte> sendToChip = new List<byte>();

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
			string log = "c# CreateCheckPWString:";
			foreach (byte b in buf)
				log += string.Format("{0:X}", b) + ", ";
			DebugConsole.Log(log);
			List<int> data = new List<int>();
			data.Add(0x42);
			data.Add(0x5a);
			data.Add(32);
			foreach(byte b in buf)
				data.Add((int)b);
			while (data.Count < 64)
				data.Add(0);
            hidUtils.WriteData(data.ToArray());
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
#endif
}
