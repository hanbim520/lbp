using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour 
{
    public int totalCredits
    {
        get { return _totalCredits; }
        set
        {
            _totalCredits = value;
            // Save
            SaveTotalCredits();

			if (_totalCredits >= GameData.GetInstance().baoji)
			{
				// 判断暴机
				IsLock = true;
				ui.ShowWarning(strBaoji[GameData.GetInstance().language]);
			}
			else if (_totalCredits < GameData.GetInstance().baoji && IsLock)
			{
				// 解锁
				IsLock = false;
				ui.HideWarning();
			}
        }
    }
    public int currentBet
	{
		get { return _currentBet; }
		set 
		{ 
			_currentBet = value;
			SaveCurrentBet();
		}
	}
    public int lastWin
	{
		get { return _lastWin; }
		set { _lastWin = value; }
	}
	public int rememberCredits
	{
		get { return _rememberCredits; }
		set 
		{
			_rememberCredits = value;
			SaveRememberCredits();
		} 
	}
	public int LogicPhase
	{
		get { return gamePhase; }
	}
	public bool IsLock
	{
		get { return isLock; }
		set { isLock = value; }
	}


    protected int _totalCredits = 0;
	protected int _currentBet = 0;
	protected int _lastWin = 0;
	protected int _rememberCredits = 0;
	protected bool isPause = false;	// true:程序不往下走
	protected bool isLock = false;	// true:不能押分
	protected int gamePhase = GamePhase.GameEnd;
	protected int ballValue = -1;
	protected string[] strBaoji = new string[]{"Please contact the assistant,\ndevice can't pay more.", "请联系服务员，\n该机台达到赢分上限。"};
	protected string[] strKeoutError = new string[]{"Can't keout now.Total Credits \nshould be greater than {0}.", "现在不能退分。\n总分必须大于 {0}"};

	// Field -- Bet
	public Dictionary<string, int> betFields = new Dictionary<string, int>();
    public MainUILogic ui;
    
    // 断电重启恢复
    protected void FixExitAbnormally()
    {
        int lastBet = PlayerPrefs.GetInt("currentBet", 0);
        _totalCredits = PlayerPrefs.GetInt("totalCredits");
        if (lastBet > 0)
        {
            _totalCredits += lastBet;
            currentBet = 0;
        }
        SaveTotalCredits();
		ui.RefreshLblCredits(totalCredits.ToString());
		ui.RefreshLblWin("0");
		ui.RefreshLblBet("0");

		// Recover remember credits
		_rememberCredits = PlayerPrefs.GetInt("rememberCredits");
		if (rememberCredits > 0)
			ui.RefreshLblRemember(rememberCredits.ToString());
		else
			ui.RefreshLblRemember(string.Empty);

		// Recover card mode ui
		if (GameData.GetInstance().IsCardMode == CardMode.YES && totalCredits == 0)
			GameData.GetInstance().IsCardMode = CardMode.Ready;
		if (GameData.GetInstance().IsCardMode != CardMode.NO)
		{
			ui.RecoverCardMode();
		}

		// 检查有没有锁死
		if (totalCredits >= GameData.GetInstance().baoji)
		{
			IsLock = true;
			ui.ShowWarning(strBaoji[GameData.GetInstance().language]);
		}
    }

    public void SaveTotalCredits()
    {
        PlayerPrefs.SetInt("totalCredits", totalCredits);
        PlayerPrefs.Save();
    }

	public void SaveCurrentBet()
	{
        PlayerPrefs.SetInt("currentBet", currentBet);
		PlayerPrefs.Save();
	}

	public void SaveRememberCredits()
	{
		PlayerPrefs.SetInt("rememberCredits", rememberCredits);
		PlayerPrefs.Save();
	}

    protected virtual void Start()
    {
		FixExitAbnormally();
        RegisterEvents();
    }

    protected virtual void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        GameEventManager.Keyin += Keyin;
		GameEventManager.Keout += Keout;
        GameEventManager.ClearAll += ClearAll;
        GameEventManager.Clear += Clear;
        GameEventManager.CleanAll += CleanAll;
		GameEventManager.FieldClick += Bet;
		GameEventManager.HIDDisconnected += HIDDisconnected;
		GameEventManager.HIDConnected += HIDConnected;
    }

    private void UnregisterEvents()
    {
        GameEventManager.Keyin -= Keyin;
		GameEventManager.Keout -= Keout;
        GameEventManager.ClearAll -= ClearAll;
        GameEventManager.Clear -= Clear;
        GameEventManager.CleanAll -= CleanAll;
		GameEventManager.FieldClick -= Bet;
		GameEventManager.HIDConnected -= HIDConnected;
		GameEventManager.HIDDisconnected -= HIDDisconnected;
    }

	// 下分
	protected void Keout()
	{
		ui.ActiveDlgCard(false);
		if (totalCredits <= 0)
			return;

		if (GameData.GetInstance().IsCardMode == CardMode.YES)
		{
			int couponsKeout = GameData.GetInstance().couponsKeoutRatio * rememberCredits;
			if (totalCredits < couponsKeout)
			{
				string str = string.Format(strKeoutError[GameData.GetInstance().language], couponsKeout);
				ui.ShowWarning(str, true);
				return;
			}

			GameData.GetInstance().IsCardMode = CardMode.NO;
			ui.DisableCardMode();
		}

		GameData.GetInstance().AppendKeyinKeoutRecords(0, totalCredits, 0, 0, 0);
		GameData.GetInstance().zongXia += totalCredits;
		GameData.GetInstance().SaveAccount();

		totalCredits = 0;
		ui.RefreshLblCredits(totalCredits.ToString());
	}

	// 上分
    protected void Keyin(int delta)
    {
		if (delta <= 0)
			return;

		if (GameData.GetInstance().IsCardMode == CardMode.Ready)
		{
			int temp = _totalCredits + delta;
			if (temp >= GameData.GetInstance().couponsStart)
			{
				GameData.GetInstance().IsCardMode = CardMode.YES;
				int giveCredits = Mathf.FloorToInt(GameData.GetInstance().couponsKeyinRatio * 0.01f * temp);
				temp += giveCredits;
				totalCredits = temp;
				rememberCredits = totalCredits;
				GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, 0, 0, giveCredits);
				GameData.GetInstance().zongShang += delta;
				GameData.GetInstance().cardCredits += giveCredits;
				GameData.GetInstance().SaveAccount();
			}
			else
			{
				rememberCredits = 0;
				totalCredits = temp;
				GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, 0, 0, 0);
				GameData.GetInstance().zongShang += delta;
				GameData.GetInstance().SaveAccount();
			}
			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblRemember(rememberCredits.ToString());
		}
		else if (GameData.GetInstance().IsCardMode == CardMode.YES)
		{
			int giveCredits = Mathf.FloorToInt(GameData.GetInstance().couponsKeyinRatio * 0.01f * delta);
			delta = delta + giveCredits;
			rememberCredits = rememberCredits + delta;
			totalCredits = totalCredits + delta;
			GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, 0, 0, giveCredits);
			GameData.GetInstance().zongShang += delta;
			GameData.GetInstance().cardCredits += giveCredits;
			GameData.GetInstance().SaveAccount();

			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblRemember(rememberCredits.ToString());
		}
		else
		{
			totalCredits += delta;
			rememberCredits = 0;
			GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, 0, 0, 0);
			GameData.GetInstance().zongShang += delta;
			GameData.GetInstance().SaveAccount();

			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblRemember(string.Empty);
		}
    }

    protected void ClearAll()
    {
        foreach (KeyValuePair<string, int> item in betFields)
        {
            totalCredits += item.Value;
        }
        currentBet = 0;
        betFields.Clear();
        ui.RefreshLblCredits(totalCredits.ToString());
        ui.RefreshLblBet(currentBet.ToString());
    }
    
    protected void Clear(string fieldName)
    {
        if (string.Equals(fieldName.Substring(0, 1), "e"))
        {
            fieldName = fieldName.Substring(1);
        }
        if (betFields.ContainsKey(fieldName))
        {
            totalCredits += betFields[fieldName];
            currentBet -= betFields[fieldName];
            betFields.Remove(fieldName);
        }
        ui.RefreshLblCredits(totalCredits.ToString());
        ui.RefreshLblBet(currentBet.ToString());
    }

    protected void CleanAll()
    {
        betFields.Clear();
    }

	// 限注
	protected int MaxBet(int maxVal, int originalVal, int betVal)
	{
		if (originalVal + betVal > maxVal)
			betVal = maxVal - originalVal;
		return betVal;
	}

	protected int Bet(string field, int betVal)
	{
		if (totalCredits <= 0)
			return 0;
		// 剩下的筹码小于押分
		if (totalCredits - betVal < 0)
			betVal = totalCredits;
		
		int maxBet = Utils.GetMaxBet(field);
		if (betFields.ContainsKey(field))
		{
			betVal = MaxBet(maxBet, betFields[field], betVal);
		}
		else
		{
			betVal = MaxBet(maxBet, 0, betVal);
		}
		if (betVal > 0)
		{
			if (betFields.ContainsKey(field))
			{
				betFields[field] += betVal;
			}
			else
			{
				betFields.Add(field, betVal);
			}
			currentBet += betVal;
			totalCredits -= betVal;
			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblBet(currentBet.ToString());
		}
		return betVal;
	}

	protected void HIDConnected()
	{
		if (!InputEx.inputEnable)
		{
			InputEx.inputEnable = true;
		}
		
		if (isPause)
		{
			isPause = false;
			ui.HideWarning();
		}
	}
	
	protected void HIDDisconnected()
	{
		if (InputEx.inputEnable)
		{
			InputEx.inputEnable = false;
		}
		
		if (!isPause)
		{
			isPause = true;
			ui.ClearAllEvent(null);
			int language = 0;	// EN
			if (GameData.GetInstance().language == 1)
				language = 1;	// CN
			ui.ShowWarning(Notifies.usbDisconnected[language]);
		}
	}

    protected void AppendLast10(int startCredit, int endCredit, int bet, int win)
    {
        BetRecord br = new BetRecord();
        br.startCredit = startCredit;
        br.endCredit = endCredit;
        br.bet = bet;
        br.win = win;
        br.bets = new List<BetInfo>();
        foreach (KeyValuePair<string, int> item in betFields)
        {
            BetInfo info = new BetInfo();
            info.betField = item.Key;
            info.betValue = item.Value;
            br.bets.Add(info);
        }
        GameData.GetInstance().betRecords.Add(br);
        while (GameData.GetInstance().betRecords.Count > 10)
        {
            GameData.GetInstance().betRecords.RemoveAt(0);
        }
        GameData.GetInstance().SaveBetRecords();
    }
}
