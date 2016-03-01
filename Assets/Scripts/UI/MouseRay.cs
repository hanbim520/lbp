using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MouseRay : MonoBehaviour
{
	void FixedUpdate()
	{
		Vector3 pos = transform.localPosition;
		float sx, sy;
		Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
		RaycastHit2D[] hit = Physics2D.RaycastAll(new Vector2(sx, sy), Vector2.zero);
		if (hit.Length == 0)
		{
			GameEventManager.OnOddsPrompt(0);
			return;
		}

		bool bFound = false;
		for (int i = 0; i < hit.Length; ++i)
		{
			if (hit[i].collider.tag == "Dialog")	// 弹出框按钮 相当于Dialog button
			{
				GameEventManager.OnOddsPrompt(0);
				return;
			}
			else if (hit[i].collider.gameObject.GetComponent<ButtonEvent>() != null)
			{
				string strEvent = hit[i].collider.gameObject.GetComponent<ButtonEvent>().inputUpEvent;
				if (string.Compare(strEvent, "FieldClickEvent") == 0)
				{
					bFound = true;
					string name = hit[i].collider.name;
					if (name.Contains("-"))
						break;
					else
					{
						GameEventManager.OnOddsPrompt(Utils.GetOdds(name));
						return;
					}
				}
			}
		}
		if (!bFound)
			GameEventManager.OnOddsPrompt(0);
	}
}
