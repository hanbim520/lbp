using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientLogic : GameLogic
{
	private UClient uclient;
	public UClient clientSocket
	{
		get { return uclient; }
	}

    private void Init()
    {
        gamePhase = GamePhase.GameStart;
		uclient = GameObject.Find("NetworkObject").GetComponent<UClient>();
    }

	protected override void Start()
	{
        base.Start();
        Init();
        RegisterListener();

		int totalLottery = GameData.GetInstance().lotteryDigit;
		if (totalLottery <= GameData.GetInstance().lotteryBase)
			totalLottery = GameData.GetInstance().lotteryBase;
		GameEventManager.OnLotteryChange(totalLottery);
	}

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnregisterListener();
    }

    protected override void Update()
    {
		base.Update();
		
        #if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ui.ActiveDlgCard(true);
        }
        #endif

		if (bChangeScene && 
		    (gamePhase == GamePhase.GameStart || gamePhase == GamePhase.GameEnd || gamePhase == GamePhase.Countdown))
		{
			ui.backendTip.SetActive(false);
			ChangeScene();
			return;
		}
	}
	
	private void RegisterListener()
	{
		GameEventManager.EndCountdown += CountdownComplete;
		GameEventManager.ClientDisconnect += ClientDisconnect;
		GameEventManager.FieldClick += Bet;
        GameEventManager.Clear += Clear;
        GameEventManager.ClearAll += ClearAll;
    }
    
    private void UnregisterListener()
    {
        GameEventManager.EndCountdown -= CountdownComplete;
		GameEventManager.ClientDisconnect -= ClientDisconnect;
		GameEventManager.FieldClick -= Bet;
        GameEventManager.Clear -= Clear;
        GameEventManager.ClearAll -= ClearAll;
    }
	
	public void HandleRecData(int instr, ref string[] words)
	{
        if (instr == NetInstr.GamePhase)
        {
            HandleGamePhase(ref words);
        }
		else if (instr == NetInstr.GetBetRecords)
		{
			SendBetRecords();
		}
        else if (instr == NetInstr.LotteryNum)
        {
            HandleLotteryNums(ref words);
        }
        else if (instr == NetInstr.SyncRecords)
        {
            SyncLast100(ref words);
        }
		else if (instr == NetInstr.LuckSum)
		{
			int value;
			if (int.TryParse(words[1], out value))
				curLuckySum = value;
		}
        else if (instr == NetInstr.SyncLottery)
        {
            int totalLottery;
            if (int.TryParse(words[1], out totalLottery))
                GameEventManager.OnLotteryChange(totalLottery);
        }
		else if (instr == NetInstr.GetTotalBet)
		{
		    SendTotalBet();
		}
		else if (instr == NetInstr.BetAbleValue)
		{
			BetCallback(ref words);
		}
		else if (instr == NetInstr.CheckRepeatAble)
		{
			int val;
			if (int.TryParse(words[1], out val))
			{
				if (val == 1)
					ui.RepeatEventCB();
			}
		}
	}

	// 发送当前局的压分记录
	private void SendBetRecords()
	{
		string items = "";
		foreach (KeyValuePair<string, int> item in betFields)
		{
			if (item.Value < GameData.GetInstance().lotteryCondition)
				continue;
			string str = string.Format(":{0}:{1}", item.Key, item.Value);
			items += str;
		}
		string msg = NetInstr.GetBetRecords.ToString() + items;
		uclient.SendToServer(msg);
	}

	// 发送当前局的总压分
	private void SendTotalBet()
	{
		string msg = NetInstr.GetTotalBet.ToString() + ":";
		int totalBet = 0;
		foreach (KeyValuePair<string, int> item in betFields)
		{
			totalBet += item.Value;
		}
		msg += totalBet.ToString();
		uclient.SendToServer(msg);
	}

    private void HandleGamePhase(ref string[] words)
    {
        int phase;
        if (int.TryParse(words[1], out phase))
        {
            gamePhase = phase;
            if (gamePhase == GamePhase.Countdown)
            {
                ui.ClearWinChips();
                Countdown();
            }
            else if (gamePhase == GamePhase.ShowResult)
            {
                int value;
                if (int.TryParse(words[2], out value))
                {
                    ballValue = value;
                    GameData.GetInstance().SaveRecord(ballValue);
                    GameEventManager.OnRefreshRecord(ballValue);
                    ShowResult();
                }
            }
            else if (gamePhase == GamePhase.GameEnd)
            {
                CloseGate();
            }
        }
    }

    private void Countdown()
    {
        ClearVariables();
        if (ui.CurChipIdx != -1)
            ui.chooseBetEffect.SetActive(true);
        ui.RefreshLblWin("0");
        ui.RefreshLblCredits(totalCredits.ToString());
        ui.ResetCountdown();
        ui.Countdown();
    }

    private void CountdownComplete()
    {
        gamePhase = GamePhase.Run;
//        ui.chooseBetEffect.SetActive(false);
		// 切换回经典压分区
//		if (GameData.GetInstance().displayType == 1)
//		{
//			GameData.GetInstance().displayType = 0;
//			GameData.GetInstance().SaveDisplayType();
//			ui.SetDisplay();
//		}
    }

    private void ShowResult()
    {
        Debug.Log("Client ShowResult");
		GameData.GetInstance().AddBeginSessions();
        StartCoroutine(Compensate());
		ui.FlashResult(ballValue);
    }

    private IEnumerator Compensate()
    {
        Debug.Log("Client Compensate");
        
		// 正常赢取的筹码数
        int win = 0;
		// 有中奖(用来判断是否闪烁中奖灯)
		bool bBingo = false;
        foreach (KeyValuePair<string, int> item in betFields)
        {
            int peilv = Utils.IsBingo(item.Key, ballValue);
            if (peilv > 0)
            {
                win += peilv * item.Value;
                ui.AddWinChip(item.Key);
				if (peilv == 36)
				{
					ui.AddWinChip("e" + item.Key);
				}
				if (!bBingo)
					bBingo = true;
            }
        }
		if (bBingo)
			GameEventManager.OnWinLightSignal(1);
		// 赢取的彩金数
		int luckyWin = 0;	
		// 计算自己赢了多少彩金
		if (GameData.GetInstance().lotteryEnable && curLuckySum > 0)
		{
			bool isLucky = false;	// 是否出彩金
			foreach (int lottery in lotteryValues)
			{
				if (lottery == ballValue)
				{
					isLucky = true;
					break;
				}
			}
			if (isLucky)
			{
				int selfBet;
				if (betFields.TryGetValue(ballValue.ToString(), out selfBet))
				{
					if (selfBet >= GameData.GetInstance().lotteryCondition)
					{
						luckyWin = Mathf.CeilToInt((float)GameData.GetInstance().lotteryDigit * 
						                           ((float)selfBet / (float)curLuckySum) * 
						                           ((float)GameData.GetInstance().lotteryAllocation * 0.01f));
						if (luckyWin > 0)
						{
							ui.CreateGoldenRain();
						}
					}
				}
			}
		}
		AppendLastBets(currentBet);
        AppendLast10(totalCredits, totalCredits + win + luckyWin, currentBet, win, luckyWin, ballValue);
		win += luckyWin;	// 加上彩金送的分
        GameData.GetInstance().ZongPei += win;
		GameData.GetInstance().lotteryCredits += luckyWin;	// 保存送出去的彩金 显示在总账
		GameData.GetInstance().jackpotDaybook += luckyWin;	// 保存送出去的彩金 显示在流水账
        currentBet = 0;
        totalCredits += win;
        if (totalCredits <= 0)
            ui.DisableCardMode();
        
        yield return new WaitForSeconds(5);
        
        ui.RefreshLblBet("0");
        if (win > 0)
            ui.RefreshLblWin(win.ToString());
        else
            ui.RefreshLblWin("0");
        ui.ClearLoseChips();
    }

    private void CloseGate()
    {
        ClearVariables();
    }

    private void ClearVariables()
    {
        ballValue = -1;
		curLuckySum = 0;
        betFields.Clear();
		lotteryValues.Clear();
        ui.StopFlash();
        ui.StopFlashLotteries();
		GameEventManager.OnWinLightSignal(0);
    }

    private void NotifyMsg(ref string msg)
    {
        Debug.Log(Time.realtimeSinceStartup + ": " + msg);
    }

    private void HandleLotteryNums(ref string[] words)
    {
        for (int i = 1; i < words.Length; ++i)
        {
            int num;
            if (int.TryParse(words[i], out num))
            {
				lotteryValues.Add(num);
			}
        }
		StartCoroutine(ui.FlashLotteries(lotteryValues));
	}

	private void ClientDisconnect()
	{
		ui.StopFlash();
		ui.StopFlashLotteries();
		ui.ClearAllEvent(null);
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
        GameEventManager.OnRefreshRecord(ballValue);
    }

	protected int Bet(string field, int betVal)
	{
		if (totalCredits <= 0)
			return 0;
		// 剩下的筹码小于押分
		if (totalCredits - betVal < 0)
			betVal = totalCredits;
		
		// 计算分机限注
		int maxBet = Utils.GetMaxBet(field);
		if (betFields.ContainsKey(field))
		{
			betVal = MaxBet(maxBet, betFields[field], betVal);
		}
		else
		{
			betVal = MaxBet(maxBet, 0, betVal);
		}
		// 计算全台限注
		string msg = NetInstr.BetAbleValue.ToString() + ":" + field + ":" + betVal.ToString();
		uclient.SendToServer(msg);
		
//		if (betVal > 0)
//		{
//			if (betFields.ContainsKey(field))
//			{
//				betFields[field] += betVal;
//			}
//			else
//			{
//				betFields.Add(field, betVal);
//			}
//			GameData.GetInstance().ZongYa += betVal;
//			currentBet += betVal;
//			totalCredits -= betVal;
//			ui.RefreshLblCredits(totalCredits.ToString());
//			ui.RefreshLblBet(currentBet.ToString());
//		}
//		return betVal;
		return 0;
	}

	// 服务器发送回来某押分区能押多少分(用于计算全台限注)
	private void BetCallback(ref string[] words)
	{
		string field = words[1];
		int betVal;
		if (int.TryParse(words[2], out betVal))
		{
			if (betVal > 0)
			{
				if (betFields.ContainsKey(field))
				{
					betFields[field] += betVal;
				}
				else
				{
					betFields.Add(field, betVal);
				}
				GameData.GetInstance().ZongYa += betVal;
				currentBet += betVal;
				totalCredits -= betVal;
				ui.RefreshLblCredits(totalCredits.ToString());
				ui.RefreshLblBet(currentBet.ToString());
				ui.FieldClickCB(field, betVal);
			}
		}
	}

	protected void Clear(string fieldName)
	{
		if (string.Equals(fieldName.Substring(0, 1), "e"))
		{
			fieldName = fieldName.Substring(1);
		}
		if (betFields.ContainsKey(fieldName))
		{
			int betVal = betFields[fieldName];
			totalCredits += betVal;
			GameData.GetInstance().ZongYa -= betVal;
			currentBet -= betVal;
			betFields.Remove(fieldName);
			// 通知主机 修改全台限注
			string msg = string.Format("{0}:{1}:{2}", NetInstr.ClearBets, fieldName, betVal);
			uclient.SendToServer(msg);
		}
		ui.RefreshLblCredits(totalCredits.ToString());
		ui.RefreshLblBet(currentBet.ToString());
	}

	protected void ClearAll()
	{
		if (betFields.Count == 0)
			return;

		string msg = NetInstr.ClearBets.ToString();
		foreach (KeyValuePair<string, int> item in betFields)
		{
			totalCredits += item.Value;
			msg += string.Format(":{0}:{1}", item.Key, item.Value);
		}
		uclient.SendToServer(msg);
		GameData.GetInstance().ZongYa -= currentBet;
		currentBet = 0;
		betFields.Clear();
		ui.RefreshLblCredits(totalCredits.ToString());
		ui.RefreshLblBet(currentBet.ToString());
	}
}
