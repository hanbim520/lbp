using UnityEngine;
using System.Collections;

public class TestBillAcceptor : MonoBehaviour
{

	void Start()
	{
		GameEventManager.ReceiveCoin += ReceiveCoin;
	}

	void ReceiveCoin(int count)
	{
		DebugConsole.Log("coin: " + count);
	}
	
	void OnDestroy()
	{
		GameEventManager.ReceiveCoin -= ReceiveCoin;
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(50, 50, 150, 100), "Quit"))
		{
			Application.Quit();
		}

		if (GUI.Button(new Rect(50, 200, 150, 100), "Clear"))
		{
			DebugConsole.Clear();
		}
	}

	public void DebugLog(string msg)
	{
		DebugConsole.Log(msg);
	}
}
