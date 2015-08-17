using UnityEngine;
using System.Collections;

public static class GameEventManager 
{
    public delegate void GameEvent();
    public static event GameEvent ObtainInput;
    public static event GameEvent GameStart, GameOver;
    public static event GameEvent SCountdown, ECountdown;
    public static event GameEvent SRun, ERun;
    public static event GameEvent SShowResult, EShowResult;
    public static event GameEvent SCompensate, ECompensate;
    public static event GameEvent OpenSerial, CloseSerial;

	public static void TriggerOpenSerial()
	{
		if (OpenSerial != null) OpenSerial();
	}

	public static void TriggerCloseSerial()
	{
		if (CloseSerial != null) CloseSerial();
	}

    public static void TriggerObtainInput()
    {
        if (ObtainInput != null) ObtainInput();
    }

    public static void TriggerGameStart()
    {
        if (GameStart != null) GameStart();
    }

    public static void TriggerGameOver()
    {
        if (GameOver != null) GameOver();
    }

    public static void TriggerSCountdown()
    {
        if (SCountdown != null) SCountdown();
    }

    public static void TriggerECountdown()
    {
        if (ECountdown != null) ECountdown();
    }

    public static void TriggerSRun()
    {
        if (SRun != null) SRun();
    }

    public static void TriggerERun()
    {
        if (ERun != null) ERun();
    }

    public static void TriggerSShowResult()
    {
        if (SShowResult != null) SShowResult();
    }

    public static void TriggerEShowResult()
    {
        if (EShowResult != null) EShowResult();
    }

    public static void TriggerSCompensate()
    {
        if (SCompensate != null) SCompensate();
    }

    public static void TriggerECompensate()
    {
        if (ECompensate != null) ECompensate();
    }
}
