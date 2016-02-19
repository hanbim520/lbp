using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecordBar : MonoBehaviour
{
    public ResultType redBar;

	private Image imgRed;
	private Image imgBlue;
	private Image imgGreen;

	void Start ()
    {
		Init();
        RegisterEvents();
	}

    void OnDestroy()
    {
        UnregisterEvents();
    }

	private void Init()
	{
		imgRed = transform.FindChild("Red").GetComponent<Image>();
		imgBlue = transform.FindChild("Blue").GetComponent<Image>();
		imgGreen = transform.FindChild("Green").GetComponent<Image>();
		RefreshRecord();
	}

    private void RegisterEvents()
    {
        GameEventManager.RefreshRecord += RefreshRecord;
    }

    private void UnregisterEvents()
    {
        GameEventManager.RefreshRecord -= RefreshRecord;
    }

    private void RefreshRecord(int result = -1)
    {
		int count = GameData.GetInstance().records.Count;
		if (count == 0)
		{
			imgRed.fillAmount = 0.42f;
			imgBlue.fillAmount = 0.42f;
			imgGreen.fillAmount = 0.16f + 0.42f;
			imgRed.transform.GetChild(0).GetComponent<RecordBarText>().Refresh(0);
			imgBlue.transform.GetChild(0).GetComponent<RecordBarText>().Refresh(0);
//			imgGreen.transform.GetChild(0).GetComponent<RecordBarText>().Refresh(0);
			imgRed.transform.GetChild(1).GetComponent<RecordBarText>().Refresh(0);
			return;
		}

		int sumRed = 0, sumBlue = 0, sumGreen = 0;
		foreach (int item in GameData.GetInstance().records)
		{
			if (item == 0 || item == 37)
				++sumGreen;
			else
			{
				if (redBar == ResultType.Red)
				{
					ResultType type;
					if (GameData.GetInstance().colorTable.TryGetValue(item, out type))
					{
						if (type == ResultType.Red)
							++sumRed;
						else
							++sumBlue;
					}
				}
				else if (redBar == ResultType.Even)
				{
					if (item % 2 == 0)
						++sumRed;
					else
						++sumBlue;
				}
				else if (redBar == ResultType.Serial1_18) 
				{
					if (item > 0 && item < 19)
						++sumRed;
					else
						++sumBlue;
				}
			}
		}
		float rPersentage = (float)sumRed / count;
		float gPersentage = (float)sumGreen / count;
		float bPersentage = (float)sumBlue / count;
        float minFillAmount = 0.16f;
        float maxFillAmount = 0.68f;
        imgRed.fillAmount = Mathf.Clamp(rPersentage - minFillAmount / 2, minFillAmount, maxFillAmount);
        imgBlue.fillAmount = Mathf.Clamp(bPersentage - minFillAmount / 2, minFillAmount, maxFillAmount);
        imgGreen.fillAmount = Mathf.Clamp(minFillAmount + gPersentage + imgRed.fillAmount, minFillAmount * 2, 1.0f - minFillAmount);

		int r = Mathf.RoundToInt(rPersentage * 100);
		int g = Mathf.RoundToInt(gPersentage * 100);
		int b = 100 - r - g;
		imgRed.transform.GetChild(0).GetComponent<RecordBarText>().Refresh(r);
		imgBlue.transform.GetChild(0).GetComponent<RecordBarText>().Refresh(b);
//		imgGreen.transform.GetChild(0).GetComponent<RecordBarText>().Refresh(g);
		// Refresh green text
		imgRed.transform.GetChild(1).GetComponent<RecordBarText>().Refresh(g);
    }
}
