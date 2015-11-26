// UsbWrapper.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "UsbWrapper.h"

#pragma comment(lib, "libusb.lib")  

#define MY_CONFIG 1

usb_dev_handle* dev_handle;

USBWRAPPER_API int OpenHid(int vid, int pid)
{
	usb_init();
	usb_find_busses();
	usb_find_devices();

	int state = 0;
	struct usb_bus *bus;
	struct usb_device *dev;

	for (bus = usb_get_busses(); bus; bus = bus->next)
	{
		for (dev = bus->devices; dev; dev = dev->next)
		{
			if (dev->descriptor.idVendor == vid
				&& dev->descriptor.idProduct == pid)
			{
				dev_handle = usb_open(dev);
				if (!dev_handle)
				{
					state = -1;
				}
				else
				{
					state = usb_set_configuration(dev_handle, MY_CONFIG);
					if (state < 0)
					{
						usb_close(dev_handle);
						return state;
					}
					state = usb_claim_interface(dev_handle, 0);
					if (state < 0)
					{
						usb_close(dev_handle);
						return state;
					}
				}
			}
		}
	}
	return state;
}

USBWRAPPER_API void CloseHid()
{
	if (!dev_handle)
	{
		usb_close(dev_handle);
		dev_handle = NULL;
	}
}

USBWRAPPER_API int WriteHid(unsigned char *data, int len)
{
	return usb_bulk_write(dev_handle, 1, (char*)data, len, 500);
}

USBWRAPPER_API void ReadHid(char **data, int *len)
{
	char *input = new char[64];
	*len = usb_bulk_read(dev_handle, 129, input, 64, 0);
	*data = input;
}

USBWRAPPER_API void FreeArrayPtr(char *ptr)
{
	delete[] ptr;
}
