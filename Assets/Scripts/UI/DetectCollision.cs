using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class DetectCollision : MonoBehaviour
{
	private RectTransform[] validFieldsButtons;
	public RectTransform[] validFieldsMain1;
	public RectTransform[] validFieldsMain2;

	void Awake()
	{
		GameObject[] btns = GameObject.FindGameObjectsWithTag("Button");
		validFieldsButtons = new RectTransform[btns.Length];
		for (int i = 0; i < btns.Length; ++i)
			validFieldsButtons[i] = btns[i].GetComponent<RectTransform>();

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
				if (item.gameObject.activeInHierarchy && Utils.PointInRect(new Vector2(pos.x, pos.y), item))
				{
					item.GetComponent<ButtonEvent>().OnInputDown(item.transform);
					break;
				}
			}
		}
		else if (InputEx.GetInputUp())
		{
			Vector2 pos;
			InputEx.InputUpPosition(out pos);
			foreach (RectTransform item in validFieldsButtons)
			{
				if (item.gameObject.activeInHierarchy && Utils.PointInRect(new Vector2(pos.x, pos.y), item))
				{
					item.GetComponent<ButtonEvent>().OnInputUp(item.transform);
					break;
				}
			}

//            float sx, sy;
//            Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
//            RaycastHit2D hit = Physics2D.Raycast(new Vector2(sx, sy), Vector2.zero);
//            if (hit.collider != null)
//            {
//                print(hit.collider.name);
//            }
		}
	}
}
