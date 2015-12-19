using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DoubleAnim : MonoBehaviour
{
	public float fps = 10.0f;
	public int nextStartFrame = 0;
	public string resPath = "";
	
	private Image image1;
	private float timeElasped1 = 0;
	private int curFrame1 = 0;

	private Image image2;
	private float timeElasped2 = 0;
	private int curFrame2 = 0;

	private Sprite[] ani;
	private bool bFireSecond = false;		// 播放第二组动画

	void Start()
	{
		LoadImgs();
		image1 = transform.GetChild(0).GetComponent<Image>();
		image2 = transform.GetChild(1).GetComponent<Image>();
		RevertImage(image1);
		image1.overrideSprite = ani[0];
		ClearImage(image2);
	}
	
	void Update()
	{
		UpdateImage(image1, Time.deltaTime, ref timeElasped1, ref curFrame1);
		if (bFireSecond)
			UpdateImage(image2, Time.deltaTime, ref timeElasped2, ref curFrame2);
	}

	private void UpdateImage(Image target, float deltaTime, ref float timeElasped, ref int frameIndex)
	{
		timeElasped += deltaTime;
		
		if(timeElasped >= 1.0 / fps)
		{
			timeElasped = 0;
			++frameIndex;
			
			if(frameIndex >= ani.Length)
			{
				frameIndex = 0;
				UpdateVariables();
				return;
			}
			UpdateVariables();
			if (Mathf.Approximately(target.color.a, 0))
				RevertImage(target);
			target.overrideSprite = ani[frameIndex];
		}
	}

	private void UpdateVariables()
	{
		if (curFrame1 >= nextStartFrame && !bFireSecond)
		{
			bFireSecond = true;
		}
		if (bFireSecond && curFrame1 == 0)
		{
			bFireSecond = false;
			Image tmp = image1;
			image1 = image2;
			image2 = tmp;
			ClearImage(image2);

			curFrame1 = curFrame2;
			curFrame2 = 0;
			timeElasped1 = timeElasped2;
			timeElasped2 = 0;
		}
	}

	public void StopAnimation()
	{
		ClearImage(image1);
		ClearImage(image2);
		timeElasped1 = 0;
		timeElasped2 = 0;
		curFrame1 = 0;
		curFrame2 = 0;
	}

	private void ClearImage(Image img)
	{
		Color c = img.color;
		c.a = 0;
		img.color = c;
	}

	private void RevertImage(Image img)
	{
		Color c = img.color;
		c.a = 255.0f;
		img.color = c;
	}

	private void LoadImgs()
	{
		ani = Resources.LoadAll<Sprite>(resPath);
	}
}
