using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class SerialPortUtils : MonoBehaviour
{
	private SerialPort sp; 
	private Queue<string> queueDataPool;
	private Queue<string> queueWritePool;
	private Thread tPort; 
	private Thread tPortDeal;
	private Thread writeThread;
	private string strOutPool = string.Empty; 
	string finalstring = string.Empty; 
	string tempstring = string.Empty; 

	void Start() 
	{ 
		queueDataPool = new Queue<string>(); 
		queueWritePool = new Queue<string>();

		sp = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
		if (!sp.IsOpen) 
		{ 
			sp.Open(); 
		} 
		tPort = new Thread(DealData); 
		tPort.Start(); 
		tPortDeal = new Thread(ReceiveData); 
		tPortDeal.Start(); 

		writeThread = new Thread(WriteThreadFunc);
		writeThread.Start();
	} 
	
	void Update() 
	{ 
		if (tPortDeal != null)
		{
			if (!tPortDeal.IsAlive) 
			{ 
				tPortDeal = new Thread(ReceiveData); 
				tPortDeal.Start(); 
			} 
			if (!tPort.IsAlive) 
			{ 
				tPort = new Thread(DealData); 
				tPort.Start(); 
			} 
		}
		if (writeThread != null)
		{
			if (!writeThread.IsAlive)
			{
				writeThread = new Thread(WriteThreadFunc);
				writeThread.Start();
			}
		}
	} 
	
	private void ReceiveData() 
	{ 
		try 
		{ 
			Byte[] buf = new Byte[1024]; 
			string sbReadline2str = string.Empty; 
			if (sp.IsOpen) 
				sp.Read(buf, 0, buf.Length); 
			if (buf.Length == 0) 
			{ 
				return; 
			} 
			if (buf != null) 
			{ 
				for (int i = 0; i < buf.Length; i++) 
				{ 
					sbReadline2str += string.Format("{0:X2}", buf[i]); 
					queueDataPool.Enqueue(sbReadline2str); 
				} 
				print (sbReadline2str);
			} 
		} 
		catch (Exception ex) 
		{ 
			Debug.Log(ex); 
		} 
	} 

	private void DealData() 
	{ 
		while (queueDataPool.Count != 0) 
		{ 
			for (int i = 0; i < queueDataPool.Count; i++) 
			{ 
				strOutPool+= queueDataPool.Dequeue(); 
				if(strOutPool.Length==16) 
				{ 
					Debug.Log(strOutPool); 
					strOutPool=string.Empty; 
				} 
			} 
			
		} 
	} 
	
	public void SendSerialPortData(string data) 
	{ 
		queueWritePool.Enqueue(data);
		print("count:" + queueWritePool.Count);
//		if(sp.IsOpen) 
//		{ 
//			sp.WriteLine(data); 
//		} 
	} 

	private void WriteThreadFunc()
	{
		while (queueWritePool.Count != 0)
		{
			for (int i = 0; i < queueWritePool.Count; ++i)
			{
				string data = queueWritePool.Dequeue();
				if (sp.IsOpen)
				{
					sp.WriteLine(data); 
				}
			}
		}
	}

	void OnDestroy()
	{
		sp.Close(); 
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 200, 150), "Send"))
		{
			SendSerialPortData("Hello");
		}
	}
}
