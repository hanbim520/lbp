using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReverseAnim : MonoBehaviour
{
	public float fps = 10.0f;
	public string resPath;
	
	private Sprite[] ani;
	private Image image;
	private float timeElasped = 0;
	private int curFrame = 0;
	private bool bReverse = false;

	void Start()
	{
		image = GetComponent<Image>();
		ani = Resources.LoadAll<Sprite>(resPath);
		image.overrideSprite = ani[0];
	}
	
	void Update()
	{
		timeElasped += Time.deltaTime;
		
		if(timeElasped >= 1.0 / fps)
		{
			timeElasped = 0;
			if (!bReverse)
				++curFrame;
			else
				--curFrame;
			
			if(!bReverse && curFrame >= ani.Length)
			{
				bReverse = true;
				curFrame = ani.Length - 2;
			}
			else if (bReverse && curFrame <= 0)
			{
				bReverse = false;
			}
			image.overrideSprite = ani[curFrame];
		}
	}
}
