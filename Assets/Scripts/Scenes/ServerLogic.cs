using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : GameLogic 
{
	private UHost host;
    private float waitSendTime = 0.1f;

	private const float kCalcRemainTime = 60.0f;
	private float remainTimeIntever = 0.0f;
	// 保存其他客户端在当前局的压分记录
	private Dictionary<string, int> clientBetFields = new Dictionary<string, int>();
    
    private void Init()
    {
        host = GetComponent<UHost>();
//        CalcRemainTime();
    }

    protected override void Start() 
    {
        base.Start();
        if (GameData.GetInstance().deviceIndex != 1)
        {
            gameObject.SetActive(false);
            return;
        }
        else
            GetComponent<UHost>().enabled = true;
        Init();
        RegisterListener();
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
	}

    private void GameOver()
    {
        StartCoroutine(NextRound());
    }

    private IEnumerator NextRound()
    {
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
		ui.chooseBetEffect.SetActive(false);
		// 收集其他机台的压分情况
		host.SendToAll(NetInstr.GetBetRecords.ToString());
		BlowBall();
    }

    private void BlowBall()
    {
		Debug.Log("BlowBall");
		gamePhase = GamePhase.Run;
		int time = GameData.GetInstance().gameDifficulty + Random.Range(1200, 1500);
        if (!GameData.debug)
		    hidUtils.BlowBall(time);
		if (GameData.debug)
		{
//			Random.seed = (int)SystemTime.time;
//            StartCoroutine(SimulateBallValue(Random.Range(0, GameData.GetInstance().maxNumberOfFields)));
            StartCoroutine(SimulateBallValue(LinuxUtils.GetRandom(0, GameData.GetInstance().maxNumberOfFields)));
		}
    }

	// 模拟收到球的号码
	private IEnumerator SimulateBallValue(int value)
	{
		yield return new WaitForSeconds(2);
		ballValue = value;
		Debug.Log("SimulateBallValue: " + ballValue);
        GameData.GetInstance().SaveRecord(ballValue);
		GameEventManager.OnRefreshRecord(ballValue);
		StartCoroutine(ShowResult());
	}

	// 收到球的号码
	private void RecBallValue(int value)
	{
		ballValue = value;
		Debug.Log("RecBallValue: " + ballValue);
        GameData.GetInstance().SaveRecord(ballValue);
        GameEventManager.OnRefreshRecord(ballValue);
		StartCoroutine(ShowResult());
	}

	// 计算彩金号码
	private int[] CalcLottery()
	{
		clientBetFields.Clear();
		foreach (KeyValuePair<string, int> item in betFields)
		{
			clientBetFields.Add(item.Key, item.Value);
		}
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
		if (betSingle.Count > 0)
		{
			// 有单点压分的情况下场次计数加一
			++GameData.GetInstance().lotteryMatchCount;
			if (GameData.GetInstance().lotteryMatchCount >= GameData.GetInstance().lotteryMaxMatch)
			{
				GameData.GetInstance().CalcLotteryIdx();
				GameData.GetInstance().lotteryMatchCount = 1;
			}
		}
		// 有单点压分 且能中彩金
		if (GameData.GetInstance().lotteryWinIdx.Contains(GameData.GetInstance().lotteryMatchCount) &&
		    betSingle.Count > 0)
		{
			int betCount = betSingle.Count;
			int noBetCount = noBetSingle.Count;
			// 中彩金
			if (betCount > 0)
			{
				List<int> retArray = new List<int>();
				int lotteryCount = 1;	// 彩金个数
				Random.seed = (int)SystemTime.time;
				lotteryCount = Random.Range(1, Mathf.Min(betCount, 6));		// 彩金个数 1~5
				int winCount = Random.Range(1, lotteryCount);				// 中奖个数 1~4
				int loseCount = lotteryCount - winCount;					// 没中奖个数 1~3
				for (int i = 0; i < winCount;)
				{
					int idx = Random.Range(0, betCount);
					int value = betSingle[idx];
					if (!retArray.Contains(value))
					{
						retArray.Add(value);
						++i;
					}
				}
				for (int i = 0; i < loseCount;)
				{
					int idx = Random.Range(0, noBetCount);
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
		// 没有单点压分 或没有到中彩金的场次
		else
		{
			int noBetCount = noBetSingle.Count;
			// 判断要不要出彩金，出的话不中。
			Random.seed = (int)SystemTime.time;
			int ret = Random.Range(0, 20);
			// 出彩金
			if (ret == 12 && noBetCount > 0)
			{
				List<int> retArray = new List<int>();
				int lotteryCount = Random.Range(1, Mathf.Min(noBetCount, 6));
				for (int i = 0; i < lotteryCount;)
				{
					int idx = Random.Range(0, noBetCount);
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
		gamePhase = GamePhase.ShowResult;
		string msg = NetInstr.GamePhase + ":" + gamePhase + ":" + ballValue;
		if (GameData.GetInstance().lotteryEnable)
		{
			int[] lotteries = CalcLottery();
			foreach (int num in lotteries)
			{
				msg += string.Format(":{0}", num);
				lotteryValues.Add(num);
			}
		}
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
		if (GameData.GetInstance().lotteryEnable)
			ui.FlashLotteries(ref lotteryValues);
		StartCoroutine(Compensate());
	}

	private IEnumerator Compensate()
    {
		Debug.Log("Compensate");
        gamePhase = GamePhase.Compensate;

        // TODO: Compensate
        // TODO: Save account
        // TODO: UI
		int win = 0;
		foreach (KeyValuePair<string, int> item in betFields)
		{
			int peilv = Utils.IsBingo(item.Key, ballValue);
			if (peilv > 0)
			{
				win += peilv * item.Value;
			}
		}
		AppendLast10(totalCredits, totalCredits + win, currentBet, win);
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

        if (!GameData.debug)
		    hidUtils.OpenGate();
		if (GameData.debug)
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
		lotteryValues.Clear();
        betFields.Clear();
		clientBetFields.Clear();
		ui.StopFlash();
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
    }

	// 保存其他客户端在当前局的压分情况
	private void CollectBetRecords(ref string[] words)
	{
		for (int idx = 0; idx < words.Length; idx += 2)
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
}
