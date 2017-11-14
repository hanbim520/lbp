using UnityEngine;
using System.Collections;

public class SetEllipseBG : MonoBehaviour
{
	public GameObject[] objCircalRecords;

	void Start()
	{
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
}
