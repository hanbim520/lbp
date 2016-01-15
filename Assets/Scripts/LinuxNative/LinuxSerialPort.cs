using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public static class LinuxSerialPort
{
	/**************************************************************************************************
	函数名称：Com_Open()
	函数功能：打开串口
	函数说明：无
	入口参数：无
	出口参数：成功返回串口设备文件描述符,失败返回-1
	调用实例：无
	**************************************************************************************************/
	[DllImport ("uart-linux")] 
	private extern static int Com_Open();
	/**************************************************************************************************
	函数名称：Com_Setup
	函数功能：串口设定函数
	函数说明：无
	入口参数：fd:串口设备文件描述符
	           baud:比特率 300、600、1200、2400、4800、9600、19200、38400、57600、115200
	           databit:一个字节的数据位个数 5、6、7、8
	           stopbit:停止位个数1、2
	           parity:奇偶校验 0:无奇偶效验  1:奇效验  2:偶效验
	           flow：硬件流控制 0：无流控、 1：软件流控  2：硬件流控
	出口参数：失败返回-1,否则返回0
	调用实例：无
	**************************************************************************************************/
	[DllImport ("uart-linux")] 
	private extern static int Com_Setup(int fd, uint baud, int databit, int stopbit, int parity, int flow);
	/**************************************************************************************************
	函数名称：Com_Close()
	函数功能：关闭串口
	函数说明：无
	入口参数：fd:串口设备文件描述符
	出口参数：失败返回-1,否则返回0
	调用实例：无
	**************************************************************************************************/
	[DllImport ("uart-linux")] 
	private extern static int Com_Close(int fd);
	[DllImport ("uart-linux")] 
	private extern static int Com_Read(int fd, out IntPtr readBuffer);
	[DllImport ("uart-linux")] 
	private extern static int Com_Write(int fd, byte[] writeBuffer, int writeSize);
	[DllImport ("uart-linux")]  
	public extern static void FreeArrayPtr(IntPtr ptr);

	public static int fd
	{
		get { return _fd; }
		set { _fd = value; }
	}
	private static int _fd = -1;

	// return: 0:success -1:failed
	public static int Open(uint baud, int databit, int stopbit, int parity, int flow)
	{
		fd = Com_Open();
		if (fd == -1)
		{
			return -1;
		}
		Debug.Log("LinuxSerialPort Open fd:" + fd);
		return Com_Setup(fd, baud, databit, stopbit, parity, flow);
	}

	// return: 0:success -1:failed
	public static int Close()
	{
		int ret = -1;
		if (fd >= 0)
		{
			ret = Com_Close(fd);
			if (ret == 0)
			{
				Debug.Log("LinuxSerialPort Close fd:" + fd);
				fd = -1;
			}
			else
			{
				Debug.Log("LinuxSerialPort Close ret:" + ret);
			}
		}
		else
		{
			Debug.Log("LinuxSerialPort Close fd < 0");
		}
		return ret;
	}

	public static bool IsOpen()
	{
		return fd >= 0;
	}

	public static int Write(ref byte[] writeBuffer, int writeSize)
	{
		int ret = 0;
		if (fd >= 0)
		{
			ret = Com_Write(fd, writeBuffer, writeSize);
		}
		return ret;
	}

	public static byte[] Read()
	{
		IntPtr data;
		int len = Com_Read(fd, out data);
		if (len <= 0)
			return new byte[0];
		byte[] source = new byte[len];
		Marshal.Copy(data, source, 0, len);
		FreeArrayPtr(data);
		return source;
	}
}
