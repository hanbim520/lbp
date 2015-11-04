using UnityEngine;
using System.Collections;

public class UpdateUtils : MonoBehaviour
{
	private char[] delimiterChars = {':'};
	void Start()
	{
		DontDestroyOnLoad(this);
	}

	public void UpdateInfos(string msg)
	{
		string[] words = msg.Split(delimiterChars);
		if (words.Length > 1)
		{
			string key = words[0];
			string value = words[1];
			int v;
			if (int.TryParse(value, out v))
			{
				PlayerPrefs.SetInt(key, v);
			}
			else
			{
				PlayerPrefs.SetString(key, value);
			}
			PlayerPrefs.Save();
		}
	}

	public void DebugLog(string msg)
	{
		DebugConsole.Log(msg);
	}

//	void OnGUI()
//	{
//		if (GUI.Button(new Rect(200, 10, 200, 100), "Exit"))
//		{
//			Application.Quit();
//		}
//	}
}
