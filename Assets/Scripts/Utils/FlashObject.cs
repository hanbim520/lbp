using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlashObject : MonoBehaviour
{
	public float interval = 3;
	public GameObject[] targets;
	private float timeDelta = 0;
	
	void FixedUpdate () 
	{
		timeDelta += Time.fixedDeltaTime;
		if (timeDelta > interval) 
		{
			timeDelta = 0;
			for (int i = 0; i < targets.Length; ++i)
			{
				targets[i].SetActive(!targets[i].activeSelf);
			}
		}
	}
}
