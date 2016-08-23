package com.zxproduct.www;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.concurrent.ConcurrentLinkedQueue;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.Window;
import android.view.WindowManager;
import android_serialport_api.SerialPort;
import android_usb_api.UsbPort;

import com.unity3d.player.UnityPlayer;

public class UnityPlayerActivity extends Activity
{
	protected UnityPlayer mUnityPlayer; // don't change the name of this variable; referenced from native code
	private String TAG = "Unity";
	private char[] encryKey = new char[]{'W', '3', 'c', 'd', '9', 'X'};
    private int encryIndex = 0;
    
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
		IntentFilter intentFilter = new IntentFilter();
		intentFilter.addAction(Intent.ACTION_MEDIA_MOUNTED);
		intentFilter.addAction(Intent.ACTION_MEDIA_EJECT);
		intentFilter.addDataScheme("file");
        registerReceiver(mReceiver, intentFilter);
	}

	// Quit Unity
	@Override protected void onDestroy ()
	{
		closeSerialPort();
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
	private InputStream mInputStream0 = null;
	private ReadThread0 mReadThread0 = null;
	
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> readUsbQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	private ConcurrentLinkedQueue<BufferStruct> writeUsbQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	
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
//							Log.d(TAG, "mInputStream0 size:" + size);
							BufferStruct buf = new BufferStruct();
							buf.buffer = new int[size];
							for (int i = 0; i < size; ++i)
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
	
	public void openSerialPort(String filePath, int baudrate, int parity, int dataBits, int stopBits)
	{
		try
		{
			mSerialPort0 = new SerialPort(new File(filePath), baudrate,  parity, dataBits, stopBits);
			mInputStream0 = mSerialPort0.getInputStream();
			
			mReadThread0 = new ReadThread0();
			mReadThread0.start();
		}
		catch (Exception e)
		{
			e.printStackTrace();
		}
	}

	public void closeSerialPort()
	{
		try
		{
			if (mSerialPort0 != null)
			{
				mSerialPort0.close();
				mSerialPort0 = null;
			}
			
			if (mReadThread0 != null)
				mReadThread0.stop();
		}
		catch(Exception ex)
		{
			Log.d(TAG, ex.toString());
		}
	}

	 public int[] readSerialPort()
	 {
		 if (!readSerialQueue0.isEmpty())
		 {
			 BufferStruct buffer = readSerialQueue0.poll();
			 return buffer.buffer;
		 }
		 
		 // Can't return null, otherwise csharp side case exception.
	     return new int[]{-1};
	 }
	 
    public void CallCSLog(String msg)
	{
		UnityPlayer.UnitySendMessage("Main Camera", "DebugLog", msg);
	}
    
    //  线号          机号          总利润              当前利润            算码次数 
    public native String GetPWCheckValue4(long LineID, long CilentID,  long  MaxProfit, long Profit, long CheckCount);
    public native byte[] CreateCheckPWString(long LineID, long CilentID, long MaxProfit, long Profit, long CheckCount, long crc, long pwstring_in);
    public native String GetCheckPWStringValue(byte[] recv_buff);
    public native byte[] EncryptIOData(byte[] inputArray);
    public native byte[] DecryptIOData(byte[] inputArray, int len);
    
    private final BroadcastReceiver mReceiver = new BroadcastReceiver() 
	{
        @Override
        public void onReceive(Context context, Intent intent) 
        {
        	CallCSLog("BroadcastReceiver:" + intent.getAction());
        	if (intent.getAction().equals(Intent.ACTION_MEDIA_MOUNTED))
	   		{
	   			String path = intent.getData().getPath();
	   			CallCSLog("Path:" + path);
	   			if (path.contains("/mnt/usbhost"))
	   			{
	   				getFiles(path);
	   			}
	   		}
        	else if (intent.getAction().equals(Intent.ACTION_MEDIA_EJECT))
        	{
        		
        	}
        }
    };
    
    private void getFiles(String filePath)
	{
    	CallCSLog("getFiles:" + filePath);
    	File root = new File(filePath);
	    File[] files = root.listFiles();
	    String fileName = "update";
	    String apkName = "update.apk";
	    for(File file:files)
	    {     
	    	if(!file.isDirectory())
	    	{
	    		if (file.getName().equals(fileName))
	    		{
	    			CallCSLog("File name:" + file.getName());
	    			decryFile(file);
	    		}
	    		else if (file.getName().equals(apkName))
	    		{
	    			installAPK(filePath + "/" + apkName);
	    		}
	    	}  
	    }
	}
    
    private void decryFile(File file) {
    	CallCSLog("decryFile:" + file.getPath());
		InputStream in = null;
		OutputStream out = null;
		encryIndex = 0;
		String tmpPath = "/mnt/sdcard/Download/update_temp.xml";
		try {
			in = new FileInputStream(file);
			out = new FileOutputStream(tmpPath);
			byte[] buff = new byte[1024]; // 缓冲区
			int n = -1;
			while ((n = in.read(buff)) != -1) {
				for (int i = 0; i < buff.length; i++) { // 对数组进行循环解密
					buff[i] -= encryKey[getIndex()];
				}
				out.write(buff, 0, n);
				out.flush();
			}
		} catch (Exception e) {
			e.printStackTrace();
			CallCSLog("decryFile exception:" + e.toString());
		} finally {
			try {
				in.close();
				out.close();
				parseUpdateFile(tmpPath);
			} catch (Exception e) {
				e.printStackTrace();
			}
		}
	}
    
    private int getIndex()
    { 
		encryIndex = encryIndex++ % encryKey.length;
		return encryIndex;
	}
    
    private void parseUpdateFile(String filePath)
	{
    	CallCSLog("parseUpdateFile:" + filePath);
		DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();  
	    try  
	    {  
	    	DocumentBuilder db = dbf.newDocumentBuilder();  
            Document doc = db.parse("file://" + filePath);  
  
            NodeList infoList = doc.getElementsByTagName("update");  
            Node info = infoList.item(0);  
            for (Node node = info.getFirstChild(); node != null; node = node.getNextSibling())  
            {  
                if (node.getNodeType() == Node.ELEMENT_NODE)  
                {  
                    String name = node.getNodeName();  
                    String value = node.getFirstChild().getNodeValue();
                    UnityPlayer.UnitySendMessage("UpdateUtils", "UpdateInfos", name + ":" + value);
                    CallCSLog("name:" + name + ", value:" + value);
                }  
            }
       }  
       catch (Exception e)  
       {  
           e.printStackTrace();  
           CallCSLog("parseUpdateFile exception:" + e.toString());
       }  
	   finally 
	   {
		   File file = new File(filePath);
		   file.delete();
	   }
	}
    
    public void installAPK(String filePath)
    {  
    	CallCSLog("installAPK:" + filePath);
        // 创建Intent意图  
        Intent intent = new Intent(Intent.ACTION_VIEW);  
        // 设置Uri和类型  
        intent.setDataAndType(Uri.parse("file://" + filePath), "application/vnd.android.package-archive");
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        // 执行意图进行安装  
        startActivity(intent);  
    }  
    
    
    
    static {
        System.loadLibrary("hello-jni");
    }
}
