package com.zxproduct.www;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.Dictionary;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
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
        
        Init();
	}

	// Quit Unity
	@Override protected void onDestroy ()
	{
		if (mReadThread != null)
			mReadThread.stop();
		
		if (mWriteThread != null)
			mWriteThread.stop();
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
	
	private ReadThread mReadThread = null;
	private WriteThread mWriteThread = null;
	
	private final String kNameCOM1	= "ttyS1";
	private final String kNameCOM2	= "ttyS2";
	private final String kNameCOM3	= "ttyS3";
	private final String kNameCOM4	= "ttyS4";
	private final int kMaxRevDataLen = 28;	// 从金手指回传的数据长度
	Map<Integer, String> ttySDic = new HashMap<Integer, String>();
	private List<SerialPort> serialPorts = new ArrayList<SerialPort>();
	private List<InputStream> inputStreams = new ArrayList<InputStream>();
	private List<OutputStream> outputStreams = new ArrayList<OutputStream>();
	private List<Integer> inputParsePhases = new ArrayList<Integer>();
	private List<ConcurrentLinkedQueue<BufferStruct>> writeQueues = new ArrayList<ConcurrentLinkedQueue<BufferStruct>>(); 
	private List<ConcurrentLinkedQueue<BufferStruct>> readQueues = new ArrayList<ConcurrentLinkedQueue<BufferStruct>>(); 
	private BufferStruct dealingCOM2Data = null;
	private ConcurrentLinkedQueue<BufferStruct> readUsbQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
//	private ConcurrentLinkedQueue<BufferStruct> writeUsbQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
	
	private class BufferStruct
	{
		public int[] buffer;
	}
	
	// 解析金手指传来的数据
	private void parsettyS2(int id)
	{
		try
		{
			int parsePhase = inputParsePhases.get(id);
			byte[] buffer = new byte[64];
			int size = inputStreams.get(id).read(buffer);
			if (size > 0)
			{
//				String log = ""; 
				for (int i = 0; i < size; ++i)
				{
					int tmp = buffer[i] & 0xff;
					if (dealingCOM2Data == null)
					{
						if (parsePhase == 0 && tmp != 0xA5)
							break;
						if ((parsePhase == 1 && tmp != 0x58) ||
							(parsePhase == 2 && tmp != 0x57))
						{
							dealingCOM2Data = null;
							parsePhase = 0;
							break;
						}
							
						dealingCOM2Data = new BufferStruct();
						dealingCOM2Data.buffer = new int[kMaxRevDataLen];
						dealingCOM2Data.buffer[kMaxRevDataLen - 1] = -1;
					}

//					log += String.format("%#x, ", tmp);
					dealingCOM2Data.buffer[parsePhase++] = tmp;
					// 接收完毕
					if (parsePhase == kMaxRevDataLen)
					{
						parsePhase = 0;
						
//						int len = dealingCOM2Data.buffer.length;
//						String endLog = "";
//						for (int j = 0; j < len; ++j)
//						{
//							endLog += String.format("%#x, ", dealingCOM2Data.buffer[j]);
//						}
//						CallCSLog("parsettyS2:" + endLog);
						readQueues.get(id).offer(dealingCOM2Data);
						dealingCOM2Data = null;
					}
				}
//				CallCSLog("ReadThread:" + log);
			}
			inputParsePhases.set(id, parsePhase);
		}
		catch(Exception e)
		{
			e.printStackTrace();
			return;
		}
	}
	
	private void parsettyS(int id)
	{
		try
		{
			byte[] buffer = new byte[64];
			int size = inputStreams.get(id).read(buffer);
			if (size > 0)
			{
				BufferStruct buf = new BufferStruct();
				buf.buffer = new int[size];
				for (int i = 0; i < size; ++i)
				{
					int tmp = buffer[i] & 0xff;
					buf.buffer[i] = tmp;
				}
				readQueues.get(id).offer(buf);
			}
		}
		catch(Exception e)
		{
			e.printStackTrace();
			return;
		}
	}
	
	private class ReadThread extends Thread 
	{	
		@Override
		public void run()
		{
			super.run();
			while(!isInterrupted()) 
			{
				try
				{
					if (!inputStreams.isEmpty()) 
					{
						int inputStreamLength = inputStreams.size();
						for (int id = 0; id < inputStreamLength; ++id)
						{
							if (ttySDic.containsKey(id))
							{
								String portName = ttySDic.get(id);
								if (portName.equals(kNameCOM2))
									parsettyS2(id);
								else
									parsettyS(id);
							}
						}
					}
				}
				catch (Exception e)
				{
					CallCSLog("ReadThread Exception:" + e.toString());
					e.printStackTrace();
					mReadThread = new ReadThread();
					mReadThread.start();
					return;
				}
			}
		}
	}
	
	private class WriteThread extends Thread
	{
		@Override
		public void run()
		{
			super.run();
			while(!isInterrupted()) 
			{
				try
				{
					if (!outputStreams.isEmpty()) 
					{
						int outputStreamLength = outputStreams.size();
						for (int id = 0; id < outputStreamLength; ++id)
						{
							if (!writeQueues.get(id).isEmpty())
							{
								BufferStruct buf = writeQueues.get(id).poll();
								int size = buf.buffer.length;
								byte[] buffer = new byte[size];
//								String log = "";
								for (int i = 0; i < size; ++i)
								{
									buffer[i] = (byte)buf.buffer[i];
//									log += String.format("%#x, ", buffer[i]);
								}
//								CallCSLog("WriteThread:" + log);
								outputStreams.get(id).write(buffer);
							}
						}
					}
				}
				catch (Exception e)
				{
					CallCSLog("WriteThread Exception:" + e.toString());
					e.printStackTrace();
					mWriteThread = new WriteThread();
					mWriteThread.start();
					return;
				}
			}
		}
	}
	
	public void Init()
	{
		mReadThread = new ReadThread();
		mReadThread.start();
		
		mWriteThread = new WriteThread();
		mWriteThread.start();
	}
	
	public int openSerialPort(String filePath, int baudrate, int parity, int dataBits, int stopBits)
	{
		try
		{
			CallCSLog("filePath:" + filePath + ", baudrate:" + baudrate + ", parity:" + parity + ", dataBits:" + dataBits + ", stopBits:"+ stopBits);
			
			SerialPort sp = new SerialPort(new File(filePath), baudrate,  parity, dataBits, stopBits);
			serialPorts.add(sp);
			writeQueues.add(new ConcurrentLinkedQueue<BufferStruct>());
			readQueues.add(new ConcurrentLinkedQueue<BufferStruct>());
			inputStreams.add(sp.getInputStream());
			outputStreams.add(sp.getOutputStream());
			int portId = serialPorts.size() - 1;
			if (filePath.contains(kNameCOM1))
				ttySDic.put(portId, kNameCOM1);
			else if (filePath.contains(kNameCOM2))
				ttySDic.put(portId, kNameCOM2);
			else if (filePath.contains(kNameCOM3))
				ttySDic.put(portId, kNameCOM3);
			else if (filePath.contains(kNameCOM4))
				ttySDic.put(portId, kNameCOM4);
			inputParsePhases.add(0);

			return portId;
		}
		catch (Exception e)
		{
			e.printStackTrace();
		}
		return -1;
	}

	public void closeSerialPort()
	{
		try
		{
			if (serialPorts.size() > 0)
			{
				for (SerialPort sp : serialPorts)
					sp.close();
				serialPorts.clear();
			}
			writeQueues.clear();
			readQueues.clear();
			inputStreams.clear();
			outputStreams.clear();
			ttySDic.clear();
			inputParsePhases.clear();
			
//			if (mReadThread != null)
//				mReadThread.stop();
//			
//			if (mWriteThread != null)
//				mWriteThread.stop();
		}
		catch(Exception ex)
		{
			Log.d(TAG, ex.toString());
		}
	}
	
	public void writeSerialPort(int[] data, int portId)
	{
		int size = data.length;
		BufferStruct buf = new BufferStruct();
		buf.buffer = new int[size];
		for (int i = 0; i < size; ++i)
		{
			buf.buffer[i] = data[i] & 0xff;
		}
		writeQueues.get(portId).offer(buf);
	}

	 public int[] readSerialPort(int portId)
	 {
		 if (!readQueues.get(portId).isEmpty())
		 {
			 String portName = ttySDic.get(portId);
			 if (portName.equals(kNameCOM2))
			 {
				 BufferStruct buf = readQueues.get(portId).poll();
				 if (buf != null && buf.buffer.length == kMaxRevDataLen)
				 {
					 if (buf.buffer[0] == 0xA5 && buf.buffer[kMaxRevDataLen - 1] == 0)
					 {
//						 buf = readQueues.get(portId).poll();
//						 CallCSLog("readQueues " + portId + " size: " + readQueues.get(portId).size());
						 return buf.buffer;
					 }
				 }
			 }
			 else
			 {
				 if (!readQueues.get(portId).isEmpty())
				 {
					 BufferStruct buf = readQueues.get(portId).poll();
					 return buf.buffer;
				 }
			 }
		 }
		 
		 // Can't return null, otherwise csharp side case exception.
	     return new int[]{-1};
	 }
	 
    public void CallCSLog(String msg)
	{
//		UnityPlayer.UnitySendMessage("Main Camera", "DebugLog", msg);
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
	   			if (path.contains("/mnt/usb"))
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
    	try
    	{
    		CallCSLog("getFiles:" + filePath);
        	File root = new File(filePath);
        	if (root.exists())
        	{
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
    	}
    	catch(Exception e)
    	{
    		CallCSLog(e.toString());
    		e.printStackTrace();
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
