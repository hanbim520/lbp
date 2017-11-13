using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// 出现最多的5个号
public class RecordMost : MonoBehaviour
{
    public GameObject[] mostGO;
    public Sprite[] images;
	private Dictionary<int, int> mostValues = new Dictionary<int, int>();
    private int fixedCount;
    
    void Start()
    {
        fixedCount = mostGO.Length;
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
            foreach (GameObject item in mostGO)
            {
                item.SetActive(false);
            }
            return;
        }
        
		mostValues.Clear();
		int num = GameData.GetInstance().maxNumberOfFields;
		for (int i = 0; i < num; ++i)
        {
			mostValues.Add(i, 0);
        }
        foreach (int item in GameData.GetInstance().records)
        {
			if (item == 37 && GameData.GetInstance().maxNumberOfFields == 37)
				continue;

			mostValues[item] += 1;
        }
		List<KeyValuePair<int, int>> lst = new List<KeyValuePair<int, int>>(mostValues);
        lst.Sort(delegate(KeyValuePair<int, int> s1, KeyValuePair<int, int> s2) {return s2.Value.CompareTo(s1.Value);});

		mostValues.Clear();
        GameData.GetInstance().hotValues.Clear();
		foreach(KeyValuePair<int, int> kvp in lst)
        {
			mostValues.Add(kvp.Key, kvp.Value);
            if (kvp.Value > 0 && GameData.GetInstance().hotValues.Count < 5)
                GameData.GetInstance().hotValues.Add(kvp.Key);
        }

        RefreshView();
    }
    
    public void RefreshView()
    {
		int count = 0;
		foreach (int key in mostValues.Keys)
        {
			if (count >= fixedCount || mostValues[key] == 0)
                break;

            if (!mostGO[count].activeSelf)
				mostGO[count].SetActive(true);
            if (GameData.GetInstance().colorTable[key] == ResultType.Red)
            {
				mostGO[count].transform.GetChild(0).GetComponent<Image>().overrideSprite = images[0];
				mostGO[count].transform.GetChild(0).Find("Text").GetComponent<Text>().text = key.ToString();
            }
            else if (GameData.GetInstance().colorTable[key] == ResultType.Black)
            {
				mostGO[count].transform.GetChild(0).GetComponent<Image>().overrideSprite = images[1];
				mostGO[count].transform.GetChild(0).Find("Text").GetComponent<Text>().text = key.ToString();
            }
            else
            {
				mostGO[count].transform.GetChild(0).GetComponent<Image>().overrideSprite = images[2];
				if (key == 0)
					mostGO[count].transform.GetChild(0).Find("Text").GetComponent<Text>().text = "0";
				else
					mostGO[count].transform.GetChild(0).Find("Text").GetComponent<Text>().text = "00";
            }
			mostGO[count].transform.GetChild(1).Find("Text").GetComponent<Text>().text = string.Format("x{0}", mostValues[key]);
			++count;
        }
    }
}
