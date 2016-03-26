using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// 出现最少的5个号
public class RecordFew : MonoBehaviour
{
    public GameObject[] fewGO;
    public Sprite[] images;
	private Dictionary<int, int> fewValues = new Dictionary<int, int>();
    private int fixedCount;

	void Start()
	{
        fixedCount = fewGO.Length;
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
            foreach (GameObject item in fewGO)
            {
                item.SetActive(false);
            }
            return;
        }

        Dictionary<int, int> dict = new Dictionary<int, int>();
		int num = GameData.GetInstance().maxNumberOfFields;
		for (int i = 0; i < num; ++i)
		{
			dict.Add(i, 0);
		}
		foreach (int item in GameData.GetInstance().records)
		{
			dict[item] += 1;
		}
        List<KeyValuePair<int, int>> lst = new List<KeyValuePair<int, int>>(dict);
        lst.Sort(delegate(KeyValuePair<int, int> s1, KeyValuePair<int, int> s2) {return s1.Value.CompareTo(s2.Value);});

        fewValues.Clear();
        GameData.GetInstance().coldValues.Clear();
		foreach(KeyValuePair<int, int> kvp in lst)
        {
			fewValues.Add(kvp.Key, kvp.Value);
            if (kvp.Value > 0 && GameData.GetInstance().coldValues.Count < 5)
                GameData.GetInstance().coldValues.Add(kvp.Key);
        }

        RefreshView();
	}

	public void RefreshView()
	{
		int count = 0;
		foreach (int key in fewValues.Keys)
        {
            if (count >= fixedCount)
                break;

            if (!fewGO[count].activeSelf)
				fewGO[count].SetActive(true);
			if (GameData.GetInstance().colorTable[key] == ResultType.Red)
            {
				fewGO[count].transform.GetChild(0).GetComponent<Image>().overrideSprite = images[0];
				fewGO[count].transform.GetChild(0).FindChild("Text").GetComponent<Text>().text = key.ToString();
            }
			else if (GameData.GetInstance().colorTable[key] == ResultType.Black)
            {
				fewGO[count].transform.GetChild(0).GetComponent<Image>().overrideSprite = images[1];
				fewGO[count].transform.GetChild(0).FindChild("Text").GetComponent<Text>().text = key.ToString();
            }
            else
            {
				fewGO[count].transform.GetChild(0).GetComponent<Image>().overrideSprite = images[2];
				if (key == 0)
					fewGO[count].transform.GetChild(0).FindChild("Text").GetComponent<Text>().text = "0";
				else
					fewGO[count].transform.GetChild(0).FindChild("Text").GetComponent<Text>().text = "00";
            }
			fewGO[count].transform.GetChild(1).FindChild("Text").GetComponent<Text>().text = string.Format("x{0}", fewValues[key]);
			++count;
        }
	}
}
