using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RecordCircle2 : MonoBehaviour
{
	public int fieldsCount = 38;
	public Sprite[] bgs;				// 球的颜色
	public Sprite[] histogramColors;	// 柱状体颜色
	public GameObject flashImg;			// 当前球号闪烁标志
	private Image[] triangles;			// 柱状图
	private Image bgBall;				// 当前球的颜色
	private Text txtNum;				// 当前球的号码

	private const float fixedFlashTime	= 0.8f;
	private float flashDeltaTime 		= 0.0f;
	private bool bFlashEnable 			= false;

	void Start()
	{
		Init();
		HandleRefreshRecord();
		GameEventManager.RefreshRecord += HandleRefreshRecord;
		GameEventManager.StartCountdown += StartCountdown;
	}

	void OnDestroy()
	{
		GameEventManager.RefreshRecord -= HandleRefreshRecord;
		GameEventManager.StartCountdown -= StartCountdown;
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

	private void StartCountdown()
	{
		bFlashEnable = false;
	}

	private void Init()
	{
		triangles = new Image[fieldsCount];
		for (int i = 0; i < fieldsCount; ++i)
		{
			triangles[i] = transform.FindChild("histogram").FindChild(i.ToString()).GetComponent<Image>();
		}
		bgBall = transform.FindChild("current ball").FindChild("bg").GetComponent<Image>();
		txtNum = transform.FindChild("current ball").FindChild("num").GetComponent<Text>();
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
		// 计算各个球号的出现次数
        Dictionary<int, int> dict = new Dictionary<int, int>();
        for (int i = 0; i < fieldsCount; ++i)
        {
            dict.Add(i, 0);
        }
        foreach (int item in records)
        {
            dict[item] += 1;
        }
		// 设置柱状体长度和颜色
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
		// 设置球颜色
        if (GameData.GetInstance().colorTable[currentValue] == ResultType.Red)
            bgBall.overrideSprite = bgs[0];
        else if (GameData.GetInstance().colorTable[currentValue] == ResultType.Black)
            bgBall.overrideSprite = bgs[1];
        else
            bgBall.overrideSprite = bgs[2];
		// 设置球号
        if (currentValue != 37)
            txtNum.text = currentValue.ToString();
        else
            txtNum.text = "00";

		// 闪烁当前的号码
		bFlashEnable = true;
		Transform targetImg = triangles[currentValue].transform;
		flashImg.transform.localPosition = targetImg.localPosition;
		flashImg.transform.localRotation = targetImg.localRotation;
		flashImg.transform.localScale = Vector3.one;

		// 显示各个球号的出现次数
		Transform timesRoot = transform.FindChild("times");
		if (timesRoot != null)
		{
			for (int i = 0; i < fieldsCount; ++i)
			{
				Transform item = timesRoot.FindChild(i.ToString());
				item.GetComponent<Text>().text = dict[i].ToString();
			}
		}
		// 计算各个区域的百分比
		Transform percentsRoot = transform.FindChild("percents");
		if (percentsRoot != null)
		{
			int _1to18 = 0;
			int _green = 0;
			int _red   = 0;
			int _odd   = 0;
			int _1st12 = 0;
			int _2nd12 = 0;
			foreach (int item in records)
			{
				if (item  == 0 || item == 37)
					++_green;
				else if (item % 2 != 0)
					++_odd;
				if (item >= 1 && item <= 18)
					++_1to18;
				if (item >= 1 && item <= 12)
					++_1st12;
				else if (item >= 13 && item <= 24)
					++_2nd12;
				if (GameData.GetInstance().colorTable[item] == ResultType.Red)
					++_red;
			}
			percentsRoot.FindChild("1to18").GetComponent<Text>().text = _1to18.ToString();
			int _19to36 = sum - _1to18 - _green;
			percentsRoot.FindChild("19to36").GetComponent<Text>().text = _19to36.ToString();
			percentsRoot.FindChild("odd").GetComponent<Text>().text = _odd.ToString();
			int _even = sum - _odd - _green;
			percentsRoot.FindChild("even").GetComponent<Text>().text = _even.ToString();
			int _3rd12 = sum - _1st12 - _2nd12;
			percentsRoot.FindChild("3rd12").GetComponent<Text>().text = _3rd12.ToString();
			percentsRoot.FindChild("2nd12").GetComponent<Text>().text = _2nd12.ToString();
			percentsRoot.FindChild("1st12").GetComponent<Text>().text = _1st12.ToString();
			percentsRoot.FindChild("green").GetComponent<Text>().text = _green.ToString();
			percentsRoot.FindChild("red").GetComponent<Text>().text = _red.ToString();
			int _black = sum - _red - _green;
			percentsRoot.FindChild("black").GetComponent<Text>().text = _black.ToString();
		}
    }
}
