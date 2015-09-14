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
	}

    public static void UISpaceToScreenSpace(float ux, float uy, out float sx, out float sy)
    {
        float resolutionWidth = GameData.GetInstance().resolutionWidth;
        float resolutionHeight = GameData.GetInstance().resolutionHeight;
        sx = (ux + resolutionWidth * 0.5f) * Screen.width / resolutionWidth;
        sy = (uy + resolutionHeight * 0.5f) * Screen.height / resolutionHeight;
    }
}
