using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public static class LinuxUtils
{
	[DllImport ("LinuxUtils")] 
	public extern static int GetRandom(int min, int max);

	[DllImport ("LinuxUtils")] 
    public extern static void SetSeed();

    [DllImport ("LinuxUtils")] 
	public extern static void Seed(int seed);
}
