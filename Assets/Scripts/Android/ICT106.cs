using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

// android上与ICT106纸钞机通过com口通讯 
public class ICT106 : MonoBehaviour
{
	private AndroidSerialPort sp;	// 纸钞机通讯
	private const float kRequestStatusInterval	= 1.0f;
	private float requestStatusElapsed			= 0f;
	private const float kParseDataInterval		= 0.1f;
	private float parseDataElapsed				= 0f;

	private int powerUpPhase	= 0;	// 启动阶段
	private int verifyPhase 	= 0;	// 验钞阶段
	private int billValue 		= 0;

	bool bOpen = false;

	void OnDestroy()
	{
		CloseCOM();
	}

	public void OpenCOM(string port)
	{
		try
		{
			if (bOpen)
				return;

			sp = new AndroidSerialPort("/dev/" + port, 9600, Parity.Even, 8, StopBits.One);
			sp.Open();
			bOpen = true;
		}
		catch(System.Exception ex)
		{
			Debug.Log("OpenCOM: " + ex.ToString());
		}
	}

	public void CloseCOM()
	{
		try
		{
			bOpen = false;
			if (sp != null)
				sp.Close();
			sp = null;
		}
		catch(System.Exception ex)
		{
			Debug.Log("CloseCOM: " + ex.ToString());
		}
	}

	void Update()
	{
		if (!bOpen)
			return;

		try
		{
			parseDataElapsed += Time.deltaTime;
			if (parseDataElapsed > kParseDataInterval)
			{
				ParseBillAcceptorData();
				parseDataElapsed = 0;
			}

			requestStatusElapsed += Time.deltaTime;
			if (requestStatusElapsed > kRequestStatusInterval)
			{
				RevBillAcceptorData();
				requestStatusElapsed = 0;
			}
		}
		catch(System.Exception ex)
		{
			Debug.Log("update: " + ex.ToString());
		}

	}

	private void ParseBillAcceptorData()
	{
		int[] data = sp.ReadData();
		if (data == null || data[0] == -1)
			return;

		int len = data.Length;
		for (int i = 0; i < len; ++i)
		{
			if (data[i] == 0x80 && powerUpPhase == 0)
				powerUpPhase = 1;
			else if (data[i] == 0x8f && powerUpPhase == 1)
			{
				// 纸钞机启动
				int[] outData = new int[] { 0x02 };
				sp.WriteData(ref outData);
				powerUpPhase = 0;
			}
			// 纸钞机处于抑制状态
			else if (data[i] == 0x5E)
			{
				int[] outData = new int[] { 0x02, 0x3E };
				sp.WriteData(ref outData);
			}
			// 记录币值
			else if (data[i] >= 0x40 && data[i] <= 0x45 && verifyPhase == 0)
			{
				verifyPhase = 1;
				billValue = data[i];
				int[] outData = new int[] { 0x02 };
				sp.WriteData(ref outData);
			}
			// 处理吃币结束
			else if (verifyPhase == 1 && data[i] == 0x10)
			{
				verifyPhase = 0;
				int coin = 0;
				if (billValue == 0x40)
					coin = GameData.GetInstance().FirstBill;
				else if (billValue == 0x41)
					coin = GameData.GetInstance().SecondBill;
				else if (billValue == 0x42)
					coin = GameData.GetInstance().ThirdBill;
				else if (billValue == 0x43)
					coin = GameData.GetInstance().FourthBill;
				else if (billValue == 0x44)
					coin = GameData.GetInstance().FifthBill;
				else if (billValue == 0x45)
					coin = GameData.GetInstance().SixthBill;
				billValue = 0;
				GameEventManager.OnReceiveCoin(coin);
			}
		}
		PrintData(ref data);
	}

	// 检测纸钞机的状态
	private void RevBillAcceptorData()
	{
		int[] outData = new int[] { 0x0C };
		sp.WriteData(ref outData);
	}

	void PrintData(ref int[] data)
	{
		//		string log = "data.Length:" + data.Length + "--";
		//		for (int i = 0; i < data.Length; ++i)
		//		{
		//			if (i > 0 && i % 20 == 0)
		//				log += "\n";
		//			log += string.Format("{0:X}", data[i]) + ", ";
		//		}
		//		DebugConsole.Log(log);
	}
}
