package android_usb_api;

public class UsbPort {
    private int mDeviceHandle;
    public final int vid = 0x0483;
    public final int pid = 0x5750;
	 
    public native int open(int vid, int pid);
    public native void close(int deviceHandle);
    public native int write(int deviceHandle, byte[] data);
    public native byte[] read(int deviceHandle);
    
    public int getDeviceHandle() {
    	return mDeviceHandle;
    }
    
    static {
		System.loadLibrary("usb1.0");
	}
}
