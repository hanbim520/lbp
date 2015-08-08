using UnityEngine;
using System.Collections;
using System.Net.NetworkInformation;

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

	//http://answers.unity3d.com/questions/39394/send-an-email-c-error.html
	//http://forum.unity3d.com/threads/get-ip-address.100109/
	public static string GetIP()
	{
		string userIp = "";
		NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface adapter in adapters)
		{
			　　if (adapter.Supports(NetworkInterfaceComponent.IPv4))
			　　{
				　　　　UnicastIPAddressInformationCollection uniCast = adapter.GetIPProperties().UnicastAddresses;
				　　　　if (uniCast.Count > 0)
				　　　　{
					　　　　　　foreach (UnicastIPAddressInformation uni in uniCast)
					　　　　　　{
						　　　　　　　　//得到IPv4的地址。 AddressFamily.InterNetwork指的是IPv4
						　　　　　　　　if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
						　　　　　　　　{
							　　　　　　　　　　userIp =uni.Address.ToString();
						　　　　　　　　}
					　　　　　　}
				　　　　}
			　　}
		}
		return userIp;
	}
}
