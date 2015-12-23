using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : GameLogic 
{
	private UHost host;
    private float waitSendTime = 0.1f;

	private const float kCalcRemainTime = 60.0f;
	private float remainTimeIntever = 0.0f;
    
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
		BlowBall();
//		if (!GameData.GetInstance().lotteryEnable)
//			BlowBall();
//		else
//			CalcLottery();
    }

	// 计算彩金号码
	private void CalcLottery()
	{
		++GameData.GetInstance().lotteryMatchCount;
		if (GameData.GetInstance().lotteryMatchCount >= GameData.GetInstance().lotteryMaxMatch)
		{
			GameData.GetInstance().CalcLotteryIdx();
			GameData.GetInstance().lotteryMatchCount = 1;
		}
		// TODO: 收集其他机台的压分情况
		if (GameData.GetInstance().lotteryMatchIdx.Contains(GameData.GetInstance().lotteryMatchCount))
		{
			// 中彩金
		}
		else
		{
			// TODO: 判断要不要出彩金，出的话不能中。
		}
	}

    private void BlowBall()
    {
		Debug.Log("BlowBall");
		gamePhase = GamePhase.Run;
		int time = GameData.GetInstance().gameDifficulty + Random.Range(1200, 1500);
        if (!GameData.debug)
		    hidUtils.BlowBall(time);
		if (GameData.debug)
            StartCoroutine(SimulateBallValue(Random.Range(0, GameData.GetInstance().maxNumberOfFields)));
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

	private IEnumerator ShowResult()
	{
		Debug.Log("ShowResult");
		gamePhase = GamePhase.ShowResult;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase + ":" + ballValue);
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
        betFields.Clear();
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
