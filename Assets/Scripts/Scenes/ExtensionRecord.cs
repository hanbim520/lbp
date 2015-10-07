using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExtensionRecord : MonoBehaviour
{
    public GameObject[] language;       // 0:EN 1:CN
    public GameObject[] displayType;    // 0:37 1:38
    public GameObject[] fonts;          // 0:start credit, 1:end credit, 2:bet, 3:win
    public GameObject animArrow;
    public RectTransform mouseIcon;

    private GameObject downHitObject;
    private int curRecordsIdx = 0;

    void Init()
    {
        //        GameData.GetInstance().records.Enqueue(23);
        //        GameData.GetInstance().records.Enqueue(2);
        //        GameData.GetInstance().records.Enqueue(3);
        //        GameData.GetInstance().records.Enqueue(28);
        //        GameData.GetInstance().records.Enqueue(4);
        GameData.GetInstance().records.Enqueue(31);
        GameData.GetInstance().records.Enqueue(18);
        GameData.GetInstance().records.Enqueue(0);
        GameData.GetInstance().records.Enqueue(32);
        GameData.GetInstance().records.Enqueue(16);
        GameData.GetInstance().records.Enqueue(37);
        GameData.GetInstance().records.Enqueue(36);
    }


	void Start()
    {
        Init();
        SetLanguage();
        SetDisplayType();
        RecoverBetRecords();
	}

    public void LeftArrowEvent()
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

    public void RightArrowEvent()
    {
        ++curRecordsIdx;
        if (curRecordsIdx > GameData.GetInstance().betRecords.Count)
            curRecordsIdx = 0;
        RecoverBetRecords();
    }

    public void ExitEvent()
    {
        GameData.GetInstance().NextLevelName = Scenes.Main;
        Application.LoadLevel(Scenes.Loading);
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

        BetRecord betRecord = GameData.GetInstance().betRecords[curRecordsIdx];
        fonts[0].GetComponent<Text>().text = betRecord.startCredit.ToString();
        fonts[1].GetComponent<Text>().text = betRecord.endCredit.ToString();
        fonts[2].GetComponent<Text>().text = betRecord.bet.ToString();
        fonts[3].GetComponent<Text>().text = betRecord.win.ToString();
    }

    void Update()
    {
        DetectInputEvents();
    }
    
    private void DetectInputEvents()
    {
        if (InputEx.GetInputDown())
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
                }
            }
            if (hit[idx].collider != null)
            {
                hit[idx].collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit[idx].collider.transform);
                downHitObject = hit[idx].collider.gameObject;
            }
            
            mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
        }
        else if (InputEx.GetInputUp())
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
            downHitObject = null;
        }
    }
}
