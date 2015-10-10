using UnityEngine;
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


    protected int _totalCredits = 0;
	protected int _currentBet = 0;
	protected int _lastWin = 0;
	protected int _rememberCredits = 0;
	protected bool isPause = false;
	protected int gamePhase = GamePhase.GameEnd;
	protected int ballValue = -1;
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

	protected virtual void Awake()
	{
		GameData.GetInstance().ReadDataFromDisk();
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
    }

    private void UnregisterEvents()
    {
        GameEventManager.Keyin -= Keyin;
		GameEventManager.Keout -= Keout;
        GameEventManager.ClearAll -= ClearAll;
        GameEventManager.Clear -= Clear;
        GameEventManager.CleanAll -= CleanAll;
    }

	// 下分
	protected void Keout()
	{
		if (GameData.GetInstance().IsCardMode == CardMode.YES)
		{
			int couponsKeout = GameData.GetInstance().couponsKeoutRatio * rememberCredits;
			if (totalCredits < couponsKeout)
				return;

			GameData.GetInstance().IsCardMode = CardMode.NO;
			ui.DisableCardMode();
		}

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
				temp += Mathf.FloorToInt(GameData.GetInstance().couponsKeyinRatio * 0.01f * temp);
				totalCredits = temp;
				rememberCredits = totalCredits;
			}
			else
			{
				rememberCredits = 0;
				totalCredits = temp;
			}
			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblRemember(rememberCredits.ToString());
		}
		else if (GameData.GetInstance().IsCardMode == CardMode.YES)
		{
			delta = delta + Mathf.FloorToInt(GameData.GetInstance().couponsKeyinRatio * 0.01f * delta);
			rememberCredits = rememberCredits + delta;
			totalCredits = totalCredits + delta;

			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblRemember(rememberCredits.ToString());
		}
		else
		{
			totalCredits += delta;
			rememberCredits = 0;
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
}
