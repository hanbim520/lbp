using UnityEngine;
using System;
using System.Collections;

public static class GameEventManager 
{
    public delegate void GameEvent();
    public delegate void RefreshRecordEvent(int result);
	public delegate void FingerEvent(UInt16 x, UInt16 y);
	public delegate void SerialMouseMoveEvent(sbyte deltaX, sbyte deltaY);
	public delegate void SerialMouseButtonEvent();
    public delegate void FieldClickEvent(string fieldName, int bet);
    public delegate void ClearEvent(string fieldName);
	public delegate void BallValueEvent(int ballValue);
    public delegate void ModifyCreditsEvent(int delta);
    public delegate void NetworkReadyEvent(bool value);
    public static event GameEvent ObtainInput;
    public static event GameEvent GameStart, GameOver, EndCountdown;
    public static event GameEvent OpenSerial, CloseSerial;
    public static event GameEvent ClearAll, CleanAll;
	public static event GameEvent HIDConnected, HIDDisconnected;
	public static event ClearEvent Clear;
	public static event FingerEvent FingerUp, FingerDown, FingerHover;
	public static event SerialMouseMoveEvent SerialMouseMove;
	public static event SerialMouseButtonEvent SMLBUp, SMLBDown, SMRBUp, SMRBDown;
    public static event RefreshRecordEvent RefreshRecord;
    public static event FieldClickEvent FieldClick;
	public static event ModifyCreditsEvent ModifyCredits;	// 上分/下分
    
	public static event GameEvent SBlowBall, EBlowBall, OpenGate, CloseGate;
	public static event BallValueEvent BallValue;

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

	public static void OnSMLBUp()
	{
		if (SMLBUp != null) SMLBUp();
	}

	public static void OnSMLBDown()
	{
		if (SMLBDown != null) SMLBDown();
	}

	public static void OnSMRBUp()
	{
		if (SMRBUp != null) SMRBUp();
	}
	
	public static void OnSMRBDown()
	{
		if (SMRBDown != null) SMRBDown();
	}

    public static void OnRefreshRecord(int result)
    {
        if (RefreshRecord != null) RefreshRecord(result);
    }

    public static void OnFieldClick(string fieldName, int bet)
    {
        if (FieldClick != null) FieldClick(fieldName, bet);
    }

	// 清除桌面筹码 不返还给玩家
	public static void OnCleanAll()
	{
		if (CleanAll != null) CleanAll();
	}

	// 清除桌面筹码 并返还给玩家
	public static void OnClearAll()
	{
		if (ClearAll != null) ClearAll();
	}

	public static void OnClear(string fieldName)
	{
		if (Clear != null) Clear(fieldName);
	}

	public static void OnEndCountdown()
	{
		if (EndCountdown != null) EndCountdown();
	}

	public static void OnSBlowBall()
	{
		if (SBlowBall != null) SBlowBall();
	}

	public static void OnEBlowBall()
	{
		if (EBlowBall != null) EBlowBall();
	}

	public static void OnOpenGate()
	{
		if (OpenGate != null) OpenGate();
	}

	public static void OnCloseGate()
	{
		if (CloseGate != null) CloseGate();
	}

	public static void OnBallValue(int ballValue)
	{
		if (BallValue != null) BallValue(ballValue);
	}

	public static void OnHIDConnected()
	{
		if (HIDConnected != null) HIDConnected();
	}

	public static void OnHIDDisconnected()
	{
		if (HIDDisconnected != null) HIDDisconnected();
	}

	// 上分/下分
    public static void OnModifyCredits(int delta)
    {
        if (ModifyCredits != null) ModifyCredits(delta);
    }
}
