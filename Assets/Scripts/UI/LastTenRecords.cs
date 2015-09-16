﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 录单场景记录
public class LastTenRecords : MonoBehaviour
{
	public GameObject[] records;
	public Sprite[] imagesSmall;
	public Sprite[] imagesBig;

	void Start()
	{
		InitRecords();
	}

	private void InitRecords()
	{
		int[] r = GameData.GetInstance().records.ToArray();
		int lastIdx = 0;
		if (r.Length <= records.Length)
			lastIdx = 0;
		else
			lastIdx = r.Length - records.Length;
		for (int i = r.Length - 1, j = 0; i <= lastIdx; --i, ++j)
		{
			Sprite[] imgs;
			if (j == 0)
				imgs = imagesBig;
			else
				imgs = imagesSmall;
			
			if (!records[j].activeSelf)
				records[j].SetActive(true);
			if (GameData.GetInstance().colorTable[r[i]] == ResultType.Red)
			{
				records[j].transform.FindChild("Image").GetComponent<Image>().overrideSprite = imgs[0];
				records[j].transform.FindChild("Text").GetComponent<Text>().text = r[i].ToString();
				Vector3 pos = records[j].transform.localPosition;
				records[j].transform.localPosition = new Vector3(pos.x, -467, 0);
			}
			else if (GameData.GetInstance().colorTable[r[i]] == ResultType.Black)
			{
				records[j].transform.FindChild("Image").GetComponent<Image>().overrideSprite = imgs[1];
				records[j].transform.FindChild("Text").GetComponent<Text>().text = r[i].ToString();
				Vector3 pos = records[j].transform.localPosition;
				records[j].transform.localPosition = new Vector3(pos.x, -494, 0);
			}
			else
			{
				if (r[i] == 0)
					records[j].transform.FindChild("Image").GetComponent<Image>().overrideSprite = imgs[2];
				else
					records[j].transform.FindChild("Image").GetComponent<Image>().overrideSprite = imgs[3];
				records[j].transform.FindChild("Text").GetComponent<Text>().text = string.Empty;
				Vector3 pos = records[j].transform.localPosition;
				records[j].transform.localPosition = new Vector3(pos.x, -481, 0);
			}
		}
	}
}