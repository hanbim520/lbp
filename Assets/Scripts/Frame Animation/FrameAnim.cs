using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 从第一帧开始播放
public class FrameAnim : MonoBehaviour
{
	public float fps = 10.0f;
	public string resPath;

	protected Sprite[] ani;
	protected Image image;
	protected float timeElasped = 0;
	protected int curFrame = 0;
	
	protected void Start()
	{
		image = GetComponent<Image>();
		ani = Resources.LoadAll<Sprite>(resPath);
		image.overrideSprite = ani[0];
	}
	
	protected void Update()
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