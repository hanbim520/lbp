﻿using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public static class WinHidPort
{
	// 传给金手指的数据，先通过它来加密。
	[DllImport ("EncryChip")]  
	private extern static int EncryptIOData(byte[] input, ushort inputSize, out IntPtr output);
	// 解密从金手指传回来的数据
	[DllImport ("EncryChip")]  
	private extern static int DecryptIOData(byte[] input, ushort inputSize, out IntPtr output);
	// 释放byte数组指针
	[DllImport ("EncryChip")]  
	private extern static void FreeByteArray(IntPtr array);

	[DllImport ("UsbWrapper")]  
	public extern static int OpenHid(int vid, int pid);
	[DllImport ("UsbWrapper")]  
	public extern static void CloseHid();
	[DllImport ("UsbWrapper")]  
	public extern static int WriteHid(byte[] data, int len);
	[DllImport ("UsbWrapper")]  
	public extern static void ReadHid(out IntPtr data, out int len);
	[DllImport ("UsbWrapper")]  
	public extern static void FreeArrayPtr(IntPtr ptr);

	public static bool bEncryptData = true;

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
			return WriteHid(dest, 63);
		}
		return WriteHid(data, len);
	}

	public static byte[] Read()
	{
		IntPtr data;
		int len;
		ReadHid(out data, out len);
		byte[] source = new byte[len];
		Marshal.Copy(data, source, 0, len);
		FreeArrayPtr(data);
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
		int ret = EncryptIOData(source, (ushort)sourceSize, out outputPtr);
		Marshal.Copy(outputPtr, dest, 0, dest.Length);
		FreeByteArray(outputPtr);
		return ret;
	}

	public static int DecryptOutputData(byte[] source, int sourceSize, ref byte[] dest)
	{
		IntPtr outputPtr;
		int ret = DecryptIOData(source, (ushort)sourceSize, out outputPtr);
		Marshal.Copy(outputPtr, dest, 0, dest.Length);
		FreeByteArray(outputPtr);
		return ret;
	}
}
