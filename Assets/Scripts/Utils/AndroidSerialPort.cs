using UnityEngine;
using System;
using System.IO.Ports;
using System.Collections;
using System.Threading;

public class AndroidSerialPort
{
	public int baudrate = 9600;

	
	private Timer timerRead;
	private Timer timerSend;

	void Start()
	{
//		InitData();
	}

	void OnDestroy()
	{
		if (timerRead != null && timerRead.IsStarted())
		{
			timerRead.Stop();
		}
	}

	// Note: java side should be return boolean
	private bool WriteData(int[] data, string methodName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
			jvalue[] blah = new jvalue[1];
			blah[0].l = pArr;
			
			IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), methodName);
			return AndroidJNI.CallBooleanMethod(jo.GetRawObject(), methodId, blah);
		}
		return false;
	}

	// Note: Java side should be return int[]
	private int[] ReadData(string methodName)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>(methodName);
			return AndroidJNIHelper.ConvertFromJNIArray<int[]>(rev.GetRawObject());
		}
		return null;
	}

	
   	
    private void OpenSerial()
    {
		if (Application.platform == RuntimePlatform.Android)
		{
			DebugConsole.Log("cs OpenSerial");
			jo.Call("openSerialPort", baudrate);
			timerRead = TimerManager.GetInstance().CreateTimer(0.1f, TimerType.Loop);
			timerRead.Tick += ReadSerialPort;
			timerRead.Start();

//			timerRead = TimerManager.GetInstance().CreateTimer(1.0f, TimerType.Loop);
//			timerRead.Tick += WriteSerialPort;
//			timerRead.Start();
		}
	}

    private void CloseSerial()
    {
		if (Application.platform == RuntimePlatform.Android)
		{
			jo.Call("closeSerialPort");
		}
	}

	private void WriteSerialPort()
	{
		WriteData(new int[]{0x55,0x54,0x11,0x13,0x0D,0x55,0x54,0x11,0x13,0x0D }, "writeSerialPort3");
	}

	private void ReadSerialPort()
	{
		int[] data = ReadData("readSerialPort3");
		if (data.Length > 0)
		{
			if (data[0] == -1)
			{
				DebugConsole.Log("ReadSerialPort get -1.");
				return;
			}
		}
	}

    public AndroidSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) 
    {
        this.portName = portName;
        this.baudrate = baudrate;
        this.parity = parity;
        this.dataBits = dataBits;
        this.stopBits = stopBits;
    }

    public void Open()
    {
        InitJNI();
        int stopbits = 0;
        if (this.stopBits == StopBits.One)
            stopbits = 1;
        else if (this.stopBits == StopBits.Two)
            stopbits = 2;
		int _parity = 0;
		if (this.parity == Parity.Odd)
			_parity = 1;
		else if (this.parity == Parity.Even)
			_parity = 2;
		jo.Call("openSerialPort", portName, baudrate, _parity, dataBits, stopbits);
    }

    public void Close()
    {
        jo.Call("closeSerialPort");
        if (jo != null)
        {
            jo.Dispose();
            jo = null;
        }
        if (jc != null)
        {
            jc.Dispose();
            jc = null;
        }
    }

    protected void InitJNI()
    {
        jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
        jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
    }

    protected string portName;
    protected int baudRate;
    protected int dataBits;
    protected StopBits stopBits;
    protected Parity parity;

    protected AndroidJavaClass jc;
    protected AndroidJavaObject jo;
}
