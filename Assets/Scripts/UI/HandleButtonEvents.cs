using UnityEngine;
using System.Collections;

public class HandleButtonEvents : MonoBehaviour
{
	public GameObject setEN;
	public GameObject setCN;
	public GameObject displayClassic;
	public GameObject displayEllipse;

	void Start()
	{
		SetLanguage();
//		SetDisplay();
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
}
