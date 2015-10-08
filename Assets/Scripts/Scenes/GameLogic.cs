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
		set { _currentBet = value; }
	}
    public int lastWin
	{
		get { return _lastWin; }
		set { _lastWin = value; }
	}
	public int LogicPhase
	{
		get { return gamePhase; }
	}

    protected int _totalCredits = 10000;
	protected int _currentBet = 0;
	protected int _lastWin = 0;
	protected bool isPause = false;
	protected int gamePhase = GamePhase.GameEnd;
	protected int ballValue = -1;


    // 断电重启恢复
    protected void FixExitAbnormally()
    {
        int lastBet = PlayerPrefs.GetInt("currentBet", 0);
        if (lastBet > 0)
        {
            _totalCredits = PlayerPrefs.GetInt("totalCredits", 0);
            _totalCredits += lastBet;
            currentBet = 0;
            SaveTotalCredits();
        }
    }

    public void SaveTotalCredits()
    {
        PlayerPrefs.SetInt("totalCredits", _totalCredits);
        PlayerPrefs.SetInt("currentBet", currentBet);
        PlayerPrefs.Save();
    }

	protected virtual void Awake()
	{
		GameData.GetInstance().ReadDataFromDisk();
	}

    protected virtual void Start()
    {
        RegisterEvents();
    }

    protected virtual void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        GameEventManager.ModifyCredits += ModifyCredits;
    }

    private void UnregisterEvents()
    {
        GameEventManager.ModifyCredits -= ModifyCredits;
    }

	// 上分/下分
    protected void ModifyCredits(int delta)
    {
        _totalCredits += delta;
        if (_totalCredits < 0)
        {
            _totalCredits = 0;
        }
    }
}
