using UnityEngine;
using System.Collections;

public class TestUSB : MonoBehaviour
{
	void Start()
	{
		bool state;
		if (bool.TryParse("False", out state))
		{
			if (state)
			print("true");
			else
				print("false");
		}
		else
			print("can't parse.");
	}
	void OnGUI()
    {
        if (GUI.Button(new Rect(200, 10, 200, 100), "Exit"))
        {
            Application.Quit();
        }
    }
}
