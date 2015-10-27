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
            if (_totalCredits < 0)
                _totalCredits = 0;
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
	protected string[] strTuiBroken = new string[]{"Ticket back device is broken,\nplease contact assistance.", "退币器故障，请\n联系服务员。"};
    protected bool bChangeScene = false;    // From main scene to anothers
    protected string strNextSceneName;
	protected Timer timerPayCoin;

	// Field -- Bet
	public Dictionary<string, int> betFields = new Dictionary<string, int>();
    public MainUILogic ui;
	public HIDUtils hidUtils;
    
	protected virtual void Update()
	{
		UpdateTimer();
	}

	protected void UpdateTimer()
	{
		if (timerPayCoin != null)
			timerPayCoin.Update(Time.deltaTime);
	}

    // 断电重启恢复
    protected void FixExitAbnormally()
    {
        int lastBet = CryptoPrefs.GetInt("currentBet", 0);
        _totalCredits = CryptoPrefs.GetInt("totalCredits");
        if (lastBet > 0)
        {
            _totalCredits += lastBet;
            GameData.GetInstance().ZongYa -= lastBet;
            currentBet = 0;
        }
        SaveTotalCredits();
		ui.RefreshLblCredits(totalCredits.ToString());
		ui.RefreshLblWin("0");
		ui.RefreshLblBet("0");

		// Recover remember credits
		_rememberCredits = CryptoPrefs.GetInt("rememberCredits");
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
        CryptoPrefs.SetInt("totalCredits", totalCredits);
        CryptoPrefs.Save();
    }

	public void SaveCurrentBet()
	{
        CryptoPrefs.SetInt("currentBet", currentBet);
        CryptoPrefs.Save();
	}

	public void SaveRememberCredits()
	{
		CryptoPrefs.SetInt("rememberCredits", rememberCredits);
        CryptoPrefs.Save();
	}

    protected virtual void Start()
    {
        ui = GameObject.Find("UILogic").GetComponent<MainUILogic>();
		hidUtils = GameObject.Find("HIDUtils").GetComponent<HIDUtils>();
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
		GameEventManager.ReceiveCoin += ReceiveCoin;
		GameEventManager.PayCoin += PayCoin;
		GameEventManager.PayCoinCallback += PayCoinCallback;
        GameEventManager.ClearAll += ClearAll;
        GameEventManager.Clear += Clear;
        GameEventManager.CleanAll += CleanAll;
		GameEventManager.FieldClick += Bet;
		GameEventManager.HIDDisconnected += HIDDisconnected;
		GameEventManager.HIDConnected += HIDConnected;
        GameEventManager.ChangeScene += ReadyToChangeScene;
    }

    private void UnregisterEvents()
    {
        GameEventManager.Keyin -= Keyin;
		GameEventManager.Keout -= Keout;
		GameEventManager.ReceiveCoin -= ReceiveCoin;
		GameEventManager.PayCoin -= PayCoin;
		GameEventManager.PayCoinCallback -= PayCoinCallback;
        GameEventManager.ClearAll -= ClearAll;
        GameEventManager.Clear -= Clear;
        GameEventManager.CleanAll -= CleanAll;
		GameEventManager.FieldClick -= Bet;
		GameEventManager.HIDConnected -= HIDConnected;
		GameEventManager.HIDDisconnected -= HIDDisconnected;
        GameEventManager.ChangeScene -= ReadyToChangeScene;
    }

	// 投币
	protected void ReceiveCoin(int count)
	{
		int score = count * GameData.GetInstance().coinToScore;
		GameEventManager.OnKeyin(score, count);
	}

	// 退币
	protected void PayCoin()
	{
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

		// 退币个数
		int coinNum = totalCredits / GameData.GetInstance().coinToScore;
		payCoinCount = 0;
		expectedPayCoinCount = coinNum;
		timerPayCoin = new Timer(3, 1);
		timerPayCoin.OnComplete += DetectPayCoinComplete;
		timerPayCoin.Start();
		hidUtils.PayCoin(coinNum);
	}

	protected void DetectPayCoinComplete()
	{
		timerPayCoin = null;
		hidUtils.StopPayCoin();
		// 处理异常情况
		if (payCoinCount < expectedPayCoinCount)
		{
			ui.ShowWarning(strTuiBroken[GameData.GetInstance().language], true, 3);
			if (payCoinCount > 0)
			{
				int scoreNum = payCoinCount * GameData.GetInstance().coinToScore;
				totalCredits -= scoreNum;
				GameData.GetInstance().AppendKeyinKeoutRecords(0, scoreNum, 0, payCoinCount, 0);
				GameData.GetInstance().zongTui += payCoinCount;
				GameData.GetInstance().zongXia += scoreNum;
				GameData.GetInstance().currentZongXia += scoreNum;
				GameData.GetInstance().totalWin = GameData.GetInstance().zongShang - GameData.GetInstance().zongXia;
				GameData.GetInstance().currentWin = GameData.GetInstance().currentZongShang - GameData.GetInstance().currentZongXia;
				GameData.GetInstance().SaveAccount();
			}
		}
	}

	protected int payCoinCount = 0;				// 已退币的个数
	protected int expectedPayCoinCount = 0;		// 期望要退币的个数
	protected void PayCoinCallback(int count)
	{
		if (timerPayCoin != null)
		{
			timerPayCoin.Restart();
		}
		payCoinCount += count;
		// 退分量
		int scoreNum = payCoinCount * GameData.GetInstance().coinToScore;
		if (payCoinCount >= expectedPayCoinCount)
		{
			hidUtils.StopPayCoin();
			// 剩余分数
			totalCredits -= scoreNum;

			GameData.GetInstance().AppendKeyinKeoutRecords(0, scoreNum, 0, payCoinCount, 0);
			GameData.GetInstance().zongTui += payCoinCount;
			GameData.GetInstance().zongXia += scoreNum;
			GameData.GetInstance().currentZongXia += scoreNum;
			GameData.GetInstance().totalWin = GameData.GetInstance().zongShang - GameData.GetInstance().zongXia;
			GameData.GetInstance().currentWin = GameData.GetInstance().currentZongShang - GameData.GetInstance().currentZongXia;
			GameData.GetInstance().SaveAccount();
			
			ui.RefreshLblCredits(totalCredits.ToString());
			timerPayCoin.Stop();
			timerPayCoin = null;
			return;
		}
		// 剩余分数
		int residue = totalCredits - scoreNum;
		ui.RefreshLblCredits(residue.ToString());
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
		GameData.GetInstance().currentZongXia += totalCredits;
		GameData.GetInstance().totalWin = GameData.GetInstance().zongShang - GameData.GetInstance().zongXia;
		GameData.GetInstance().currentWin = GameData.GetInstance().currentZongShang - GameData.GetInstance().currentZongXia;
		GameData.GetInstance().SaveAccount();

		totalCredits = 0;
		ui.RefreshLblCredits(totalCredits.ToString());
	}

	// 上分
    protected void Keyin(int delta, int coinNum = 0)
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
				GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, coinNum, 0, giveCredits);
				GameData.GetInstance().zongTou += coinNum;
				GameData.GetInstance().zongShang += delta;
				GameData.GetInstance().currentZongShang += delta;
				GameData.GetInstance().cardCredits += giveCredits;
				GameData.GetInstance().totalWin = GameData.GetInstance().zongShang - GameData.GetInstance().zongXia;
				GameData.GetInstance().currentWin = GameData.GetInstance().currentZongShang - GameData.GetInstance().currentZongXia;
				GameData.GetInstance().SaveAccount();
			}
			else
			{
				rememberCredits = 0;
				totalCredits = temp;
				GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, coinNum, 0, 0);
				GameData.GetInstance().zongTou += coinNum;
				GameData.GetInstance().zongShang += delta;
				GameData.GetInstance().currentZongShang += delta;
				GameData.GetInstance().totalWin = GameData.GetInstance().zongShang - GameData.GetInstance().zongXia;
				GameData.GetInstance().currentWin = GameData.GetInstance().currentZongShang - GameData.GetInstance().currentZongXia;
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
			GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, coinNum, 0, giveCredits);
			GameData.GetInstance().zongTou += coinNum;
			GameData.GetInstance().zongShang += delta;
			GameData.GetInstance().currentZongShang += delta;
			GameData.GetInstance().cardCredits += giveCredits;
			GameData.GetInstance().totalWin = GameData.GetInstance().zongShang - GameData.GetInstance().zongXia;
			GameData.GetInstance().currentWin = GameData.GetInstance().currentZongShang - GameData.GetInstance().currentZongXia;
			GameData.GetInstance().SaveAccount();

			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblRemember(rememberCredits.ToString());
		}
		else
		{
			totalCredits += delta;
			rememberCredits = 0;
			GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, coinNum, 0, 0);
			GameData.GetInstance().zongTou += coinNum;
			GameData.GetInstance().zongShang += delta;
			GameData.GetInstance().currentZongShang += delta;
			GameData.GetInstance().totalWin = GameData.GetInstance().zongShang - GameData.GetInstance().zongXia;
			GameData.GetInstance().currentWin = GameData.GetInstance().currentZongShang - GameData.GetInstance().currentZongXia;
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
        GameData.GetInstance().ZongYa -= currentBet;
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
            GameData.GetInstance().ZongYa -= betFields[fieldName];
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
            GameData.GetInstance().ZongYa += betVal;
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

		isPause = false;
		ui.HideWarning();
		StartCoroutine(AfterConnHID());
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
			ui.ShowWarning(Notifies.usbDisconnected[GameData.GetInstance().language]);
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

    protected void ReadyToChangeScene(string sceneName)
    {
        bChangeScene = true;
        strNextSceneName = sceneName;
        ui.ActiveBackendTip("Ready To\n" + sceneName);
    }

    protected void ChangeScene()
    {
        bChangeScene = false;
        GameData.GetInstance().NextLevelName = strNextSceneName;
        Application.LoadLevel(Scenes.Loading);
    }

	// 第一次连上机芯应打开一次门
	protected void OpenGate()
	{
		if (GameData.GetInstance().deviceIndex == 1)
		{
			hidUtils.OpenGate();
		}
	}

	// 发送guid给加密片验证
	protected void SendCheckInfo()
	{
		hidUtils.SendCheckInfo();
	}

	protected IEnumerator AfterConnHID()
	{
		SendCheckInfo();
		yield return new WaitForSeconds(2);
		OpenGate();
	}
}
