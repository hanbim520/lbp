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
	public Transform accountRoot;

	void Start()
	{
		SetLanguage();
		RecoverDaybook();
		RecoverAccount();
	}

	private void SetLanguage()
	{
		if (GameData.GetInstance().language == 0)
		{
			if (en != null) en.SetActive(true);
			if (cn != null) cn.SetActive(false);
            SetActiveTitles(en.transform.GetChild(1));
		}
		else
		{
			if (en != null) en.SetActive(false);
			if (cn != null) cn.SetActive(true);
            SetActiveTitles(cn.transform.GetChild(1));
		}
	}

	private void RecoverDaybook()
	{
        if (GameData.GetInstance().keyinKeoutRecords.Count == 0)
            return;

		KeyinKeoutRecord[] records = GameData.GetInstance().keyinKeoutRecords.ToArray();
        int count = Mathf.Min(records.Length, 20);
		for (int i = 0; i < count; ++i)
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
        int activeIdx = SetActiveTitles(accountRoot);
        accountRoot.GetChild(activeIdx).FindChild("keyin").GetComponent<Text>().text = GameData.GetInstance().zongShang.ToString();
        accountRoot.GetChild(activeIdx).FindChild("keout").GetComponent<Text>().text = GameData.GetInstance().zongXia.ToString();
        accountRoot.GetChild(activeIdx).FindChild("tou").GetComponent<Text>().text = GameData.GetInstance().zongTou.ToString();
        accountRoot.GetChild(activeIdx).FindChild("tui").GetComponent<Text>().text = GameData.GetInstance().zongTui.ToString();
        if (activeIdx == 0)
            accountRoot.GetChild(activeIdx).FindChild("winnings").GetComponent<Text>().text = GameData.GetInstance().currentWin.ToString();
        accountRoot.GetChild(activeIdx).FindChild("total winnings").GetComponent<Text>().text = GameData.GetInstance().totalWin.ToString();
        accountRoot.GetChild(activeIdx).FindChild("card").GetComponent<Text>().text = GameData.GetInstance().cardCredits.ToString();
	}

    private int SetActiveTitles(Transform root)
    {
        int activeIdx = GameData.controlCode ? 0 : 1;
        int nonactiveIdx = Mathf.Abs(activeIdx - 1);
        root.GetChild(activeIdx).gameObject.SetActive(true);
        root.GetChild(nonactiveIdx).gameObject.SetActive(false);
        return activeIdx;
    }
}
