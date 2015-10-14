package com.zxproduct.www;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Queue;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.security.InvalidParameterException;

import com.unity3d.player.*;

import android.app.Activity;
import android.app.Instrumentation;
import android.app.PendingIntent;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.hardware.usb.UsbConstants;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbEndpoint;
import android.hardware.usb.UsbInterface;
import android.hardware.usb.UsbManager;
import android.os.Bundle;
import android.os.Message;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.widget.Toast;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android_serialport_api.SerialPort;
import android_serialport_api.SerialPortFinder;

public class UnityPlayerActivity extends Activity
{
	protected UnityPlayer mUnityPlayer; // don't change the name of this variable; referenced from native code
	private String TAG = "Unity";

	// Setup activity layout
	@Override protected void onCreate (Bundle savedInstanceState)
	{
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		super.onCreate(savedInstanceState);

		getWindow().setFormat(PixelFormat.RGBX_8888); // <--- This makes xperia play happy

		mUnityPlayer = new UnityPlayer(this);
		if (mUnityPlayer.getSettings().getBoolean("hide_status_bar", true))
		{
			getWindow ().setFlags (WindowManager.LayoutParams.FLAG_FULLSCREEN,
			                       WindowManager.LayoutParams.FLAG_FULLSCREEN);
		}

		setContentView(mUnityPlayer);
		mUnityPlayer.requestFocus();
		detectHIDViaTimer();
	}

	// Quit Unity
	@Override protected void onDestroy ()
	{
		mUnityPlayer.quit();
		super.onDestroy();
		closeUsb();
	}

	// Pause Unity
	@Override protected void onPause()
	{
		super.onPause();
		mUnityPlayer.pause();
	}

	// Resume Unity
	@Override protected void onResume()
	{
		super.onResume();
		mUnityPlayer.resume();
	}

	// This ensures the layout will be correct.
	@Override public void onConfigurationChanged(Configuration newConfig)
	{
		super.onConfigurationChanged(newConfig);
		mUnityPlayer.configurationChanged(newConfig);
	}

	// Notify Unity of the focus change.
	@Override public void onWindowFocusChanged(boolean hasFocus)
	{
		super.onWindowFocusChanged(hasFocus);
		mUnityPlayer.windowFocusChanged(hasFocus);
	}

	// For some reason the multiple keyevent type is not supported by the ndk.
	// Force event injection by overriding dispatchKeyEvent().
	@Override public boolean dispatchKeyEvent(KeyEvent event)
	{
		if (event.getAction() == KeyEvent.ACTION_MULTIPLE)
			return mUnityPlayer.injectEvent(event);
		return super.dispatchKeyEvent(event);
	}

	// Pass any events not handled by (unfocused) views straight to UnityPlayer
	@Override public boolean onKeyUp(int keyCode, KeyEvent event)     { return mUnityPlayer.injectEvent(event); }
	@Override public boolean onKeyDown(int keyCode, KeyEvent event)   { return mUnityPlayer.injectEvent(event); }
	@Override public boolean onTouchEvent(MotionEvent event)          { return mUnityPlayer.injectEvent(event); }
	/*API12*/ public boolean onGenericMotionEvent(MotionEvent event)  { return mUnityPlayer.injectEvent(event); }
	
	
	private SerialPort mSerialPort0 = null;
	private SerialPort mSerialPort1 = null;
	private SerialPort mSerialPort2 = null;
	private SerialPort mSerialPort3 = null;
	
	private OutputStream mOutputStream0 = null;
	private OutputStream mOutputStream1 = null;
	private OutputStream mOutputStream2 = null;
	private OutputStream mOutputStream3 = null;
	private InputStream mInputStream0 = null;
	private InputStream mInputStream1 = null;
	private InputStream mInputStream2 = null;
	private InputStream mInputStream3 = null;
	private ReadThread0 mReadThread0 = null;
	private ReadThread1 mReadThread1 = null;
	private ReadThread2 mReadThread2 = null;
	private ReadThread3 mReadThread3 = null;
	private SendThread0 mSendThread0 = null;
	private SendThread1 mSendThread1 = null;
	private SendThread2 mSendThread2 = null;
	private SendThread3 mSendThread3 = null;
	
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue1 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue2 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue3 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeSerialQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeSerialQueue1 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeSerialQueue2 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeSerialQueue3 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> readUsbQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
//	private ConcurrentLinkedQueue<BufferStruct> writeUsbQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	
	private class BufferStruct
	{
		public int[] buffer;
	}
	
	private class ReadThread0 extends Thread 
	{	
		@Override
		public void run()
		{
			super.run();
			while(!isInterrupted()) 
			{
				try
				{
					if (mInputStream0 != null) 
					{
						byte[] buffer = new byte[128];
						int size = mInputStream0.read(buffer);
						if (size > 0)
						{
							BufferStruct buf = new BufferStruct();
							int count = buffer.length;
							buf.buffer = new int[count];
							for (int i = 0; i < count; ++i)
							{
								buf.buffer[i] = buffer[i] & 0xff;
							}
							readSerialQueue0.offer(buf);
						}
					}
				}
				catch (Exception e)
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	private class ReadThread1 extends Thread 
	{	
		@Override
		public void run()
		{
			super.run();
			while(!isInterrupted()) 
			{
				try
				{
					if (mInputStream1 != null) 
					{
						byte[] buffer = new byte[128];
						int size = mInputStream1.read(buffer);
						if (size > 0)
						{
							BufferStruct buf = new BufferStruct();
							int count = buffer.length;
							buf.buffer = new int[count];
							for (int i = 0; i < count; ++i)
							{
								buf.buffer[i] = buffer[i] & 0xff;
							}
							readSerialQueue1.offer(buf);
						}
					}
				}
				catch (Exception e)
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	private class ReadThread2 extends Thread 
	{	
		@Override
		public void run()
		{
			super.run();
			while(!isInterrupted()) 
			{
				try
				{
					if (mInputStream2 != null) 
					{
						byte[] buffer = new byte[128];
						int size = mInputStream2.read(buffer);
						if (size > 0)
						{
							BufferStruct buf = new BufferStruct();
							int count = buffer.length;
							buf.buffer = new int[count];
							for (int i = 0; i < count; ++i)
							{
								buf.buffer[i] = buffer[i] & 0xff;
							}
							readSerialQueue2.offer(buf);
						}
					}
				}
				catch (Exception e)
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	private class ReadThread3 extends Thread 
	{	
		@Override
		public void run()
		{
			super.run();
			while(!isInterrupted()) 
			{
				try
				{
					if (mInputStream3 != null) 
					{
						byte[] buffer = new byte[128];
						int size = mInputStream3.read(buffer);
						if (size > 0)
						{
							BufferStruct buf = new BufferStruct();
							buf.buffer = new int[size];
							for (int i = 0; i < size; ++i)
							{
								buf.buffer[i] = buffer[i] & 0xff;
							}
							readSerialQueue3.offer(buf);
						}
					}
				}
				catch (Exception e)
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	private class SendThread0 extends Thread 
	{
		@Override
		public void run()
		{
			super.run();
			while (!isInterrupted())
			{
				try
				{
					if (!writeSerialQueue0.isEmpty() && mOutputStream0 != null)
					{
						BufferStruct buffer = writeSerialQueue0.poll();
						int count = buffer.buffer.length;
						byte[] buf = new byte[count];
						for (int i = 0; i < count; ++i)
						{
							buf[i] = (byte)buffer.buffer[i];
						}
						mOutputStream0.write(buf);
					}
				}
				catch (Exception e) 
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	private class SendThread1 extends Thread
	{
		@Override
		public void run()
		{
			super.run();
			while (!isInterrupted())
			{
				try
				{
					if (!writeSerialQueue1.isEmpty() && mOutputStream1 != null)
					{
						BufferStruct buffer = writeSerialQueue1.poll();
						int count = buffer.buffer.length;
						byte[] buf = new byte[count];
						for (int i = 0; i < count; ++i)
						{
							buf[i] = (byte)buffer.buffer[i];
						}
						mOutputStream1.write(buf);
					}
				}
				catch (Exception e) 
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	private class SendThread2 extends Thread
	{
		@Override
		public void run()
		{
			super.run();
			while (!isInterrupted())
			{
				try
				{
					if (!writeSerialQueue2.isEmpty() && mOutputStream2 != null)
					{
						BufferStruct buffer = writeSerialQueue2.poll();
						int count = buffer.buffer.length;
						byte[] buf = new byte[count];
						for (int i = 0; i < count; ++i)
						{
							buf[i] = (byte)buffer.buffer[i];
						}
						mOutputStream2.write(buf);
					}
				}
				catch (Exception e) 
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	private class SendThread3 extends Thread
	{
		@Override
		public void run()
		{
			super.run();
			while (!isInterrupted())
			{
				try
				{
					if (!writeSerialQueue3.isEmpty() && mOutputStream3 != null)
					{
						BufferStruct buffer = writeSerialQueue3.poll();
						int count = buffer.buffer.length;
						byte[] buf = new byte[count];
						for (int i = 0; i < count; ++i)
						{
							buf[i] = (byte)buffer.buffer[i];
						}
						mOutputStream3.write(buf);
					}
				}
				catch (Exception e) 
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	public void openSerialPort(int baudrate)
	{
		try
		{
			mSerialPort0 = new SerialPort(new File("/dev/ttyS0"), baudrate, 0);
			mOutputStream0 = mSerialPort0.getOutputStream();
			mInputStream0 = mSerialPort0.getInputStream();
			
			mSerialPort1 = new SerialPort(new File("/dev/ttyS1"), baudrate, 0);
			mOutputStream1 = mSerialPort1.getOutputStream();
			mInputStream1 = mSerialPort1.getInputStream();
			
			mSerialPort2 = new SerialPort(new File("/dev/ttyS2"), baudrate, 0);
			mOutputStream2 = mSerialPort2.getOutputStream();
			mInputStream2 = mSerialPort2.getInputStream();
			
			mSerialPort3 = new SerialPort(new File("/dev/ttyS3"), baudrate, 0);
			mOutputStream3 = mSerialPort3.getOutputStream();
			mInputStream3 = mSerialPort3.getInputStream();
			
			mReadThread0 = new ReadThread0();
			mReadThread0.start();
			mReadThread1 = new ReadThread1();
			mReadThread1.start();
			mReadThread2 = new ReadThread2();
			mReadThread2.start();
			mReadThread3 = new ReadThread3();
			mReadThread3.start();
			
			mSendThread0 = new SendThread0();
			mSendThread0.start();
			mSendThread1 = new SendThread1();
			mSendThread1.start();
			mSendThread2 = new SendThread2();
			mSendThread2.start();
			mSendThread3 = new SendThread3();
			mSendThread3.start();
		}
		catch (Exception e)
		{
			e.printStackTrace();
		}
	}

	public void closeSerialPort()
	{
		if (mSerialPort0 != null)
		{
			mSerialPort0.close();
			mSerialPort0 = null;
		}
		if (mSerialPort1 != null)
		{
			mSerialPort1.close();
			mSerialPort1 = null;
		}
		if (mSerialPort2 != null)
		{
			mSerialPort2.close();
			mSerialPort2 = null;
		}
		if (mSerialPort3 != null)
		{
			mSerialPort3.close();
			mSerialPort3 = null;
		}
		
		if (mReadThread0 != null)
			mReadThread0.interrupt();
		if (mReadThread1 != null)
			mReadThread1.interrupt();
		if (mReadThread2 != null)
			mReadThread2.interrupt();
		if (mReadThread3 != null)
			mReadThread3.interrupt();
		if (mSendThread0 != null)
			mSendThread0.interrupt();
		if (mSendThread1 != null)
			mSendThread1.interrupt();
		if (mSendThread2 != null)
			mSendThread2.interrupt();
		if (mSendThread3 != null)
			mSendThread3.interrupt();
	}

	 public int[] readSerialPort(int queueIdx)
	 {
		 if (queueIdx == 0)
		 {
			 if (!readSerialQueue0.isEmpty())
			 {
				 BufferStruct buffer = readSerialQueue0.poll();
				 return buffer.buffer;
			 }
		 }
		 else if (queueIdx == 1)
		 {
			 if (!readSerialQueue1.isEmpty())
			 {
				 BufferStruct buffer = readSerialQueue1.poll();
				 return buffer.buffer;
			 }
		 }
		 else if (queueIdx == 2)
		 {
			 if (!readSerialQueue2.isEmpty())
			 {
				 BufferStruct buffer = readSerialQueue2.poll();
				 return buffer.buffer;
			 }
		 }
		 else if (queueIdx == 3)
		 {
			 if (!readSerialQueue3.isEmpty())
			 {
				 BufferStruct buffer = readSerialQueue3.poll();
				 return buffer.buffer;
			 }
		 }
		 
		 // Can't return null, otherwise csharp side case exception.
	     return new int[]{-1};
	 }
	 
	 public boolean writeSerialPort(int[] data, int queueIdx)
	 {
		 BufferStruct buffer = new BufferStruct();
		 buffer.buffer = data;
		 if (queueIdx == 0)
			 return writeSerialQueue0.offer(buffer);
		 else if (queueIdx == 1)
			 return writeSerialQueue1.offer(buffer);
		 else if (queueIdx == 2)
			 return writeSerialQueue2.offer(buffer);
		 else if (queueIdx == 3)
			 return writeSerialQueue3.offer(buffer);
		 return false;
	 }
	 
	 private UsbManager mUsbManager = null;
	 private UsbEndpoint epIntEndpointOut;
	 private UsbEndpoint epIntEndpointIn;
	 private UsbDevice mUsbDevice;
	 private UsbDeviceConnection mDeviceConnection;
	 private TReadUsb0 mTReadUsb0 = null;
	 private final long detectHIDIntervalInMs = 2000;
	 private final int ft232rUartVid = 0x0483;
	 private final int ft232rUartPid = 0x5750;
	 private boolean bHIDConnected = false;
	 private boolean bHIDConnecting = false;
	 
	private class TReadUsb0 extends Thread
	{
		@Override
		public void run()
		{
			super.run();
			while (!isInterrupted())
			{
				try
				{
					if (mDeviceConnection != null && epIntEndpointIn != null)
					{
						byte[] buffer = new byte[64];
						for (int i = 0; i < buffer.length; ++i)
							buffer[i] = 0;
						int count = mDeviceConnection.bulkTransfer(epIntEndpointIn, buffer, buffer.length, 0);
						if (count > 0)
						{
							BufferStruct buf = new BufferStruct();
							buf.buffer = new int[count];
							for (int i = 0; i < count; ++i)
							{
								buf.buffer[i] = buffer[i] & 0xff;
							}
							while (readUsbQueue0.size() > 5)
							{
								readUsbQueue0.poll();
							}
							readUsbQueue0.offer(buf);
						}
					}
				}
				catch(Exception e)
				{
					e.printStackTrace();
				}
			}
		}
	}
	 
	 public void openUsb()
	 {
		 bHIDConnecting = true;
		 mUsbManager = (UsbManager) getSystemService(Context.USB_SERVICE);
		 if(mUsbManager == null)
			 return;
		 // 枚举设备  
         enumerateDevice(mUsbManager);  
         // 查找设备接口  
         UsbInterface usbInterface = getDeviceInterface(); 
         if (usbInterface != null)
         {
             // 获取设备endpoint  
             assignEndpoint(usbInterface);
             // 打开conn连接通道  
             openDevice(usbInterface);  
         }
        
         mTReadUsb0 = new TReadUsb0();
         mTReadUsb0.start();
         bHIDConnecting = false;
	 }
	 
	 public void closeUsb()
	 {
		 if (mTReadUsb0 != null && mTReadUsb0.isAlive())
		 {
			 mTReadUsb0.stop();
			 mTReadUsb0 = null;
		 }
	 }
	 
	 // 枚举设备函数  
	 private void enumerateDevice(UsbManager mUsbManager) 
	 {
		 HashMap<String, UsbDevice> deviceList = mUsbManager.getDeviceList();  
		 if (!(deviceList.isEmpty())) 
		 {
			 Iterator<UsbDevice> deviceIterator = deviceList.values().iterator();  
			 while (deviceIterator.hasNext()) 
			 {  
				 UsbDevice device = deviceIterator.next();
				 // 保存设备VID和PID  
				 int vendorID = device.getVendorId();  
				 int productID = device.getProductId();  
				 // 保存匹配到的设备  
				 if (vendorID == ft232rUartVid && productID == ft232rUartPid)
				 {   
					 mUsbDevice = device; // 获取USBDevice  
	             }
			 }
		 } 
	}  

	// 寻找设备接口
	private UsbInterface getDeviceInterface()
	{
		for (int i = 0; i < mUsbDevice.getInterfaceCount(); i++)
		{
			UsbInterface intf = mUsbDevice.getInterface(i);
			
			if (i == 0)
			{
				return intf; // 保存设备接口
			}
		}
		return null;
	}
	
	// 分配端点，IN | OUT，即输入输出；可以通过判断
	private void assignEndpoint(UsbInterface usbInterface)
	{
		for (int i = 0; i < usbInterface.getEndpointCount(); i++)
		{
			UsbEndpoint ep = usbInterface.getEndpoint(i);
			// look for interrupte endpoint
			if (ep.getType() == UsbConstants.USB_ENDPOINT_XFER_INT)
			{
				if (ep.getDirection() == UsbConstants.USB_DIR_OUT)
				{
					epIntEndpointOut = ep;
				}
				if (ep.getDirection() == UsbConstants.USB_DIR_IN) 
				{
					epIntEndpointIn = ep;
				}
			}
		}
	}
	
	// 模拟键盘按键，Keycode对应Android键盘按键的的keycode
	public void setKeyPress(int keycode){
	        try
	        {
	            String keyCommand = "input keyevent " + keycode;
	            Runtime runtime = Runtime.getRuntime();
	            Process proc = runtime.exec(keyCommand);
	        }
	        catch (IOException e)
	        {
	            e.printStackTrace();
	        }
	    }
	
	// 打开设备
	public void openDevice(UsbInterface usbInterface)
	{
		if (usbInterface != null)
		{
			UsbDeviceConnection conn = null;
			// 在open前判断是否有连接权限；对于连接权限可以静态分配，也可以动态分配权限
			if (mUsbManager.hasPermission(mUsbDevice)) 
			{
				conn = mUsbManager.openDevice(mUsbDevice);
			}
			else
			{
				CallCSLog("没有权限");
				String ACTION_USB_PERMISSION = "android.hardware.usb.action.USB_DEVICE_ATTACHED";
				PendingIntent localPendingIntent = PendingIntent.getBroadcast(this, 0, new Intent(ACTION_USB_PERMISSION), 0);
				mUsbManager.requestPermission(mUsbDevice, localPendingIntent);
				return;
			}

			if (conn == null)
			{
				return;
			}

			if (conn.claimInterface(usbInterface, true)) 
			{
				mDeviceConnection = conn;
				if (mDeviceConnection != null)// 到此你的android设备已经连上zigbee设备
				{
					bHIDConnected = true;
					UnityPlayer.UnitySendMessage("HIDUtils", "SetState", "True");
					OpenGate();
				}
			} 
			else 
			{
				conn.close();
			}
		}
	}
	
	public void OpenGate()
	{
		int[] buffer = new int[]{0x58, 0x57, 0x02, 0x0E, 0xA6, 0, 0, 0, 
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0};
		writeUsbPort(buffer);
	}
	
	public int writeUsbPort(int[] buffer)
	{
		int count = buffer.length;
		byte[] buf = new byte[count];
		for (int i = 0; i < count; ++i)
		{
			buf[i] = (byte)buffer[i];
		}
		return mDeviceConnection.bulkTransfer(epIntEndpointOut, buf, buf.length, 0);
	}
	
	public int[] readHID()
	{
		if (!readUsbQueue0.isEmpty())
		{
			BufferStruct buffer = readUsbQueue0.poll();
			return buffer.buffer;
		}
		// Can't return null, otherwise csharp side case exception.
	    return new int[]{-1};
	}
	
    public void detectHIDViaTimer(){
        final Timer detectTimer = new Timer();
        TimerTask detectTimerTask = new TimerTask() {
        	@Override
            public void run() {
            	if (bHIDConnected)
            		return;
            	
            	boolean bfound = false;
                UsbManager manager = (UsbManager) getSystemService(Context.USB_SERVICE);
                HashMap<String, UsbDevice> deviceList = manager.getDeviceList();
                Iterator<UsbDevice> deviceIterator = deviceList.values().iterator();
                while(deviceIterator.hasNext()){
                    UsbDevice device = deviceIterator.next();
                    int usbVid = device.getVendorId();
                    int usbPid = device.getProductId();
                    if((usbVid == ft232rUartVid) && (usbPid == ft232rUartPid) ){
                    	bfound = true;
//                    	CallCSLog("HID 插入:" + usbVid);
                    	if (!bHIDConnected && !bHIDConnecting) {
                    		openUsb();
                    	}
                    }
                }
                if (!bfound) {
                	bHIDConnected = false;
                	UnityPlayer.UnitySendMessage("HIDUtils", "SetState", "False");
                	closeUsb();
                }
        }};
         
        detectTimer.scheduleAtFixedRate(detectTimerTask, 5000, detectHIDIntervalInMs);
    }
    
    public void CallCSLog(String msg)
	{
//		UnityPlayer.UnitySendMessage("Main Camera", "DebugLog", msg);
	}
    
    //  线号          机号          总利润              当前利润            算码次数 
    public native String GetPWCheckValue4(long LineID, long CilentID,  long  MaxProfit, long Profit, long CheckCount);
    
    static {
        System.loadLibrary("hello-jni");
    }
}
