using UnityEngine;
using System.Collections;

public class TestPlayerPrebs : MonoBehaviour
{

	void Start()
	{
	
	}
	
	void Update()
	{
	
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 200, 150), "Save"))
		{
			PlayerPrefs.SetInt("Test1", 12);
			PlayerPrefs.SetString("Test2", "cx");
			PlayerPrefs.Save();
		}
	

		if (GUI.Button(new Rect(100, 300, 200, 150), "Get"))
		{
			int t1 = PlayerPrefs.GetInt("Test1");
			string t2 = PlayerPrefs.GetString("Test2");
			DebugConsole.Log(t1.ToString());
			DebugConsole.Log(t2);
		}
	}
}
