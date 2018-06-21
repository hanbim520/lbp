using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System;

class JCMMsg
{
	public int[] data;
}

// android上与JCM纸钞机通过com口通讯 
public class JCMBVAndroid : MonoBehaviour
{
	private AndroidSerialPort sp;	// 纸钞机通讯
	private const float kReqStatusInterval				= 1.0f;
	private float reqStatusElapsed						= 0f;
	private const float kParseDataInterval				= 0.1f;
	private float parseDataElapsed						= 0f;

	bool bOpen = false;
	bool bNeedACK = false;
	bool bInitializing = false;

	int[] lastMsg;
	Queue<JCMMsg> revMsgs = new Queue<JCMMsg>();
	bool isBeginning = false;
	JCMMsg entireMsg;
	int msgSize = 0;
	int msgIndex = 0;
	int billValue = 0;

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
		catch(Exception ex)
		{
			Debug.Log("OpenCOM " + ex.ToString());
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
		catch(Exception ex)
		{
			Debug.Log("CloseCOM " + ex.ToString());
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
				parseDataElapsed = 0;
				ReadBVData();
				HandleBVData();
			}

			reqStatusElapsed += Time.deltaTime;
			if (reqStatusElapsed > kReqStatusInterval)
			{
				reqStatusElapsed = 0;
				ReqStatus();
			}
		}
		catch(System.Exception ex)
		{
			Debug.Log("Update " + ex.ToString());
		}

	}
		
	void ReadBVData()
	{
		int[] data = sp.ReadData();
		if (data == null || data[0] == -1)
			return;

		int size = data.Length;
		for (int i = 0; i < size; ++i)
		{
			if (data[i] == 0xFC && msgSize == 0)
			{
				isBeginning = true;
				continue;
			}
			if (isBeginning)
			{
				isBeginning = false;
				entireMsg = new JCMMsg();
				msgSize = data[i];
				entireMsg.data = new int[msgSize];
				entireMsg.data[0] = 0xFC;
				entireMsg.data[1] = msgSize;
				msgIndex = 2;
				continue;
			}
			if (msgSize == 0)
				continue;
			
			entireMsg.data[msgIndex] = data[i];
			++msgIndex;
			if (msgIndex >= msgSize)
			{
				msgSize = 0;
				msgIndex = 0;
				revMsgs.Enqueue(entireMsg);
			}
		}
	}

	void HandleBVData()
	{
		while (revMsgs.Count > 0)
		{
			JCMMsg msg = revMsgs.Dequeue();
			int[] data = msg.data;
//			PrintData(ref data);
			int size = data[1];

			int[] crc = CRCUtils.JCMCRC(msg.data, size - 2, 0);
			if (crc[0] != data[size - 2] ||
				crc[1] != data[size - 1])
			{
				continue;
			}

			int cmd = data[2];
			if ((!bInitializing) && (cmd == 0x40 || cmd == 0x41 || cmd == 0x42))	// Power Up 
				ResetCMD();
			else if (cmd == 0x50 && bNeedACK)		// ACK
			{
				bNeedACK = false;
				if (bInitializing)
					EnableBV();
			}
			else if (cmd == 0xC0)		// Enable/Disable
				StandardSecurity();
			else if (cmd == 0xC1)		// Security
				Inhibit(true);
			else if (cmd == 0x1B)
				GameEventManager.OnBVTip("BV Initializing");
			else if (cmd == 0x11)		// Idling
			{
				bInitializing = false;
				GameEventManager.OnBVTip(string.Empty);
			}
			else if (cmd == 0x12)
				GameEventManager.OnBVTip("Accepting");
			else if (cmd == 0x13)
			{
				GameEventManager.OnBVTip("Escrow");
				AcceptBill();
				int val = data[3];
				if (val == 0x61)
					billValue = 1;
				else if (val == 0x63)
					billValue = 5;
				else if (val == 0x64)
					billValue = 10;
				else if (val == 0x65)
					billValue = 20;
				else if (val == 0x66)
					billValue = 50;
				else if (val == 0x67)
					billValue = 100;
			}
			else if (cmd == 0x14)
				GameEventManager.OnBVTip("Stacking");
			else if (cmd == 0x15)
			{
				if (billValue > 0)
				{
					GameEventManager.OnReceiveCoin(billValue);
					billValue = 0;
				}
				GameEventManager.OnBVTip("Vend Valid");
				SndACK();
			}
			else if (cmd == 0x16)
				GameEventManager.OnBVTip("Stacked");
			else if (cmd == 0x1A)
			{
				GameEventManager.OnBVTip("BV Disable");
				Inhibit(true);
			}
			else if (cmd == 0x43)
				GameEventManager.OnBVTip("Stacker Full");
			else if (cmd == 0x44)
				GameEventManager.OnBVTip("Stacker Open");
			else if (cmd == 0x45)
				GameEventManager.OnBVTip("Jam In Acceptor");
			else if (cmd == 0x46)
				GameEventManager.OnBVTip("Jam In Stacker");
			else if (cmd == 0x47)
				GameEventManager.OnBVTip("Pause");
			else if (cmd == 0x47)
				GameEventManager.OnBVTip("Cheated");
			
		}
	}
		
	void VesionRequest()
	{
		int[] outdata = new int[] { 0xFC, 0x05, 0x88, 0x6F, 0x5F };
		sp.WriteData(ref outdata);
		lastMsg = outdata;
	}

	void ResetCMD()
	{
		bInitializing = true;
		int[] outdata = new int[] { 0xFC, 0x05, 0x40, 0x2B, 0x15 };
		sp.WriteData(ref outdata);
		bNeedACK = true;
		lastMsg = outdata;
	}

	void StandardSecurity()
	{
		int[] outdata = new int[] { 0xFC, 0x07, 0xC1, 0x82, 0x0, 0x51, 0x0A };
		sp.WriteData(ref outdata);
		lastMsg = outdata;
	}

	public void AcceptBill()
	{
		int[] outdata = new int[]{ 0xFC, 0x05, 0x42, 0x39, 0x36 };
		sp.WriteData(ref outdata);
		lastMsg = outdata;
	}

	// 禁止接收所有纸钞
	public void DisableBV()
	{
		int[] outdata = new int[] { 0xFC, 0x07, 0xC0, 0, 0, 0x2D, 0xB5 };
		sp.WriteData(ref outdata);
		lastMsg = outdata;
	}

	// 允许接收所有纸钞
	public void EnableBV()
	{
		int[] outdata = new int[] { 0xFC, 0x07, 0xC0, 0x82, 0, 0x51, 0x0A };
		sp.WriteData(ref outdata);
		lastMsg = outdata;
	}

	// 状态查询
	public void ReqStatus()
	{
		int[] outdata = new int[] { 0xFC, 0x05, 0x11, 0x27, 0x56 };
		sp.WriteData(ref outdata);
		lastMsg = outdata;
	}

	// 关闭/打开接收所有纸币和彩票
	void Inhibit(bool enable)
	{
		int[] outdata = !enable ? new int[] { 0xFC, 0x06, 0xC3, 1, 0x8D, 0xC7 } : new int[] { 0xFC, 0x06, 0xC3, 0, 0x04, 0xD6 };
		sp.WriteData(ref outdata);
		lastMsg = outdata;
	}

	// controller应答 (controller -> acceptor)
	public void SndACK()
	{
		int[] outdata = new int[] { 0xFC, 0x05, 0x50, 0xAA, 0x05 };
		sp.WriteData(ref outdata);
		lastMsg = outdata;
	}

	private void PrintData(ref int[] data)
	{
		string log = "data.Length:" + data.Length + "--";
		for (int i = 0; i < data.Length; ++i)
		{
			if (i > 0 && i % 20 == 0)
				log += "\n";
			log += string.Format("{0:X}", data[i]) + ", ";
		}
		DebugConsole.Log(log);
	}
}
