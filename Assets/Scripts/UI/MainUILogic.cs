using UnityEngine;
using System.Collections;

public class MainUILogic : MonoBehaviour
{
	public GameObject setEN;
	public GameObject setCN;
	public GameObject fields37;
	public GameObject fields38;
	public GameObject backendTip;

	private GameObject displayClassic;
	private GameObject displayEllipse;
	private GameObject eraser;

	void Start()
	{
		Init();
		SetLanguage();
		SetDisplay();
		SetBetChips();
	}

	private void Init()
	{
		eraser = GameObject.Find("Canvas/eraser");
		eraser.SetActive(false);
	}

	public void ChangeLanguage(Transform hitObject)
	{
		if (GameData.GetInstance().language == 0)		// EN
		{
			GameData.GetInstance().language = 1;
		}
		else if (GameData.GetInstance().language == 1)	// CN
		{
			GameData.GetInstance().language = 0;
		}
		GameData.GetInstance().SaveLanguage();
		SetLanguage();
	}

	public void SetLanguage()
	{
		if (GameData.GetInstance().language == 0)		// EN
		{
			if (setEN != null) setEN.SetActive(true);
			if (setCN != null) setCN.SetActive(false);
		}
		else if (GameData.GetInstance().language == 1)	// CN
		{
			if (setEN != null) setEN.SetActive(false);
			if (setCN != null) setCN.SetActive(true);
		}
	}

	public void ChangeDisplay()
	{
		if (GameData.GetInstance().displayType == 0)	// classic
		{
			GameData.GetInstance().displayType = 1;
		}
		else if (GameData.GetInstance().displayType == 1)	// ellipse
		{
			GameData.GetInstance().displayType = 0;
		}
		GameData.GetInstance().SaveDisplayType();
		SetDisplay();
	}

	public void SetDisplay()
	{
		if (GameData.GetInstance().maxNumberOfFields == 37)
		{
			fields37.SetActive(true);
			fields38.SetActive(false);
			displayClassic = GameObject.Find("Canvas/37 Fields/Classic");
			displayEllipse = GameObject.Find("Canvas/37 Fields/Ellipse");
		}
		else if (GameData.GetInstance().maxNumberOfFields == 38)
		{
			fields37.SetActive(false);
			fields38.SetActive(true);
			displayClassic = GameObject.Find("Canvas/38 Fields/Classic");
			displayEllipse = GameObject.Find("Canvas/38 Fields/Ellipse");
		}

		if (GameData.GetInstance().displayType == 0)	// classic
		{
			if (displayClassic != null) displayClassic.SetActive(true);
			if (displayEllipse != null) displayEllipse.SetActive(false);
		}
		else if (GameData.GetInstance().displayType == 1)	// ellipse
		{
			if (displayClassic != null) displayClassic.SetActive(false);
			if (displayEllipse != null) displayEllipse.SetActive(true);
		}
	}

	public void SetBetChips()
	{
		GameObject root = GameObject.Find("BetChips");
		string path = "Bet Chips/";
		float y = -492.0f;
		float start = 0.0f, dist = 0.0f;
		int num = GameData.GetInstance().maxNumberOfChips;
		if (num == 6)
		{
			start = -644.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 5)
		{
			start = -594.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 4)
		{
			start = -544.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 3)
		{
			start = -494.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 2)
		{
			start = -544.0f;
			dist = 300;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 1)
		{
			start = -394.0f;
		}
		for (int i = 1; i <= num; ++i)
		{
			Object prefab = (Object)Resources.Load(path + "BetChip" + i);
			GameObject betChip = (GameObject)Instantiate(prefab);
			betChip.transform.SetParent(root.transform);
			betChip.transform.localPosition = new Vector3(start + (i - 1) * dist, y, 0);
			betChip.transform.localScale = Vector3.one;
			prefab = null;
		}
	}

	public void ClearEvent()
	{
		if (eraser != null) eraser.SetActive(true);
	}

	public void ClearAllEvent()
	{

	}

	public void RepeatEvent()
	{
		
	}
}
