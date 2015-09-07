using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Records : MonoBehaviour 
{
    public GameObject[] records;
    public Sprite[] imagesSmall;
    public Sprite[] imagesBig;

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
        if (GameData.GetInstance().records.Count == 0)
        {
            foreach (GameObject item in records)
                item.SetActive(false);
            return;
        }

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
                records[j].transform.FindChild("Image").GetComponent<Image>().sprite = imgs[0];
                records[j].transform.FindChild("Text").GetComponent<Text>().text = r[i].ToString();
                records[j].transform.FindChild("Text").localPosition = new Vector3(0, 14, 0);
            }
            else if (GameData.GetInstance().colorTable[r[i]] == ResultType.Black)
            {
                records[j].transform.FindChild("Image").GetComponent<Image>().sprite = imgs[1];
                records[j].transform.FindChild("Text").GetComponent<Text>().text = r[i].ToString();
                records[j].transform.FindChild("Text").localPosition = new Vector3(0, -14, 0);
            }
            else
            {
                records[j].transform.FindChild("Image").GetComponent<Image>().sprite = imgs[2];
                records[j].transform.FindChild("Text").GetComponent<Text>().text = string.Empty;
                records[j].transform.FindChild("Text").localPosition = Vector3.zero;
            }
        }
    }
}
