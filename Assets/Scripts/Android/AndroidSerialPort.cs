using UnityEngine;
using System;
using System.IO.Ports;
using System.Collections;
using System.Threading;

public class AndroidSerialPort
{
#if UNITY_ANDROID
	protected int portId;

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
	protected string strWriteMethod = "writeSerialPort";
	protected IntPtr writeMethodId = IntPtr.Zero;

	// Note: Java side should be return int[]
	public int[] ReadData()
	{
		if (Application.platform == RuntimePlatform.OSXEditor ||
			Application.platform == RuntimePlatform.WindowsEditor)
			return null;

		AndroidJavaObject readMethod = jo.Call<AndroidJavaObject>(strReadMethod, portId);
		return AndroidJNIHelper.ConvertFromJNIArray<int[]>(readMethod.GetRawObject());
	}

	public void WriteData(ref int[] data)
	{
		if (Application.platform == RuntimePlatform.OSXEditor ||
			Application.platform == RuntimePlatform.WindowsEditor)
			return;
		
		IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
		jvalue[] blah = new jvalue[2];
		blah[0].l = pArr;
		blah[1].i = portId;
		
		if (writeMethodId == IntPtr.Zero)
			writeMethodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), strWriteMethod);
		AndroidJNI.CallVoidMethod(jo.GetRawObject(), writeMethodId, blah);
		AndroidJNI.DeleteLocalRef(pArr);
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
		portId = jo.Call<int>(strOpenMethod, portName, baudRate, _parity, dataBits, stopbits);
    }

    public void Close()
    {
        if (jo != null)
        {
       		jo.Call(strCloseMethod);
            jo = null;
        }
        if (jc != null)
        {
            jc = null;
        }
    }

    protected void InitJNI()
    {
        jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
        jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
    }

	public int GetPortId()
	{
		return portId;
	}
#endif
}
