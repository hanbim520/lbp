﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RecordCircle : MonoBehaviour
{
	public int fieldsCount = 37;
	public Sprite[] bgs;
	private Image[] triangles;
	private Image bgBall;

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
		bgBall = transform.FindChild("Bg").GetComponent<Image>();
	}

	private void HandleRefreshRecord(int result = -1)
	{
		int count = GameData.GetInstance().records.Count;
		int[] records = GameData.GetInstance().records.ToArray();
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
		foreach (int item in records)
		{
			dict[item] += 1;
		}
		foreach (var item in dict)
		{
			triangles[item.Key].fillAmount = (float)item.Value / sum;
		}
		int currentValue = records[count - 1];
		if (GameData.GetInstance().colorTable[currentValue] == ResultType.Red)
			bgBall.overrideSprite = bgs[0];
		else if (GameData.GetInstance().colorTable[currentValue] == ResultType.Black)
			bgBall.overrideSprite = bgs[1];
		else
			bgBall.overrideSprite = bgs[2];
	}
}
