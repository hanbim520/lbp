using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVersion : MonoBehaviour 
{
	public Text txtversion;

	void Start () 
	{
		txtversion.text = string.Format("Version: {0}", GameData.version);
	}
	
}
