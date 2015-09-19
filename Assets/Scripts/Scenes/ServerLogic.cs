using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : GameLogic 
{
    public MainUILogic ui;

    private float timeInterval = 0;
    private float longPressTime = 3;
    private bool bLoadBackend = false;
	private UHost host;

	private int gamePhase = GamePhase.GameEnd;
	private int ballValue = -1;

    // Current round variables
    private int redFieldVal = 0;
    private int blackFieldVal = 0;
    private int evenFieldVal = 0;
    private int oddFieldVal = 0;
    private int bigFieldVal = 0;
    private int smallFieldVal = 0;

    // Field -- Bet
    private Dictionary<string, int> betFields = new Dictionary<string, int>();

    private void Init()
    {
		InputEx.inputEnable = false;
        host = GetComponent<UHost>();
        FixExitAbnormally();
    }

	void Start() 
    {
        if (GameData.GetInstance().deviceIndex != 1)
        {
            gameObject.SetActive(false);
            return;
        }
        Init();
        RegisterListener();
	}

    void OnDestroy()
    {
        UnregisterListener();
    }

    private void RegisterListener()
    {
		GameEventManager.GameStart += GameStart;
        GameEventManager.GameOver += GameOver;
		GameEventManager.FieldClick += Bet;
		GameEventManager.ClearAll += ClearAll;
		GameEventManager.Clear += Clear;
		GameEventManager.EndCountdown += CountdownComplete;
    }

    private void UnregisterListener()
    {
		GameEventManager.GameStart -= GameStart;
        GameEventManager.GameOver -= GameOver;
		GameEventManager.FieldClick -= Bet;
		GameEventManager.ClearAll -= ClearAll;
		GameEventManager.Clear -= Clear;
		GameEventManager.EndCountdown -= CountdownComplete;
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
#endif
	}
	
	private IEnumerator LoadBackend()
	{
		yield return new WaitForSeconds(2.0f);
        if (GameData.GetInstance().language == 1)
            GameData.GetInstance().NextLevelName = "Backend CN";
        else if (GameData.GetInstance().language == 0)
            GameData.GetInstance().NextLevelName = "Backend EN";
        Application.LoadLevel("Loading");
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
        ClearVariables();
        Countdown();
	}
	
    private void Countdown()
    {
		print("logic countdown");
		gamePhase = GamePhase.Countdown;
		host.SendToAll(NetInstr.GamePhase + ":"+ gamePhase);
		InputEx.inputEnable = true;
		ui.Countdown();
    }

    private void CountdownComplete()
    {
		InputEx.inputEnable = false;
		StartCoroutine(GoBall());
    }

    private IEnumerator GoBall()
    {
		print("Goball");
		gamePhase = GamePhase.Run;
        host.SendToAll(NetInstr.GamePhase + ":"+ gamePhase);
		// TODO: chui qiu
		
		yield return new WaitForSeconds(2);
		SetBallValue();
    }

	private void SetBallValue()
	{
		ballValue = Random.Range(0, 37);
		print("SetBallValue: " + ballValue);
		while (GameData.GetInstance().records.Count >= 100)
			GameData.GetInstance().records.Dequeue();
		GameData.GetInstance().records.Enqueue(ballValue);
        GameEventManager.OnRefreshRecord(ballValue);
		StartCoroutine(ShowResult());
	}

	private IEnumerator ShowResult()
	{
		print("ShowResult");
		gamePhase = GamePhase.ShowResult;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase + ":" + ballValue);

        // TODO: UI
		ui.FlashResult(ballValue);
		yield return new WaitForSeconds(2);
		StartCoroutine(Compensate());
	}

	private IEnumerator Compensate()
    {
		print("Compensate");
        gamePhase = GamePhase.Compensate;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase);

        // TODO: Compensate
        // TODO: Save account
        // TODO: UI

		yield return new WaitForSeconds(2);
		CompensateComplete();
    }

    private void CompensateComplete()
    {
		print("CompensateComplete");
        gamePhase = GamePhase.GameEnd;
        host.SendToAll(NetInstr.GamePhase + ":" + gamePhase);
        // TODO: UI
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
//			print(betFields[field].ToString());
//          DebugConsole.Log(Time.realtimeSinceStartup + ": field-" + field + ", betVal-" + betFields[field]);
            totalCredits -= betVal;
			currentBet += betVal;
        }
    }

	private void ClearAll()
	{
		foreach (KeyValuePair<string, int> item in betFields)
		{
			totalCredits += item.Value;
		}
		betFields.Clear();
	}

	private void Clear(string fieldName)
	{
		if (betFields.ContainsKey(fieldName))
		{
			totalCredits += betFields[fieldName];
			betFields.Remove(fieldName);
		}
	}

    void OnGUI()
    {
//        if (GUI.Button(new Rect(10, 50, 150, 100), "限注"))
//        {
//            DebugConsole.Clear();
//        }
        if (GUI.Button(new Rect(300, 50, 150, 100), "限红" + Fields.Red))
        {
			GameEventManager.TriggerGameStart();
        }
    }
}
