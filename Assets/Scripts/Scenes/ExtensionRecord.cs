using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExtensionRecord : MonoBehaviour
{
    public GameObject[] language;       // 0:EN 1:CN
    public GameObject[] displayType;    // 0:EN 1:CN
    public GameObject[] fonts;          // 0:start credit, 1:end credit, 2:bet, 3:win
    public GameObject animArrow;

    private int curRecordsIdx = 0;

	void Start()
    {
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
        Application.LoadLevel("StartInfo");
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
}
