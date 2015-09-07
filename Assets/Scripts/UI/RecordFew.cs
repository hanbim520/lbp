using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RecordFew : MonoBehaviour
{
	private List<int> few = new List<int>();
	private int fewValue = 0;

	void Start()
	{
		HandleRefreshRecord();
		GameEventManager.RefreshRecord += HandleRefreshRecord;
	}

	void OnDestroy()
	{
		GameEventManager.RefreshRecord -= HandleRefreshRecord;
	}

	private void HandleRefreshRecord(int result = -1)
	{
		Dictionary<int, int> dict = new Dictionary<int, int>();
		for (int i = 0; i < 37; ++i)
		{
			dict.Add(i, 0);
		}
		foreach (int item in GameData.GetInstance().records)
		{
			dict[item] += 1;
		}
		foreach (var item in dict)
		{
			if (item.Value <= fewValue)
			{
				few.Add(item.Key);
			}
		}
	}

	private void RefreshFew()
	{

	}
}
