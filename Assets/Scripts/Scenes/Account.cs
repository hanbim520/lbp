using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 分机账逻辑
public class Account : MonoBehaviour
{
	public GameObject en;
	public GameObject cn;
	public Transform daybookIndexRoot;
	public Transform daybookItemRoot;

	void Start()
	{
		SetLanguage();
//		RecoverDaybook();
//		RecoverAccount();
	}

	private void SetLanguage()
	{
		if (GameData.GetInstance().language == 0)
		{
			if (en != null) en.SetActive(true);
			if (cn != null) cn.SetActive(false);
		}
		else
		{
			if (en != null) en.SetActive(false);
			if (cn != null) cn.SetActive(true);
		}
	}

	private void RecoverDaybook()
	{
		KeyinKeoutRecord[] records =	GameData.GetInstance().keyinKeoutRecords.ToArray();
		
		for (int i = 0; i < 20; ++i)
		{
			Object prefab = (Object)Resources.Load("Account/daybook item");
			GameObject go = (GameObject)Instantiate(prefab);
			go.transform.SetParent(daybookItemRoot);
			go.transform.localPosition = new Vector3(0, daybookIndexRoot.GetChild(i).localPosition.y, 0);
			go.transform.localScale = Vector3.one;
			go.transform.FindChild("time").GetComponent<Text>().text = records[i].time;
			go.transform.FindChild("keyin").GetComponent<Text>().text = records[i].keyin.ToString();
			go.transform.FindChild("keout").GetComponent<Text>().text = records[i].keout.ToString();
			go.transform.FindChild("toubi").GetComponent<Text>().text = records[i].toubi.ToString();
			go.transform.FindChild("tuibi").GetComponent<Text>().text = records[i].tuibi.ToString();
			go.transform.FindChild("card").GetComponent<Text>().text = records[i].card.ToString();
			prefab = null;
		}
	}

	private void RecoverAccount()
	{

	}
}
