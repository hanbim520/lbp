using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

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
			log = ex.ToString();
		}

		dealThread = new Thread(DealReceivedData); 
		dealThread.Start(); 
		readThread = new Thread(ReceiveData);
		readThread.IsBackground = true;
		readThread.Start(); 

		writeThread = new Thread(WriteThreadFunc);
		writeThread.Start();

//		StartCoroutine(delay());
		Timer t = TimerManager.GetInstance ().CreateTimer(1, TimerType.Loop);
		t.Tick += delay;
		t.Start();
	}

	void OnDestroy()
	{
		readThread.Abort();
		isReadThreadExit = true;
		isDealTheadExit = true;
		isWriteThreadExit = true;

		if (sp.IsOpen)
		{
			print ("close");
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
					print(buf);
				}
			}
		} 
		catch (Exception ex) 
		{ 
			log = ex.ToString ();
			Debug.Log(ex); 
		} 
	} 

	private void DealReceivedData() 
	{ 
		while (!isDealTheadExit)
		{
			if (queueReadPool.Count != 0) 
			{ 
				for (int i = 0; i < queueReadPool.Count; i++) 
				{ 
					strOutPool+= queueReadPool.Dequeue(); 
					if(strOutPool.Length==16) 
					{ 
						Debug.Log(strOutPool); 
						strOutPool=string.Empty; 
					} 
				} 
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

	string log = "Hello";
	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 200, 150), "Send"))
		{
			SendSerialPortData("Hello");
		}

		if (GUI.Button(new Rect(10, 300, 500, 450), log))
		{}
	}

	void delay()
	{
		print ("send");
		SendSerialPortData("Hello");
	}
}
