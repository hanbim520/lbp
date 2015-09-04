using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class SerialMousePort : MonoBehaviour
{
	public string comName = "COM1";
	public int baudRate = 1200; // or 2400
	public Parity parityBit = Parity.None;
	public int dataBits = 8;    // or 7
	public StopBits stopBits = StopBits.One;

	private SerialPort sp; 
	private Queue<byte> queueReadPool = new Queue<byte>();
	private Thread readThread; 
	private Thread dealThread;
	private bool isReadThreadExit = false;
	private bool isDealTheadExit = false;

	void Start()
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
		}
		catch(Exception ex)
		{
			Debug.Log(ex.ToString());
		}
#endif

#if UNITY_ANDROID

#endif

		dealThread = new Thread(DealReceivedData); 
		dealThread.Start(); 
		readThread = new Thread(ReceiveData);
		readThread.Start(); 
	}

	void OnDestroy()
	{
		readThread.Abort();
		isReadThreadExit = true;
		isDealTheadExit = true;

#if UNITY_EDITOR
		if (sp.IsOpen)
		{
			sp.DtrEnable = false;
			sp.RtsEnable = false;
			sp.Close();
		}
#endif
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

#if UNITY_ANDROID
			
#endif
		} 
		catch (Exception ex) 
		{ 
			Debug.Log(ex.ToString()); 
		} 
	} 

	/*
Byte1：X 1 LB RB Y7 Y6 X7 X6 
Byte2：X 0 X5 X4 X3 X2 X1 X0 
Byte3：X 0 Y5 Y4 Y3 Y2 Y1 Y0 

X：   无用  
1/0： bit6一直为1/0（时钟信号） 
LB：  左键按下=0，未按=1 
RB：  右键按下=0，未按=1 
X7-X0：当前位置与上次数据发送时位置的X方向相对位移值 
Y7-Y0：当前位置与上次数据发送时位置的Y方向相对位移值 
X，Y方向的两个8位数据为有符号的整数，范围是-128—+127，
	 */
	private void DealReceivedData() 
	{ 
		while (!isDealTheadExit)
		{
			if (queueReadPool.Count >= 3) 
			{ 
				byte data = queueReadPool.Dequeue(); 
				int lb = (0x20 & data) >> 5;
				int rb = (0x10 & data) >> 4;
				int y7 = (0x08 & data) >> 3;
				int y6 = (0x04 & data) >> 2;
				int x7 = (0x02 & data) >> 1;
				int x6 = 0x01 & data;
                data = queueReadPool.Dequeue();
                sbyte deltaX = (sbyte)(0x3F & data | (x7 << 7) | (x6 << 6));
                data = queueReadPool.Dequeue();
				sbyte deltaY = (sbyte)(0x3F & data | (y7 << 7) | (y6 << 6));
                GameEventManager.OnSerialMouseMove(deltaX, deltaY);
//                Debug.Log(string.Format("{0}, {1}", deltaX, deltaY));
			} 
		}
	} 
}
