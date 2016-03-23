using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 不从第一帧开始播
public class RandomAnim : FrameAnim
{
	public int firstFrame = 0;

	protected override void Start()
	{
		image = GetComponent<Image>();
		ani = Resources.LoadAll<Sprite>(resPath);
//		Utils.SetSeed();
//		curFrame = Utils.GetRandom(0, ani.Length);
		curFrame = firstFrame;
		image.overrideSprite = ani[curFrame];
	}

}
