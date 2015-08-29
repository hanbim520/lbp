using UnityEngine;
using System.Collections;

public class test : MonoBehaviour
{

	void Start()
	{
	
	}
	
	void Update()
	{
	
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 50, 100, 50), "To Server"))
		{
			Application.LoadLevel("Client");
		}
	}
}
