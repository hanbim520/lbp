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
		
		BufferStruct b1 = new BufferStruct();
		b1.buffer = new byte[] {1, 2, 3};
		BufferStruct b2 = new BufferStruct();
		b2.buffer = new byte[] {4, 5, 6};
		readSerialQueue0.offer(b1);
		readSerialQueue0.offer(b2);
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
	
	private OutputStream mOutputStream0;
	private OutputStream mOutputStream1;
	private OutputStream mOutputStream2;
	private OutputStream mOutputStream3;
	private InputStream mInputStream0;
	private InputStream mInputStream1;
	private InputStream mInputStream2;
	private InputStream mInputStream3;
	private ReadThread mReadThread0;
	private ReadThread mReadThread1;
	private ReadThread mReadThread2;
	private ReadThread mReadThread3;
	private SendingThread mSendingThread0;
	byte[] mBuffer;
	
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
	
	private class ReadThread extends Thread {
		private InputStream inputStream;
		
		public ReadThread(InputStream inputStream)
		{
			this.inputStream = inputStream;
		}
		
		@Override
		public void run() {
			super.run();
			while(!isInterrupted()) {
				int size;
				try {
					byte[] buffer = new byte[64];
					if (inputStream == null) return;
					size = inputStream.read(buffer);
					if (size > 0) {
//						onDataReceived(buffer, size);
					}
				} catch (IOException e) {
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	private class SendingThread extends Thread {
		private OutputStream outputStream;
		
		public SendingThread(OutputStream outputStream)
		{
			this.outputStream = outputStream;
		}
		
		@Override
		public void run() {
			while (!isInterrupted()) {
				try {
					if (outputStream != null) {
						outputStream.write(mBuffer);
					} else {
						return;
					}
				} catch (IOException e) {
					e.printStackTrace();
					return;
				}
			}
		}
	}
	
	public void openSerialPort(int idx, int baudrate)
	{
		try 
		{
			if (idx == 0)
			{
				mSerialPort0 = new SerialPort(new File("/dev/ttyS0"), baudrate, 0);
				mOutputStream0 = mSerialPort0.getOutputStream();
				mInputStream0 = mSerialPort0.getInputStream();
				
				mReadThread0 = new ReadThread(mInputStream0);
				mReadThread0.start();
				mSendingThread0 = new SendingThread(mOutputStream0);
				mSendingThread0.start();
				
				readSerialQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
			}
			else if (idx == 1)
			{
				mSerialPort1 = new SerialPort(new File("/dev/ttyS1"), baudrate, 0);
				
				readSerialQueue1 = new ConcurrentLinkedQueue<BufferStruct>();
			}
			else if (idx == 2)
			{
				mSerialPort2 = new SerialPort(new File("/dev/ttyS2"), baudrate, 0);
				
				readSerialQueue2 = new ConcurrentLinkedQueue<BufferStruct>();
			}
			else if (idx == 3)
			{
				mSerialPort3 = new SerialPort(new File("/dev/ttyS3"), baudrate, 0);
				
				readSerialQueue3 = new ConcurrentLinkedQueue<BufferStruct>();
			}
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

	public void closeSerialPort(int idx)
	{
		if (idx != 0) 
		{
			closeSerialPort(mSerialPort0);
			mSerialPort0 = null;
		}
		else if (idx == 1)
		{
			closeSerialPort(mSerialPort1);
			mSerialPort1 = null;
		}
		else if (idx == 2)
		{
			closeSerialPort(mSerialPort2);
			mSerialPort2 = null;
		}
		else if (idx == 3)
		{
			closeSerialPort(mSerialPort3);
			mSerialPort3 = null;
		}
	}
	
	private void closeSerialPort(SerialPort port)
	{
		if (port != null)
		{
			port.close();
		}
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
