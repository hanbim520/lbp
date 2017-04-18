using UnityEngine;
using System.Collections;

// 顶部路单屏
public class TopStatistics : MonoBehaviour
{
	public GameObject[] objCircleRecords;	// 0:国际38孔排列 1:特殊38孔排列 2:37
	private UStats ustats;
	
	void Start()
	{
		ustats = GetComponent<UStats>();
        SetRouletteType();
        SetLanguage();
	}

    private void SetRouletteType()
    {
        if (GameData.GetInstance().maxNumberOfFields == 38)
        {
            if (GameData.rouletteType == RouletteType.Standard)
            {
                objCircleRecords[0].SetActive(true);
                objCircleRecords[1].SetActive(false);
            }
            else if (GameData.rouletteType == RouletteType.Special1)
            {
                objCircleRecords[0].SetActive(false);
                objCircleRecords[1].SetActive(true);
            }
            objCircleRecords[2].SetActive(false);
        }
        else
        {
            objCircleRecords[0].SetActive(false);
            objCircleRecords[1].SetActive(false);
            objCircleRecords[2].SetActive(true);
        }
    }

    private void SetLanguage()
    {
        GameObject cn = GameObject.Find("/Canvas/CN");
        GameObject en = GameObject.Find("/Canvas/EN");
        if (GameData.GetInstance().topScreenLanguage == 0)
        {
            cn.SetActive(false);
            en.SetActive(true);
        }
        else
        {
            cn.SetActive(true);
            en.SetActive(false);
        }
    }

	public void HandleRecData(int instr, ref string[] words)
	{
		if (instr == NetInstr.SyncLottery)
		{
			int totalLottery;
			if (int.TryParse(words[1], out totalLottery))
				GameEventManager.OnLotteryChange(totalLottery);
		}
        else if (instr == NetInstr.SyncTSLanguage)
        {
            int language;
            if (int.TryParse(words[1], out language))
            {
                GameData.GetInstance().topScreenLanguage = language;
                GameData.GetInstance().SaveMonitorLanguage();
                SetLanguage();
            }
        }
		else if (instr == NetInstr.GamePhase)
		{
			HandleGamePhase(ref words);
		}
		else if (instr == NetInstr.SyncData)
		{
			SyncData(ref words);
		}
		else if (instr == NetInstr.SyncRecords)
		{
			SyncLast100(ref words);
		}
	}

	private void SyncLast100(ref string[] words)
	{
		int idx = -1;
		for (int i = 1; i < words.Length; ++i)
		{
			int record;
			if (int.TryParse(words[i], out record))
			{
				++idx;
				PlayerPrefs.SetInt("R" + idx, record);
			}
		}
		PlayerPrefs.Save();
		if (idx < 99 && idx != -1)
		{
			for (int i = idx + 1; i <= 99; ++i)
				PlayerPrefs.DeleteKey("R" + i);
		}
		GameData.GetInstance().ReadRecords();
		GameEventManager.OnRefreshRecord(0);
	}

	private void SyncData(ref string[] words)
	{
		int lotteryDigit;
		int lotteryAlloc;
		int topScreenLanguge;
        int maxNumberOfFields;
        if(int.TryParse(words[20], out maxNumberOfFields))
            GameData.GetInstance().maxNumberOfFields = maxNumberOfFields;
		if (int.TryParse(words[26], out lotteryAlloc))
			GameData.GetInstance().lotteryAllocation = lotteryAlloc;
		if (int.TryParse(words[28], out lotteryDigit))
		{
			GameData.GetInstance().lotteryDigit = lotteryDigit;
			GameEventManager.OnLotteryChange(lotteryDigit);
		}
		if (int.TryParse(words[30], out topScreenLanguge))
			GameData.GetInstance().topScreenLanguage = topScreenLanguge;
		GameData.GetInstance().SaveSetting();
		SetLanguage();
        SetRouletteType();
	}

	private void HandleGamePhase(ref string[] words)
	{
		int phase;
		if (int.TryParse(words[1], out phase))
		{
			if (phase == GamePhase.ShowResult)
			{
				int value;
				if (int.TryParse(words[2], out value))
				{
					GameData.GetInstance().SaveRecord(value);
					GameEventManager.OnRefreshRecord(value);
				}
			}
			else if (phase == GamePhase.Countdown)
			{
				GameEventManager.OnStartCountdown();
			}
		}
	}

	public void ClearDeviceIndex()
	{
		PlayerPrefs.DeleteKey("deviceIndex");
	}
}
