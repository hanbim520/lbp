﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientLogic : GameLogic
{
	private UClient uclient;

    private void Init()
    {
        gamePhase = GamePhase.GameEnd;
		uclient = GameObject.Find("NetworkObject").GetComponent<UClient>();
    }

	protected override void Start()
	{
        base.Start();
        Init();
        RegisterListener();
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
    }

    private void RegisterListener()
    {
        GameEventManager.EndCountdown += CountdownComplete;
		GameEventManager.ClientDisconnect += ClientDisconnect;
    }
    
    private void UnregisterListener()
    {
        GameEventManager.EndCountdown -= CountdownComplete;
		GameEventManager.ClientDisconnect -= ClientDisconnect;
    }
	
    public void HandleRecData(ref string[] words)
	{
        int instr;
        if (!int.TryParse(words[0], out instr))
        {
            return;
        }
        
        if (instr == NetInstr.SyncData)
        {
            SyncData(ref words);
        }
        else if (instr == NetInstr.GamePhase)
        {
            HandleGamePhase(ref words);
        }
		else if (instr == NetInstr.CheckAccount)
		{
			SendAccountToHost();
		}
		else if (instr == NetInstr.ClearAccount)
		{
			ClearAccount();
		}
		else if (instr == NetInstr.ClearCurrentWin)
		{
			ClearCurrentWin();
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

	private void ClearCurrentWin()
	{
		GameData.GetInstance().currentWin = 0;
		GameData.GetInstance().currentZongShang = 0;
		GameData.GetInstance().currentZongXia = 0;
		GameData.GetInstance().SaveAccount();
		SendAccountToHost();
	}

	private void ClearAccount()
	{
		GameData.GetInstance().ClearAccount();
		SendAccountToHost();
	}

	private void SendAccountToHost()
	{
		string msg = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", 
		                           NetInstr.CheckAccount, GameData.GetInstance().deviceIndex, 
		                           GameData.GetInstance().zongShang, GameData.GetInstance().zongXia,
		                           GameData.GetInstance().zongTou, GameData.GetInstance().zongTui,
		                           GameData.GetInstance().currentWin, GameData.GetInstance().totalWin,
		                           GameData.GetInstance().cardCredits);
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
                ui.backendTip.SetActive(false);
                if (bChangeScene)
                {
                    ChangeScene();
                    return;
                }
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
        if (ui.CurChipIdx != -1)
            ui.chooseBetEffect.SetActive(true);
        ui.RefreshLblWin("0");
        ui.ResetCountdown();
        ui.Countdown();
    }

    private void CountdownComplete()
    {
        gamePhase = GamePhase.Run;
//        ui.chooseBetEffect.SetActive(false);
    }

    private void ShowResult()
    {
        Debug.Log("Client ShowResult");
		GameData.GetInstance().AddBeginSessions();
		// 切换回经典压分区
		if (GameData.GetInstance().displayType == 1)
		{
			GameData.GetInstance().displayType = 0;
			GameData.GetInstance().SaveDisplayType();
			ui.SetDisplay();
		}
		ui.FlashResult(ballValue);
        StartCoroutine(Compensate());
    }

    private IEnumerator Compensate()
    {
        Debug.Log("Client Compensate");
        
		// 正常赢取的筹码数
        int win = 0;
        foreach (KeyValuePair<string, int> item in betFields)
        {
            int peilv = Utils.IsBingo(item.Key, ballValue);
            if (peilv > 0)
            {
                win += peilv * item.Value;
            }
        }
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
							string msg = NetInstr.LuckWin.ToString() + ":" + luckyWin.ToString();
							uclient.SendToServer(msg);
							ui.CreateGoldenRain();
						}
					}
				}
			}
		}
        AppendLast10(totalCredits, totalCredits + win + luckyWin, currentBet, win, luckyWin, ballValue);
		win += luckyWin;	// 加上彩金送的分
        GameData.GetInstance().ZongPei += win;
        currentBet = 0;
        totalCredits += win;
        if (totalCredits <= 0)
            ui.DisableCardMode();
        
        yield return new WaitForSeconds(5);
        
        ui.RefreshLblBet("0");
        ui.RefreshLblCredits(totalCredits.ToString());
        if (win > 0)
            ui.RefreshLblWin(win.ToString());
        else
            ui.RefreshLblWin("0");
        ui.CleanAll();
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
    }

    private void NotifyMsg(ref string msg)
    {
        Debug.Log(Time.realtimeSinceStartup + ": " + msg);
    }

    private void ResponseBet(ref string[] words)
    {
		string field = words[1];
        int betVal;
        if (int.TryParse(words[2], out betVal))
        {
            if (betFields.ContainsKey(field))
            {
                betFields[field] += betVal;
            }
            else
            {
                betFields.Add(field, betVal);
            }
            totalCredits -= betVal;
            Debug.Log(Time.realtimeSinceStartup + ": field-" + field + ", betVal-" + betFields[field]);
        }
    }

    private void SyncData(ref string[] words)
    {
        int betTimeLimit, coinToScore, baoji;
        int max36Value, max18Value, max12Value, max9Value, max6Value, max3Value, max2Value;
        int betChipValue0, betChipValue1, betChipValue2, betChipValue3, betChipValue4, betChipValue5;
        int couponsStart, couponsKeyinRatio, couponsKeoutRatio;
        int maxNumberOfFields;
        int lineId, machineId;
		int lotteryCondition, lotteryBase, lotteryRate, lotteryAlloc, lotteryDigit;
		int inputDevice;

        if(int.TryParse(words[1], out betTimeLimit))
            GameData.GetInstance().betTimeLimit = betTimeLimit;
        if(int.TryParse(words[2], out coinToScore))
            GameData.GetInstance().coinToScore = coinToScore;
        if(int.TryParse(words[3], out baoji))
            GameData.GetInstance().baoji = baoji;

        if(int.TryParse(words[4], out betChipValue0))
            GameData.GetInstance().betChipValues[0] = betChipValue0;
        if(int.TryParse(words[5], out betChipValue1))
            GameData.GetInstance().betChipValues[1] = betChipValue1;
        if(int.TryParse(words[6], out betChipValue2))
            GameData.GetInstance().betChipValues[2] = betChipValue2;
        if(int.TryParse(words[7], out betChipValue3))
            GameData.GetInstance().betChipValues[3] = betChipValue3;
        if(int.TryParse(words[8], out betChipValue4))
            GameData.GetInstance().betChipValues[4] = betChipValue4;
        if(int.TryParse(words[9], out betChipValue5))
            GameData.GetInstance().betChipValues[5] = betChipValue5;

        if(int.TryParse(words[10], out max36Value))
            GameData.GetInstance().max36Value = max36Value;
        if(int.TryParse(words[11], out max18Value))
            GameData.GetInstance().max18Value = max18Value;
        if(int.TryParse(words[12], out max12Value))
            GameData.GetInstance().max12Value = max12Value;
        if(int.TryParse(words[13], out max9Value))
            GameData.GetInstance().max9Value = max9Value;
        if(int.TryParse(words[14], out max6Value))
            GameData.GetInstance().max6Value = max6Value;
        if(int.TryParse(words[15], out max3Value))
            GameData.GetInstance().max3Value = max3Value;
        if(int.TryParse(words[16], out max2Value))
            GameData.GetInstance().max2Value = max2Value;

        if(int.TryParse(words[17], out couponsStart))
            GameData.GetInstance().couponsStart = couponsStart;
        if(int.TryParse(words[18], out couponsKeyinRatio))
            GameData.GetInstance().couponsKeyinRatio = couponsKeyinRatio;
        if(int.TryParse(words[19], out couponsKeoutRatio))
            GameData.GetInstance().couponsKeoutRatio = couponsKeoutRatio;

        if(int.TryParse(words[20], out maxNumberOfFields))
            GameData.GetInstance().maxNumberOfFields = maxNumberOfFields;

        if(int.TryParse(words[21], out lineId))
            GameData.GetInstance().lineId = lineId;
        if(int.TryParse(words[22], out machineId))
            GameData.GetInstance().machineId = machineId;
		if(int.TryParse(words[23], out lotteryCondition))
		    GameData.GetInstance().lotteryCondition = lotteryCondition;
		if(int.TryParse(words[24], out lotteryBase))
			GameData.GetInstance().lotteryBase= lotteryBase;
		if(int.TryParse(words[25], out lotteryRate))
			GameData.GetInstance().lotteryRate= lotteryRate;
		if(int.TryParse(words[26], out lotteryAlloc))
			GameData.GetInstance().lotteryAllocation= lotteryAlloc;
		if(int.TryParse(words[27], out inputDevice))
			GameData.GetInstance().inputDevice = inputDevice;
        if(int.TryParse(words[28], out lotteryDigit))
            GameData.GetInstance().lotteryDigit = lotteryDigit;

        GameData.GetInstance().SaveSetting();
		GameData.GetInstance().SaveInputDevice();
        ui.SetDisplay();
        ui.SetBetChips();
    }

    private void HandleLotteryNums(ref string[] words)
    {
        List<int> nums = new List<int>();
        for (int i = 1; i < words.Length; ++i)
        {
            int num;
            if (int.TryParse(words[i], out num))
            {
                nums.Add(num);
            }
        }
        ui.FlashLotteries(nums);
    }

	private void ClientDisconnect()
	{
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
}
