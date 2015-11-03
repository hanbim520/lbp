package android_usb_api;

import java.io.FileDescriptor;

import android.hardware.usb.*;
import android.os.ParcelFileDescriptor;

public class UsbPort {
//	private final UsbDevice mDevice;
    // used by the JNI code
    private int mNativeContext;
    
    public native boolean open(String name, FileDescriptor descriptor);
    public native void close();
    public native boolean usbClaimInterface(int interfaceID, boolean force);
    public native int usbControlRequest(int requestType, int request, int value,
            int index, byte[] buffer, int length, int timeout);
    public native int usbBulkRequest(int endpointAddress, byte[] buffer, int length, int timeout);
 
    public boolean open(String name, ParcelFileDescriptor pfd) {
        return open(name, pfd.getFileDescriptor());
    }

    public boolean claimInterface(UsbInterface intf, boolean force) {
        return usbClaimInterface(intf.getId(), force);
    }
     
    public int controlTransfer(int requestType, int request, int value,
            int index, byte[] buffer, int length, int timeout) {
        return usbControlRequest(requestType, request, value, index, buffer, length, timeout);
    }
 
    public int bulkTransfer(UsbEndpoint endpoint, byte[] buffer, int length, int timeout) {
        return usbBulkRequest(endpoint.getAddress(), buffer, length, timeout);
    }
    
    static {
		System.loadLibrary("usbhost");
	}
}
