using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : GameLogic 
{
	public HIDUtils hidUtils;
    private float timeInterval = 0;
    private float longPressTime = 3;
    private bool bLoadBackend = false;
	private UHost host;
    private float waitSendTime = 0.1f;
    
    private void Init()
    {
        host = GetComponent<UHost>();
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

	void Update()
    {
#if UNITY_EDITOR
		if (!bLoadBackend && Input.GetKey(KeyCode.Return))
		{
			timeInterval += Time.deltaTime;
			if (timeInterval > longPressTime)
			{
				timeInterval = 0;
				bLoadBackend = true;
				StartCoroutine(LoadBackend());
				ui.backendTip.SetActive(true);
			}
		}
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			ui.ActiveDlgCard(true);
		}
#endif
	}
	
	private IEnumerator LoadBackend()
	{
		yield return new WaitForSeconds(2.0f);
        GameData.GetInstance().NextLevelName = Scenes.Backend;
		Application.LoadLevel(Scenes.Loading);
    }

    private void GameOver()
    {
        if (bLoadBackend)
            LoadBackend();
        else
            StartCoroutine(NextRound());
    }

    private IEnumerator NextRound()
    {
        yield return new WaitForSeconds(2.0f);
        GameEventManager.TriggerGameStart();
    }

    private void GameStart()
    {
        gamePhase = GamePhase.GameStart;
		ui.ResetCountdown();
        StartCoroutine(Countdown());
	}
	
    private IEnumerator Countdown()
    {
		print("logic countdown");
		if (isPause)
		{
			yield return new WaitForSeconds(1);
			StartCoroutine(Countdown());
			yield break;
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
    }

    private void BlowBall()
    {
		print("BlowBall");
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
		print("SimulateBallValue: " + ballValue);
        GameData.GetInstance().SaveRecord(ballValue);
		GameEventManager.OnRefreshRecord(ballValue);
		StartCoroutine(ShowResult());
	}

	// 收到球的号码
	private void RecBallValue(int value)
	{
		ballValue = value;
		print("RecBallValue: " + ballValue);
        GameData.GetInstance().SaveRecord(ballValue);
        GameEventManager.OnRefreshRecord(ballValue);
		StartCoroutine(ShowResult());
	}

	private IEnumerator ShowResult()
	{
		print("ShowResult");
		gamePhase = GamePhase.ShowResult;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase + ":" + ballValue);
        yield return new WaitForSeconds(waitSendTime);
        
        ui.FlashResult(ballValue);
		StartCoroutine(Compensate());
	}

	private IEnumerator Compensate()
    {
		print("Compensate");
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
		print("CloseGate");
        gamePhase = GamePhase.GameEnd;
        host.SendToAll(NetInstr.GamePhase + ":" + gamePhase);
		ClearVariables();
		GameEventManager.TriggerGameOver();
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
        
        if (instr == NetInstr.Bet)
        {
			// 限红
//            LimitBet(ref words, connectionId);
        }
        else if (instr == NetInstr.GetGamePhase)
        {
            host.SendToPeer(NetInstr.GamePhase + ":" + gamePhase, connectionId);
        }
    }
}
