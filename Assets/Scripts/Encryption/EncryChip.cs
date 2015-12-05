using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public static class EncryChip
{
	// 传给金手指的数据，先通过它来加密。
	[DllImport ("EncryChip")]  
	public extern static int EncryptIOData(byte[] input, ushort inputSize, out IntPtr output);
	// 解密从金手指传回来的数据
	[DllImport ("EncryChip")]  
	public extern static int DecryptIOData(byte[] input, ushort inputSize, out IntPtr output);
	// 释放byte数组指针
	[DllImport ("EncryChip")]  
	public extern static void FreeByteArray(IntPtr array);
	// 获取校验码，显示在屏幕上。
	[DllImport ("EncryChip")]  
	public extern static String GetPWCheckValue4(long LineID, long CilentID,  long  MaxProfit, long Profit, long CheckCount);
	/// <summary>
	/// 验证用户输入的码是否正确
	/// </summary>
	/// <returns>把返回的数组传给加密片.</returns>
	/// <param name="LineID">线号.</param>
	/// <param name="CilentID">机台号.</param>
	/// <param name="MaxProfit">总盈利.</param>
	/// <param name="Profit">当前盈利.</param>
	/// <param name="CheckCount">打码次数.</param>
	/// <param name="crc">校验码.</param>
	/// <param name="pwstring_in">用户输入.</param>
	[DllImport ("EncryChip")]  
	public extern static byte[] CreateCheckPWString(long LineID, long CilentID, long MaxProfit, long Profit, long CheckCount, long crc, long pwstring_in);
	/// <summary>
	/// 解析加密片传回的打码结果
	/// </summary>
	/// <returns>解析的结果.</returns>
	/// <param name="recv_buff">加密片传回的数据.</param>
	[DllImport ("EncryChip")]  
	public extern static String GetCheckPWStringValue(byte[] recv_buff);

}
