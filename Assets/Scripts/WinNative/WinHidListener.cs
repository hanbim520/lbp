using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UsbHid.USB.Classes;

public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

public class WinHidListener : MonoBehaviour
{
	public IntPtr interactionWindow;
	IntPtr hMainWindow;
	IntPtr oldWndProcPtr;
	IntPtr newWndProcPtr;
	WndProcDelegate newWndProc;
	bool isrunning = false;

	[DllImport("user32.dll")]
	private static extern System.IntPtr GetActiveWindow();

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern System.IntPtr GetForegroundWindow();
	
	[DllImport("user32.dll")]
	static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
	
	[DllImport("user32.dll")]
	static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	
	void Start()
	{
		if (isrunning) return;

//		hMainWindow = GetForegroundWindow();
		hMainWindow = GetActiveWindow();
		newWndProc = new WndProcDelegate(WndProc);
		newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
		oldWndProcPtr = SetWindowLong(hMainWindow, -4, newWndProcPtr);
		print("openHid: " + WinHidPort.OpenHid(0x0483, 0x5750));

		byte[] data = new byte[61];
		data[0] = 0x58;
		data[1] = 0x57;


		byte[] dest = new byte[63];
		WinHidPort.EncryptInputData(data, 61, ref dest);
		string logg = "";
		for (int i = 0; i < dest.Length; ++i)
		{
			logg += dest[i].ToString() + ", ";
		}
		print("write data: " + logg);

		print("writeHid: " + WinHidPort.WriteHid(dest, 63));

		IntPtr input;
		int inputsize;
		WinHidPort.ReadHid(out input, out inputsize);
		byte[] source = new byte[61];
		byte[] source2 = new byte[59];
		Marshal.Copy(input, source, 0, 61);
		WinHidPort.DecryptOutputData(source, 61, ref source2);

//		WinHid.FreeByteArray(input);
		print("ReadHid: " + inputsize);
		string log = "";
		for (int i = 0; i < source2.Length; ++i)
			log += source2[i].ToString() + ", ";
		print("Receive Data: " + log);
		WinHidPort.CloseHid();
		isrunning = true;
	}
	
	private static IntPtr StructToPtr(object obj)
	{
		var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
		Marshal.StructureToPtr(obj, ptr, false);
		return ptr;
	}

	IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
	{
		// Trap WM_DEVICECHANGE
		if (msg == Constants.WmDevicechange)
		{
			DeviceChangeEvent(wParam, lParam);
		}
		return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
	}

	void DeviceChangeEvent(IntPtr wParam, IntPtr lParam)
	{

	}
	
	void OnDisable()
	{
		Debug.Log("Uninstall Hook");
		if (!isrunning) return;
		SetWindowLong(hMainWindow, -4, oldWndProcPtr);
		hMainWindow = IntPtr.Zero;
		oldWndProcPtr = IntPtr.Zero;
		newWndProcPtr = IntPtr.Zero;
		newWndProc = null;
		isrunning = false;
	}
}