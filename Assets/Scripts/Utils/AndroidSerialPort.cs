using UnityEngine;
using System;
using System.IO.Ports;
using System.Collections;
using System.Threading;

public class AndroidSerialPort
{
#if UNITY_ANDROID
	protected string portName;
	protected int baudRate;
	protected int dataBits;
	protected StopBits stopBits;
	protected Parity parity;
	
	protected AndroidJavaClass jc;
	protected AndroidJavaObject jo;

	protected string strOpenMethod = "openSerialPort";
	protected string strCloseMethod = "closeSerialPort";
	protected string strReadMethod = "readSerialPort";

	// Note: Java side should be return int[]
	public int[] ReadData()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>(strReadMethod);
			return AndroidJNIHelper.ConvertFromJNIArray<int[]>(rev.GetRawObject());
		}
		return null;
	}

	//"/dev/ttyS1"
    public AndroidSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) 
    {
        this.portName = portName;
		this.baudRate = baudRate;
        this.parity = parity;
        this.dataBits = dataBits;
        this.stopBits = stopBits;
    }

    public void Open()
    {
        InitJNI();
        int stopbits = 1;
        if (this.stopBits == StopBits.Two)
            stopbits = 2;
		int _parity = 0;
		if (this.parity == Parity.Odd)
			_parity = 1;
		else if (this.parity == Parity.Even)
			_parity = 2;
		jo.Call(strOpenMethod, portName, baudRate, _parity, dataBits, stopbits);
    }

    public void Close()
    {
        jo.Call(strCloseMethod);
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
#endif
}
