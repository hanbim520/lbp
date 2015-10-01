using UnityEngine;
using System.Collections;

public class Calc : MonoBehaviour
{
	public GameObject cn;
	public GameObject en;
	void OnEnable()
	{
		if (GameData.GetInstance().language == 0)
		{
			if (cn != null) cn.SetActive(false);
			if (en != null) en.SetActive(true);
		}
		else
		{
			if (cn != null) cn.SetActive(true);
			if (en != null) en.SetActive(false);
		}
	}
}
