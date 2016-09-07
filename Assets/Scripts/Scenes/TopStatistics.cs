using UnityEngine;
using System.Collections;

// 顶部路单屏
public class TopStatistics : MonoBehaviour
{
	private UStats ustats;
	
	void Start()
	{
		ustats = GetComponent<UStats>();
        SetLanguage();
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
		}
	}
}
