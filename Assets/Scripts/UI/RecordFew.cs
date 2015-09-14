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
        for (int i = 0; i < fixedCount; ++i)
        {
            fewValues.Add(lst[i].Key);
        }
        RefreshView();
	}

	public void RefreshView()
	{
        for (int i = 0; i < fewValues.Count; ++i)
        {
            if (i > fixedCount)
                break;

            if (!fewGO[i].activeSelf)
                fewGO[i].SetActive(true);
            if (GameData.GetInstance().colorTable[fewValues[i]] == ResultType.Red)
            {
                fewGO[i].transform.FindChild("Image").GetComponent<Image>().sprite = images[0];
                fewGO[i].transform.FindChild("Text").GetComponent<Text>().text = fewValues[i].ToString();
            }
            else if (GameData.GetInstance().colorTable[fewValues[i]] == ResultType.Black)
            {
                fewGO[i].transform.FindChild("Image").GetComponent<Image>().sprite = images[1];
                fewGO[i].transform.FindChild("Text").GetComponent<Text>().text = fewValues[i].ToString();
            }
            else
            {
				if (fewValues[i] == 0)
					fewGO[i].transform.FindChild("Image").GetComponent<Image>().sprite = images[2];
				else
					fewGO[i].transform.FindChild("Image").GetComponent<Image>().sprite = images[3];
				fewGO[i].transform.FindChild("Text").GetComponent<Text>().text = string.Empty;
            }
        }
	}
}
