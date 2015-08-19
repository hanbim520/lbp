package com.zxproduct.www;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Queue;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.security.InvalidParameterException;

import com.unity3d.player.*;

import android.app.Activity;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.content.SharedPreferences;
import android_serialport_api.SerialPort;
import android_serialport_api.SerialPortFinder;

public class UnityPlayerActivity extends Activity
{
	protected UnityPlayer mUnityPlayer; // don't change the name of this variable; referenced from native code

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
	private ReadThread0 mReadThread0;
	private ReadThread1 mReadThread1;
	private ReadThread2 mReadThread2;
	private ReadThread3 mReadThread3;
	private SendingThread mSendingThread = null;
	private boolean isExit0 = true;
	private boolean isExit1 = true;
	private boolean isExit2 = true;
	private boolean isExit3 = true;
	private boolean isSendExit = true;
	
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue1 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue2 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue3 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeSerialQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeSerialQueue1 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeSerialQueue2 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeSerialQueue3 = new ConcurrentLinkedQueue<BufferStruct>();
	
	private class BufferStruct
	{
		public byte[] buffer;
	}
	
	private class ReadThread0 extends Thread 
	{	
		public ReadThread0()
		{
			isExit0 = false;
		}
		@Override
		public void run()
		{
			super.run();
			while(!isExit0) 
			{
				try
				{
					if (mInputStream0 != null) 
					{
						byte[] buffer = new byte[128];
						int size = mInputStream0.read(buffer);
						if (size > 0)
						{
							synchronized(readSerialQueue0)
							{
								BufferStruct buf = new BufferStruct();
								buf.buffer = buffer;
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
			}
		}
	}
	
	private class ReadThread1 extends Thread 
	{	
		public ReadThread1()
		{
			isExit1 = false;
		}
		@Override
		public void run()
		{
			super.run();
			while(!isExit1) 
			{
				try
				{
					if (mInputStream1 != null) 
					{
						byte[] buffer = new byte[128];
						int size = mInputStream1.read(buffer);
						if (size > 0)
						{
							synchronized(readSerialQueue1)
							{
								BufferStruct buf = new BufferStruct();
								buf.buffer = buffer;
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
			}
		}
	}
	
	private class ReadThread2 extends Thread 
	{	
		public ReadThread2()
		{
			isExit2 = false;
		}
		@Override
		public void run()
		{
			super.run();
			while(!isExit2) 
			{
				try
				{
					if (mInputStream2 != null) 
					{
						byte[] buffer = new byte[128];
						int size = mInputStream2.read(buffer);
						if (size > 0)
						{
							synchronized(readSerialQueue2)
							{
								BufferStruct buf = new BufferStruct();
								buf.buffer = buffer;
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
			}
		}
	}
	
	private class ReadThread3 extends Thread 
	{	
		public ReadThread3()
		{
			isExit3 = false;
		}
		@Override
		public void run()
		{
			super.run();
			while(!isExit3) 
			{
				try
				{
					if (mInputStream3 != null) 
					{
						byte[] buffer = new byte[128];
						int size = mInputStream3.read(buffer);
						if (size > 0)
						{
							synchronized(readSerialQueue3)
							{
								BufferStruct buf = new BufferStruct();
								buf.buffer = buffer;
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
			}
		}
	}
	
	private class SendingThread extends Thread 
	{
		@Override
		public void run()
		{
			super.run();
			while (!isSendExit)
			{
				try
				{
					if (!writeSerialQueue0.isEmpty() && mOutputStream0 != null)
					{
						synchronized(writeSerialQueue0)
						{
							BufferStruct buffer = writeSerialQueue0.poll();
							mOutputStream0.write(buffer.buffer);
						}
					}
					if (!writeSerialQueue1.isEmpty() && mOutputStream1 != null)
					{
						synchronized(writeSerialQueue1)
						{
							BufferStruct buffer = writeSerialQueue1.poll();
							mOutputStream1.write(buffer.buffer);
						}
					}
					if (!writeSerialQueue2.isEmpty() && mOutputStream2 != null)
					{
						synchronized(writeSerialQueue2)
						{
							BufferStruct buffer = writeSerialQueue2.poll();
							mOutputStream2.write(buffer.buffer);
						}
					}
					if (!writeSerialQueue3.isEmpty() && mOutputStream3 != null)
					{
						synchronized(writeSerialQueue3)
						{
							BufferStruct buffer = writeSerialQueue3.poll();
							mOutputStream3.write(buffer.buffer);
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
			
			mSendingThread = new SendingThread();
			mSendingThread.start();
			
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
		isExit0 = true;
		isExit1 = true;
		isExit2 = true;
		isExit3 = true;
		isSendExit = true;
		
		mSerialPort0.close();
		mSerialPort0 = null;
		mSerialPort1.close();
		mSerialPort1 = null;
		mSerialPort2.close();
		mSerialPort2 = null;
		mSerialPort3.close();
		mSerialPort3 = null;
	}

	 public byte[] readSerialPort0()
	 {
		 synchronized(readSerialQueue0)
		 {
			 if (!readSerialQueue0.isEmpty())
			 {
				 BufferStruct buffer = readSerialQueue0.poll();
				 return buffer.buffer;
			 }
		 }
		 // Can't return null, otherwise csharp side case exception.
	     return new byte[]{0};
	 }
	 
	 public byte[] readSerialPort1()
	 {
		 synchronized(readSerialQueue1)
		 {
			 if (!readSerialQueue1.isEmpty())
			 {
				 BufferStruct buffer = readSerialQueue1.poll();
				 return buffer.buffer;
			 }
		 }
		// Can't return null, otherwise csharp side case exception.
		 return new byte[]{0};
	 }
	 
	 public byte[] readSerialPort2()
	 {
		 synchronized(readSerialQueue2)
		 {
			 if (!readSerialQueue2.isEmpty())
			 {
				 BufferStruct buffer = readSerialQueue2.poll();
				 return buffer.buffer;
			 }
		 }
		// Can't return null, otherwise csharp side case exception.
		 return new byte[]{0};
	 }
	 
	 public byte[] readSerialPort3()
	 {
		 synchronized(readSerialQueue3)
		 {
			 if (!readSerialQueue3.isEmpty())
			 {
				 BufferStruct buffer = readSerialQueue3.poll();
				 return buffer.buffer;
			 }
		 }
		// Can't return null, otherwise csharp side case exception.
		 return new byte[]{0};
	 }
	 
	 public boolean writeSerialPort0(byte[] data)
	 {
		 synchronized(writeSerialQueue0)
		 {
			 BufferStruct buffer = new BufferStruct();
			 buffer.buffer = data;
			 return writeSerialQueue0.offer(buffer);
		 }
	 }
	 
	 public boolean writeSerialPort1(byte[] data)
	 {
		 synchronized(writeSerialQueue1)
		 {
			 BufferStruct buffer = new BufferStruct();
			 buffer.buffer = data;
			 return writeSerialQueue1.offer(buffer);
		 }
	 }
	 
	 public boolean writeSerialPort2(byte[] data)
	 {
		 synchronized(writeSerialQueue2)
		 {
			 BufferStruct buffer = new BufferStruct();
			 buffer.buffer = data;
			 return writeSerialQueue2.offer(buffer);
		 }
	 }
	 
	 public boolean writeSerialPort3(byte[] data)
	 {
		 synchronized(writeSerialQueue3)
		 {
			 BufferStruct buffer = new BufferStruct();
			 buffer.buffer = data;
			 return writeSerialQueue3.offer(buffer);
		 }
	 }
}
