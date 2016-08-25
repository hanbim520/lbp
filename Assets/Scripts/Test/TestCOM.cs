using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class TestCOM : MonoBehaviour
{
	AndroidSerialPort serialPort;
	float readElapsed  = 0;
	float readInterval = 0.1f;
	bool bReadTime = false;

	void Start()
	{
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
				Rev();
				readElapsed = 0;
			}
		}

	}

	void Rev()
	{
		int[] data = serialPort.ReadData();
		if (data.Length == 1 && data[0] == -1)
			return;
		string log = "data.Length:" + data.Length + "--";
		for (int i = 0; i < data.Length; ++i)
			log += string.Format("{0:X}", data[i]) + ", ";
		DebugConsole.Log(log);
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
		if (GUI.Button(new Rect(250, 50, 150, 100), "RealTime"))
		{
			bReadTime = !bReadTime;
			DebugConsole.Clear();
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
	}
}
