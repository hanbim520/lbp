using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRCUtils 
{
	// JCM 纸钞机通讯用的crc函数
	public static byte[] JCMCRC(byte[] data, int length, UInt32 seed)
	{
		UInt32 crcval = seed;
		UInt32 i, q, c;
		for (i = 0; i < length; ++i)
		{
			c = data[i] & 0xFFu;
			q = (crcval ^ c) & 0x0F;
			crcval = (crcval >> 4) ^ (q * 0x1081);
			q = (crcval ^ ( c >> 4)) & 0x0F;
			crcval = (crcval >> 4) ^ (q * 0x1081);
		}
		return new byte[] { (byte)(crcval & 0x00FF), (byte)((crcval & 0xFF00) >> 8) };
	}
}
