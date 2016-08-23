using UnityEngine;
using System.Collections;

public class TestResolution : MonoBehaviour
{

	void Start()
	{
		Screen.SetResolution(1440, 1080, true);
	}
	
	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 100, 50), "Quit"))
		{
			Application.Quit();
		}
	}
}
