using UnityEngine;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Collections;

public static class Utils
{
    public static string ToString<T>(ref T[] data)
    {
        string str = "";
        int len = data.Length;
        for (int i = 0; i < len; ++i)
        {
            str += string.Format("{0:X}, ", data[i]);
        }
        return str;
    }

	public static byte[] StringToBytes(string str)
	{
//		byte[] bytes = new byte[str.Length * sizeof(char)];
//		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
//		return bytes;
		return System.Text.ASCIIEncoding.Default.GetBytes(str);
	}
	
	public static string BytesToString(byte[] bytes)
	{
//		char[] chars = new char[bytes.Length / sizeof(char)];
//		System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
//		return new string(chars);
		return System.Text.ASCIIEncoding.Default.GetString(bytes);
	}

	public static string GetIPv4()
	{
		string ipv4 = "";
		IPHostEntry hostInfo = Dns.GetHostEntry(System.Net.Dns.GetHostName());
		foreach(IPAddress ip in hostInfo.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				ipv4 = ip.ToString();
			}
		}
		return ipv4;
	}

	public static void LEqations3x3(double[,] det, out double x, out double y, out double z)
	{
		x = y = z = 0;
		double[,] det0 = new double[3, 3];
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 3; j++) det0[i, j] = det[i, j];
		double det00 = determinant(det0);
		double[,] detx = new double[3, 3];
		detx[0, 0] = det[0, 3];
		detx[1, 0] = det[1, 3];
		detx[2, 0] = det[2, 3];
		for (int i = 0; i < 3; i++)
			for (int j = 1; j < 3; j++) detx[i, j] = det[i, j];
		double detx0 = determinant(detx);
		double[,] dety = new double[3, 3];
		dety[0, 1] = det[0, 3];
		dety[1, 1] = det[1, 3];
		dety[2, 1] = det[2, 3];
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 3; j++)
		{
			if (j != 1)
				dety[i, j] = det[i, j];
		}
		double dety0 = determinant(dety);
		double[,] detz = new double[3, 3];
		detz[0, 2] = det[0, 3];
		detz[1, 2] = det[1, 3];
		detz[2, 2] = det[2, 3];
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 2; j++) detz[i, j] = det[i, j];
		double detz0 = determinant(detz);
		x = detx0 / det00;
		y = dety0 / det00;
		z = detz0 / det00;
	}

	private static double determinant(double[,] x)//3×3行列式
	{
		return x[0, 0] * x[1, 1] * x[2, 2] + x[1, 0] * x[2, 1] * x[0, 2] + x[0, 1] * x[1, 2] * x[2, 0] - x[2, 0] * x[1, 1] * x[0, 2] - x[2, 1] * x[1, 2] * x[0, 0] - x[1, 0] * x[0, 1] * x[2, 2];
	}

	public static bool PointInRect(Vector2 point, RectTransform rt)
	{
		float xMax = rt.localPosition.x + rt.rect.xMax;
		float xMin = rt.localPosition.x + rt.rect.xMin;
		float yMax = rt.localPosition.y + rt.rect.yMax;
		float yMin = rt.localPosition.y + rt.rect.yMin;
		return point.x <= xMax && point.x >= xMin && point.y <= yMax && point.y >= yMin;
	}

	public static bool PointInRect(Vector2 point, RectTransform rt, float scaleX, float scaleY)
	{
		float xMax = rt.localPosition.x + rt.rect.xMax * scaleX;
		float xMin = rt.localPosition.x + rt.rect.xMin * scaleX;
		float yMax = rt.localPosition.y + rt.rect.yMax * scaleY;
		float yMin = rt.localPosition.y + rt.rect.yMin * scaleY;
		return point.x <= xMax && point.x >= xMin && point.y <= yMax && point.y >= yMin;
	}

	public static void TouchScreenToLCD(float tx, float ty, out float lx, out float ly)
	{
		lx = GameData.GetInstance().TA * tx + GameData.GetInstance().TB * ty + GameData.GetInstance().TC;
		ly = GameData.GetInstance().TD * tx + GameData.GetInstance().TE * ty + GameData.GetInstance().TF;
	}

	// Convert position from screen space to ugui space.
	public static void ScreenSpaceToUISpace(float sx, float sy, out float ux, out float uy)
	{
		float resolutionWidth = GameData.GetInstance().resolutionWidth;
		float resolutionHeight = GameData.GetInstance().resolutionHeight;
		ux = resolutionWidth / Screen.width * sx - resolutionWidth * 0.5f;
		uy = resolutionHeight / Screen.height * sy - resolutionHeight * 0.5f;
//		DebugConsole.Log("Screen.width:" + Screen.width + ", " + "Screen.height:" + Screen.height + ", " +
//		                 "sx:" + sx + ", " + "sy:" + sy + ", " +
//		                 "ux:" + ux + ", " + "uy:" + uy);
	}

    public static void UISpaceToScreenSpace(float ux, float uy, out float sx, out float sy)
    {
        float resolutionWidth = GameData.GetInstance().resolutionWidth;
        float resolutionHeight = GameData.GetInstance().resolutionHeight;
        sx = (ux + resolutionWidth * 0.5f) * Screen.width / resolutionWidth;
        sy = (uy + resolutionHeight * 0.5f) * Screen.height / resolutionHeight;
//		DebugConsole.Log("Screen.width:" + Screen.width + ", " + "Screen.height:" + Screen.height + ", " +
//		                 "ux:" + ux + ", " + "uy:" + uy + ", " +
//		                 "sx:" + sx + ", " + "sy:" + sy);
    }

    public static int IsBingo(string fieldName, int value)
    {
        if (string.Equals(fieldName, "Even"))
        {
            if (value % 2 == 0 && value != 0) return 2;
            else                return 0;

        }
        else if (string.Equals(fieldName, "odd"))
        {
            if (value % 2 == 1 && value != 37) return 2;
            else                return 0;
        }
        else if (string.Equals(fieldName, "red"))
        {
            if (GameData.GetInstance().colorTable[value] == ResultType.Red)
                return 2;
            else
                return 0;
        }
        else if (string.Equals(fieldName, "black"))
        {
            if (GameData.GetInstance().colorTable[value] == ResultType.Black)
                return 2;
            else
                return 0;
        }
        else if (string.Equals(fieldName, "1to18"))
        {
            if (value >= 1 && value <= 18)  return 2;
            else                            return 0;
        }
        else if (string.Equals(fieldName, "19to36"))
        {
            if (value >= 19 && value <= 36)  return 2;
            else                             return 0;
        }
        else if (string.Equals(fieldName, "1st12"))
        {
            if (value >= 1 && value <= 12)  return 3;
            else                            return 0;
        }
        else if (string.Equals(fieldName, "2nd12"))
        {
            if (value >= 13 && value <= 24)  return 3;
            else                             return 0;
        }
        else if (string.Equals(fieldName, "3rd12"))
        {
            if (value >= 25 && value <= 36)  return 3;
            else                             return 0;
        }
        else if (string.Equals(fieldName, "2to1 up"))
        {
            for (int i = 3; i <= 36; i += 3)
            {
                if (i == value)
                    return 3;
            }
            return 0;
        }
        else if (string.Equals(fieldName, "2to1 middle"))
        {
            for (int i = 2; i <= 35; i += 3)
            {
                if (i == value)
                    return 3;
            }
            return 0;
        }
        else if (string.Equals(fieldName, "2to1 down"))
        {
            for (int i = 1; i <= 34; i += 3)
            {
                if (i == value)
                    return 3;
            }
            return 0;
        }
        else
        {
            // Ellipse
            if (string.Equals(fieldName.Substring(0, 1), "e"))
            {
                string strField = fieldName.Substring(1);
                char[] separator = {'-'};
                string[] fields = strField.Split(separator);
                foreach(string f in fields)
                {
                    int v;
					if (string.Compare(f, "00") == 0)
					{
						v = 37;
						if (v == value)
							return 36;
					}
					else if (int.TryParse(f, out v))
                    {
                        if (v == value)
                            return 36;
                    }
                }
                return 0;
            }
            // Classic
			else
            {
                char[] separator = {'-'};
                string[] fields = fieldName.Split(separator);
                foreach(string f in fields)
                {
                    int v;
					if (string.Compare(f, "00") == 0)
					{
						v = 37;
						if (v == value)
							return 36;
					}
                    else if (int.TryParse(f, out v))
                    {
                        if (v == value)
							return 36 / fields.Length;
                    }
                }
                return 0;
            }
        }
    }

	public static int GetOdds(string fieldName)
	{
		int odds = 0;
		if (string.Equals(fieldName, "Even") ||
		    string.Equals(fieldName, "odd") ||
		    string.Equals(fieldName, "red") ||
		    string.Equals(fieldName, "black") ||
		    string.Equals(fieldName, "1to18") ||
		    string.Equals(fieldName, "19to36"))
		{
			odds = 2;
		}
		else if (string.Equals(fieldName, "1st12") ||
		         string.Equals(fieldName, "2nd12") ||
		         string.Equals(fieldName, "3rd12") ||
		         string.Equals(fieldName, "2to1 up") ||
		         string.Equals(fieldName, "2to1 middle") ||
		         string.Equals(fieldName, "2to1 down"))
		{
			odds = 3;
		}
		else
		{
			// Ellipse
			if (string.Equals(fieldName.Substring(0, 1), "e"))
				odds = 36;
			else
			{
				// Classic
				char[] separator = {'-'};
				string[] fields = fieldName.Split(separator);
				odds = 36 / fields.Length;
			}
		}
		return odds;
	}

	public static int GetMaxBet(string fieldName)
	{
		int maxBet = 0;
		int odds = GetOdds(fieldName);
		if (odds == 36)
			maxBet = GameData.GetInstance().max36Value;
		else if (odds == 18)
			maxBet = GameData.GetInstance().max18Value;
		else if (odds == 12)
			maxBet = GameData.GetInstance().max12Value;
		else if (odds == 9)
			maxBet = GameData.GetInstance().max9Value;
		else if (odds == 6)
			maxBet = GameData.GetInstance().max6Value;
		else if (odds == 3)
			maxBet = GameData.GetInstance().max3Value;
		else if (odds == 2)
			maxBet = GameData.GetInstance().max2Value;
		return maxBet;
	}

	public static int GetAllMaxBet(string fieldName)
	{
		int allMaxBet = 0;
		int odds = GetOdds(fieldName);
		if (odds == 36)
			allMaxBet = GameData.GetInstance().allMax36Val;
		else if (odds == 18)
			allMaxBet = GameData.GetInstance().allMax18Val;
		else if (odds == 12)
			allMaxBet = GameData.GetInstance().allMax12Val;
		else if (odds == 9)
			allMaxBet = GameData.GetInstance().allMax9Val;
		else if (odds == 6)
			allMaxBet = GameData.GetInstance().allMax6Val;
		else if (odds == 3)
			allMaxBet = GameData.GetInstance().allMax3Val;
		else if (odds == 2)
			allMaxBet = GameData.GetInstance().allMax2Val;
		return allMaxBet;
//		return int.MaxValue;
	}

	public static string GetSystemTime()
	{
		return DateTime.Now.Date.ToShortDateString() + " " + DateTime.Now.ToString("HH:mm:ss");
	}

	public static string GuidTo16String()  
	{  
		long baseNum = 1;
		byte[] array = System.Guid.NewGuid().ToByteArray();
		foreach (byte b in array)
			baseNum *= ((int)b + 1);  
		return string.Format("{0:x}", baseNum - System.DateTime.Now.Ticks);  
	} 

	/// <summary>
	/// 求num在n位上的数字,取个位,取十位
	/// </summary>
	/// <param name="num">正整数</param>
	/// <param name="n">所求数字位置(个位 1,十位 2 依此类推)</param>
	public static int FindNum(int num, int n)
	{
		int power = (int)Math.Pow(10, n);
		return (num - num / power * power) * 10 / power;
	}

	public static void SetSeed()
	{
		#if UNITY_STANDALONE_LINUX
			LinuxUtils.SetSeed();
		#endif

#if UNITY_ANDROID
		int seed = (int)System.DateTime.Now.ToUniversalTime().ToBinary();
		UnityEngine.Random.InitState(seed);
#endif
	}

    public static void Seed(int seed)
    {
		#if UNITY_STANDALONE_LINUX
        LinuxUtils.Seed(seed);
		#endif

#if UNITY_ANDROID
		UnityEngine.Random.InitState(seed);
#endif
    }

	// Returns a random integer number between min[inclusive] and max[exclusive]
	public static int GetRandom(int min, int max)
	{
		#if UNITY_STANDALONE_LINUX
		return LinuxUtils.GetRandom(min, max);
		#endif

#if UNITY_ANDROID
		return UnityEngine.Random.Range(min, max);
#endif
	}

	public static bool StringIsEquals(string strA, string strB)
	{
		return string.Compare(strA, strB) == 0;
	}

	// 返回：发给金手指的校验位
	public static int CrcAddXor(int[] array, int len) 
	{
		int crc2 = 0;
		for (int i = 0; i < len; ++i)
		{
			crc2 += array[i];
			if (crc2 > 255) crc2 = crc2 - 256;
		}
		
		crc2 ^= 0xff;
		crc2++;
		return crc2;
	}

	public static int GetNumLength(int num)
	{
		if (num == 0)
			return 1;

		int ret = 0;
		num = Mathf.Abs(num);
		while (num > 0)
		{
			++ret;
			num = num / 10;
		}
		return ret;
	}
}
