using UnityEngine;
using System.Collections;

public class TestUSB : MonoBehaviour
{
	void OnGUI()
    {
        if (GUI.Button(new Rect(200, 10, 200, 100), "Exit"))
        {
            Application.Quit();
        }
    }
}
