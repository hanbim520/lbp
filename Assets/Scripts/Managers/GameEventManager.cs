using UnityEngine;
using System;
using System.Collections;

public static class GameEventManager 
{
    public delegate void GameEvent();
	public delegate void FingerEvent(UInt16 x, UInt16 y);
	public delegate void SerialMouseMoveEvent(sbyte deltaX, sbyte deltaY);
	public delegate void SerialMouseButtonEvent(int x, int y);
    public static event GameEvent ObtainInput;
    public static event GameEvent GameStart, GameOver;
    public static event GameEvent SCountdown, ECountdown;
    public static event GameEvent SRun, ERun;
    public static event GameEvent SShowResult, EShowResult;
    public static event GameEvent SCompensate, ECompensate;
    public static event GameEvent OpenSerial, CloseSerial;
	public static event FingerEvent FingerUp, FingerDown, FingerHover;
	public static event SerialMouseMoveEvent SerialMouseMove;
	public static event SerialMouseButtonEvent SMLBUp, SMLBDown, SMRBUp, SMRBDown;

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

	public static void OnFingerUp(UInt16 x, UInt16 y)
	{
		if (FingerUp != null) FingerUp(x, y);
	}

	public static void OnFingerDown(UInt16 x, UInt16 y)
	{
		if (FingerDown != null) FingerDown(x, y);
	}

	public static void OnFingerHover(UInt16 x, UInt16 y)
	{
		if (FingerHover != null) FingerHover(x, y);
	}

	public static void OnSerialMouseMove(sbyte deltaX, sbyte deltaY)
	{
		if (SerialMouseMove != null) SerialMouseMove(deltaX, deltaY);
	}

	public static void OnSMLBUp(int x, int y)
	{
		if (SMLBUp != null) SMLBUp(x, y);
	}

	public static void OnSMLBDown(int x, int y)
	{
		if (SMLBDown != null) SMLBDown(x, y);
	}

	public static void OnSMRBUp(int x, int y)
	{
		if (SMRBUp != null) SMRBUp(x, y);
	}
	
	public static void OnSMRBDown(int x, int y)
	{
		if (SMRBDown != null) SMRBDown(x, y);
	}
}
