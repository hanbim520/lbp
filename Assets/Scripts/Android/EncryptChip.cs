using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

// android板商操作加密芯片的工具类
public class EncryptChip 
{
	[DllImport("chip")]
	public extern static int encryption_chip_open ();
	
	[DllImport("chip")]
	public extern static int encryption_chip_close ();
	
	[DllImport("chip")]
	public extern static int encryption_chip_firmware_upgrade ();
	
	[DllImport("chip")]
	public extern static int encryption_chip_get_uuid (byte[] buf, int len);
}
