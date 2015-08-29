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

	private int gamePhase;
	private int ballValue = -1;

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
    }

    private void UnregisterListener()
    {
		GameEventManager.GameStart -= GameStart;
		
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
		GameEventManager.TriggerSCountdown();
	}
	
    private void SCountdown()
    {
		gamePhase = GamePhase.SCountdown;
		int time = GameData.GetInstance().betTimeLimit;
		host.SendToAll(gamePhase + ":" + time.ToString());
		Timer t = TimerManager.GetInstance().CreateTimer(time);
		t.Tick += ECountdown;
		t.Start();
    }

    private void ECountdown()
    {
		GameEventManager.TriggerSRun();
    }

    private void SRun()
    {
		gamePhase = GamePhase.SRun;
		host.SendToAll(gamePhase.ToString());
		// TODO: chui qiu
		// simulation
		Timer t = TimerManager.GetInstance().CreateTimer(Random.Range(2, 5));
		t.Tick += SetBallValue;
		t.Start();

		Timer t1 = TimerManager.GetInstance().CreateTimer(0.5f);
		t1.Tick += GetBallValue;
		t1.Start();
    }

	private void GetBallValue()
	{
		if (ballValue != -1)
			GameEventManager.TriggerERun();
	}

	private void SetBallValue()
	{
		ballValue = Random.Range(0, 37);
	}

    private void ERun()
    {
		GameEventManager.TriggerSShowResult();
    }

	private void SShowResult()
	{
		gamePhase = GamePhase.SShowResult;
		host.SendToAll(gamePhase + ":" + ballValue);
		Timer t = TimerManager.GetInstance().CreateTimer(3);
		t.Tick += EShowResult;
		t.Start();
	}

	private void EShowResult()
	{
		GameEventManager.TriggerSCompensate();
	}

    private void SCompensate()
    {
		gamePhase = GamePhase.SShowResult;
		host.SendToAll(gamePhase.ToString());
    }

    private void ECompensate()
    {
		GameEventManager.TriggerGameOver();
    }
}
