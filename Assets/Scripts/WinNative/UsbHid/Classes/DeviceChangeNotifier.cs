using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using UsbHid;
using UsbHid.USB.Classes;
using UsbHid.USB.Classes.DllWrappers;
using UsbHid.USB.Classes.Messaging;
using UsbHid.USB.Structures;

public class DeviceChangeNotifier : MonoBehaviour
{
    public delegate void DeviceAttachedDelegate();
    public static event DeviceAttachedDelegate DeviceAttached;

    public delegate void DeviceDetachedDelegate();
    public static event DeviceDetachedDelegate DeviceDetached;

    public IntPtr DeviceNotificationHandle;
	
    private static string devicepath;
	private UsbHidDevice device;

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

		hMainWindow = GetActiveWindow();
		newWndProc = new WndProcDelegate(WndProc);
		newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
		oldWndProcPtr = SetWindowLong(hMainWindow, -4, newWndProcPtr);
		device = new UsbHidDevice(0x0483, 0x5750);
		device.DataReceived += DeviceDataReceived;
		print("device.Connect: " + device.Connect());
		isrunning = true;
    }

	IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
	{
		// Trap WM_DEVICECHANGE
		if (msg == Constants.WmDevicechange)
		{
			print("IsNotificationForTargetDevice msg:" + msg + ", wParam:" + wParam.ToInt32() + ", lParam:" + lParam.ToInt32());
			if (IsNotificationForTargetDevice(wParam, lParam))
				HandleDeviceNotificationMessages(msg, wParam, lParam);
		}
		return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
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
		DeviceAttached = null;
		DeviceDetached = null;
		isrunning = false;
	}

    public static void Go(string devicePath)
    {
        devicepath = devicePath;
    }
    
    public static void Stop()
    {
		devicepath = string.Empty;
    }

	public static bool IsNotificationForTargetDevice(IntPtr wParam, IntPtr lParam)
    {
        if (string.IsNullOrEmpty(devicepath)) return false;

        try
        {
            var devBroadcastDeviceInterface = new DevBroadcastDeviceinterface1();
            var devBroadcastHeader = new DevBroadcastHdr();

            try
            {
				Marshal.PtrToStructure(lParam, devBroadcastHeader);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }

            // Is the notification event concerning a device interface?
            if ((devBroadcastHeader.dbch_devicetype == Constants.DbtDevtypDeviceinterface))
            {
                // Get the device path name of the affected device
                var stringSize = Convert.ToInt32((devBroadcastHeader.dbch_size - 32) / 2);
                devBroadcastDeviceInterface.dbcc_name = new Char[stringSize + 1];
                Marshal.PtrToStructure(lParam, devBroadcastDeviceInterface);
                var deviceNameString = new string(devBroadcastDeviceInterface.dbcc_name, 0, stringSize);
                // Compare the device name with our target device's pathname (strings are moved to lower case
                return (string.Compare(deviceNameString.ToLower(), devicepath.ToLower(), StringComparison.OrdinalIgnoreCase) == 0);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            return false;
        }
        return false;
    }

	public void HandleDeviceNotificationMessages(uint msg, IntPtr wParam, IntPtr lParam)
	{
		// Make sure this is a device notification
		if (msg != Constants.WmDevicechange) return;

		switch (wParam.ToInt32())
		{
			// Device attached
			case Constants.DbtDevicearrival:
				Debug.Log("HandleDeviceNotificationMessages() -> Device attached");
				ReportDeviceAttached();
				break;
				
				// Device removed
			case Constants.DbtDeviceremovecomplete:
				Debug.Log("HandleDeviceNotificationMessages() -> Device removed");
				ReportDeviceDetached();
				break;
				
				// Other message
			default:
				Debug.Log("HandleDeviceNotificationMessages() -> Unknown notification message");
				break;
		}
	}

    private void ReportDeviceDetached()
    {
        if (DeviceDetached != null) DeviceDetached();
    }

    private void ReportDeviceAttached()
    {
        if (DeviceAttached != null) DeviceAttached();
    }

	private void DeviceDataReceived(byte[] data)
	{
		string log =  "";
		foreach (byte b in data)
			log += b.ToString() + ", ";
		print(log);
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 200, 150), "Send Data"))
		{
			byte[] data = new byte[] { 0x58, 0x57, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0};
			var command = new CommandMessage(0x86, data);
			print(device.SendMessage(command));
		}
	}
}
