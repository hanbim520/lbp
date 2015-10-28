using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestCoinDevice : MonoBehaviour
{
    protected HIDUtils hidUtils;
	
	protected int touBiCount = 0;

	void Start()
	{
        hidUtils = gameObject.GetComponent<HIDUtils>();
	}

	// 10
	void OnGUI()
	{
		if (GUI.Button(new Rect(200, 10, 200, 100), "Exit"))
		{
			Application.Quit();
		}
		
		if (GUI.Button(new Rect(200, 150, 200, 100), "退币"))
		{
			hidUtils.PayCoin(10);
		}

		if (GUI.Button(new Rect(200, 300, 200, 100), "换场景"))
		{
			Application.LoadLevel("TestEncryChip 1");
		}

		if (GUI.Button(new Rect(200, 300, 200, 100), "回主景"))
		{
			Application.LoadLevel("TestEncryChip");
		}
	}
}
