using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrameAnim : MonoBehaviour
{
	public float fps = 10.0f;
	public Sprite[] ani;

	private Image image;
	private float timeElasped = 0;
	private int curFrame = 0;
	
	void Start ()
	{
		image = GetComponent<Image> ();
	}
	
	void FixedUpdate ()
	{
		timeElasped += Time.fixedDeltaTime;
		
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