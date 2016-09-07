using UnityEngine;
using System.Collections;

// 顶部路单屏
public class TopStatistics : MonoBehaviour
{
	private UStats ustats;
	
	void Start()
	{
		ustats = GetComponent<UStats>();
	}

	public void HandleRecData(int instr, ref string[] words)
	{
		if (instr == NetInstr.SyncLottery)
		{
			int totalLottery;
			if (int.TryParse(words[1], out totalLottery))
				GameEventManager.OnLotteryChange(totalLottery);
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
