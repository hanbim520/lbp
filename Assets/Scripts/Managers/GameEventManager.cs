﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class GameEventManager 
{
    public delegate void GameEvent();
    public delegate void GameEventWithId(int eventId);
    public delegate void GameEventWithString(string text);
	public delegate void FingerEvent(UInt16 x, UInt16 y);
	public delegate void SerialMouseMoveEvent(sbyte deltaX, sbyte deltaY);
	public delegate void SerialMouseButtonEvent();
    public delegate int FieldClickEvent(string fieldName, int bet);
    public delegate void KeyinEvent(int delta, int coinNum);
	public delegate void ChooseFieldsEvent(Transform hitObject);	// 选中多个区域 选中区域显亮色
	public delegate void DebugLogEvent(int eventId, string log);
	public delegate void RakeInitEvent(int type, int lineId, float startX1, float startX2, ref List<Transform> winChips);
    public static event GameEvent GameStart, GameOver, EndCountdown;
    public static event GameEvent OpenSerial, CloseSerial;
    public static event GameEvent ClearAll, CleanAll;
	public static event GameEvent HIDConnected, HIDDisconnected;
	public static event GameEventWithString Clear;
	public static event FingerEvent FingerUp, FingerDown, FingerHover;
	public static event SerialMouseMoveEvent SerialMouseMove;
	public static event SerialMouseButtonEvent SMLBUp, SMLBDown, SMRBUp, SMRBDown;
    public static event GameEventWithId RefreshRecord;
    public static event FieldClickEvent FieldClick;
	public static event KeyinEvent Keyin;	// 上分
	public static event GameEvent Keout;	// 下分
    public static event GameEventWithId ReceiveCoin;	// 投币
	public static event GameEvent PayCoin;				// 退币
    public static event GameEventWithId PayCoinCallback;	// 退币机发来的退币数
	public static event GameEvent OpenKey;				// 旋转物理钥匙 (上下分)
	public static event GameEventWithString ChangeScene;
	public static event GameEvent PrintCodeSuccess, PrintCodeFail;
	public static event GameEvent ClientDisconnect;	// 分机通讯断开
    public static event GameEventWithId LotteryChange;
	public static event ChooseFieldsEvent ChooseFields;	
	public static event DebugLogEvent DebugLog;		// 在屏幕上显示log
    
	public static event GameEvent SBlowBall, EBlowBall, OpenGate, CloseGate;
    public static event GameEventWithId BallValue;
    public static event GameEventWithId Prompt, ResultPrompt;
    public static event GameEventWithId OddsPrompt;
	public static event GameEvent SyncData;
	public static event GameEventWithId BreakdownTip;
	public static event GameEvent SyncUI;			// 同步后台设置后，设置分机ui
	public static event GameEvent SyncInputDevice;  // 同步输入设备
	public static event RakeInitEvent RakeInit;

	public static void TriggerOpenSerial()
	{
		if (OpenSerial != null) OpenSerial();
	}

	public static void TriggerCloseSerial()
	{
		if (CloseSerial != null) CloseSerial();
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

    public static int OnFieldClick(string fieldName, int bet)
    {
        if (FieldClick != null) 
			return FieldClick(fieldName, bet);
		return 0;
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

	// 上分
    public static void OnKeyin(int delta, int coinNum = 0)
    {
        if (Keyin != null) Keyin(delta, coinNum);
    }

	// 下分
	public static void OnKeout()
	{
		if (Keout != null) Keout();
	}

    public static void OnChangeScene(string sceneName)
    {
        if (ChangeScene != null) ChangeScene(sceneName);
    }

	public static void OnPrintCodeSuccess()
	{
		if (PrintCodeSuccess != null) PrintCodeSuccess();
	}

	public static void OnPrintCodeFail()
	{
		if (PrintCodeFail != null) PrintCodeFail();
	}

	public static void OnClientDisconnect()
	{
		if (ClientDisconnect != null) ClientDisconnect();
	}

	public static void OnReceiveCoin(int count)
	{
		if (ReceiveCoin != null) ReceiveCoin(count);
	}

	public static void OnPayCoin()
	{
		if (PayCoin != null) PayCoin();
	}

	public static void OnPayCoinCallback(int count)
	{
		if (PayCoinCallback != null) PayCoinCallback(count);
	}

	public static void OnOpenKey()
	{
		if (OpenKey != null) OpenKey();
	}

	public static void OnLotteryChange(int value)
	{
		if (LotteryChange != null) LotteryChange(value); 
	}

	public static void OnPrompt(int promptId)
	{
		if (Prompt != null) Prompt(promptId);
	}

	public static void OnResultPrompt(int result)
	{
		if (ResultPrompt != null) ResultPrompt(result);
	}

	public static void OnOddsPrompt(int odds)
	{
		if (OddsPrompt != null) OddsPrompt(odds);
	}

	public static void OnSyncData()
	{
		if (SyncData != null) SyncData();
	}

	public static void OnChooseFields(Transform hitObject)
	{
		if (ChooseFields != null) ChooseFields(hitObject);
	}

	public static void OnBreakdownTip(int breakdownType)
	{
		if (BreakdownTip != null) BreakdownTip(breakdownType);
	}

	public static void OnSyncUI()
	{
		if (SyncUI != null) SyncUI();
	}

	public static void OnSyncInputDevice()
	{
		if (SyncInputDevice != null) SyncInputDevice();
	}

	public static void OnDebugLog(int eventId, string log)
	{
		if (DebugLog != null) DebugLog(eventId, log);
	}

	public static void OnRakeInit(int type, int lineId, float startX1, float startX2, ref List<Transform> winChips)
	{
		if (RakeInit != null) RakeInit(type, lineId, startX1, startX2, ref winChips);
	}
}
