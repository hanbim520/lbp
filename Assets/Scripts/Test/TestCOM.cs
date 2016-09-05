using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class TestCOM : MonoBehaviour
{
	AndroidSerialPort serialPort;
	float readElapsed  = 0;
	float readInterval = 0.1f;
	bool bReadTime = false;

	int doorState = 0;
	int ballValue = 0;

	void Start()
	{
		serialPort = new AndroidSerialPort("/dev/ttyS2", 115200, Parity.None, 8, StopBits.One);
		serialPort.Open();
	}

	IEnumerator DelayOpenCOM()
	{
		yield return new WaitForSeconds(2.0f);
		serialPort = new AndroidSerialPort("/dev/ttyS2", 115200, Parity.None, 8, StopBits.One);
		serialPort.Open();
	}
	
	void Update()
	{
		if (bReadTime)
		{
			readElapsed += Time.deltaTime;
			if (readElapsed > readInterval)
			{
				Send();
//				Rev();
				ParseGoldfingerData();
				readElapsed = 0;
			}
		}
	}

	void Rev()
	{
		int[] data = serialPort.ReadData();
		if (data == null || data[0] == -1)
			return;
		string log = "data.Length:" + data.Length + "--";
		for (int i = 0; i < data.Length; ++i)
			log += string.Format("{0:X}", data[i]) + ", ";
		DebugConsole.Log(log);

		int[] temp = new int[14];
		System.Array.Copy(data, 1, temp, 0, 14);
		if (data[15] != Utils.CrcAddXor(temp, 14))
		{
			// 校验不通过
			DebugConsole.Log("校验不通过");
		}
	}

	// 解析数据
	void ParseGoldfingerData()
	{
		int[] data = serialPort.ReadData();
		if (data == null || data[0] == -1)
			return;

		string log = "data.Length:" + data.Length + "--";
		for (int i = 0; i < data.Length; ++i)
			log += string.Format("{0:X}", data[i]) + ", ";
		DebugConsole.Log(log);

		if (data[0] != 0xA5 ||
		    data[1] != 0x58 ||
		    data[2] != 0x57)
			return;

		int[] temp = new int[14];
		System.Array.Copy(data, 1, temp, 0, 14);
		if (data[15] != Utils.CrcAddXor(temp, 14))
		{
			// 校验不通过
			return;
		}

		// 门状态
		doorState = data[5];
		// 点数
		int idx = data[6];
		ballValue = GameData.GetInstance().ballValue38[idx];
	}

	// 吹风
	public void BlowBall(int blowTime)
	{
		int hight = blowTime >> 8 & 0xff;
		int low = blowTime & 0xff;
		
		Utils.Seed(System.DateTime.Now.Millisecond + System.DateTime.Now.Second);
		// 控制吹风在轮盘转到第几个格子后启动
		int cellNum = Utils.GetRandom(1, GameData.GetInstance().maxNumberOfFields);
		
		int[] outData = new int[]{
			0xD5, 0x58, 0x57, 14, 3, 
			hight, low, cellNum, 0, 0, 
			0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0};
		int[] temp = new int[17];
		System.Array.Copy(outData, 1, temp, 0, 17);
		int crc = Utils.CrcAddXor(temp, 17);
		outData[18] = crc;
		serialPort.WriteData(ref outData);
	}

	// 开门
	private void OpenGate()
	{
		int[] outData = new int[]{
			0xD5, 0x58, 0x57, 14, 2,
			0x0E, 0xA6, 0, 0, 0,
			0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		int[] temp = new int[17];
		System.Array.Copy(outData, 1, temp, 0, 17);
		int crc = Utils.CrcAddXor(temp, 17);
		outData[18] = crc;
		serialPort.WriteData(ref outData);
	}

	public void DebugLog(string msg)
	{
		DebugConsole.Log(msg);
	}

	public void Send()
	{
		int[] outData = new int[]{
			0xD5, 0x58, 0x57, 14, 0,
			0, 0, 0, 0, 0,
			0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
		int[] temp = new int[17];
		System.Array.Copy(outData, 1, temp, 0, 17);
		int crc = Utils.CrcAddXor(temp, 17);
		outData[18] = crc;
		serialPort.WriteData(ref outData);
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(250, 50, 150, 100), "门状态 " + doorState))
		{
			
		}

		if (GUI.Button(new Rect(250, 200, 150, 100), "开门"))
		{
			OpenGate();
		}

		if (GUI.Button(new Rect(250, 350, 150, 100), "吹风"))
		{
			Utils.Seed(System.DateTime.Now.Millisecond + System.DateTime.Now.Second + System.DateTime.Now.Minute + System.DateTime.Now.Hour);
			int time = GameData.GetInstance().gameDifficulty + Utils.GetRandom(1200, 3000);
			BlowBall(time);
		}

		if (GUI.Button(new Rect(250, 500, 150, 100), "点数 " + ballValue))
		{
			
		}

		if (GUI.Button(new Rect(50, 50, 150, 100), "Quit"))
		{
			Application.Quit();
		}

		if (GUI.Button(new Rect(50, 200, 150, 100), "Send"))
		{
			Send();
		}

		if (GUI.Button(new Rect(50, 350, 150, 100), "Rev"))
		{
			Rev();
		}

		if (GUI.Button(new Rect(50, 500, 150, 100), "RealTime"))
		{
			bReadTime = !bReadTime;
			DebugConsole.Clear();
		}
	}
}
