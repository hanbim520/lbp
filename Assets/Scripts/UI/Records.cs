using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// 20场记录
public class Records : MonoBehaviour 
{
    public GameObject[] records;
    public Sprite[] imagesSmall;
    public Sprite[] imagesBig;
    public float redPosY = 24.0f;
    public float greenPosY = 12.0f;
    public float blackPosY = 1.0f; 

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
        for (int i = r.Length - 1, j = 0; i >= lastIdx; --i, ++j)
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
				records[j].transform.Find("Image").GetComponent<Image>().overrideSprite = imgs[0];
                records[j].transform.Find("Text").GetComponent<Text>().text = r[i].ToString();
				Vector3 pos = records[j].transform.localPosition;
				if (j != 0)
					records[j].transform.localPosition = new Vector3(pos.x, redPosY, 0);
				else
					records[j].transform.localPosition = new Vector3(pos.x, greenPosY, 0);
            }
            else if (GameData.GetInstance().colorTable[r[i]] == ResultType.Black)
            {
				records[j].transform.Find("Image").GetComponent<Image>().overrideSprite = imgs[1];
                records[j].transform.Find("Text").GetComponent<Text>().text = r[i].ToString();
				Vector3 pos = records[j].transform.localPosition;
				if (j != 0)
					records[j].transform.localPosition = new Vector3(pos.x, blackPosY, 0);
				else
					records[j].transform.localPosition = new Vector3(pos.x, greenPosY, 0);
            }
            else
            {
				records[j].transform.Find("Image").GetComponent<Image>().overrideSprite = imgs[2];
				string text = "0";
				if (r[i] != 0)
					text = "00";
                records[j].transform.Find("Text").GetComponent<Text>().text = text;
				Vector3 pos = records[j].transform.localPosition;
				records[j].transform.localPosition = new Vector3(pos.x, greenPosY, 0);
            }
        }
    }
}
