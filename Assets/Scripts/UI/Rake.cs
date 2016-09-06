using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rake : MonoBehaviour
{
	private const float step = 650.0f;
	private float startX2 = -688.0f;
	private const float endX = 780.0f;
	private const float firstY = 58.0f;
	private const float secondY = -232.0f;
	private const float chipDeadline = 780.0f;
	private List<Transform> chips = new List<Transform>();
	private List<Transform> winChips;

	private int type = 1;	// 1--刮一次 2--刮两次
	private bool bStop = true;

	void Start()
	{
		GameEventManager.GameStart += HandleGameStart;
		GameEventManager.RakeInit += RakeInit;
	}

	void OnDestroy()
	{
		GameEventManager.GameStart -= HandleGameStart;
		GameEventManager.RakeInit -= RakeInit;
	}
	
	void Update()
	{
		if (bStop)
			return;
		float deltaX = Time.deltaTime * step;
		transform.localPosition += new Vector3(deltaX, 0, 0);
		for (int i = 0; i < chips.Count; ++i)
		{
			Transform t = chips[i];
			if (t.localPosition.x >= chipDeadline)
			{
				chips.RemoveAt(i);
				continue;
			}
			if (t != null && deltaX > 0)
				t.localPosition += new Vector3(deltaX, 0, 0);
			else
				chips.RemoveAt(i);
		}
		if (transform.localPosition.x > endX || 
		    Mathf.Approximately(transform.localPosition.x, endX))
		{
			if (type == 1)
				bStop = true;
			else if (type == 2)
			{
				if (Mathf.Approximately(transform.localPosition.y, firstY))
				{
					Vector3 p = transform.localPosition;
					p.x = startX2;
					p.y = secondY;
					transform.localPosition = p;
				}
				else if (Mathf.Approximately(transform.localPosition.y, secondY))
					bStop = true;
			}
		}

	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (!winChips.Contains(other.transform) && 
		    other.tag == "Field Chip")
			chips.Add(other.transform);
	}

	private void HandleGameStart ()
	{
		chips.Clear();
	}

	public void RakeInit(int type, int lineId, float startX1, float startX2, ref List<Transform> winChips)
	{
		chips.Clear();
		this.winChips = winChips;
		this.type = type;
		if (lineId == 1)
			transform.localPosition = new Vector3(startX1, firstY, 0);
		else if (lineId == 2)
			transform.localPosition = new Vector3(startX2, secondY, 0);
		this.startX2 = startX2;
		bStop = false;
	}
}
