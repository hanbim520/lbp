using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : GameLogic 
{
	private UHost host;
    private float waitSendTime = 0.1f;

	// 保存所有机台在当前局的压分记录 (用来计算全台限注)
	public Dictionary<string, int> clientBets = new Dictionary<string, int>();
	// 保存所有机台符合中彩金条件的压分记录 (用来计算压中的彩金分数)
	public Dictionary<string, int> betForLucky = new Dictionary<string, int>();
	// 保存所有机台在当前局的总压分
	private List<int> totalBets = new List<int>();
	private Timer timerConnectClients = null;
	private int othersLucyWin = 0;	// 其他分机中的彩金数额
    
    private void Init()
    {
		host = GameObject.Find("NetworkObject").GetComponent<UHost>();
//        CalcRemainTime();
    }

    protected override void Start() 
    {
        base.Start();
        Init();
        RegisterListener();
		StartConnectClients();

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

    private void RegisterListener()
    {
		GameEventManager.GameStart += GameStart;
        GameEventManager.GameOver += GameOver;
		GameEventManager.EndCountdown += CountdownComplete;
		GameEventManager.BallValue += RecBallValue;
		GameEventManager.CloseGate += CloseGate;
		GameEventManager.FieldClick += Bet;
        GameEventManager.Clear += Clear;
        GameEventManager.ClearAll += ClearAll;
    }

    private void UnregisterListener()
    {
		GameEventManager.GameStart -= GameStart;
        GameEventManager.GameOver -= GameOver;
		GameEventManager.EndCountdown -= CountdownComplete;
		GameEventManager.BallValue -= RecBallValue;
		GameEventManager.CloseGate -= CloseGate;
		GameEventManager.FieldClick -= Bet;
        GameEventManager.Clear -= Clear;
        GameEventManager.ClearAll -= ClearAll;
    }

	protected override void Update()
    {
		base.Update();
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            GameData.GetInstance().RemoveStatisticBalls();
            StatisticBall(-1);
        }
#if UNITY_EDITOR
		if (GameData.debug)
		{
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				ui.ActiveDlgCard(true);
			}
			else if (Input.GetKeyUp(KeyCode.C))			// Check touch
			{
				GameEventManager.OnChangeScene(Scenes.TouchCheck);
			}
			else if (Input.GetKeyUp(KeyCode.Space))	// Main menu
			{
				GameEventManager.OnOpenKey();
			}
		}
#endif
		UpdateServerTimer();
	}

    private void GameOver()
    {
        StartCoroutine(NextRound());
    }

    private IEnumerator NextRound()
    {
        GameEventManager.OnSyncData();
        yield return new WaitForSeconds(2.0f);
        GameEventManager.TriggerGameStart();
    }

    private void GameStart()
    {
		ui.backendTip.SetActive(false);
        if (bChangeScene)
        {
            ChangeScene();
            return;
        }
        gamePhase = GamePhase.GameStart;
		ui.ResetCountdown();
        ui.ClearWinChips();
		StartCoroutine(Countdown());
	}
	
    private IEnumerator Countdown()
    {
		Debug.Log("logic countdown");
		while (isPause || bFirstOpenGate)
		{
			yield return new WaitForSeconds(1);
			Countdown();
		}
		gamePhase = GamePhase.Countdown;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase);
        yield return new WaitForSeconds(waitSendTime);

		int totalLottery = GameData.GetInstance().lotteryDigit;
		if (totalLottery <= GameData.GetInstance().lotteryBase)
			totalLottery = GameData.GetInstance().lotteryBase;
		host.SendToAll(NetInstr.SyncLottery.ToString() + ":" + totalLottery.ToString());
		GameEventManager.OnLotteryChange(totalLottery);
        yield return new WaitForSeconds(waitSendTime);
		
		if (ui.CurChipIdx != -1)
			ui.chooseBetEffect.SetActive(true);
		ui.RefreshLblBet("0");
		ui.RefreshLblWin("0");
        ui.RefreshLblCredits(totalCredits.ToString());
		ui.Countdown();
		GameEventManager.OnStartCountdown();
    }

    private void CountdownComplete()
    {
		timerConnectClients = new Timer(120, 0);	// 2分钟后不认球 则当故障处理
		timerConnectClients.Tick += RecogBallTimeout;
		timerConnectClients.Start();
        StartCoroutine(StartLottery());
		BlowBall();
    }

	// 认球超时
	private void RecogBallTimeout()
	{
		isPause = true;
		timerConnectClients = null;
		GameEventManager.OnBreakdownTip(BreakdownType.RecognizeBallTimeout);
	}

    private IEnumerator StartLottery()
    {
		//主机的总压分
		int serverBets = 0;	
		foreach (KeyValuePair<string, int> item in betFields)
		{
			// 保存主机台的压分情况
			if (item.Value >= GameData.GetInstance().lotteryCondition)
				betForLucky.Add(item.Key, item.Value);
			serverBets += item.Value;
		}
		totalBets.Add(serverBets);
		// 收集其他机台的符合彩金门槛的压分
		host.SendToAll(NetInstr.GetBetRecords.ToString());
		// 收集其他机台的总压分
		host.SendToAll(NetInstr.GetTotalBet.ToString());
		// 等收到分机的压分情况
		yield return new WaitForSeconds(3.0f);		

        if (GameData.GetInstance().lotteryEnable)
        {
            Debug.Log("lottery able");
            // 累积彩金池
            int allBets = 0;
            foreach (int item in totalBets)
            {
				allBets += item;
            }

			float ratio = (float)GameData.GetInstance().lotteryRate * 0.001f;
			int sum = GameData.GetInstance().lotteryBetPool + allBets;
			int accumulation = Mathf.FloorToInt(sum * ratio);
			GameData.GetInstance().lotteryBetPool = sum - Mathf.FloorToInt((float)accumulation / ratio);
			GameData.GetInstance().SaveLotteryBetPool();
			int totalLottery = accumulation + GameData.GetInstance().lotteryDigit;
			if (totalLottery > 999999)
				totalLottery = 999999;
			host.SendToAll(NetInstr.SyncLottery.ToString() + ":" + totalLottery.ToString());
			GameEventManager.OnLotteryChange(totalLottery);

            int[] lotteries = CalcLottery();
            if (lotteries.Length > 0)
            {
                string msg = NetInstr.LotteryNum.ToString();
                foreach (int num in lotteries)
                {
                    msg += string.Format(":{0}", num);
                    lotteryValues.Add(num);
                }
                // 通知分机彩票号码
                host.SendToAll(msg);
                StartCoroutine(ui.FlashLotteries(lotteryValues));
            }
        }
        else
        {
            Debug.Log("lottery disable");
        }
    }

    private void BlowBall()
    {
		Debug.Log("BlowBall");
		gamePhase = GamePhase.Run;
		if (GameData.GetInstance().blowTiming > 0)	// 已提前吹球
			return;
		
		Utils.Seed(System.DateTime.Now.Millisecond + System.DateTime.Now.Second + System.DateTime.Now.Minute + System.DateTime.Now.Hour);
		int time = GameData.GetInstance().gameDifficulty + Utils.GetRandom(1200, 3000);
        if (!GameData.debug)
		    base.BlowBall(time);
		else
		{
			Utils.SetSeed();
			StartCoroutine(SimulateBallValue(Utils.GetRandom(0, GameData.GetInstance().maxNumberOfFields)));
//			StartCoroutine(SimulateBallValue(1));
		}
    }

	// 模拟收到球的号码
	private IEnumerator SimulateBallValue(int value)
	{
		yield return new WaitForSeconds(6);
		if (timerConnectClients != null)
		{
			timerConnectClients.Stop();
			timerConnectClients = null;
		}
		ballValue = value;
		Debug.Log("SimulateBallValue: " + ballValue);
        GameData.GetInstance().SaveRecord(ballValue);
		GameEventManager.OnRefreshRecord(ballValue);
		CalcLuckySum();
		StartCoroutine(ShowResult());
		StatisticBall(ballValue);
	}

	// 收到球的号码
	private void RecBallValue(int value)
	{
		if (timerConnectClients != null)
		{
			timerConnectClients.Stop();
			timerConnectClients = null;
		}
		ballValue = value;
		Debug.Log("RecBallValue: " + ballValue);
        GameData.GetInstance().SaveRecord(ballValue);
        GameEventManager.OnRefreshRecord(ballValue);
		CalcLuckySum();
		StartCoroutine(ShowResult());
		StatisticBall(ballValue);
	}

	// 计算压中彩金号的总金额
	private void CalcLuckySum()
	{
		curLuckySum = 0;
		bool isLucky = false;	// 出不出彩金
		foreach (int lottery in lotteryValues)
		{
			if (lottery == ballValue)
			{
				isLucky = true;
				break;
			}
		}
		if (!isLucky)
			return;

		string strBall = ballValue == 37 ? "00" : ballValue.ToString();
		int tmpLuckSum;
		if (betForLucky.TryGetValue(strBall, out tmpLuckSum))
			curLuckySum = tmpLuckSum;

		string msg = NetInstr.LuckSum.ToString() + ":" + curLuckySum.ToString();
		host.SendToAll(msg);
	}

	// 不带控制的出彩金
	private int[] CalcLottery()
	{
        int dif = GameData.GetInstance().lotteryLv;             // 多少场出一次彩金
		int count = GameData.GetInstance().jackpotMatchCount;	// 当前第几场
		int bingoIdx = GameData.GetInstance().jackpotBingoIdx;	// 第几场bingo
		++count;
		int[] ret = new int[0];
		if (bingoIdx == 0)
		{
			bingoIdx = Random.Range(1, dif + 1);
			GameData.GetInstance().jackpotBingoIdx = bingoIdx;
		}
		if (count == bingoIdx)
		{
			// 出彩金
			int jackpotNum = Random.Range(1, 4);
			if (jackpotNum == 1)
				ret = new int[]{Random.Range(0 ,37)};
			else if (jackpotNum == 2)
				ret = new int[]{Random.Range(0, 19), Random.Range(19, 37)};
			else
				ret = new int[]{Random.Range(0, 13), Random.Range(13, 25), Random.Range(25, 37)};
		}
		if (count >= dif)
		{
			count = 0;
			GameData.GetInstance().jackpotBingoIdx = Random.Range(1, dif + 1);
		}
		GameData.GetInstance().jackpotMatchCount = count;

		return ret;
	}

	// 带控制的出彩金
	private int[] CalcLottery2()
	{
		// 单点压分号码
		List<int> betSingle = new List<int>();
		foreach (KeyValuePair<string, int> item in betForLucky)
		{
			int key;
			if (string.Compare(item.Key, "00") == 0)
				betSingle.Add(37);
			else if (int.TryParse(item.Key, out key))
				betSingle.Add(key);
		}
		// 没压的单点号码
		List<int> noBetSingle = new List<int>();
		for (int i = 0; i < GameData.GetInstance().maxNumberOfFields; ++i)
		{
			if (!betSingle.Contains(i))
				noBetSingle.Add(i);
		}
		// 场次计数加一
		++GameData.GetInstance().lotteryMatchCount;
		if (GameData.GetInstance().lotteryMatchCount >= GameData.GetInstance().lotteryMaxMatch)
		{
			GameData.GetInstance().CalcLotteryIdx();
			GameData.GetInstance().lotteryMatchCount = 1;
		}
		// 有单点压分 且能中彩金
		if (GameData.GetInstance().lotteryWinIdx.Contains(GameData.GetInstance().lotteryMatchCount) &&
		    betSingle.Count > 0)
		{
			int betCount = betSingle.Count;
			// 中彩金
			if (betCount > 0)
			{
				List<int> retArray = new List<int>();
				int lotteryCount = 1;	// 彩金个数
				Utils.SetSeed();
				lotteryCount = Utils.GetRandom(1, Mathf.Min(betCount, 6));		// 彩金个数 1~5
				int winCount = Utils.GetRandom(1, lotteryCount);				// 中奖个数 1~4
				for (int i = 0; i < winCount;)
				{
					int idx = Utils.GetRandom(0, betCount);
					int value = betSingle[idx];
					if (!retArray.Contains(value))
					{
						retArray.Add(value);
						++i;
					}
				}
				return retArray.ToArray();
			}
		}
		// 没有单点压分 或没有到中彩金的场次
		else
		{
			int noBetCount = noBetSingle.Count;
			// 判断要不要出彩金，出的话不中。
			Utils.Seed(System.DateTime.Now.Millisecond + System.DateTime.Now.Minute);
			int ret = Utils.GetRandom(0, 20);
			//			int ret = 12;
			// 出彩金
			if (ret == 12 && noBetCount > 0)
			{
				List<int> retArray = new List<int>();
				int lotteryCount = Utils.GetRandom(1, Mathf.Min(noBetCount, 6));
				for (int i = 0; i < lotteryCount;)
				{
					int idx = Utils.GetRandom(0, noBetCount);
					int value = noBetSingle[idx];
					if (!retArray.Contains(value))
					{
						retArray.Add(value);
						++i;
					}
				}
				return retArray.ToArray();
				//				return new int[]{7, 17, 27};
			}
		}
		return new int[0];
		//		return new int[]{7, 17, 27};
	}

	private IEnumerator ShowResult()
	{
		Debug.Log("ShowResult");
        // sync last 100 records
        int recordsCount = GameData.GetInstance().records.Count;
		if (recordsCount > 1)
		{
			string syncMsg = NetInstr.SyncRecords.ToString();
			int[] r = GameData.GetInstance().records.ToArray();
			for (int i = 0; i < recordsCount - 1; ++i)	// 不同步最后一个号码
			{
				syncMsg += string.Format(":{0}", r[i]);
			}
			host.SendToAll(syncMsg);
			yield return new WaitForSeconds(waitSendTime);
		}

		gamePhase = GamePhase.ShowResult;
		string msg = NetInstr.GamePhase + ":" + gamePhase + ":" + ballValue;
		host.SendToAll(msg);
		GameData.GetInstance().AddBeginSessions();
        yield return new WaitForSeconds(waitSendTime);


		StartCoroutine(Compensate());
		ui.FlashResult(ballValue);
	}

	private IEnumerator Compensate()
    {
		Debug.Log("Compensate");
        gamePhase = GamePhase.Compensate;

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
		if (GameData.GetInstance().lotteryEnable)
		{
			// 计算自己赢了多少彩金
			if (curLuckySum > 0)
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
                float othersRatio = 1.0f;
				if (isLucky)
				{
					int selfBet;
					string betField = ballValue == 37 ? "00" : ballValue.ToString();
					if (betFields.TryGetValue(betField, out selfBet))
					{
						if (selfBet >= GameData.GetInstance().lotteryCondition)
						{
							float selfRatio = (float)selfBet / (float)curLuckySum;
							othersRatio = 1.0f - selfRatio;
							luckyWin = Mathf.FloorToInt((float)GameData.GetInstance().lotteryDigit * 
							                            selfRatio * 
							                           ((float)GameData.GetInstance().lotteryAllocation * 0.01f));
							if (luckyWin > 0)
							{
								ui.CreateGoldenRain();
							}
                        }
                    }
				}
                othersLucyWin = Mathf.FloorToInt((float)GameData.GetInstance().lotteryDigit * 
                                                 othersRatio * 
                                                 ((float)GameData.GetInstance().lotteryAllocation * 0.01f));
                GameData.GetInstance().lotteryDigit -= (luckyWin + othersLucyWin);
            }
        }
        AppendLastBets(currentBet);
		AppendLast10(totalCredits, totalCredits + win + luckyWin, currentBet, win, luckyWin, ballValue);
		win += luckyWin;	// 加上彩金送的分
        GameData.GetInstance().zongPei += win;
		GameData.GetInstance().totalZongPei += win;
		GameData.GetInstance().totalZongYa += currentBet;
		GameData.GetInstance().lotteryCredits += luckyWin;	// 保存送出去的彩金 显示在总账
		GameData.GetInstance().jackpotDaybook += luckyWin;	// 保存送出去的彩金 显示在流水账
		GameData.GetInstance().SaveAccount();
		currentBet = 0;
		totalCredits += win;
		if (totalCredits <= 0)
			ui.DisableCardMode();
		
		yield return new WaitForSeconds(0.2f);

        // sync lottery digit
        int totalLottery = GameData.GetInstance().lotteryDigit;
		if (totalLottery <= 0)
			totalLottery = 0;
        host.SendToAll(NetInstr.SyncLottery.ToString() + ":" + totalLottery.ToString());
        GameEventManager.OnLotteryChange(totalLottery);

		if (win > 0)
		{
			ui.RefreshLblWin(win.ToString());
			AudioController.Play("win");
		}
		else
		{
			ui.RefreshLblWin("0");
		}
		ui.ClearLoseChips();
			
		if (!GameData.debug)
		{
			#if UNITY_STANDALONE_LINUX
			hidUtils.OpenGate();
			#endif
			#if UNITY_ANDROID
			goldfingerUtils.OpenGate();
			#endif
		}
		else
			StartCoroutine(SimulateCloseGate());
    }

	private IEnumerator SimulateCloseGate()
	{
		yield return new WaitForSeconds(5);
		CloseGate();
	}

    private void CloseGate()
    {
		Debug.Log("CloseGate");
		if (bFirstOpenGate)
		{
			StartCoroutine(WaitAfterOpenGate());
			return;
		}
        gamePhase = GamePhase.GameEnd;
        host.SendToAll(NetInstr.GamePhase + ":" + gamePhase);
		ClearVariables();
		GameEventManager.TriggerGameOver();
    }

	private IEnumerator WaitAfterOpenGate()
	{
		yield return new WaitForSeconds(5);
		bFirstOpenGate = false;
	}

    private void ClearVariables()
    {
        ballValue = -1;
		curLuckySum = 0;
		othersLucyWin = 0;
		lotteryValues.Clear();
        betFields.Clear();
		betForLucky.Clear();
		clientBets.Clear();
		totalBets.Clear();
		ui.StopFlash();
        ui.StopFlashLotteries();
		GameEventManager.OnWinLightSignal(0);
	}

    public void HandleRecData(ref string[] words, int connectionId)
    {
        int instr;
        if (!int.TryParse(words[0], out instr))
        {
            return;
        }
        
        if (instr == NetInstr.GetGamePhase)
        {
            host.SendToPeer(NetInstr.GamePhase + ":" + gamePhase, connectionId);
        }
		else if (instr == NetInstr.GetBetRecords)
		{
			if (gamePhase >= GamePhase.ShowResult)
				return;
			CollectBetRecords(ref words);
		}
		else if (instr == NetInstr.GetTotalBet)
		{
			if (gamePhase >= GamePhase.ShowResult)
				return;
			CollectTotalBets(ref words);
		}
		else if (instr == NetInstr.BetAbleValue)
		{
			string field = words[1];
			int wantBat;
			if (int.TryParse(words[2], out wantBat))
			{
				SendBetAbleValue(true, connectionId, field, wantBat);
			}
		}
		else if (instr == NetInstr.ClearBets)
		{
			HandleClearInstr(ref words);
		}
		else if (instr == NetInstr.CheckRepeatAble)
		{
			HandleCheckRepeatAble(true, connectionId, ref words);
		}
		else if (instr == NetInstr.ReportClientProfit)
		{
			RevClientProfit(ref words);
		}
    }

	/// <summary>
	/// 发送给分机该押分区能押的分数(用于全台限注).
	/// </summary>
	/// <param name="isClient">是否是分机.</param>
	/// <param name="connectionId">分机联网id.</param>
	/// <param name="field">押分区.</param>
	/// <param name="wantBatVal">希望押的分数.</param>
	private void SendBetAbleValue(bool isClient, int connectionId, string field, int wantBatVal)
	{
		int canBetVal = wantBatVal;						// 还能押的分数
		int maxBet = Utils.GetAllMaxBet(field); 		// 全台限注额度
		if (maxBet > 0)	// 开启了全台限注的情况
		{
			if (clientBets.ContainsKey(field))
			{
				int betted = clientBets[field];			// 已经押的分数
				int remain = maxBet - betted;
				if (remain < wantBatVal)
					canBetVal = remain;
				else
					canBetVal = wantBatVal;
				clientBets[field] += canBetVal;
			}
			else
			{
				if (wantBatVal < maxBet)
					canBetVal = wantBatVal;
				else
					canBetVal = maxBet;
				clientBets.Add(field, canBetVal);
			}
		}
		if (isClient)
		{
			string msg = NetInstr.BetAbleValue.ToString() + ":" + field + ":" + canBetVal;
			host.SendToPeer(msg, connectionId);
		}
		else
		{
			BetCallback(field, canBetVal);
		}
	}

	// 保存其他客户端在当前局的压分情况
	private void CollectBetRecords(ref string[] words)
	{
		if (words.Length <= 1)
			return;
		for (int idx = 1; idx < words.Length; idx += 2)
		{
			if (betForLucky.ContainsKey(words[idx]))
			{
				int value;
				if (int.TryParse(words[idx + 1], out value))
				{
					betForLucky[words[idx]] += value;
				}
			}
			else
			{
				int value;
				if (int.TryParse(words[idx + 1], out value))
				{
					string key = words[idx];
					betForLucky.Add(key, value);
				}
			}
		}
	}

	// 保存所有机台的总压分
	private void CollectTotalBets(ref string[] words)
	{
		if (words.Length <= 1)
			return;

		int totalBet;
		if (int.TryParse(words[1], out totalBet))
		{
			totalBets.Add(totalBet);
		}
	}

	private void UpdateServerTimer()
	{
		if (timerConnectClients != null)
			timerConnectClients.Update(Time.deltaTime);
	}

	private void StartConnectClients()
	{
		timerConnectClients = new Timer(GameData.GetInstance().ConnectClientsTime, 0);
		timerConnectClients.Tick += StopConnectClients;
		timerConnectClients.Start();
	}

	private void StopConnectClients()
	{
		timerConnectClients = null;
		GameEventManager.TriggerGameStart();
	}

	private void StatisticBall(int ballValue)
	{
        if (ballValue >= 0)
		    GameData.GetInstance().StatisticBall(ballValue);

		string log = "";
        int sum = 0;
        for (int i = 0; i < GameData.GetInstance().maxNumberOfFields; ++i)
        {
            sum += PlayerPrefs.GetInt("ballValue" + i, 0);
        }
		for (int i = 0; i < GameData.GetInstance().maxNumberOfFields; ++i)
		{
            int count = PlayerPrefs.GetInt("ballValue" + i, 0);
            log += string.Format("{0}:{1:0.0}%, ", i, sum > 0 ? (float)count / sum * 100 : 0f);
			if (i % 10 == 0 && i > 0)
                log += "\n";
		}
//        GameEventManager.OnDebugLog(1, string.Format("Total: {0}", sum));
//		GameEventManager.OnDebugLog(2, log);
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
//		SendBetAbleValue(false, 0, field, betVal);
		
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
			AudioController.Play("betClick");
		}
		return betVal;
//		return 0;
	}

	// 计算完全台限注的回调
	private void BetCallback(string field, int betVal)
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
			// 修改全台限注
			if (clientBets.ContainsKey(fieldName))
				clientBets[fieldName] -= betVal;
		}
		ui.RefreshLblCredits(totalCredits.ToString());
		ui.RefreshLblBet(currentBet.ToString());
	}

	private void HandleClearInstr(ref string[] words)
	{
		for (int i = 1; i < words.Length; i += 2)
		{
			string field = words[i];
			int betVal;
			if (int.TryParse(words[i + 1], out betVal))
			{
				if (clientBets.ContainsKey(field))
					clientBets[field] -= betVal;
			}
		}
	}

	protected void ClearAll()
	{
		if (betFields.Count == 0)
			return;

		foreach (KeyValuePair<string, int> item in betFields)
		{
			totalCredits += item.Value;
			if (clientBets.ContainsKey(item.Key))
				clientBets[item.Key] -= item.Value;
		}
		GameData.GetInstance().ZongYa -= currentBet;
		currentBet = 0;
		betFields.Clear();
		ui.RefreshLblCredits(totalCredits.ToString());
		ui.RefreshLblBet(currentBet.ToString());
	}

    public void HandleCheckRepeatAble(bool isClient, int connectionId, ref string[] words)
    {
        int ret = 1; // 0:表示不可以续押 1:表示可以续押
        bool isRepeatData = false;
        for (int i = 1; i < words.Length; i += 2)
        {
            
            if (string.Compare(words[i], "repeats") == 0)
            {
                isRepeatData = true;
                continue;
            }
            string field = words[i];
            int betVal;
            if (int.TryParse(words[i + 1], out betVal))
            {
                if (isRepeatData)   // 准备重复押的分数
                {
                    // 检查分机限注
                    if (betVal > Utils.GetMaxBet(field))
                    {
                        ret = 0;
                        break;
                    }
                    // 检查全台限注
                    int allMaxBet = Utils.GetAllMaxBet(field);
                    int alreadyBet;
                    if (clientBets.TryGetValue(field, out alreadyBet))
                    {
                        if ((alreadyBet + betVal) > allMaxBet)
                        {
                            ret = 0;
                            break;
                        }
                        else
                        {
                            clientBets[field] += betVal;
                        }
                    }
                    else
                    {
                        if (betVal > allMaxBet)
                        {
                            ret = 0;
                            break;
                        }
                        else
                        {
                            clientBets.Add(field, betVal);
                        }
                    }
                }
            }
        }
        if (isClient)
        {
            string msg = string.Format("{0}:{1}", NetInstr.CheckRepeatAble, ret);
            host.SendToPeer(msg, connectionId);
        }
        else
        {
            if (ret == 1)
                ui.RepeatEventCB();
        }
    }

	public void RevClientProfit(ref string[] words)
	{
		int zongya, zongpei;
		if (int.TryParse(words[1], out zongya))
			GameData.GetInstance().TotalZongYa += zongya;
		if (int.TryParse(words[2], out zongpei))
			GameData.GetInstance().TotalZongPei += zongpei;
	}
}
