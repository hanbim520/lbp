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
	public int baudRate = 1200; // or 2400
	public Parity parityBit = Parity.None;
	public int dataBits = 7;    // or 8
	public StopBits stopBits = StopBits.One;

	private SerialPort sp; 
	private Queue<byte> queueReadPool = new Queue<byte>();
	private Thread readThread; 
	private bool isReadThreadExit = false;
    private bool blAlreadyDown = false;
    private bool rlAlreadyDown = false;
    private float ratio = 1.2f;
    private float xMax;
    private float xMin;
    private float yMax;
    private float yMin;
	private bool bFindMouse = false;

	private const int refrenceWidth = 704;

	void OnEnable()
	{
		Init();
        RegisterEvents();
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
		
		isReadThreadExit = false;
		readThread = new Thread(ReceiveData);
		readThread.Start(); 
	}

	void OnDisable()
	{
        UnregisterEvents();
		readThread.Abort();
		isReadThreadExit = true;

#if UNITY_EDITOR
        if (sp.IsOpen)
        {
            sp.DtrEnable = false;
            sp.RtsEnable = false;
            sp.Close();
        }
#endif
        
#if UNITY_ANDROID
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
//					Debug.Log(buf);
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

	void FixedUpdate()
	{
		DealReceivedData();
		MoveMouse();
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
			int lb = (0x20 & data) >> 5;
			int rb = (0x10 & data) >> 4;
			int y7 = (0x08 & data) >> 3;
			int y6 = (0x04 & data) >> 2;
			int x7 = (0x02 & data) >> 1;
			int x6 = 0x01 & data;
            data = queueReadPool.Dequeue();
//                sbyte deltaX = (sbyte)(0x3F & data | (x7 << 7) | (x6 << 6));
			int x = 0x3F & data | (x7 << 7) | (x6 << 6);
			if (x > 127)
				x -= 256;
			sbyte deltaX = (sbyte)x;
            data = queueReadPool.Dequeue();
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
        GameData.GetInstance().serialMouseX -= deltaX * ratio;
        GameData.GetInstance().serialMouseY += deltaY * (ratio + 0.1f);
		
		if (GameData.GetInstance().serialMouseX >= xMax)
            GameData.GetInstance().serialMouseX = xMax;
        else if (GameData.GetInstance().serialMouseX <= xMin)
            GameData.GetInstance().serialMouseX = xMin;
        else if (GameData.GetInstance().serialMouseY >= yMax)
            GameData.GetInstance().serialMouseY = yMax;
        else if (GameData.GetInstance().serialMouseY <= yMin)
            GameData.GetInstance().serialMouseY = yMin;
    }

    private void Init()
    {
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
			mouse.transform.localPosition = new Vector3(GameData.GetInstance().serialMouseX, GameData.GetInstance().serialMouseY, 0);
		}
	}
}
