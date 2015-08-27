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
							synchronized(readSerialQueue0)
							{
								readSerialQueue0.offer(buf);
							}
						}
					}
				}
				catch (IOException e) 
				{
					e.printStackTrace();
					return;
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
							synchronized(readSerialQueue1)
							{
								readSerialQueue1.offer(buf);
							}
						}
					}
				}
				catch (IOException e) 
				{
					e.printStackTrace();
					return;
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
							synchronized(readSerialQueue2)
							{
								readSerialQueue2.offer(buf);
							}
						}
					}
				}
				catch (IOException e) 
				{
					e.printStackTrace();
					return;
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
							synchronized(readSerialQueue3)
							{
								readSerialQueue3.offer(buf);
							}
						}
					}
				}
				catch (IOException e) 
				{
					e.printStackTrace();
					return;
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
						synchronized(writeSerialQueue0)
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
				}
				catch (IOException e) 
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
						synchronized(writeSerialQueue1)
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
				}
				catch (IOException e) 
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
						synchronized(writeSerialQueue2)
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
				}
				catch (IOException e) 
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
						synchronized(writeSerialQueue3)
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
				}
				catch (IOException e) 
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
		catch (SecurityException e)
		{
			e.printStackTrace();
		}
		catch (IOException e)
		{
			e.printStackTrace();
		} 
	}

	public void closeSerialPort()
	{
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
	}

	 public int[] readSerialPort0()
	 {
		 if (!readSerialQueue0.isEmpty())
		 {
			 synchronized(readSerialQueue0)
			 {
				 BufferStruct buffer = readSerialQueue0.poll();
				 return buffer.buffer;
			 }
		 }
		 // Can't return null, otherwise csharp side case exception.
	     return new int[]{0};
	 }
	 
	 public int[] readSerialPort1()
	 {
		 if (!readSerialQueue1.isEmpty())
		 {
			 synchronized(readSerialQueue1)
			 {
				 BufferStruct buffer = readSerialQueue1.poll();
				 return buffer.buffer;
			 }
		 }
		// Can't return null, otherwise csharp side case exception.
		 return new int[]{0};
	 }
	 
	 public int[] readSerialPort2()
	 {
		 if (!readSerialQueue2.isEmpty())
		 {
			 synchronized(readSerialQueue2)
			 {
				 BufferStruct buffer = readSerialQueue2.poll();
				 return buffer.buffer;
			 }
		 }
		// Can't return null, otherwise csharp side case exception.
		 return new int[]{0};
	 }
	 
	 public int[] readSerialPort3()
	 {
		 if (!readSerialQueue3.isEmpty())
		 {
			 synchronized(readSerialQueue3)
			 {
				 BufferStruct buffer = readSerialQueue3.poll();
				 return buffer.buffer;
			 }
		 }
		// Can't return null, otherwise csharp side case exception.
		 return new int[]{-1};
	 }
	 
	 public boolean writeSerialPort0(int[] data)
	 {
		 synchronized(writeSerialQueue0)
		 {
			 BufferStruct buffer = new BufferStruct();
			 buffer.buffer = data;
			 for (int i = 0; i < buffer.buffer.length; ++i)
			 {
				 Log.i("Unity", "java write: " + buffer.buffer[i]);
			 }
			 return writeSerialQueue0.offer(buffer);
		 }
	 }
	 
	 public boolean writeSerialPort1(int[] data)
	 {
		 synchronized(writeSerialQueue1)
		 {
			 BufferStruct buffer = new BufferStruct();
			 buffer.buffer = data;
			 return writeSerialQueue1.offer(buffer);
		 }
	 }
	 
	 public boolean writeSerialPort2(int[] data)
	 {
		 synchronized(writeSerialQueue2)
		 {
			 BufferStruct buffer = new BufferStruct();
			 buffer.buffer = data;
			 return writeSerialQueue2.offer(buffer);
		 }
	 }
	 
	 public boolean writeSerialPort3(int[] data)
	 {
		 synchronized(writeSerialQueue3)
		 {
			 BufferStruct buffer = new BufferStruct();
			 buffer.buffer = data;
			 return writeSerialQueue3.offer(buffer);
		 }
	 }
	 
	 private final int kUsbReadTimeout = 150;
	 private UsbManager mUsbManager = null;
	 private UsbEndpoint epBulkOut;
	 private UsbEndpoint epBulkIn;
	 private UsbEndpoint epControl;
	 private UsbEndpoint epIntEndpointOut;
	 private UsbEndpoint epIntEndpointIn;
	 private UsbDevice mUsbDevice;
	 private UsbDeviceConnection mDeviceConnection;
	 private UsbInterface Interface1;
	 private UsbInterface Interface2;
	 private int ProductID;
	 private int VendorID;
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
					if (mDeviceConnection != null && epBulkIn != null)
					{
						byte[] buffer = new byte[128];
						int count = mDeviceConnection.bulkTransfer(epBulkIn, buffer, buffer.length, kUsbReadTimeout);
						if (count > 0)
						{
							BufferStruct buf = new BufferStruct();
							buf.buffer = new int[count];
							for (int i = 0; i < count; ++i)
							{
								buf.buffer[i] = buffer[i] & 0xff;
							}
							synchronized(readUsbQueue0)
							{
								readUsbQueue0.offer(buf);
							}
						}
					}
				}
				catch(Exception e)
				{
					e.printStackTrace();
					return;
				}
			}
		}
	}
	 
	 public void openUsb()
	 {
		 mUsbManager = (UsbManager) getSystemService(Context.USB_SERVICE);
		 if(mUsbManager == null)
			 return;
		 
		// 枚举设备  
        enumerateDevice(mUsbManager);  
        // 查找设备接口  
        getDeviceInterface();  
        // 获取设备endpoint  
        assignEndpoint(Interface2);  
        // 打开conn连接通道  
        openDevice(Interface2);  
        
        mTReadUsb0 = new TReadUsb0();
        mTReadUsb0.start();
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
			 System.out.println("deviceList is not null!");  
			 Iterator<UsbDevice> deviceIterator = deviceList.values().iterator();  
			 while (deviceIterator.hasNext()) 
			 {  
				 UsbDevice device = deviceIterator.next();  
				 // 输出设备信息  
				 Log.i(TAG, "DeviceInfo: " + device.getVendorId() + " , "  
                        + device.getProductId());  
				 // 保存设备VID和PID  
				 VendorID = device.getVendorId();  
				 ProductID = device.getProductId();  
				 // 保存匹配到的设备  
				 if (VendorID == 0x0483 && ProductID == 0x1000)
				 {   
					 mUsbDevice = device; // 获取USBDevice  
	                 System.out.println("发现待匹配设备:" + device.getVendorId()  
	                            + "," + device.getProductId());  
	                 Context context = getApplicationContext();  
	                 Toast.makeText(context, "发现待匹配设备", Toast.LENGTH_SHORT).show();  
	             }
			 }
		 } 
		 else
		 {
			 Context context = getApplicationContext();  
	         Toast.makeText(context, "请连接USB设备至PAD！", Toast.LENGTH_SHORT).show();  
	     }  
	}  

	// 寻找设备接口
	private void getDeviceInterface()
	{
		Log.d(TAG, "interfaceCounts : " + mUsbDevice.getInterfaceCount());
		for (int i = 0; i < mUsbDevice.getInterfaceCount(); i++)
		{
			UsbInterface intf = mUsbDevice.getInterface(i);
			
			if (i == 0)
			{
				Interface1 = intf; // 保存设备接口
				System.out.println("成功获得设备接口:" + Interface1.getId());
			}
			if (i == 1)
			{
				Interface2 = intf;
				System.out.println("成功获得设备接口:" + Interface2.getId());
			}
		}
	}
	
	// 分配端点，IN | OUT，即输入输出；可以通过判断
	private UsbEndpoint assignEndpoint(UsbInterface mInterface)
	{
		for (int i = 0; i < mInterface.getEndpointCount(); i++)
		{
			UsbEndpoint ep = mInterface.getEndpoint(i);
			// look for bulk endpoint
			if (ep.getType() == UsbConstants.USB_ENDPOINT_XFER_BULK)
			{
				if (ep.getDirection() == UsbConstants.USB_DIR_OUT) 
				{
					epBulkOut = ep;
					System.out.println("Find the BulkEndpointOut," + "index:"
							+ i + "," + "使用端点号："
							+ epBulkOut.getEndpointNumber());
				} 
				else
				{
					epBulkIn = ep;
					System.out
							.println("Find the BulkEndpointIn:" + "index:" + i
									+ "," + "使用端点号："
									+ epBulkIn.getEndpointNumber());
				}
			}
			// look for contorl endpoint
			if (ep.getType() == UsbConstants.USB_ENDPOINT_XFER_CONTROL)
			{
				epControl = ep;
				System.out.println("find the ControlEndPoint:" + "index:" + i
						+ "," + epControl.getEndpointNumber());
			}
			// look for interrupte endpoint
			if (ep.getType() == UsbConstants.USB_ENDPOINT_XFER_INT)
			{
				if (ep.getDirection() == UsbConstants.USB_DIR_OUT)
				{
					epIntEndpointOut = ep;
					System.out.println("find the InterruptEndpointOut:"
							+ "index:" + i + ","
							+ epIntEndpointOut.getEndpointNumber());
				}
				if (ep.getDirection() == UsbConstants.USB_DIR_IN) 
				{
					epIntEndpointIn = ep;
					System.out.println("find the InterruptEndpointIn:"
							+ "index:" + i + ","
							+ epIntEndpointIn.getEndpointNumber());
				}
			}
		}
		if (epBulkOut == null && epBulkIn == null && epControl == null
				&& epIntEndpointOut == null && epIntEndpointIn == null) 
		{
			throw new IllegalArgumentException("not endpoint is founded!");
		}
		return epIntEndpointIn;
	}
	
	// 打开设备
	public void openDevice(UsbInterface mInterface)
	{
		if (mInterface != null)
		{
			UsbDeviceConnection conn = null;
			// 在open前判断是否有连接权限；对于连接权限可以静态分配，也可以动态分配权限
			if (mUsbManager.hasPermission(mUsbDevice)) 
			{
				conn = mUsbManager.openDevice(mUsbDevice);
			}

			if (conn == null)
			{
				return;
			}

			if (conn.claimInterface(mInterface, true)) 
			{
				mDeviceConnection = conn;
				if (mDeviceConnection != null)// 到此你的android设备已经连上zigbee设备
					System.out.println("open设备成功！");
				final String mySerial = mDeviceConnection.getSerial();
				System.out.println("设备serial number：" + mySerial);
			} 
			else 
			{
				System.out.println("无法打开连接通道。");
				conn.close();
			}
		}
	}
	
	public int writeUsbPort(byte[] buffer)
	{
		return mDeviceConnection.bulkTransfer(epBulkOut, buffer, buffer.length, 0);
	}
	
	public void CallCSLog(String msg)
	{
		UnityPlayer.UnitySendMessage("SerialUtils", "DebugLog", msg);
	}
}
