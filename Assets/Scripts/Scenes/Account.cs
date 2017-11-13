using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

// 分机账逻辑
public class Account : MonoBehaviour
{
	public GameObject en;
	public GameObject cn;
	public Transform daybookIndexRoot;
    public Transform daybookItemRoot;
	public Transform accountRoot;

	private RectTransform mouseIcon;
	private GameObject downHitObject;
	private bool hitDownOpt = false;

	void Start()
	{
		mouseIcon = GameObject.Find("Canvas/mouse icon").GetComponent<RectTransform>();
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
			go.transform.Find("time").GetComponent<Text>().text = records[i].time;
			go.transform.Find("keyin").GetComponent<Text>().text = records[i].keyin.ToString();
			go.transform.Find("keout").GetComponent<Text>().text = records[i].keout.ToString();
			go.transform.Find("toubi").GetComponent<Text>().text = records[i].toubi.ToString();
			go.transform.Find("tuibi").GetComponent<Text>().text = records[i].tuibi.ToString();
			go.transform.Find("card").GetComponent<Text>().text = records[i].card.ToString();
			prefab = null;
		}
	}

	private void RecoverAccount()
	{
        int activeIdx = SetActiveTitles(accountRoot);
        accountRoot.GetChild(activeIdx).Find("keyin").GetComponent<Text>().text = GameData.GetInstance().zongShang.ToString();
        accountRoot.GetChild(activeIdx).Find("keout").GetComponent<Text>().text = GameData.GetInstance().zongXia.ToString();
        accountRoot.GetChild(activeIdx).Find("tou").GetComponent<Text>().text = GameData.GetInstance().zongTou.ToString();
        accountRoot.GetChild(activeIdx).Find("tui").GetComponent<Text>().text = GameData.GetInstance().zongTui.ToString();
        if (activeIdx == 0)
            accountRoot.GetChild(activeIdx).Find("winnings").GetComponent<Text>().text = GameData.GetInstance().currentWin.ToString();
        accountRoot.GetChild(activeIdx).Find("total winnings").GetComponent<Text>().text = GameData.GetInstance().totalWin.ToString();
        accountRoot.GetChild(activeIdx).Find("card").GetComponent<Text>().text = GameData.GetInstance().cardCredits.ToString();
	}

    private int SetActiveTitles(Transform root)
    {
        int activeIdx = GameData.controlCode ? 0 : 1;
        int nonactiveIdx = Mathf.Abs(activeIdx - 1);
        root.GetChild(activeIdx).gameObject.SetActive(true);
        root.GetChild(nonactiveIdx).gameObject.SetActive(false);
        return activeIdx;
    }

	public void ExitEvent(Transform hitObject)
	{
		SceneManager.LoadScene(Scenes.StartInfo);
	}

	void Update()
	{
        DetectInputEvents();
	}

	private void DetectInputEvents()
	{
		if (InputEx.GetInputDown() && !hitDownOpt)
		{
			Vector2 pos;
			InputEx.InputDownPosition(out pos);
			if (pos == new Vector2(-1, -1))
				return;
			
			float sx, sy;
			Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
			RaycastHit2D[] hit = Physics2D.RaycastAll(new Vector2(sx, sy), Vector2.zero);
			if (hit.Length == 0)
				return;
			
			int idx = 0;
			if (hit.Length > 1)
			{
				for (int i = 0; i < hit.Length; ++i)
				{
					if (hit[i].collider.tag == "Dialog")
					{
						idx = i;
						break;
					}
					else if (hit[i].collider.gameObject.GetComponent<ButtonEvent>() != null)
						idx = i;
				}
			}
			if (hit[idx].collider != null)
			{
				hit[idx].collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit[idx].collider.transform);
				downHitObject = hit[idx].collider.gameObject;
			}
			
			mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
			hitDownOpt = true;
		}
		else if (InputEx.GetInputUp() && hitDownOpt)
		{
			Vector2 pos;
			InputEx.InputUpPosition(out pos);
			if (pos == new Vector2(-1, -1))
				return;
			
			mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
			
			if (downHitObject != null)
			{
				downHitObject.GetComponent<ButtonEvent>().OnInputUp(downHitObject.transform);
			}
			hitDownOpt = false;
		}
	}
}
