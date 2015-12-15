using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 跑马灯效果
public class ScrollingImage : MonoBehaviour
{

	private Image image;
	private RectTransform rt;
	private float deltaTime = 0.01f;	// 移动一个像素花费的时间
	private float destX;
	private int direction = 1;

	void Start()
	{
		image = GetComponent<Image>();
		rt = GetComponent<RectTransform>();
	}

	public void MoveToX(float _destX)
	{
		destX = _destX;
		Vector2 pos = transform.localPosition;
		if (pos.x > _destX)
		{
			_destX += rt.rect.width / 2;
			direction = -1;
		}
		else
		{
			_destX -= rt.rect.width / 2;
			direction = 1;
		}
		float time = Mathf.Abs(_destX - pos.x) * deltaTime;
		iTween.MoveBy(gameObject, iTween.Hash("x", _destX - pos.x, "time", time, "easetype", iTween.EaseType.linear, "oncomplete" , "OnCompleteTween", "oncompletetarget", gameObject));
	}

	private void OnCompleteTween()
	{
		float time = rt.rect.width * deltaTime;
		iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", time, "onupdate", "OnUpdateTween", "easetype", iTween.EaseType.linear));
		iTween.MoveBy(gameObject, iTween.Hash("x", direction * rt.rect.width, "time", time, "easetype", iTween.EaseType.linear));
	}

	private void OnUpdateTween(float value)
	{
		image.fillAmount = value;
	}
}
