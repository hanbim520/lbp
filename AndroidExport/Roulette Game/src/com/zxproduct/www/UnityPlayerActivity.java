package com.zxproduct.www;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
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

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

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
import android.net.Uri;
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
		detectHIDViaTimer();
		IntentFilter intentFilter = new IntentFilter();
		intentFilter.addAction(Intent.ACTION_MEDIA_MOUNTED);
		intentFilter.addAction(Intent.ACTION_MEDIA_EJECT);
		intentFilter.addDataScheme("file");
        registerReceiver(mReceiver, intentFilter);
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
	private InputStream mInputStream0 = null;
	private ReadThread0 mReadThread0 = null;
	
	private ConcurrentLinkedQueue<BufferStruct> readSerialQueue0 = new ConcurrentLinkedQueue<BufferStruct>();
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
			 for (int i = 0; i< buffer.buffer.length; ++i)
			 Log.i(TAG, "" + buffer.buffer[i]);
			 return buffer.buffer;
		 }
		 
		 // Can't return null, otherwise csharp side case exception.
	     return new int[]{-1};
	 }
	 
	 private UsbManager mUsbManager = null;
	 private UsbEndpoint epIntEndpointOut;
	 private UsbEndpoint epIntEndpointIn;
	 private UsbDevice mUsbDevice;
	 private UsbDeviceConnection mDeviceConnection;
	 private TReadUsb0 mTReadUsb0 = null;
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
							while (readUsbQueue0.size() > 20)
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
		 try
		 {
			 if (mTReadUsb0 != null && mTReadUsb0.isAlive())
			 {
				 mTReadUsb0.stop();
				 mTReadUsb0 = null;
			 }
		 }
		 catch(Exception ex)
		 {
			 Log.d(TAG, ex.toString());
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
				}
			} 
			else 
			{
				conn.close();
			}
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
		return mDeviceConnection.bulkTransfer(epIntEndpointOut, buf, buf.length, 500);
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
         
        detectTimer.scheduleAtFixedRate(detectTimerTask, 5000, 2000);
    }
    
    public boolean getUsbConnected()
    {
    	return bHIDConnected;
    }
    
    public void CallCSLog(String msg)
	{
//		UnityPlayer.UnitySendMessage("Main Camera", "DebugLog", msg);
	}
    
    //  线号          机号          总利润              当前利润            算码次数 
    public native String GetPWCheckValue4(long LineID, long CilentID,  long  MaxProfit, long Profit, long CheckCount);
    public native byte[] CreateCheckPWString(long LineID, long CilentID, long MaxProfit, long Profit, long CheckCount, long crc, long pwstring_in);
    public native String GetCheckPWStringValue(byte[] recv_buff);
    
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
