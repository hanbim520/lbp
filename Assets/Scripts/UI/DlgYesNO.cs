using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DlgYesNO : MonoBehaviour
{
	public GameObject en;
	public GameObject cn;
    public Transform mouseIcon;
    
	private GameObject downHitObject;
	
	void OnEnable()
	{
		SetLanguage();
	}

	private void SetLanguage()
	{
		if (GameData.GetInstance().language == 0)
		{
			if (en != null) en.SetActive(true);
			if (cn != null) cn.SetActive(false);
		}
		else 
		{
			if (en != null) en.SetActive(false);
			if (cn != null) cn.SetActive(true);
		}
	}

	public void FieldDownEvent(Transform hitObject)
	{
		SetAlpha(hitObject, 255);
	}

	public void FieldUpEvent(Transform hitObject)
	{
		SetAlpha(hitObject, 0);

		if (string.Equals(hitObject.name, "NO"))
			gameObject.SetActive(false);
		else if (string.Equals(hitObject.name, "Yes"))
		{
			BackTicket();
			gameObject.SetActive(false);
		}
	}

	private void SetAlpha(Transform target, int alpha)
	{
		Image field = target.GetComponent<Image>();
		Color c = field.color;
		c.a = alpha;
		field.color = c;
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
			if (pos == new Vector2(-1, -1))
				return;
			
			float sx, sy;
			Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
			RaycastHit2D[] hit = Physics2D.RaycastAll(new Vector2(sx, sy), Vector2.zero);
			if (hit.Length == 0)
				return;
			
			int idx = -1;
			if (hit.Length > 0)
			{
				for (int i = 0; i < hit.Length; ++i)
				{
					if (hit[i].collider.tag == "Dialog")
					{
						idx = i;
						break;
					}
				}
			}
			if (idx > -1 && hit[idx].collider != null)
			{
				hit[idx].collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit[idx].collider.transform);
				downHitObject = hit[idx].collider.gameObject;
			}
			
			mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
		}
		else if (InputEx.GetInputUp())
		{
			Vector2 pos;
			InputEx.InputUpPosition(out pos);
			if (pos == new Vector2(-1, -1))
				return;
			
			mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
			
			if (downHitObject != null)
			{
				downHitObject.GetComponent<ButtonEvent>().OnInputUp(downHitObject.transform);
			}
			downHitObject = null;
		}
	}

	// 退币功能
	private void BackTicket()
	{
		GameEventManager.OnPayCoin();
	}
}
