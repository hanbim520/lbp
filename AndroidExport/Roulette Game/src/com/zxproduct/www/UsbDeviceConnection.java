package com.zxproduct.www;

import java.io.FileDescriptor;

import android.hardware.usb.*;
import android.os.ParcelFileDescriptor;

public class UsbDeviceConnection {
//	private final UsbDevice mDevice;
    // used by the JNI code
    private int mNativeContext;
    
    public native boolean usbOpen(String name, FileDescriptor descriptor);
    public native void usbClose();
    public native boolean usbClaimInterface(int interfaceID, boolean force);
    public native int usbControlRequest(int requestType, int request, int value,
            int index, byte[] buffer, int length, int timeout);
    public native int usbBulkRequest(int endpointAddress, byte[] buffer, int length, int timeout);
 
    public boolean open(String name, ParcelFileDescriptor pfd) {
        return usbOpen(name, pfd.getFileDescriptor());
    }
    public void close() {
    	usbClose();
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
}
