using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientLogic : MonoBehaviour
{
    private UClient client;
    private int gamePhase = GamePhase.GameEnd;
    private bool inputEnable = false;

    private int totalCredits = 10000;
    // Field -- Bet
    private Dictionary<int, int> betFields = new Dictionary<int, int>();

	void Start()
	{
        client = GetComponent<UClient>();
	}
	
	void Update()
	{
	    if (inputEnable)
        {
            HandleInput();
        }
	}

    private void HandleInput()
    {

    }

    public void HandleRecData(ref string[] words)
	{
        int instr;
        if (!int.TryParse(words[0], out instr))
        {
            return;
        }
        
        if (instr == NetInstr.SynData && words.Length >= 8)
        {
            SynData(ref words);
        }
        else if (instr == NetInstr.NoLimitBet)
        {
            ResponseBet(ref words);
        }
        else if (instr == NetInstr.LimitBet)
        {
            NotifyMsg(ref Notifies.LimitBet);
        }
        else if (instr == NetInstr.GamePhase)
        {
            HandleGamePhase(ref words);
        }
	}

    private void HandleGamePhase(ref string[] words)
    {
        int phase;
        if (int.TryParse(words[1], out phase))
        {
            gamePhase = phase;
            if (gamePhase == GamePhase.Countdown)
            {
                inputEnable = true;
                Timer t = TimerManager.GetInstance().CreateTimer(1.0f, TimerType.Loop, GameData.GetInstance().betTimeLimit);
                t.Tick += CountdownTick;
                t.OnComplete += CountdownComplete;
                t.Start();
            }
            else if (gamePhase == GamePhase.ShowResult)
            {
                int value;
                if (int.TryParse(words[2], out value))
                {
                    // TODO: UI
                    
                    Timer t = TimerManager.GetInstance().CreateTimer(3);
                    t.OnComplete += Compensate;
                    t.Start();
                }
            }
        }
    }

    private void CountdownTick()
    {
        // TODO: UI
    }

    private void CountdownComplete()
    {
        inputEnable = false;
    }

    private void Compensate()
    {
        // TODO: Compensate
        // TODO: Save account
        // TODO: UI
        
        Timer t = TimerManager.GetInstance().CreateTimer(3);
        t.OnComplete += CompensateComplete;
        t.Start();
    }

    private void CompensateComplete()
    {
        // TODO: UI
        ClearVariables();
    }

    private void ClearVariables()
    {
        gamePhase = GamePhase.GameEnd;
        betFields.Clear();
    }

    private void NotifyMsg(ref string msg)
    {
        DebugConsole.Log(Time.realtimeSinceStartup + ": " + msg);
    }

    private void ResponseBet(ref string[] words)
    {
        int field;
        int betVal;
        if (int.TryParse(words[1], out field) && int.TryParse(words[2], out betVal))
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

    private void SynData(ref string[] words)
    {
        float yanseOdds;
        float shuangOdds;
        float danOdds;
        float daOdds;
        float xiaoOdds;
        float duOdds;
        int betTimeLimit;
        if(float.TryParse(words[1], out yanseOdds))
            GameData.GetInstance().yanseOdds = yanseOdds;
        if(float.TryParse(words[2], out shuangOdds))
            GameData.GetInstance().shuangOdds = shuangOdds;
        if(float.TryParse(words[3], out danOdds))
            GameData.GetInstance().danOdds = danOdds;
        if(float.TryParse(words[4], out daOdds))
            GameData.GetInstance().daOdds = daOdds;
        if(float.TryParse(words[5], out xiaoOdds))
            GameData.GetInstance().xiaoOdds = xiaoOdds;
        if(float.TryParse(words[6], out duOdds))
            GameData.GetInstance().duOdds = duOdds;
        if(int.TryParse(words[7], out betTimeLimit))
            GameData.GetInstance().betTimeLimit = betTimeLimit;
        DebugConsole.Log("SynData:"+ yanseOdds + ", " + betTimeLimit);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 50, 150, 100), "限注"))
        {
            DebugConsole.Clear();
        }
//        if (GUI.Button(new Rect(300, 50, 150, 100), "限红" + Fields.Red))
        if (GUI.Button(new Rect(300, 50, 150, 100), "限红" + Fields.Black))
        {
            int betVal = 1000;
            // TODO: 剩下的筹码小于最小押分
            // TODO: 程序模拟压分
            if (totalCredits <= 0 || totalCredits - betVal < 0)
            {
                DebugConsole.Log(Time.realtimeSinceStartup + ": totalCredits-" + totalCredits);
                return;
            }

            client.SendToServer(NetInstr.Bet + ":" + Fields.Black + ":" + betVal);
//            client.SendToServer(NetInstr.Bet + ":" + Fields.Red + ":" + betVal);
        }
    }
}
