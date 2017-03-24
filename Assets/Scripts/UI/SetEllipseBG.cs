using UnityEngine;
using System.Collections;

public class SetEllipseBG : MonoBehaviour
{
	public GameObject[] objCircalRecords;

	void Start()
	{
		LoadEllipsePrefabs();
		SetRouletteType();
	}

	void SetRouletteType()
	{
		if (GameData.rouletteType == RouletteType.Standard)
		{
			objCircalRecords[0].SetActive(true);
			objCircalRecords[1].SetActive(false);
		}
		else if (GameData.rouletteType == RouletteType.Special1)
		{
			objCircalRecords[0].SetActive(false);
			objCircalRecords[1].SetActive(true);
		}
	}

	void LoadEllipsePrefabs()
	{
		string prefabPath = "";
		if (GameData.rouletteType == RouletteType.Standard)
		{
			prefabPath = "Ellipse38/Standard/Choose Effect";
		}
		else if (GameData.rouletteType == RouletteType.Special1)
		{
			prefabPath = "Ellipse38/Speial1/Choose Effect";
		}
		Object prefab = (Object)Resources.Load(prefabPath);
		GameObject objChooseEffect = (GameObject)Instantiate(prefab);
		objChooseEffect.name = "Choose Effect";
		objChooseEffect.transform.SetParent(transform);
		objChooseEffect.transform.localPosition = Vector3.zero;
		objChooseEffect.transform.localScale = Vector3.one;

		GameObject uilogic = GameObject.Find("UILogic");
		for (int i = 0; i <= 37; ++i)
		{
			string name = i != 37 ? string.Format("e{0}", i) : "e00";
//			print(name);
			objChooseEffect.transform.FindChild(name).GetComponent<ButtonEvent>().receiver = uilogic;
		}
	}
}
