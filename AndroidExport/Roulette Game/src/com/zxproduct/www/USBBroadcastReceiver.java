 package com.zxproduct.www;
 import android.content.BroadcastReceiver;
 import android.content.Context;
 import android.content.Intent;
 import android.widget.Toast;
 
 public class USBBroadcastReceiver extends BroadcastReceiver
 {
	 @Override
	 public void onReceive(Context context, Intent intent) 
	 {
//		 if(intent.getAction().equals("android.hardware.usb.action.USB_STATE"))
//		 {
//			 if (intent.getExtras().getBoolean("connected"))
//			 {
//				 // usb 插入                  
//				 Toast.makeText(context, "插入", Toast.LENGTH_LONG).show();
//			 }
//			 else
//			 {
//				 // usb 拔出
//				 Toast.makeText(context, "拔出", Toast.LENGTH_LONG).show();
//			 }
//			 Toast.makeText(context, "收到", Toast.LENGTH_LONG).show();
//		 }
//		 Toast.makeText(context, intent.getAction(), Toast.LENGTH_LONG).show();
		 Toast.makeText(context, intent.getAction() + ". " + intent.getData().getPath(), Toast.LENGTH_LONG).show();
    }
 }