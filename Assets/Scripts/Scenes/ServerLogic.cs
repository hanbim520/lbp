using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : GameLogic 
{
    public MainUILogic ui;
	public HIDUtils hidUtils;
    private float timeInterval = 0;
    private float longPressTime = 3;
    private bool bLoadBackend = false;
	private UHost host;
	// Field -- Bet
	private Dictionary<string, int> betFields = new Dictionary<string, int>();

    // Current round variables
    private int redFieldVal = 0;
    private int blackFieldVal = 0;
    private int evenFieldVal = 0;
    private int oddFieldVal = 0;
    private int bigFieldVal = 0;
    private int smallFieldVal = 0;

    private void Init()
    {
        host = GetComponent<UHost>();
        FixExitAbnormally();
		ui.RefreshLalCredits(totalCredits.ToString());
		ui.RefreshLblWin("0");
		ui.RefreshLalBet("0");
    }

    protected override void Start() 
    {
        base.Start();
        if (GameData.GetInstance().deviceIndex != 1)
        {
            gameObject.SetActive(false);
            return;
        }
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
		GameEventManager.FieldClick += Bet;
		GameEventManager.CleanAll += CleanAll;
		GameEventManager.ClearAll += ClearAll;
		GameEventManager.Clear += Clear;
		GameEventManager.EndCountdown += CountdownComplete;
		GameEventManager.BallValue += RecBallValue;
		GameEventManager.HIDDisconnected += HIDDisconnected;
		GameEventManager.HIDConnected += HIDConnected;
		GameEventManager.CloseGate += CloseGate;
    }

    private void UnregisterListener()
    {
		GameEventManager.GameStart -= GameStart;
        GameEventManager.GameOver -= GameOver;
		GameEventManager.FieldClick -= Bet;
		GameEventManager.CleanAll -= CleanAll;
		GameEventManager.ClearAll -= ClearAll;
		GameEventManager.Clear -= Clear;
		GameEventManager.EndCountdown -= CountdownComplete;
		GameEventManager.BallValue -= RecBallValue;
		GameEventManager.HIDDisconnected -= HIDDisconnected;
		GameEventManager.HIDConnected -= HIDConnected;
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
        host.SendToAll(NetInstr.GamePhase + ":"+ gamePhase);
		int time = GameData.GetInstance().gameDifficulty + Random.Range(1200, 1500);
		hidUtils.BlowBall(time);
		if (GameData.debug)
//			StartCoroutine(SimulateBallValue(Random.Range(0, GameData.GetInstance().maxNumberOfFields)));
			StartCoroutine(SimulateBallValue(16));
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
		ui.FlashResult(ballValue);

		gamePhase = GamePhase.ShowResult;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase + ":" + ballValue);
		StartCoroutine(Compensate());
		yield break;
	}

	private IEnumerator Compensate()
    {
		print("Compensate");
        gamePhase = GamePhase.Compensate;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase);

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
		
		yield return new WaitForSeconds(5);

		ui.RefreshLalBet("0");
		ui.RefreshLalCredits(totalCredits.ToString());
		if (win > 0)
			ui.RefreshLblWin(win.ToString());
		else
			ui.RefreshLblWin("0");
		ui.CleanAll();

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
        blackFieldVal = 0;
        evenFieldVal = 0;
        redFieldVal = 0;
        bigFieldVal = 0;
        smallFieldVal = 0;
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
            LimitBet(ref words, connectionId);
        }
        else if (instr == NetInstr.GetGamePhase)
        {
            host.SendToPeer(NetInstr.GamePhase + ":" + gamePhase, connectionId);
        }
    }

    private bool LimitBet(ref string[] words, int connectionId)
    {
//        int field;
//        int bet;
//        if (int.TryParse(words[1], out field) && int.TryParse(words[2], out bet))
//        {
//            if (field == Fields.Black &&
//                CanBet(GameData.GetInstance().yanSeXianHong, blackFieldVal + bet, redFieldVal))
//            {
//                blackFieldVal += bet;
//            }
//            else if (field == Fields.Red &&
//                     CanBet(GameData.GetInstance().yanSeXianHong, redFieldVal + bet, blackFieldVal))
//            {
//                redFieldVal += bet;
//            }
//            else if (field == Fields.Even &&
//                     CanBet(GameData.GetInstance().danShuangXianHong, evenFieldVal + bet, oddFieldVal))
//            {
//                evenFieldVal += bet;
//            }
//            else if (field == Fields.Odd &&
//                     CanBet(GameData.GetInstance().danShuangXianHong, oddFieldVal + bet, evenFieldVal))
//            {
//                oddFieldVal += bet;
//            }
//            else if (field == Fields.Big &&
//                     CanBet(GameData.GetInstance().daXiaoXianHong, bigFieldVal + bet, smallFieldVal))
//            {
//                bigFieldVal += bet;
//            }
//            else if (field == Fields.Small &&
//                     CanBet(GameData.GetInstance().daXiaoXianHong, smallFieldVal + bet, bigFieldVal))
//            {
//                smallFieldVal += bet;
//            }
//            else
//            {
//                if (connectionId > 0)
//                    host.SendToPeer(NetInstr.LimitBet + ":" + field, connectionId);
//                DebugConsole.Log(Time.realtimeSinceStartup + ": LimitBet" + " field-" + field + ", betVal-" + bet);
//                return true;
//            }
//
//            if (connectionId > 0)
//                host.SendToPeer(NetInstr.NoLimitBet + ":" + field + ":" + bet, connectionId);
//            DebugConsole.Log(Time.realtimeSinceStartup + ": NoLimitBet" + " field-" + field + ", betVal-" + bet);
//        }
        return false;
    }

    private bool CanBet(int maxVal, int minuend, int subtrahend)
    {
        return Mathf.Abs(minuend - subtrahend) <= maxVal;
    }

    private void Bet(string field, int betVal)
    {
		 if (!InputEx.inputEnable)
			return;

        // TODO: 剩下的筹码小于最小押分
        if (totalCredits <= 0 || totalCredits - betVal < 0)
        {
            DebugConsole.Log(Time.realtimeSinceStartup + ": totalCredits-" + totalCredits);
            return;
        }
        
        string msg = NetInstr.Bet + ":" + field + ":" + betVal;
        char[] d = {':'};
        string[] words = msg.Split(d);
        if (LimitBet(ref words, -1))
        {
            DebugConsole.Log(Time.realtimeSinceStartup + ": " + msg);
        }
        else
        {
            if (betFields.ContainsKey(field))
            {
                betFields[field] += betVal;
            }
            else
            {
                betFields.Add(field, betVal);
            }
			currentBet += betVal;
			totalCredits -= betVal;
			ui.RefreshLalCredits(totalCredits.ToString());
			ui.RefreshLalBet(currentBet.ToString());
        }
    }

	private void CleanAll()
	{
		betFields.Clear();
	}

	private void ClearAll()
	{
		foreach (KeyValuePair<string, int> item in betFields)
		{
			totalCredits += item.Value;
		}
		currentBet = 0;
		betFields.Clear();
		ui.RefreshLalCredits(totalCredits.ToString());
		ui.RefreshLalBet(currentBet.ToString());
	}

	private void Clear(string fieldName)
	{
		if (betFields.ContainsKey(fieldName))
		{
			totalCredits += betFields[fieldName];
			currentBet -= betFields[fieldName];
			betFields.Remove(fieldName);
		}
		ui.RefreshLalCredits(totalCredits.ToString());
		ui.RefreshLalBet(currentBet.ToString());
	}

	private void HIDConnected()
	{
		if (!InputEx.inputEnable)
		{
			InputEx.inputEnable = true;
		}

		if (isPause)
		{
			isPause = false;
			ui.HideWarning();
		}
	}

	private void HIDDisconnected()
	{
		if (InputEx.inputEnable)
		{
			InputEx.inputEnable = false;
		}

		if (!isPause)
		{
			isPause = true;
			ui.ClearAllEvent(null);
			int language = 0;	// EN
			if (GameData.GetInstance().language == 1)
				language = 1;	// CN
			ui.ShowWarning(Notifies.usbDisconnected[language]);
		}
	}

	protected void AppendLast10(int startCredit, int endCredit, int bet, int win)
	{
		BetRecord br = new BetRecord();
		br.startCredit = startCredit;
		br.endCredit = endCredit;
		br.bet = bet;
		br.win = win;
		br.bets = new List<BetInfo>();
		foreach (KeyValuePair<string, int> item in betFields)
		{
			BetInfo info = new BetInfo();
			info.betField = item.Key;
			info.betValue = item.Value;
			br.bets.Add(info);
		}
		GameData.GetInstance().betRecords.Add(br);
		while (GameData.GetInstance().betRecords.Count > 10)
		{
			GameData.GetInstance().betRecords.RemoveAt(0);
		}
		GameData.GetInstance().SaveBetRecords();
	}

    void OnGUI()
    {
//        if (GUI.Button(new Rect(10, 50, 150, 100), "限注"))
//        {
//            DebugConsole.Clear();
//        }
//        if (GUI.Button(new Rect(300, 50, 150, 100), "限红" + Fields.Red))
//        {
//			GameEventManager.TriggerGameStart();
//        }
    }
}
