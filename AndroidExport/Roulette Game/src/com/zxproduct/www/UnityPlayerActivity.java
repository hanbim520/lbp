package com.zxproduct.www;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Queue;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.security.InvalidParameterException;

import com.unity3d.player.*;

import android.app.Activity;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.hardware.usb.UsbConstants;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbEndpoint;
import android.hardware.usb.UsbInterface;
import android.hardware.usb.UsbManager;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.widget.Toast;
import android.content.Context;
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
	}

	// Quit Unity
	@Override protected void onDestroy ()
	{
		mUnityPlayer.quit();
		super.onDestroy();
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
//	 private UsbReadThread usbReadThread;
	 
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
						int count = mDeviceConnection.bulkTransfer(epIntEndpointIn, buffer, buffer.length, 2000);
						if (count > 0)
						{
							BufferStruct buf = new BufferStruct();
							buf.buffer = new int[count];
							for (int i = 0; i < count; ++i)
							{
								buf.buffer[i] = buffer[i] & 0xff;
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
		 CallCSLog("java openUsb 1");
		 mUsbManager = (UsbManager) getSystemService(Context.USB_SERVICE);
		 if(mUsbManager == null)
			 return;
		 CallCSLog("java openUsb 2");
		// 枚举设备  
        enumerateDevice(mUsbManager);  
        // 查找设备接口  
        CallCSLog("java openUsb 3");
        UsbInterface usbInterface = getDeviceInterface(); 
        if (usbInterface != null)
        {
        	CallCSLog("java openUsb 4");
            // 获取设备endpoint  
            assignEndpoint(usbInterface);
            CallCSLog("java openUsb 5");
            // 打开conn连接通道  
            openDevice(usbInterface);  
            CallCSLog("java openUsb 6");
        }
        
        mTReadUsb0 = new TReadUsb0();
        CallCSLog("java openUsb 7");
        mTReadUsb0.start();
        CallCSLog("java openUsb 8");
	 }
	 
	 public void closeUsb()
	 {
		 if (mTReadUsb0 != null)
			 mTReadUsb0.interrupt();
	 }
	 
	 // 枚举设备函数  
	 private void enumerateDevice(UsbManager mUsbManager) 
	 {
		 HashMap<String, UsbDevice> deviceList = mUsbManager.getDeviceList();  
		 if (!(deviceList.isEmpty())) 
		 {
			 CallCSLog("deviceList is not null!");  
			 Iterator<UsbDevice> deviceIterator = deviceList.values().iterator();  
			 while (deviceIterator.hasNext()) 
			 {  
				 UsbDevice device = deviceIterator.next();  
				 // 输出设备信息  
				 CallCSLog("DeviceInfo: " + device.getVendorId() + " , "  
                        + device.getProductId());  
				 // 保存设备VID和PID  
				 int vendorID = device.getVendorId();  
				 int productID = device.getProductId();  
				 // 保存匹配到的设备  
				 if (vendorID == 0x0483 && productID == 0x5750)
				 {   
					 mUsbDevice = device; // 获取USBDevice  
	                 CallCSLog("发现待匹配设备:" + device.getVendorId()  
	                            + "," + device.getProductId());  
	             }
			 }
		 } 
		 else
		 {
			 CallCSLog("请连接USB设备至PAD！");  
	     }  
	}  

	// 寻找设备接口
	private UsbInterface getDeviceInterface()
	{
		CallCSLog("interfaceCounts : " + mUsbDevice.getInterfaceCount());
		for (int i = 0; i < mUsbDevice.getInterfaceCount(); i++)
		{
			UsbInterface intf = mUsbDevice.getInterface(i);
			
			if (i == 0)
			{
				CallCSLog("成功获得设备接口:" + intf.getId());
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
					CallCSLog("find the InterruptEndpointOut:"
							+ "index:" + i + ","
							+ epIntEndpointOut.getEndpointNumber());
				}
				if (ep.getDirection() == UsbConstants.USB_DIR_IN) 
				{
					epIntEndpointIn = ep;
					CallCSLog("find the InterruptEndpointIn:"
							+ "index:" + i + ","
							+ epIntEndpointIn.getEndpointNumber());
				}
			}
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
				CallCSLog("有连接权限");
				conn = mUsbManager.openDevice(mUsbDevice);
			}
			else
			{
				CallCSLog("没有连接权限");
			}

			if (conn == null)
			{
				CallCSLog("openDevice conn = null");
				return;
			}

			if (conn.claimInterface(usbInterface, true)) 
			{
				mDeviceConnection = conn;
				if (mDeviceConnection != null)// 到此你的android设备已经连上zigbee设备
					CallCSLog("open设备成功！");
				final String mySerial = mDeviceConnection.getSerial();
				CallCSLog("设备serial number：" + mySerial);
			} 
			else 
			{
				CallCSLog("无法打开连接通道。");
				conn.close();
			}
		}
	}
	
	public void openGate()
	{
		try
		{
			byte[] buffer = {0x58, 0x57, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78};
			int result = mDeviceConnection.bulkTransfer(epIntEndpointOut, buffer, buffer.length, 0);
			CallCSLog("bulkTransfer result:" + result);
			CallCSLog("java openGate");
		}
		catch(Exception ex)
		{
			CallCSLog(ex.getMessage());
		}
	}
	
	public void blowBall()
	{
		try
		{
			byte[] buffer = {0x58, 0x57, 0x03, 0x01, 0x00, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78,
					 0x02, 0x10, 0x02, 0x10, 0x78, 0x02, 0x10, 0x78};
			int result = mDeviceConnection.bulkTransfer(epIntEndpointOut, buffer, buffer.length, 0);
			CallCSLog("bulkTransfer result:" + result);
			CallCSLog("java blowBall");
		}
		catch(Exception ex)
		{
			CallCSLog(ex.getMessage());
		}
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
	
	public int[] readUsb0()
	{
		if (!readUsbQueue0.isEmpty())
		{
			BufferStruct buffer = readUsbQueue0.poll();
			return buffer.buffer;
		}
		// Can't return null, otherwise csharp side case exception.
	    return new int[]{-1};
	}

	public void CallCSLog(String msg)
	{
//		UnityPlayer.UnitySendMessage("Main Camera", "DebugLog", msg);
	}
}
