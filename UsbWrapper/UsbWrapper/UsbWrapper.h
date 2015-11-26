// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the USBWRAPPER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// USBWRAPPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.

#ifndef _USB_WRAPPER_H
#define _USB_WRAPPER_H

#include "lusb0_usb.h"

#ifdef USBWRAPPER_EXPORTS
#define USBWRAPPER_API __declspec(dllexport)
#else
#define USBWRAPPER_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C"
{
#endif

	USBWRAPPER_API int OpenHid(int vid, int pid);
	USBWRAPPER_API void CloseHid();
	USBWRAPPER_API int WriteHid(unsigned char *data, int len);
	USBWRAPPER_API void ReadHid(char **data, int *len);
	USBWRAPPER_API void FreeArrayPtr(char *ptr);

#ifdef __cplusplus
}
#endif

#endif

