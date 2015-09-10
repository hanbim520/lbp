using UnityEngine;
using System.Collections;

public class HandleButtonEvents : MonoBehaviour
{
	public GameObject setEN;
	public GameObject setCN;

	void Start()
	{

	}

	public void ChangeLanguage(Transform hitObject)
	{
		if (GameData.GetInstance().language == 0)		// EN
		{
			GameData.GetInstance().language = 1;
			GameData.GetInstance().SaveLanguage();
		}
		else if (GameData.GetInstance().language == 1)	// CN
		{
			GameData.GetInstance().language = 0;
			GameData.GetInstance().SaveLanguage();
		}
		SetLanguage();
	}

	public void SetLanguage()
	{
		if (GameData.GetInstance().language == 0)		// EN
		{
			setEN.SetActive(true);
			setCN.SetActive(false);
		}
		else if (GameData.GetInstance().language == 1)	// CN
		{
			setEN.SetActive(false);
			setCN.SetActive(true);
		}
	}
}
