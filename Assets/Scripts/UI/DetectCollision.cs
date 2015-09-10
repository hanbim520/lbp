using UnityEngine;
using System.Collections;

public class DetectCollision : MonoBehaviour
{
	public RectTransform[] validFieldsButtons;
	public RectTransform[] validFieldsMain1;
	public RectTransform[] validFieldsMain2;

	void Start()
	{
	
	}
	
	void Update()
	{
		DetectInputEvents();
	}

	private void DetectInputEvents()
	{
		if (InputEx.GetInputDown())
		{
			Vector2 pos;
			InputEx.InputDownPosition(out pos);
			foreach (RectTransform item in validFieldsButtons)
			{
				if (Utils.PointInRect(new Vector2(pos.x, pos.y), item))
				{
					item.GetComponent<ButtonEvent>().OnInputDown(item.transform);
				}
			}
		}
		else if (InputEx.GetInputUp())
		{
			Vector2 pos;
			InputEx.InputUpPosition(out pos);
			foreach (RectTransform item in validFieldsButtons)
			{
				if (Utils.PointInRect(new Vector2(pos.x, pos.y), item))
				{
					item.GetComponent<ButtonEvent>().OnInputUp(item.transform);
				}
			}
		}
	}
}
