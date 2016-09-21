using UnityEngine;
using System.Collections;

public class TestMultiTouch : MonoBehaviour
{

	void Start()
	{
		Screen.SetResolution(1440, 1080, true);
	}
	
	void Update()
	{
//		if (Input.GetMouseButtonDown(0))
//		{ 
//			DebugConsole.Log("Pressed left click. " + Input.mousePosition.ToString());
//		}
//		if (Input.GetMouseButtonDown(1))
//		{
//			DebugConsole.Log("Pressed right click. " + Input.mousePosition.ToString());
//		}
//		if (Input.GetMouseButtonDown(2))
//		{
//			DebugConsole.Log("Pressed middle click. " + Input.mousePosition.ToString());
//		}
		Touch t = Input.GetTouch(0);
		if (t.phase == TouchPhase.Began)
		{
			DebugConsole.Log("Touch down. " + t.position.ToString());
		}
	}
}
