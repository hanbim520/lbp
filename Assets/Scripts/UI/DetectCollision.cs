using UnityEngine;
using System.Collections;

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
			print("1");
			RaycastHit hit;
			if (Physics.Raycast(pos, Vector3.back, out hit)) {
				print(hit.collider.name);
			}
			print("2");
		}
	}
}
