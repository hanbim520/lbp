using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

public struct BufferStruct
{
	public byte[] buf;
}

public class SerialPortUtils : MonoBehaviour
{
	public string comName = "COM1";
	public int baudRate = 9600;
	public Parity parityBit = Parity.None;
	public int dataBits = 8;
	public StopBits stopBits = StopBits.One;

	private SerialPort sp; 
	private Queue<byte> queueReadPool = new Queue<byte>();
	private Queue<string> queueWritePool = new Queue<string>();
	private Thread readThread; 
	private Thread dealThread;
	private Thread writeThread;
	private bool isReadThreadExit = false;
	private bool isDealTheadExit = false;
	private bool isWriteThreadExit = false;
	private string strOutPool = string.Empty; 

	private const int kWriteInterval = 10;

	void Start() 
	{ 
		// 端口名称 波特率 奇偶校验位 数据位值 停止位
		try
		{
			sp = new SerialPort(comName, baudRate, parityBit, dataBits, stopBits);
			if (!sp.IsOpen)
			{
				sp.Open();
				sp.DtrEnable = true;
				sp.RtsEnable = true;
			}
		}
		catch(Exception ex)
		{
			Debug.Log(ex.ToString());
		}

		dealThread = new Thread(DealReceivedData); 
		dealThread.Start(); 
		readThread = new Thread(ReceiveData);
		readThread.Start(); 

//		writeThread = new Thread(WriteThreadFunc);
//		writeThread.Start();
	
	}

	void OnDestroy()
	{
		readThread.Abort();
		isReadThreadExit = true;
		isDealTheadExit = true;
		isWriteThreadExit = true;

		if (sp.IsOpen)
		{
			sp.DtrEnable = false;
			sp.RtsEnable = false;
			sp.Close();
		}
	}
	

	private void ReceiveData() 
	{ 
		try 
		{ 
			while (!isReadThreadExit)
			{
				if (sp != null && sp.IsOpen)
				{
					byte buf = (byte)sp.ReadByte();
					queueReadPool.Enqueue(buf);
				}
			}
		} 
		catch (Exception ex) 
		{ 
			Debug.Log(ex.ToString()); 
		} 
	} 

	private void DealReceivedData() 
	{ 
		while (!isDealTheadExit)
		{
			if (queueReadPool.Count >= 10) 
			{ 
				byte data = queueReadPool.Dequeue(); 
				if (data == 0x55)
				{
					byte next = queueReadPool.Dequeue();
					if (next == 0x54)
					{
						int x = 0;
						int y = 0;
						for (int i = 0; i < 2; ++i)
						{
							uint count = 4;
							byte[] element = new byte[count];
							for (int j = 0; j < count; ++j)
							{
								element[j] = queueReadPool.Dequeue();
							}

							if (System.BitConverter.IsLittleEndian)
							{
								System.Array.Reverse(element);
							}
							if (i == 0)
							{
								x = BitConverter.ToInt32(element, 0);
							}
							else if (i == 1)
							{
								y = BitConverter.ToInt32(element, 0);
							}
						}
						Debug.Log("x:" + x + ", y:" +y);
					}
				}

//				Debug.Log(string.Format("{0:X}", data));
			} 
		}
	} 
	
	public void SendSerialPortData(string data) 
	{ 
		queueWritePool.Enqueue(data);
	} 

	private void WriteThreadFunc()
	{
		while (!isWriteThreadExit)
		{
			Thread.Sleep(kWriteInterval);
			if (queueWritePool.Count != 0)
			{
				for (int i = 0; i < queueWritePool.Count; ++i)
				{
					string data = queueWritePool.Dequeue();
					if (sp.IsOpen)
						sp.Write(data);
				}
			}
		}
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 200, 150), "Send"))
		{
			SendSerialPortData("Hello");
		}
	}
}
