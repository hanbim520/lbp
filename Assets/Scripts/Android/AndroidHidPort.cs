using UnityEngine;
using System;
using System.Collections;
 
public static class AndroidHidPort
{
	private static AndroidJavaClass jc;
	private static AndroidJavaObject jo;
	private static IntPtr writeMethodId;
	private static AndroidJavaObject readMethod;


	public static void Init()
	{
		jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
	}

	public static void Close()
	{
		jo.Call("closeUsb");
	}

	public static int Write(ref int[] data)
	{
		IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
		jvalue[] blah = new jvalue[1];
		blah[0].l = pArr;

		if (writeMethodId == null)
			writeMethodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "writeUsbPort");
		int ret = AndroidJNI.CallIntMethod(jo.GetRawObject(), writeMethodId, blah);
		AndroidJNI.DeleteLocalRef(pArr);
		return ret;
	}

	public static int[] Read()
	{
		if (readMethod == null)
			readMethod = jo.Call<AndroidJavaObject>("readHID");
		return AndroidJNIHelper.ConvertFromJNIArray<int[]>(readMethod.GetRawObject());
	}

}
