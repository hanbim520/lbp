package android_usb_api;

public class UsbPort {
    public final int vid = 0x0483;
    public final int pid = 0x5750;
	 
    public native int open(int vid, int pid);
    public native void close();
    public native int write(byte[] data);
    public native byte[] read();

    static {
		System.loadLibrary("usb1.0");
	}
}
