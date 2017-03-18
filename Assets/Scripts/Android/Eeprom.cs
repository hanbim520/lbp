using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

// android板商操作eeprom的工具类
public class Eeprom 
{
	[DllImport("chip")]
	public extern static int eeprom_open (int type);
	
	[DllImport("chip")]
	public extern static int eeprom_close ();
	
	[DllImport("chip")]
	public extern static int eeprom_data_read (byte[] data, int offset, int len);
	
	[DllImport("chip")]
	public extern static int eeprom_data_write (byte[] data, int offset, int len);
}
