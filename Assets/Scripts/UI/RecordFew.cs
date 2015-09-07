using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RecordFew : MonoBehaviour
{
    public GameObject[] fewGO;
    public Sprite[] images;
	private List<int> fewValues = new List<int>();
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
		for (int i = 0; i < 37; ++i)
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
        for (int i = 0; i < fixedCount; ++i)
        {
            fewValues.Add(lst[i].Key);
        }
        RefreshFew();
	}

	public void RefreshFew()
	{
        for (int i = 0; i < fewValues.Count; ++i)
        {
            if (i > fixedCount)
                break;

            fewGO[i].SetActive(true);
            if (GameData.GetInstance().colorTable[fewValues[i]] == ResultType.Red)
                fewGO[i].transform.FindChild("Image").GetComponent<Image>().sprite = images[0];
            else
                fewGO[i].transform.FindChild("Image").GetComponent<Image>().sprite = images[1];
            fewGO[i].transform.FindChild("Text").GetComponent<Text>().text = fewValues[i].ToString();
        }
	}
}
