using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 基于UGUI的打码界面
public class DlgPrintCode2 : MonoBehaviour 
{
	public Text inputCodes;			// 输入码
	public Text inputContent;		// 计算器
	public Text inputA;
	public Text inputB;
	public Text inputC;
	public Text inputD;
	public Text inputE;

	const int kMaxNumSize = 8;
	int codesValue;		// 打码器算出来的码
	int zongya;			// 总押
	int zongpei;		// 总赔
	int lineId;			// 线号
	int machineId;		// 机台号
	int checkcode;		// 校验码
	int printtimes;		// 打码次数
	int profit;			// 总盈利

	AndroidJavaClass jc;
	AndroidJavaObject jo;
	IntPtr encryptMethodId = IntPtr.Zero;
	IntPtr decryptMethodId = IntPtr.Zero;

	void Start()
	{
		jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

		GameEventManager.GetChipData += GetChipData;

		Init();
	}

	void OnDestroy()
	{
		GameEventManager.GetChipData -= GetChipData;
	}

	// 处理加密片的回传数据
	void GetChipData(int[] data)
	{
		try
		{
			int len = 14;
			byte[] tmp = new byte[len];
			for (int i = 0; i < len; ++i)
				tmp[i] = (byte)data[i + 4];
			IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(tmp);
			jvalue[] blah = new jvalue[1];
			blah[0].l = pArr;

			IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "GetCheckPWStringValue");
			// 获取打码结果
			string result = AndroidJNI.CallStringMethod(jo.GetRawObject(), methodId, blah);
			DebugConsole.Log("result: " + result);
			char[] split = {':'};
			string[] word = result.Split(split);
			if (word.Length < 3)
				return;

			int value, days, type;
			if (!int.TryParse(word[0], out value))
				return;

			GameData.GetInstance().printTimes += 1;
			if (value != 1)
			{
				DebugConsole.Log("打码失败");

				printtimes = GameData.GetInstance().printTimes;
				inputD.text = printtimes.ToString();
				checkcode = GetCheckCode(lineId, machineId, profit, profit, printtimes);
				inputE.text = checkcode.ToString();
				codesValue = 0;
				inputCodes.text = "0";

				GameEventManager.OnPrintCodeFail();
				return;
			}

			DebugConsole.Log("打码成功");
			if (int.TryParse(word[1], out days))
			{
				long ticks = System.DateTime.Now.Ticks + new System.TimeSpan(days, 0, 0, 0).Ticks;
				PlayerPrefs.SetString("ExpiredDate", ticks.ToString());
				PlayerPrefs.Save();
			}
			if (int.TryParse(word[2], out type)) 	// 40000--清账
			{
				DebugConsole.Log("打码类型: " + type);
				if (type == 40000)
				{
					GameData.GetInstance().ClearAccount();
				}
			}
			GameEventManager.OnPrintCodeSuccess(type);
		}
		catch(System.Exception ex)
		{
			Debug.Log("GetChipData " + ex.ToString());
		}
	}

	void Init()
	{
		lineId = GameData.GetInstance().lineId;
		machineId = GameData.GetInstance().machineId;
		zongya = GameData.GetInstance().TotalZongYa;
		zongpei = GameData.GetInstance().TotalZongPei;
		printtimes = GameData.GetInstance().printTimes;

		inputA.text = zongya.ToString();
		inputB.text = zongpei.ToString();
		inputC.text = machineId.ToString();
		inputD.text = printtimes.ToString();

		profit = zongya - zongpei;
		checkcode = GetCheckCode(lineId, machineId, profit, profit, printtimes);
		inputE.text = checkcode.ToString();
	}

	// 获取校验码
	int GetCheckCode(int lineId, int machineId, int totalWin, int currentWin, int printTimes)
	{
		string strCrc = jo.Call<string>("GetPWCheckValue4", (long)lineId, (long)machineId, (long)totalWin, (long)currentWin, (long)printTimes);
		int value;
		if (int.TryParse(strCrc, out value))
			return value;
		return 0;
	}

	// 加密传给加密片的数据
	byte[] EncryptIOData(byte[] data)
	{
		IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
		jvalue[] blah = new jvalue[2];
		blah[0].l = pArr;

		if (encryptMethodId == IntPtr.Zero)
			encryptMethodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "EncryptIOData");
		IntPtr ret = AndroidJNI.CallObjectMethod(jo.GetRawObject(), encryptMethodId, blah);
		AndroidJNI.DeleteLocalRef(pArr);
		return AndroidJNIHelper.ConvertFromJNIArray<byte[]>(ret);
	}

	// 解密加密片回传的数据
	byte[] DecryptIOData(byte[] data, int size)
	{
		IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
		jvalue[] blah = new jvalue[2];
		blah[0].l = pArr;
		blah[1].i = size;

		if (decryptMethodId == IntPtr.Zero)
			decryptMethodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "DecryptIOData");
		IntPtr ret = AndroidJNI.CallObjectMethod(jo.GetRawObject(), decryptMethodId, blah);
		AndroidJNI.DeleteLocalRef(pArr);
		return AndroidJNIHelper.ConvertFromJNIArray<byte[]>(ret);
	}

	public void OnPressDown(int value)
	{
		if (value >= 0)
		{
			if (Utils.GetNumLength(codesValue) < kMaxNumSize)
				codesValue = codesValue * 10 + value;
		}
		else 
		{
			codesValue /= 10;
		}
		inputContent.text = inputCodes.text = codesValue.ToString();
	}

	// 尝试解锁
	public void OnEnterDown()
	{
		try
		{
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>("CreateCheckPWString", 
				(long)lineId, (long)machineId, (long)profit, (long)profit, (long)printtimes, (long)checkcode, (long)codesValue);
			byte[] buf = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(rev.GetRawObject());
			int len = buf.Length;
			int[] tmp = new int[len];
			for (int i = 0; i < len; ++i)
				tmp[i] = buf[i];
			GameEventManager.OnSetChipData(tmp);
			GameEventManager.OnTalkChipEnable(true);
		}
		catch (System.Exception ex)
		{
			Debug.Log(ex.ToString());
		}

	}
}
