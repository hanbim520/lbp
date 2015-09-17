﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : MonoBehaviour 
{
    public MainUILogic ui;

    private float timeInterval = 0;
    private float longPressTime = 3;
    private bool bLoadBackend = false;
	private UHost host;

	private int gamePhase = GamePhase.GameEnd;
	private int ballValue = -1;
    private bool inputEnable = false;

    // Current round variables
    private int redFieldVal = 0;
    private int blackFieldVal = 0;
    private int evenFieldVal = 0;
    private int oddFieldVal = 0;
    private int bigFieldVal = 0;
    private int smallFieldVal = 0;
    private Timer tGetBallVal = null;

    private int totalCredits = 10000;
	private int currentBet = 0;
	private int lastWin = 0;
    // Field -- Bet
    private Dictionary<string, int> betFields = new Dictionary<string, int>();

	void Start() 
    {
		host = GetComponent<UHost>();
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
    }

    private void UnregisterListener()
    {
		GameEventManager.GameStart -= GameStart;
        GameEventManager.GameOver -= GameOver;
		GameEventManager.FieldClick -= Bet;
		GameEventManager.ClearAll -= ClearAll;
		GameEventManager.Clear -= Clear;
    }

	void Update()
    {
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
        if (inputEnable)
        {
            HandleInput();
        }
	}

    private void HandleInput()
    {

    }

    private IEnumerator LoadBackend()
    {
        yield return new WaitForSeconds(2.0f);
        if (Config.Language == "CN")
            GameData.GetInstance().NextLevelName = "Backend CN";
        else if (Config.Language == "EN")
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
		gamePhase = GamePhase.Countdown;
		host.SendToAll(NetInstr.GamePhase + ":"+ gamePhase);
        inputEnable = true;

		Timer t = TimerManager.GetInstance().CreateTimer(1.0f, TimerType.Loop, GameData.GetInstance().betTimeLimit);
        t.Tick += CountdownTick;
        t.OnComplete += CountdownComplete;
		t.Start();
    }

    private void CountdownTick()
    {
        // TODO: UI
    }

    private void CountdownComplete()
    {
        inputEnable = false;
        GoBall();
    }

    private void GoBall()
    {
		gamePhase = GamePhase.Run;
        host.SendToAll(NetInstr.GamePhase + ":"+ gamePhase);
		// TODO: chui qiu
		// simulation
		Timer t = TimerManager.GetInstance().CreateTimer(Random.Range(2, 5));
		t.Tick += SetBallValue;
		t.Start();

        tGetBallVal = TimerManager.GetInstance().CreateTimer(0.5f, TimerType.Loop);
        tGetBallVal.Tick += GetBallValue;
        tGetBallVal.Start();
    }

	private void GetBallValue()
	{
		if (ballValue != -1)
        {
            if (tGetBallVal != null)
            {
                tGetBallVal.Stop();
                tGetBallVal = null;
                ShowResult();
                GameEventManager.OnRefreshRecord(ballValue);
            }
        }
	}

	private void SetBallValue()
	{
		ballValue = Random.Range(0, 37);
    }

	private void ShowResult()
	{
		gamePhase = GamePhase.ShowResult;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase + ":" + ballValue);

        // TODO: UI

		Timer t = TimerManager.GetInstance().CreateTimer(3);
        t.OnComplete += Compensate;
		t.Start();
	}

    private void Compensate()
    {
        gamePhase = GamePhase.Compensate;
		host.SendToAll(NetInstr.GamePhase + ":" + gamePhase);

        // TODO: Compensate
        // TODO: Save account
        // TODO: UI

        Timer t = TimerManager.GetInstance().CreateTimer(3);
        t.OnComplete += CompensateComplete;
        t.Start();
    }

    private void CompensateComplete()
    {
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

//    void OnGUI()
//    {
//        if (GUI.Button(new Rect(10, 50, 150, 100), "限注"))
//        {
//            DebugConsole.Clear();
//        }
//        if (GUI.Button(new Rect(300, 50, 150, 100), "限红" + Fields.Red))
//        {
//            Bet("red", 1000);
//        }
//    }
}
