using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public static class EncryChipLib
{
	// 解密从金手指传回来的数据
	[DllImport ("EncryChip")]  
	private extern static int EncryptIOData(byte[] input, ushort inputSize, out IntPtr output);

	// 传给金手指的数据，先通过它来加密。
	[DllImport ("EncryChip")]  
	private extern static int DecryptIOData(byte[] input, ushort inputSize, out IntPtr output);

	// 释放byte数组指针
	[DllImport ("EncryChip")]  
	public extern static void FreeByteArray(IntPtr array);



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
