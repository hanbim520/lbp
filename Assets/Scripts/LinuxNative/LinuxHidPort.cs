using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public static class LinuxHidPort
{
	public static bool bEncryptData = true;

	[DllImport ("usb-1.0")]  
	private extern static int Linux_UsbPort_Open(int vid, int pid);
	[DllImport ("usb-1.0")]  
	private extern static void Linux_UsbPort_Close();
	[DllImport ("usb-1.0")]  
	private extern static int Linux_UsbPort_Read(out IntPtr data, out int len);
	[DllImport ("usb-1.0")]  
	private extern static int Linux_UsbPort_Write(byte[] data, int len);
	[DllImport ("usb-1.0")]  
	private extern static int Linux_UsbPort_FreePtr(IntPtr ptr);

	public static int Open(int vid, int pid)
	{
		return Linux_UsbPort_Open(vid, pid);
	}

	public static void Close()
	{
		Linux_UsbPort_Close();
	}

	public static int Write(ref byte[] data, int len)
	{
		if (bEncryptData)
		{
			byte[] dest = new byte[63];
			if (len > 61)
			{
				byte[] _data = new byte[61];
				for (int i = 0; i < 61; ++i)
				{
					_data[i] = data[i];
				}
				EncryptInputData(_data, 61, ref dest);
			}
			else if (len == 61)
				EncryptInputData(data, 61, ref dest);
			else
			{
				return -1;
			}
			return Linux_UsbPort_Write(dest, 63);
		}
		return Linux_UsbPort_Write(data, len);
	}

	public static byte[] Read()
	{
		IntPtr data;
		int len;
		Linux_UsbPort_Read(out data, out len);
		byte[] source = new byte[len];
		Marshal.Copy(data, source, 0, len);
		Linux_UsbPort_FreePtr(data);
		if (bEncryptData)
		{
			byte[] dest = new byte[len - 2];
			DecryptOutputData(source, len, ref dest);
			return dest;
		}
		return source;
	}

	public static int EncryptInputData(byte[] source, int sourceSize, ref byte[] dest)
	{
		IntPtr outputPtr;
		int ret = EncryChip.EncryptIOData(source, (ushort)sourceSize, out outputPtr);
		Marshal.Copy(outputPtr, dest, 0, dest.Length);
		EncryChip.FreeByteArray(outputPtr);
		return ret;
	}

	public static int DecryptOutputData(byte[] source, int sourceSize, ref byte[] dest)
	{
		IntPtr outputPtr;
		int ret = EncryChip.DecryptIOData(source, (ushort)sourceSize, out outputPtr);
		Marshal.Copy(outputPtr, dest, 0, dest.Length);
		EncryChip.FreeByteArray(outputPtr);
		return ret;
	}
}
