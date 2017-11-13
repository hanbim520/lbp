using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PathologicalGames;

public class ExtensionRecord : MonoBehaviour
{
    public GameObject[] language;       // 0:EN 1:CN
    public GameObject[] displayType;    // 0:37 1:38
    public GameObject[] fonts;          // 0:start credit, 1:end credit, 2:bet, 3:win
    public RectTransform mouseIcon;
	public LastTenRecords lastTenRecords;

    private GameObject downHitObject;
	private bool hitDownOpt = false;
    private int curRecordsIdx = 0;
	private Transform betChipsRoot;

    void Init()
    {
		curRecordsIdx = Mathf.Max(GameData.GetInstance().betRecords.Count - 1, 0);
		betChipsRoot = GameObject.Find("Canvas/BetChips").transform;
    }

	void Start()
    {
        Init();
        SetLanguage();
        SetDisplayType();
        RecoverBetRecords();
	}

	public void RightArrowEvent()
    {
        --curRecordsIdx;
        if (curRecordsIdx < 0)
        {
            if (GameData.GetInstance().betRecords.Count > 0)
                curRecordsIdx = GameData.GetInstance().betRecords.Count - 1;
            else
                curRecordsIdx = 0;
        }
        RecoverBetRecords();
    }

	public void LeftArrowEvent()
    {
        ++curRecordsIdx;
        if (curRecordsIdx >= GameData.GetInstance().betRecords.Count)
            curRecordsIdx = 0;
        RecoverBetRecords();
    }

    public void ExitEvent()
    {
        GameData.GetInstance().NextLevelName = Scenes.Main;
		UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.Loading);
    }

    public void LanguageEvent()
    {
        if (GameData.GetInstance().language == 0)
            GameData.GetInstance().language = 1;
        else
            GameData.GetInstance().language = 0;
        SetLanguage();
    }

    private void SetLanguage()
    {
        if (GameData.GetInstance().language == 0)
        {
            language[0].SetActive(true);
            language[1].SetActive(false);
        }
        else
        {
            language[0].SetActive(false);
            language[1].SetActive(true);
        }
    }

    private void SetDisplayType()
    {
        if (GameData.GetInstance().maxNumberOfFields == 37)
        {
            displayType[0].SetActive(true);
            displayType[1].SetActive(false);
        }
        else
        {
            displayType[0].SetActive(false);
            displayType[1].SetActive(true);
        }
    }

    private void RecoverBetRecords()
    {
        if (GameData.GetInstance().betRecords.Count == 0)
            return;

		// Refresh arrow
		lastTenRecords.MoveArrow(GameData.GetInstance().betRecords.Count - curRecordsIdx - 1);

		// Refresh strings
        BetRecord betRecord = GameData.GetInstance().betRecords[curRecordsIdx];
        fonts[0].GetComponent<Text>().text = betRecord.startCredit.ToString();
        fonts[1].GetComponent<Text>().text = betRecord.endCredit.ToString();
        fonts[2].GetComponent<Text>().text = betRecord.bet.ToString();
        fonts[3].GetComponent<Text>().text = betRecord.win.ToString();
		fonts[4].GetComponent<Text>().text = betRecord.luckyWin.ToString();

		// Delete children of betChipsRoot
		foreach (Transform child in betChipsRoot)
			Destroy(child.gameObject);

		// Recover bet fields
		Transform root = displayType[0].activeSelf ? displayType[0].transform.Find("Valid Fields") : displayType[1].transform.Find("Valid Fields");
		if (root != null) 
		{
			string path = "Bet Chips/";
			int count = GameData.GetInstance().betChipValues.Count;
			foreach (BetInfo info in betRecord.bets)
			{
				Transform location = root.Find(info.betField);
				if (location != null)
				{
					int chipIdx = 0;
					for (int i = 0; i < count; ++i)
					{
						if (info.betValue >= GameData.GetInstance().betChipValues[i])
							chipIdx = i;
					}
//					Object prefab = (Object)Resources.Load(path + "BetChip" + chipIdx.ToString());
//					GameObject chip = (GameObject)Instantiate(prefab); 
					GameObject chip = PoolManager.Pools["Stuff"].Spawn("BetChip" + chipIdx.ToString()).gameObject;
					chip.transform.SetParent(betChipsRoot);
					chip.transform.localPosition = location.localPosition;
					chip.transform.localScale = Vector3.one;
					chip.transform.GetChild(0).GetComponent<Text>().text = info.betValue.ToString();
//					prefab = null;
				}
				else
				{
					print("can't find:" + info.betField);
				}
			}
		}
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
