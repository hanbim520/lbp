using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RecordCircle2 : MonoBehaviour
{
	public int fieldsCount = 37;
	public Sprite[] bgs;
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
                triangles[item.Key].color = new Color(1f, 0.3960f, 0.004f);
            else if (GameData.GetInstance().coldValues.Contains(item.Key))
				triangles[item.Key].color = new Color(0.4196f, 0.9960f, 0.9255f);
			else
				triangles[item.Key].color = new Color(0.6078f, 0.6392f, 0.6510f);
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
