using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

public class AndroidHIDUtils : MonoBehaviour
{
	private AndroidSerialPort sp1;	// 金手指通讯
	private AndroidSerialPort sp2;
	private AndroidSerialPort sp3;	// 纸钞机通讯
	private AndroidSerialPort sp4;

	private const float kRevGoldFingerDataInterval	= 0.1f;
	private float revGoldFingerDataElapsed			= 0f;
	private const float kParseDataInterval			= 0.1f;
	private float parseDataElapsed					= 0f;

	void Start()
	{
		DontDestroyOnLoad(this);
		OpenCOM();
	}

	void OnDestroy()
	{
		CloseCOM();
	}

	private void OpenCOM()
	{
		sp1 = new AndroidSerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
		sp1.Open();
//		sp3 = new AndroidSerialPort("/dev/ttyS3", 9600, Parity.None, 8, StopBits.One);
	}

	public void CloseCOM()
	{
		if (sp1 != null)
			sp1.Close();
		if (sp3 != null)
			sp3.Close();
	}

	void Update()
	{
		UpdateTimers();

		parseDataElapsed += Time.deltaTime;
		if (parseDataElapsed > kParseDataInterval)
		{
			ParseGoldfingerData();
			parseDataElapsed = 0;
		}

		revGoldFingerDataElapsed += Time.deltaTime;
		if (revGoldFingerDataElapsed > kRevGoldFingerDataInterval)
		{
			RevGoldfingerData();
			revGoldFingerDataElapsed = 0;
		}
	}

	private void UpdateTimers()
	{

	}

	private void ParseGoldfingerData()
	{
		int[] data = sp1.ReadData();
		if (data == null || data[0] == -1)
		{ 
//			if (timerHeartBeat == null)
//			{
//				timerHeartBeat = new Timer(10.0f, 0);
//				timerHeartBeat.Tick += HeartBeatOver;
//				timerHeartBeat.Start();
//			}
			return;
		}

		int len = data.Length;
		for (int i = 0; i < len; ++i)
		{
			if (data[i] == 0xA5)
			{

			}
			else if (data[i] == 0x58)
			{

			}
		}
	}

	private void RevGoldfingerData()
	{
		int[] outData = new int[]{
			0xD5, 0x58, 0x57, 14, 0,
			0, 0, 0, 0, 0,
			0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0};
		int[] temp = new int[17];
		System.Array.Copy(outData, 1, temp, 0, 17);
		int crc = Utils.CrcAddXor(temp, 17);
		outData[18] = crc;
		sp1.WriteData(ref outData);
	}
}
