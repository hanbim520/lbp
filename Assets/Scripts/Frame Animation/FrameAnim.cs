using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrameAnim : MonoBehaviour
{
	public float fps = 10.0f;
	public string resPath;

	private Sprite[] ani;
	private Image image;
	private float timeElasped = 0;
	private int curFrame = 0;
	
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
			++curFrame;

			if(curFrame >= ani.Length)
			{
				curFrame = 0;
			}
			image.overrideSprite = ani[curFrame];
		}
	}
}