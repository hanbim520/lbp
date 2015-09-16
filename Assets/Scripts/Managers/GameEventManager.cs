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
    public static event GameEvent ObtainInput;
    public static event GameEvent GameStart, GameOver;
    public static event GameEvent OpenSerial, CloseSerial;
	public static event FingerEvent FingerUp, FingerDown, FingerHover;
	public static event SerialMouseMoveEvent SerialMouseMove;
	public static event SerialMouseButtonEvent SMLBUp, SMLBDown, SMRBUp, SMRBDown;
    public static event RefreshRecordEvent RefreshRecord;
    public static event FieldClickEvent FieldClick;

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
}
