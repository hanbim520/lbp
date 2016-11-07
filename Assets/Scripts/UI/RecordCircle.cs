using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RecordCircle : MonoBehaviour
{
	public int fieldsCount = 37;
	public Sprite[] bgs;
	public Sprite[] histogramColors;	// 柱状体颜色
	public GameObject flashImg;
	private Image[] triangles;
	private Image bgBall;
	private Text txtNum;

	private const float fixedFlashTime = 0.8f;
	private float flashDeltaTime = 0.0f;
	private bool bFlashEnable = false;

	void Start()
	{
		Init();
		HandleRefreshRecord();
		GameEventManager.RefreshRecord += HandleRefreshRecord;
		GameEventManager.EndCountdown += EndCountdown;
	}

	void OnDestroy()
	{
		GameEventManager.RefreshRecord -= HandleRefreshRecord;
		GameEventManager.EndCountdown -= EndCountdown;
	}

	void Update()
	{
		if (bFlashEnable)
		{
			flashDeltaTime += Time.deltaTime;
			if (fixedFlashTime <= flashDeltaTime)
			{
				flashDeltaTime = 0.0f;
				flashImg.SetActive(!flashImg.activeSelf);
			}
		}
		else if (!bFlashEnable && flashImg.activeSelf)
			flashImg.SetActive(false);
	}

	private void EndCountdown()
	{
		bFlashEnable = false;
	}

	private void Init()
	{
		triangles = new Image[fieldsCount];
		for (int i = 0; i < fieldsCount; ++i)
		{
			triangles[i] = transform.FindChild("T" + i).GetComponent<Image>();
		}
		bgBall = transform.FindChild("Bg").GetComponent<Image>();
		txtNum = transform.FindChild("Text").GetComponent<Text>();
	}

	private void HandleRefreshRecord(int result = -1)
	{
        StartCoroutine(RefreshView());
	}

    private IEnumerator RefreshView()
    {
		// 为了等冷热号处理结束
        yield return new WaitForSeconds(1.0f);
        int count = GameData.GetInstance().records.Count;
        int[] records = GameData.GetInstance().records.ToArray();
        if (count == 0)
        {
            foreach (Image item in triangles)
                item.fillAmount = 0;
            yield break;
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
            triangles[item.Key].fillAmount = (float)item.Value * 10 / sum;
            if (GameData.GetInstance().hotValues.Contains(item.Key))
				triangles[item.Key].overrideSprite = histogramColors[0];
            else if (GameData.GetInstance().coldValues.Contains(item.Key))
				triangles[item.Key].overrideSprite = histogramColors[1];
			else
				triangles[item.Key].overrideSprite = histogramColors[2];
        }
        int currentValue = records[count - 1];
        if (GameData.GetInstance().colorTable[currentValue] == ResultType.Red)
            bgBall.overrideSprite = bgs[0];
        else if (GameData.GetInstance().colorTable[currentValue] == ResultType.Black)
            bgBall.overrideSprite = bgs[1];
        else
            bgBall.overrideSprite = bgs[2];
        if (currentValue != 37)
            txtNum.text = currentValue.ToString();
        else
            txtNum.text = "00";

		bFlashEnable = true;
		Transform targetImg = triangles[currentValue].transform;
		flashImg.transform.localPosition = targetImg.localPosition;
		flashImg.transform.localRotation = targetImg.localRotation;
		flashImg.transform.localScale = Vector3.one;
    }
}
