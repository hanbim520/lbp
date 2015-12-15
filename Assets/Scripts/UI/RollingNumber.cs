using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 拉霸机中的滚筒效果
public class RollingNumber : MonoBehaviour
{
	public Image image1;
	public Image image2;

	private Material main;
	private Material second;
	private int digit;
	private float deltaTime = 0.25f;		// 滚动一个格子花费的时间

	void Start()
	{
		main = image1.material;
		second = image2.material;
		main.mainTextureOffset = Vector2.zero;
		second.mainTextureOffset = Vector2.one;
		digit = 0;
	}

	public void SetDigit(int value)
	{
		if (digit != value)
		{
			float oldVal = GetOffset(digit);
			digit = value;
			float newVal = GetOffset(digit);
			if (oldVal > newVal)
			{
				float time = (1 - oldVal + newVal) * 10 * deltaTime;
				iTween.ValueTo(gameObject, iTween.Hash("from", oldVal, "to", newVal + 1, "time", time, "onupdate", "OnUpdateTween", "onupdatetarget", gameObject));
			}
			else
			{
				float time = (newVal - oldVal) * 10 * deltaTime;
				iTween.ValueTo(gameObject, iTween.Hash("from", oldVal, "to", newVal, "time", time, "onupdate", "OnUpdateTween", "onupdatetarget", gameObject));
			}
		}
	}

	private void OnUpdateTween(float value)
	{
		if (value <= 0.9f)
		{
			main.mainTextureOffset = new Vector2(0, value);
		}
		else if (value > 0.9f && value < 1.0f)
		{
			main.mainTextureOffset = new Vector2(0, value);
			second.mainTextureOffset = new Vector2(0, value - 1.0f);
		}
		else if (value >= 1.0f)
		{
			if (second.mainTextureOffset != Vector2.one)
			{
				Material tmp = main;
				main = second;
				second = tmp;
				second.mainTextureOffset = Vector2.one;
			}
			main.mainTextureOffset = new Vector2(0, value - 1.0f);
		}
	}

	private float GetOffset(int value)
	{
		float ret = value * 0.1f;
		return ret;
	}
}
