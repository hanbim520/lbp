using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LotteryNum : MonoBehaviour
{
	public RollingNumber[] cells;
	private int lotteryDigit;

	void Start()
	{
		GameEventManager.LotteryChange += HandleLotteryChange;
		ShowLottery();
	}

	void OnDestroy()
	{
		GameEventManager.LotteryChange -= HandleLotteryChange;
	}

	private void HandleLotteryChange(int digit)
	{
		GameData.GetInstance().lotteryDigit = digit;
		GameData.GetInstance().SaveLotteryDigit();
		ChangeLottery();
	}

	private void ShowLottery()
	{
        if (GameData.GetInstance().lotteryEnable)
		    lotteryDigit = GameData.GetInstance().lotteryDigit;
        else
            lotteryDigit = 0;
		int unit = Utils.FindNum(lotteryDigit, 1);
		int decade = Utils.FindNum(lotteryDigit, 2);
		int hundred = Utils.FindNum(lotteryDigit, 3);
		int kilobit = Utils.FindNum(lotteryDigit, 4);
		int myriabit = Utils.FindNum(lotteryDigit, 5);
		int hundredThousand = Utils.FindNum(lotteryDigit, 6);

		cells[0].SetDigit(unit);
		cells[1].SetDigit(decade);
		cells[2].SetDigit(hundred);
		cells[3].SetDigit(kilobit);
		cells[4].SetDigit(myriabit);
		cells[5].SetDigit(hundredThousand);
	}

	private void ChangeLottery()
	{
		if (GameData.GetInstance().lotteryDigit != lotteryDigit)
		{
			ShowLottery();
		}
	}
}
