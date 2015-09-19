using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlashImage : MonoBehaviour
{
	public float interval = 3;
	public int flashCount = 0;
	private float timeDelta = 0;
	private int curFlashCount = 0;
	private bool stopFlash = false;

	void Update() 
	{
		if (stopFlash)
			return;

		timeDelta += Time.deltaTime;
		if (timeDelta > interval) 
		{
			timeDelta = 0;
			Color c = gameObject.GetComponent<Image>().color;
			if (c.a > 0)
				c.a = 0;
			else
				c.a = 255;
			gameObject.GetComponent<Image>().color = c;
			++curFlashCount;
			if (curFlashCount >= flashCount)
			{
				stopFlash = true;
				StartCoroutine(DelayDestroy());
			}
		}
	}

	private IEnumerator DelayDestroy()
	{
		yield return new WaitForSeconds(3.0f);
		Color c = gameObject.GetComponent<Image>().color;
		c.a = 0;
		gameObject.GetComponent<Image>().color = c;
		Destroy(this);
	}
}
