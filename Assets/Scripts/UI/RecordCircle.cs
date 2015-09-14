using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RecordCircle : MonoBehaviour
{
	public int fieldsCount = 37;
	private Image[] triangles;

	void Start()
	{
		Init();
		HandleRefreshRecord();
		GameEventManager.RefreshRecord += HandleRefreshRecord;
	}

	void OnDestroy()
	{
		GameEventManager.RefreshRecord -= HandleRefreshRecord;
	}

	private void Init()
	{
		triangles = new Image[fieldsCount];
		for (int i = 0; i < fieldsCount; ++i)
		{
			triangles[i] = transform.FindChild("T" + i).GetComponent<Image>();
		}
	}

	private void HandleRefreshRecord(int result = -1)
	{
		int count = GameData.GetInstance().records.Count;
		if (count == 0)
		{
			foreach (Image item in triangles)
				item.fillAmount = 0;
			return;
		}
		int sum = 100;
		Dictionary<int, int> dict = new Dictionary<int, int>();
		for (int i = 0; i < fieldsCount; ++i)
		{
			dict.Add(i, 0);
		}
		foreach (int item in GameData.GetInstance().records)
		{
			dict[item] += 1;
		}
		foreach (var item in dict)
		{
			triangles[item.Key].fillAmount = (float)item.Value / sum;
		}
	}
}
