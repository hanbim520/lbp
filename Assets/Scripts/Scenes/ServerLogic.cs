using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : GameLogic 
{
	private UHost host;
    private float waitSendTime = 0.1f;

	private const float kCalcRemainTime = 60.0f;
	private float remainTimeIntever = 0.0f;
	// 保存分机和主机在当前局的压分记录(用来计算压中的彩金分数)
	private Dictionary<string, int> clientBetFields = new Dictionary<string, int>();
	// 保存分机和主机在当前局的总压分
	private List<int> totalBets = new List<int>();
	private Timer timerConnectClients = null;
    
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
    }

    private void UnregisterListener()
    {
		GameEventManager.GameStart -= GameStart;
        GameEventManager.GameOver -= GameOver;
		GameEventManager.EndCountdown -= CountdownComplete;
		GameEventManager.BallValue -= RecBallValue;
		GameEventManager.CloseGate -= CloseGate;
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
		if (GameData.controlCode)
		{
			remainTimeIntever += Time.deltaTime;
			if (remainTimeIntever >= kCalcRemainTime)
			{
				remainTimeIntever = 0;
				CalcRemainTime();
			}
		}
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

		if (ui.CurChipIdx != -1)
			ui.chooseBetEffect.SetActive(true);
		ui.RefreshLblWin("0");
		ui.Countdown();
    }

    private void CountdownComplete()
    {
//		ui.chooseBetEffect.SetActive(false);
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
		GameEventManager.OnBreakdownTip(BreakdownType.USBDisconnect);
	}

    private IEnumerator StartLottery()
    {
		//主机的总压分
		int serverBets = 0;	
		foreach (KeyValuePair<string, int> item in betFields)
		{
			// 保存主机台的压分情况
			if (item.Value >= GameData.GetInstance().lotteryCondition)
				clientBetFields.Add(item.Key, item.Value);
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
			GameEventManager.OnLotteryChange(totalLottery);

			int[] lotteries = CalcLottery();
//            int[] lotteries = new int[]{1, 2, 3};
            if (lotteries.Length > 0)
            {
                string msg = NetInstr.LotteryNum + ":";
                foreach (int num in lotteries)
                {
                    Debug.Log("lottery:" + num);
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
		Utils.Seed(System.DateTime.Now.Millisecond);
		int time = GameData.GetInstance().gameDifficulty + Utils.GetRandom(1200, 3000);
//		int[] t = new int[]{1200, 1500, 2000, 2500, 3000};
//		int time = t[Utils.GetRandom(0, 5)];
//		GameEventManager.OnDebugLog(1, string.Format("吹风：{0}毫秒", time));
        if (!GameData.debug)
		    hidUtils.BlowBall(time);
		else
		{
			Utils.SetSeed();
			StartCoroutine(SimulateBallValue(Utils.GetRandom(0, GameData.GetInstance().maxNumberOfFields)));
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

		string strBall = ballValue.ToString();
		int tmpLuckSum;
		if (clientBetFields.TryGetValue(strBall, out tmpLuckSum))
			curLuckySum = tmpLuckSum;

		string msg = NetInstr.LuckSum.ToString() + ":" + curLuckySum.ToString();
		host.SendToAll(msg);
	}

	// 计算彩金号码
	private int[] CalcLottery()
	{
		// 单点压分号码
		List<int> betSingle = new List<int>();
		foreach (KeyValuePair<string, int> item in clientBetFields)
		{
			int key;
			if (int.TryParse(item.Key, out key))
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
			Utils.SetSeed();
			int ret = Utils.GetRandom(0, 20);
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
			}
		}
		return new int[0];
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
		Debug.Log("Compensate");
        gamePhase = GamePhase.Compensate;

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
				if (isLucky)
				{
					int selfBet;
					if (betFields.TryGetValue(ballValue.ToString(), out selfBet))
					{
						if (selfBet >= GameData.GetInstance().lotteryCondition)
						{
							luckyWin = Mathf.FloorToInt((float)GameData.GetInstance().lotteryDigit * 
							                           ((float)selfBet / (float)curLuckySum) * 
							                           ((float)GameData.GetInstance().lotteryAllocation * 0.01f));
							if (luckyWin > 0)
							{
								GameData.GetInstance().lotteryDigit -= luckyWin;
								ui.CreateGoldenRain();
							}
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

        // sync lottery digit
        int totalLottery = GameData.GetInstance().lotteryDigit;
        if (totalLottery <= 0)
            totalLottery = GameData.GetInstance().lotteryBase;
        host.SendToAll(NetInstr.SyncLottery.ToString() + ":" + totalLottery.ToString());
        GameEventManager.OnLotteryChange(totalLottery);

        ui.RefreshLblBet("0");
        ui.RefreshLblCredits(totalCredits.ToString());
		if (win > 0)
			ui.RefreshLblWin(win.ToString());
		else
			ui.RefreshLblWin("0");
		ui.CleanAll();

        if (!GameData.debug)
		    hidUtils.OpenGate();
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
		lotteryValues.Clear();
        betFields.Clear();
		clientBetFields.Clear();
		totalBets.Clear();
		ui.StopFlash();
        ui.StopFlashLotteries();
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
		else if (instr == NetInstr.LuckWin)
		{
			int luckyWin;
			if (int.TryParse(words[1], out luckyWin))
			{
                GameData.GetInstance().lotteryDigit -= luckyWin;
			}
		}
		else if (instr == NetInstr.GetTotalBet)
		{
			if (gamePhase >= GamePhase.ShowResult)
				return;
			CollectTotalBets(ref words);
		}
    }

	// 保存其他客户端在当前局的压分情况
	private void CollectBetRecords(ref string[] words)
	{
		if (words.Length <= 1)
			return;
		for (int idx = 1; idx < words.Length; idx += 2)
		{
			if (clientBetFields.ContainsKey(words[idx]))
			{
				int value;
				if (int.TryParse(words[idx + 1], out value))
				{
					clientBetFields[words[idx]] += value;
				}
			}
			else
			{
				int value;
				if (int.TryParse(words[idx + 1], out value))
				{
					string key = words[idx];
					clientBetFields.Add(key, value);
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

	// 计算跳码时间
	public void CalcRemainTime()
	{
        int remainMins = GameData.GetInstance().remainMins;
        if (remainMins <= 0)
        {
            GameData.GetInstance().remainMins = 0;
            GameEventManager.OnChangeScene(Scenes.Backend);
            return;
        }
        else
        {
            --remainMins;
            if (remainMins <= 0)
            {
                GameData.GetInstance().remainMins = 0;
                GameEventManager.OnChangeScene(Scenes.Backend);
            }
            else
            {
                GameData.GetInstance().remainMins = remainMins;
            }
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
		GameData.GetInstance().StatisticBall(ballValue);
		string log = "";
		for (int i = 0; i < GameData.GetInstance().maxNumberOfFields; ++i)
		{
			if (i % 10 == 0 && i > 0)
				log += string.Format("{0}:{1},\n", i, PlayerPrefs.GetInt("ballValue" + i, 0));
			else
				log += string.Format("{0}:{1}, ", i, PlayerPrefs.GetInt("ballValue" + i, 0));
		}
		GameEventManager.OnDebugLog(2, log);
	}
}
