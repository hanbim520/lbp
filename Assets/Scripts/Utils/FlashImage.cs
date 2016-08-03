using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 闪烁一定次数后Destroy这个object
public class FlashImage : MonoBehaviour
{
	public float interval = 3;
	public int flashCount = 0;	// greater than 0:loop times. less and equal than 0:loop forever
	private float timeDelta = 0;
	private int curFlashCount = 0;
	private bool bStopFlash = false;
	private bool bDestroyFlash = false;

	void Update() 
	{
		if (bStopFlash)
			return;

		if (bDestroyFlash)
		{
			bStopFlash = true;
			DestroyItself();
			return;
		}

		timeDelta += Time.deltaTime;
		if (timeDelta > interval) 
		{
			timeDelta = 0;
			float alpha = gameObject.GetComponent<Image>().color.a;
			if (alpha > 0)
				SetAlpha(0);
			else
				SetAlpha(255);
			if (flashCount > 0)
			{
				++curFlashCount;
				if (curFlashCount >= flashCount)
				{
					bStopFlash = true;
					DestroyItself();
				}
			}
		}
	}

	private void DestroyItself()
	{
		SetAlpha(0);
		Destroy(this);
	}

	public void StopFlash()
	{
		bDestroyFlash = true;
	}

	private void SetAlpha(int alpha)
	{
		Color c = gameObject.GetComponent<Image>().color;
		c.a = alpha;
		gameObject.GetComponent<Image>().color = c;
	}
}
