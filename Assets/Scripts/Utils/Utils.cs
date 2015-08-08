using UnityEngine;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Collections;

public static class Utils
{
	public static byte[] StringToBytes(string str)
	{
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}
	
	public static string BytesToString(byte[] bytes)
	{
		char[] chars = new char[bytes.Length / sizeof(char)];
		System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		return new string(chars);
	}

	public static string GetIPv4()
	{
		string ipv4 = "";
		IPHostEntry hostInfo = Dns.GetHostByName(System.Net.Dns.GetHostName());
		foreach(IPAddress ip in hostInfo.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				ipv4 = ip.ToString();
			}
		}
		return ipv4;
	}

}
