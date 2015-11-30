using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class TouchScreenPort : MonoBehaviour
{
	public string comName = "COM1";
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
	
	void OnEnable() 
	{ 
#if UNITY_EDITOR
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
	}

	void OnDisable()
	{
#if UNITY_EDITOR
		readThread.Abort();
		isReadThreadExit = true;
		if (sp.IsOpen)
		{
			sp.DtrEnable = false;
			sp.RtsEnable = false;
			sp.Close();
		}
#endif

#if UNITY_ANDROID
		androidSP.Close();
#endif
	}

	void FixedUpdate()
	{
		DealReceivedData();
	}
	
	private void ReceiveData() 
	{ 
		try 
		{ 
#if UNITY_EDITOR
			while (!isReadThreadExit)
			{
				if (sp != null && sp.IsOpen)
				{
					byte buf = (byte)sp.ReadByte();
					queueReadPool.Enqueue(buf);
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
		if (queueReadPool.Count >= 10) 
		{ 
			byte data = queueReadPool.Dequeue(); 
			if (data == 0x55)
			{
				byte next = queueReadPool.Dequeue();
				if (next == 0x54)
				{
					byte flag = queueReadPool.Dequeue();
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
//						Debug.Log("x:" + x + ", y:" +y);
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
		androidSP.Close();
	}
}
