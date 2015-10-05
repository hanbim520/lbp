using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlashImage : MonoBehaviour
{
	public float interval = 3;
	public int flashCount = 0;	// greater than 0:loop times. less and equal than 0:loop forever
	private float timeDelta = 0;
	private int curFlashCount = 0;
	private bool bStopFlash = false;
	private bool bDestroyFlash = false;

	void Start()
	{
		GameEventManager.StopFlash += StopFlash;
	}

	void OnDestroy()
	{
		GameEventManager.StopFlash -= StopFlash;
	}

	void Update() 
	{
		if (bStopFlash)
			return;

		if (bDestroyFlash)
		{
			bStopFlash = true;
			StartCoroutine(DelayDestroy());
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
					StartCoroutine(DelayDestroy());
				}
			}
		}
	}

	private IEnumerator DelayDestroy()
	{
		yield return new WaitForSeconds(3.0f);
		SetAlpha(0);
		Destroy(this);
	}

	private void StopFlash()
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
