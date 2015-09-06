using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ServerLogic : MonoBehaviour 
{
    public MainSceneUI ui;

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
    // Field -- Bet
    private Dictionary<int, int> betFields = new Dictionary<int, int>();

	void Start() 
    {
		host = GetComponent<UHost>();
        RegisterListener();
		Test();
	}

	private void Test()
	{
		//81 81 08 EC
		//81 D5 02 EC
		//81 B9 05 A0
//		byte[] b = {0x81, 0x81, 0x08, 0xEC};
////		byte[] b = {0x81, 0xD5, 0x02, 0xE8};
////		byte[] b = {0x81, 0xB9, 0x05, 0xA0};
//		if (System.BitConverter.IsLittleEndian)
//		{
//			Debug.Log("IsLittleEndian");
//			System.Array.Reverse(b);
//			for (int i = 0; i < b.Length; ++i)
//			{
//				print(string.Format("{0:X}", b[i]));
//			}
//		}
//		else
//			Debug.Log("IsNotLittleEndian");
//
//		uint x = System.BitConverter.ToUInt32(b, 0);
//		Debug.Log("x: " + x);

		double[,] det = new double[3, 4];
		det[0, 0] = 2.0d;
		det[0, 1] = 3.0d;
		det[0, 2] = -5.0d;
		det[0, 3] = 3.0d;
		det[1, 0] = 1.0d;
		det[1, 1] = -2.0d;
		det[1, 2] = 1.0d;
		det[1, 3] = 0d;
		det[2, 0] = 3.0d;
		det[2, 1] = 1.0d;
		det[2, 2] = 3.0d;
		det[2, 3] = 7.0d;
		double x, y, z;
		Utils.LEqations3x3(det, out x, out y, out z);
		print(x);
		print(y);
		print(z);
	}

    void OnDestroy()
    {
        UnregisterListener();
    }

    private void RegisterListener()
    {
		GameEventManager.GameStart += GameStart;
        GameEventManager.GameOver += GameOver;
    }

    private void UnregisterListener()
    {
		GameEventManager.GameStart -= GameStart;
        GameEventManager.GameOver -= GameOver;
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

    private void Bet(int field, int betVal)
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
            totalCredits -= betVal;
            DebugConsole.Log(Time.realtimeSinceStartup + ": field-" + field + ", betVal-" + betFields[field]);
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 50, 150, 100), "限注"))
        {
            DebugConsole.Clear();
        }
        if (GUI.Button(new Rect(300, 50, 150, 100), "限红" + Fields.Red))
        {
            Bet(Fields.Red, 1000);
        }
    }
}
