using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class TouchScreenPort : MonoBehaviour
{
	public string comName = "COM4";
	public int baudRate = 9600;
	public Parity parityBit = Parity.None;
	public int dataBits = 8;
	public StopBits stopBits = StopBits.One;

	private AndroidSerialPort androidSP;
	private SerialPort sp; 
	private Queue<byte> queueReadPool = new Queue<byte>();
	private Thread readThread; 
	private bool isReadThreadExit = false;
	private string portName = "/dev/ttyS1";
	private int iCorrectNum = 0;
	private int[] filtedArray = new int[10];

	void OnEnable() 
	{ 
		filtedArray[0] = 0x55;
		filtedArray[1] = 0x54;

#if UNITY_STANDALONE_WIN
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
			isReadThreadExit = false;
			readThread = new Thread(ReceiveData);
			readThread.Start(); 
		}
		catch(Exception ex)
		{
			Debug.Log(ex.ToString());
		}
#endif

#if UNITY_ANDROID
		try
		{
			androidSP = new AndroidSerialPort(portName, baudRate, parityBit, dataBits, stopBits);
			androidSP.Open();
		}
		catch(Exception ex)
		{
			Debug.Log(ex.ToString());
		}
#endif

#if UNITY_STANDALONE_LINUX
		try
		{
			if (!LinuxSerialPort.IsOpen())
			{
				int state = LinuxSerialPort.Open(9600, 8, 1, 0, 0);
				if (state == 0)
				{
					isReadThreadExit = false;
					readThread = new Thread(ReceiveData);
					readThread.Start(); 
				}
			}
		}
		catch(Exception ex)
		{
			Debug.Log(ex.ToString());
		}
#endif
	}

	void OnDisable()
	{
		Close();

	}

	void Update()
	{
		DealReceivedData();
#if UNITY_ANDROID
		int[] data = androidSP.ReadData();
		FilterData(ref data);
#endif
	}
	
	private void ReceiveData() 
	{ 
		try 
		{ 
#if UNITY_STANDALONE_WIN
			while (!isReadThreadExit)
			{
				if (sp != null && sp.IsOpen)
				{
					byte buf = (byte)sp.ReadByte();
					queueReadPool.Enqueue(buf);
				}
			}
#endif
#if UNITY_STANDALONE_LINUX
			Thread.Sleep(3000);	// linux需要等2秒才能读
			while (!isReadThreadExit)
			{
				if (LinuxSerialPort.IsOpen())
				{
					byte[] buf = LinuxSerialPort.Read();
					if (buf.Length <= 0)
						continue;
					foreach(byte b in buf)
						queueReadPool.Enqueue(b);
				}
			}
#endif
		} 
		catch (Exception ex) 
		{ 
			Debug.Log(ex.ToString()); 
		} 
	} 

	private void DealReceivedData() 
	{ 
		while (queueReadPool.Count >= 10) 
		{ 
			byte data = queueReadPool.Dequeue(); 
			if (data == 0x55)
			{
				byte next = queueReadPool.Dequeue();
				if (next == 0x54)
				{
					byte flag = queueReadPool.Dequeue();
					// 过滤0x82悬停的部分
					if (flag == 0x82)
						continue;
					UInt16 x = 0;
					UInt16 y = 0;
					for (int i = 0; i < 2; ++i)
					{
						int count = 2;
						byte[] element = new byte[count];
						for (int j = 0; j < count; ++j)
							element[j] = queueReadPool.Dequeue();

//							if (System.BitConverter.IsLittleEndian)
//							{
//								System.Array.Reverse(element);
//							}
						if (i == 0)
						{
							x = BitConverter.ToUInt16(element, 0);
						}
						else if (i == 1)
						{
							y = BitConverter.ToUInt16(element, 0);
						}
					}
//					Debug.Log("x:" + x + ", y:" +y);
					if (flag == 0x81)
						GameEventManager.OnFingerDown(x, y);
					else if (flag == 0x82)
						GameEventManager.OnFingerHover(x, y);
					else if (flag == 0x84)
						GameEventManager.OnFingerUp(x, y);
				}
			}
		} 
	}

	public void Close()
	{
		try
		{
			#if UNITY_STANDALONE_WIN
			isReadThreadExit = true;
			if (sp.IsOpen)
			{
				sp.DtrEnable = false;
				sp.RtsEnable = false;
				sp.Close();
			}
			readThread.Abort();
			#endif
			
			#if UNITY_ANDROID
			androidSP.Close();
			#endif
			
			#if UNITY_STANDALONE_LINUX
			isReadThreadExit = true;
			if (LinuxSerialPort.IsOpen())
			{
				LinuxSerialPort.Close();
			}
			if (readThread != null)
			{
				readThread.Abort();
				readThread = null;
			}
			#endif
		}
		catch(Exception ex)
		{
			Debug.Log(ex.ToString());
		}
	}

	protected void FilterData(ref int[] data)
	{
		if (data != null && data.Length > 0 && data[0] >= 0)
		{
			for (int i = 0; i < data.Length; ++i)
			{
				if (data[i] == 0x82)
				{
					iCorrectNum = 0;
					continue;
				}
				if (data[i] == 0x55 && iCorrectNum == 0)
					iCorrectNum = 1;
				else if (data[i] == 0x54 && iCorrectNum == 1)
					iCorrectNum = 2;
				else if (data[i] != 0x82 && iCorrectNum == 2)
					iCorrectNum = 3;
				if (iCorrectNum >= 3 && iCorrectNum <= 10)
				{
					filtedArray[iCorrectNum - 1] = data[i];
					++iCorrectNum;
				}

				if (iCorrectNum > 10)
				{
					iCorrectNum = 0;
//					string log = "";
					foreach (int d in filtedArray)
					{
						queueReadPool.Enqueue((byte)d);
//						log += string.Format("{0:X}, ", (byte)d);
					}
//					DebugConsole.Log("FilterData received:" + log);
				}
			}
		}
	}
}
