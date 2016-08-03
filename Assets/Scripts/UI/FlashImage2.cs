using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 闪烁一定次数后 恢复image原来的alpha
public class FlashImage2 : MonoBehaviour
{
	public int count = 6;
	public float interval = 0.7f;

	private int flashCount = 0;
	private float timeElapsed;
	private float oringinalAlpha;
	private Image img;
	private bool bStop = false;

	void OnEnable()
	{
		img = GetComponent<Image>();
		oringinalAlpha = img.color.a;
	}

	void Update()
	{
		if (bStop)
			return;

		if (flashCount < count)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed > interval)
			{
				timeElapsed = 0;
				++flashCount;
				if (img.color.a > 0)
					Transparency(img);
				else
					Opacity(img);
			}
		}
		else
		{
			bStop = true;
			ResumeAlpha(img);
		}
	}

	void ResumeAlpha(Image img)
	{
		Color c = img.color;
		c.a = oringinalAlpha;
		img.color = c;
	}

	void Transparency(Image img)
	{
		Color c = img.color;
		c.a = 0;
		img.color = c;
	}

	void Opacity(Image img)
	{
		Color c = img.color;
		c.a = 255;
		img.color = c;
	}
}
