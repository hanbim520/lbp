using UnityEngine;
using System.Collections;

public class CameraPrintLog : MonoBehaviour
{
	public void DebugLog(string msg)
	{
		DebugConsole.Log(msg);
	}
}
