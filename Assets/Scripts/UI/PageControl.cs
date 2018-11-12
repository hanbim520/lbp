using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PageControl : MonoBehaviour
{
	public int pageCount = 1;
	public GameObject[] objBGPages;
	public GameObject[] objValidsPages;
	public GameObject[]	objENPages;
	public GameObject[] objCNPages;
	public Text txtPage;
	public GameObject btnRight;
	public GameObject btnLeft;
	public Transform objCalc;

	public int curPage {get;set;}
	private int lastPage;

	void OnEnable()
	{
		lastPage = curPage = 0;
		RefreshPage();
	}

	void NextPage()
	{
		lastPage = curPage++;
		if (curPage >= pageCount)
			curPage = pageCount - 1;
		else
			RefreshPage();
	}

	void PrePage()
	{
		lastPage = curPage--;
		if (curPage < 0)
			curPage = 0;
		else
			RefreshPage();
	}

	void RefreshPage()
	{
		for (int i = 0; i < pageCount; ++i)
		{
			if (curPage == i)
			{
				objBGPages[i].SetActive(true);
				objValidsPages[i].SetActive(true);
			}
			else
			{
				objBGPages[i].SetActive(false);
				objValidsPages[i].SetActive(false);
			}
		}
		SetLanguage();
		txtPage.text = string.Format("{0}/{1}", curPage + 1, pageCount);
		if (curPage == 0)
		{
			objCalc.localPosition = new Vector3(0, -26, 0);
		}
		else if (curPage == 1)
		{
			objCalc.localPosition = new Vector3(-526, -26, 0);
		}
	}

	void SetLanguage()
	{
        if (GameData.GetInstance().backendLanguage == 0)
		{
			foreach (GameObject obj in objCNPages)
				obj.SetActive(false);
			for (int i = 0; i < objENPages.Length; ++i)
			{
				if (i == curPage)
					objENPages[i].SetActive(true);
				else
					objENPages[i].SetActive(false);
			}
		}
		else
		{
			foreach (GameObject obj in objENPages)
				obj.SetActive(false);
			for (int i = 0; i < objCNPages.Length; ++i)
			{
				if (i == curPage)
					objCNPages[i].SetActive(true);
				else
					objCNPages[i].SetActive(false);
			}
		}
	}
}
