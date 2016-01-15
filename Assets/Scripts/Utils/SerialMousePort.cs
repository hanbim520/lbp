using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class SerialMousePort : MonoBehaviour
{
    public GameObject mouse;
	public string comName = "COM1";
	public int baudRate = 1200; 
	public Parity parityBit = Parity.None;
	public int dataBits = 7;    
	public StopBits stopBits = StopBits.One;

	private AndroidSerialPort androidSP;
	private SerialPort sp; 
	private Queue<byte> queueReadPool = new Queue<byte>();
	private Thread readThread; 
	private bool isReadThreadExit = false;
    private bool blAlreadyDown = false;
    private bool rlAlreadyDown = false;
    private float ratio = 0.8f;
    private float xMax;
    private float xMin;
    private float yMax;
    private float yMin;
	private bool bFindMouse = false;

	private const int refrenceWidth = 704;
	private string portName = "/dev/ttyS1";

	void OnEnable()
	{
		Init();
        RegisterEvents();
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
				int state = LinuxSerialPort.Open(1200, 7, 1, 0, 0);
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
        UnregisterEvents();        
		Close();
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
//					DebugConsole.Log(buf.ToString());
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

	void Update()
	{
		DealReceivedData();
#if UNITY_ANDROID
		int[] data = androidSP.ReadData();
		if (data != null && data.Length > 0 && data[0] >= 0)
		{
//			string log = "";
			foreach (int d in data)
			{
				queueReadPool.Enqueue((byte)d);
//				log += string.Format("{0:X}, ", (byte)d);
			}
//			DebugConsole.Log("FixedUpdate received:" + log);
		}
#endif
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
		if (queueReadPool.Count >= 3) 
		{ 
			if (!bFindMouse)
				bFindMouse = true;
			byte data = queueReadPool.Dequeue();
			int isHead = data & 0x40;
			if (isHead == 0)
				return;
//            string log = "";
//            log += string.Format("data={0:X}", data);
			int lb = (0x20 & data) >> 5;
			int rb = (0x10 & data) >> 4;
			int y7 = (0x08 & data) >> 3;
			int y6 = (0x04 & data) >> 2;
			int x7 = (0x02 & data) >> 1;
			int x6 = 0x01 & data;
            data = queueReadPool.Dequeue();
//            log += string.Format(", {0:X}", data);
//                sbyte deltaX = (sbyte)(0x3F & data | (x7 << 7) | (x6 << 6));
			int x = 0x3F & data | (x7 << 7) | (x6 << 6);
			if (x > 127)
				x -= 256;
			sbyte deltaX = (sbyte)x;
            data = queueReadPool.Dequeue();
//            log += string.Format(", {0:X}", data);
//            DebugConsole.Log(log);
//				sbyte deltaY = (sbyte)(0x3F & data | (y7 << 7) | (y6 << 6));
			int y = 0x3F & data | (y7 << 7) | (y6 << 6);
			if (y > 127)
				y -= 256;
			sbyte deltaY = (sbyte)y;
        	GameEventManager.OnSerialMouseMove(deltaX, deltaY);
            if (lb == 1)
                GameEventManager.OnSMLBDown();
            else if (lb == 0 && blAlreadyDown)
            {
				print("OnSMLBUp");
                GameEventManager.OnSMLBUp();
                blAlreadyDown = false;
            }
            if (rb == 1)
                GameEventManager.OnSMRBDown();
            else if (rb == 0 && rlAlreadyDown)
            {
				print("OnSMRBUp");
                GameEventManager.OnSMRBUp();
                rlAlreadyDown = false;
            }
//                Debug.Log(string.Format("{0}, {1}", deltaX, deltaY));
		}
	} 

    private void RegisterEvents()
    {
        GameEventManager.SMLBDown += SMLBDown;
        GameEventManager.SMRBDown += SMRBDown;
        GameEventManager.SerialMouseMove += SerialMouseMove;
    }

    private void UnregisterEvents()
    {
        GameEventManager.SMLBDown -= SMLBDown;
        GameEventManager.SMRBDown -= SMRBDown;
        GameEventManager.SerialMouseMove -= SerialMouseMove;
    }

    private void SMLBDown()
    {
        blAlreadyDown = true;
		print("SMLBDown");
    }

    private void SMRBDown()
    {
        rlAlreadyDown = true;
		print("SMRBDown");
    }

    private void SerialMouseMove(sbyte deltaX, sbyte deltaY)
    {
        GameData.GetInstance().serialMouseX += deltaX * ratio;
        GameData.GetInstance().serialMouseY += deltaY * (ratio + 0.1f);
		
		if (GameData.GetInstance().serialMouseX >= xMax)
            GameData.GetInstance().serialMouseX = xMax;
        else if (GameData.GetInstance().serialMouseX <= xMin)
            GameData.GetInstance().serialMouseX = xMin;
        else if (GameData.GetInstance().serialMouseY >= yMax)
            GameData.GetInstance().serialMouseY = yMax;
        else if (GameData.GetInstance().serialMouseY <= yMin)
            GameData.GetInstance().serialMouseY = yMin;
		MoveMouse();
    }

    private void Init()
    {
		InitMouseIcon();
        int resolutionWidth = GameData.GetInstance().resolutionWidth;
        int resolutionHeight = GameData.GetInstance().resolutionHeight;
        xMax = resolutionWidth / 2;
        xMin = -resolutionWidth / 2;
        yMax = resolutionHeight / 2;
        yMin = -resolutionHeight / 2;
		ratio = (float)Screen.width / refrenceWidth * ratio;
		bFindMouse = false;
		blAlreadyDown = false;
		rlAlreadyDown = false;
	}

	private void MoveMouse()
	{
		if (bFindMouse)
		{
			if (mouse == null)
				InitMouseIcon();
			else
				mouse.transform.localPosition = new Vector3(GameData.GetInstance().serialMouseX, GameData.GetInstance().serialMouseY, 0);
		}
	}

	private void InitMouseIcon()
	{
		if (mouse == null)
		{
			mouse = GameObject.Find("mouse icon");
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

//	void OnGUI()
//	{
//		if (GUI.Button(new Rect(200, 10, 200, 100), "Exit"))
//		{
//			Application.Quit();
//		}
//	}

}
