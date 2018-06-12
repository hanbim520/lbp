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
    public bool bCanBet = true;
    public bool isPayingCoin = false;


    protected int _totalCredits = 0;
	protected int _currentBet = 0;
	protected int _lastWin = 0;
	protected int _rememberCredits = 0;
	protected bool isPause = false;	// true:程序不往下走
	protected bool isLock = false;	// true:不能押分
	protected int gamePhase = GamePhase.GameStart;
	protected int ballValue = -1;
	protected int curLuckySum = 0;							// 当前局压中彩金的总筹码数
	protected List<int> lotteryValues = new List<int>();	// 彩票值
	protected string[] strBaoji = new string[]{"Please contact the assistant,\ndevice can't pay more.", "请联系服务员，\n该机台达到赢分上限。"};
	protected string[] strKeoutError = new string[]{"Can't keout now.Total Credits \nshould be greater than {0}.", "现在不能退分。\n总分必须大于 {0}"};
	protected string[] strTuiBroken = new string[]{"Ticket back device is broken,\nplease contact assistance.", "退币器故障，请\n联系服务员。"};
    protected bool bChangeScene = false;    // From main scene to anothers
    protected string strNextSceneName;
    protected Timer timerPayCoin;
	protected Timer timerRevCoin;
    protected int payCoinCount = 0;             // 已退币的个数
    protected int expectedPayCoinCount = 0;     // 期望要退币的个数
	protected int revCoinCount = 0;				// 已投币的个数
	protected bool bFirstOpenGate = false;

	// Field -- Bet
	public Dictionary<string, int> betFields = new Dictionary<string, int>();		// 本机押分情况
    public MainUILogic ui;
	public HIDUtils hidUtils;
	public GoldfingerUtils goldfingerUtils;
    
	protected virtual void Update()
	{
		UpdateTimer();
	}

	protected void UpdateTimer()
	{
		if (timerPayCoin != null)
			timerPayCoin.Update(Time.deltaTime);
        if (timerRevCoin != null)
            timerRevCoin.Update(Time.deltaTime);
	}

    // 断电重启恢复
    protected void FixExitAbnormally()
    {
        int lastBet = CryptoPrefs.GetInt("currentBet", 0);
        _totalCredits = CryptoPrefs.GetInt("totalCredits");
		int powerOffCompensate = GameData.GetInstance().powerOffCompensate;
        if (lastBet > 0)
        {
			// 断电赔付
			if (powerOffCompensate > 0)
			{
				_totalCredits += lastBet;
				GameData.GetInstance().ZongYa -= lastBet;
			}
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
		#if UNITY_STANDALONE_LINUX
		hidUtils = GameObject.Find("HIDUtils").GetComponent<HIDUtils>();
		#endif
		#if UNITY_ANDROID
		goldfingerUtils = GameObject.Find("AndroidHIDUtils").GetComponent<GoldfingerUtils>();
		#endif
		FixExitAbnormally();
        RegisterEvents();
    }

    protected virtual void OnDestroy()
    {
        UnregisterEvents();
		if (revCoinCount > 0)
		{
			GameData.GetInstance().AppendKeyinKeoutRecords(revCoinCount, 0, 0, 0, 0);
			revCoinCount = 0;
		}
    }

    private void RegisterEvents()
    {
        GameEventManager.Keyin += Keyin;
		GameEventManager.Keout += Keout;
		GameEventManager.ReceiveCoin += ReceiveCoin;
		GameEventManager.PayCoin += PayCoin;
		GameEventManager.PayCoinCallback += PayCoinCallback;
        GameEventManager.CleanAll += CleanAll;
		GameEventManager.HIDDisconnected += HIDDisconnected;
		GameEventManager.HIDConnected += HIDConnected;
        GameEventManager.ChangeScene += ReadyToChangeScene;
		GameEventManager.KeyinOnce += KeyinOnce;
		GameEventManager.KeyinHold += KeyinHold;
        GameEventManager.DetectPayCoinError += DetectPayCoinError;
        GameEventManager.DetectRevCoinError += DetectRevCoinError;
    }

    private void UnregisterEvents()
    {
        GameEventManager.Keyin -= Keyin;
		GameEventManager.Keout -= Keout;
		GameEventManager.ReceiveCoin -= ReceiveCoin;
		GameEventManager.PayCoin -= PayCoin;
		GameEventManager.PayCoinCallback -= PayCoinCallback;
        GameEventManager.CleanAll -= CleanAll;
		GameEventManager.HIDConnected -= HIDConnected;
		GameEventManager.HIDDisconnected -= HIDDisconnected;
        GameEventManager.ChangeScene -= ReadyToChangeScene;
		GameEventManager.KeyinOnce -= KeyinOnce;
		GameEventManager.KeyinHold -= KeyinHold;
        GameEventManager.DetectPayCoinError -= DetectPayCoinError;
        GameEventManager.DetectRevCoinError -= DetectRevCoinError;
    }

	// 投币
	protected void ReceiveCoin(int count)
	{
        if (timerRevCoin != null)
        {
            timerRevCoin.Restart();
        }

		int score = count * GameData.GetInstance().coinToScore;
		GameEventManager.OnKeyin(score, count);
	}

    protected void DetectRevCoinError()
    {
        if (timerRevCoin != null)
            return;

        timerRevCoin = new Timer(3, 1);
        timerRevCoin.Tick += DetectRevCoinComplete;
        timerRevCoin.Start();
    }

    protected void DetectRevCoinComplete()
    {
		GameData.GetInstance().AppendKeyinKeoutRecords(revCoinCount, 0, 0, 0, 0);
		revCoinCount = 0;
        timerRevCoin = null;
        goldfingerUtils.StopRevCoin();
    }

	// 退币
	protected void PayCoin()
	{
		if (totalCredits <= 0 ||
            isPayingCoin)
			return;

        isPayingCoin = true;
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
		PayCoin(coinNum);
	}

    protected void DetectPayCoinError()
    {
        if (timerPayCoin != null)
            return;

        timerPayCoin = new Timer(3, 1);
        timerPayCoin.Tick += DetectPayCoinComplete;
        timerPayCoin.Start();
    }

	protected void DetectPayCoinComplete()
	{
		timerPayCoin = null;
		StopPayCoin();
		GameData.GetInstance().AppendKeyinKeoutRecords(0, 0, 0, payCoinCount, 0);
		// 处理异常情况
		if (payCoinCount < expectedPayCoinCount)
		{
			ui.ShowWarning(strTuiBroken[GameData.GetInstance().language], true, 3);
		}
        bCanBet = true;
        isPayingCoin = false;
	}
	
	protected void PayCoinCallback(int count)
	{
        bCanBet = false;
        isPayingCoin = true;
		if (timerPayCoin != null)
		{
			timerPayCoin.Restart();
		}
		payCoinCount += count;
        // 当前退分量
        int deltaNum = count * GameData.GetInstance().coinToScore;
		if (payCoinCount >= expectedPayCoinCount)
		{
			StopPayCoin();
			GameData.GetInstance().AppendKeyinKeoutRecords(0, 0, 0, payCoinCount, 0);
            if (timerPayCoin != null)
            {
                timerPayCoin.Stop();
                timerPayCoin = null;
            }
            bCanBet = true;
            isPayingCoin = false;
		}
        // 剩余分数
        totalCredits -= deltaNum;
        
		GameEventManager.OnStopWatch(0, 0, 0, count);
        GameData.GetInstance().zongTui += count;
		GameData.GetInstance().totalWin = GameData.GetInstance().zongShang + GameData.GetInstance().zongTou - GameData.GetInstance().zongXia - GameData.GetInstance().zongTui;
        GameData.GetInstance().SaveAccount();
        
        ui.RefreshLblCredits(totalCredits.ToString());
    }
    
    // 下分
    protected void Keout()
	{
		ui.ActiveDlgCard(false);
		if (totalCredits <= 0 ||
            isPayingCoin)
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
		GameEventManager.OnStopWatch(0, totalCredits, 0, 0);
		GameData.GetInstance().zongXia += totalCredits;
		GameData.GetInstance().totalWin = GameData.GetInstance().zongShang + GameData.GetInstance().zongTou - GameData.GetInstance().zongTui - GameData.GetInstance().zongXia;
		GameData.GetInstance().SaveAccount();

		totalCredits = 0;
		ui.RefreshLblCredits(totalCredits.ToString());
	}

	protected void KeyinOnce()
	{
        if (isPayingCoin)
            return;

		int betVal = GameData.GetInstance().betChipValues[ui.CurChipIdx];
		Keyin(betVal);
	}

	protected void KeyinHold()
	{
        if (isPayingCoin)
            return;

		int betVal = GameData.GetInstance().betChipValues[ui.CurChipIdx] * 10;
		Keyin(betVal);
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
				if (coinNum > 0)
				{
					GameData.GetInstance().zongTou += coinNum;
					revCoinCount += coinNum;
					GameEventManager.OnStopWatch(0, 0, coinNum, 0);
				}
				else
				{
					GameData.GetInstance().zongShang += delta;
					GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, 0, 0, giveCredits);
					GameEventManager.OnStopWatch(delta, 0, 0, 0);
				}
				GameData.GetInstance().cardCredits += giveCredits;
				GameData.GetInstance().totalWin = (GameData.GetInstance().zongShang + GameData.GetInstance().zongTou) - (GameData.GetInstance().zongXia + GameData.GetInstance().zongTui);
				GameData.GetInstance().SaveAccount();
			}
			else
			{
				rememberCredits = 0;
				totalCredits = temp;
				if (coinNum > 0)
				{
					GameData.GetInstance().zongTou += coinNum;
					revCoinCount += coinNum;
					GameEventManager.OnStopWatch(0, 0, coinNum, 0);
				}
				else
				{
					GameData.GetInstance().zongShang += delta;
					GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, 0, 0, 0);
					GameEventManager.OnStopWatch(delta, 0, 0, 0);
				}
				GameData.GetInstance().totalWin = (GameData.GetInstance().zongShang + GameData.GetInstance().zongTou) - (GameData.GetInstance().zongXia + GameData.GetInstance().zongTui);
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
			if (coinNum > 0)
			{
				GameData.GetInstance().zongTou += coinNum;
				revCoinCount += coinNum;
				GameEventManager.OnStopWatch(0, 0, coinNum, 0);
			}
			else
			{
				GameData.GetInstance().zongShang += delta;
				GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, 0, 0, giveCredits);
				GameEventManager.OnStopWatch(delta, 0, 0, 0);
			}
			GameData.GetInstance().cardCredits += giveCredits;
			GameData.GetInstance().totalWin = (GameData.GetInstance().zongShang + GameData.GetInstance().zongTou) - (GameData.GetInstance().zongXia + GameData.GetInstance().zongTui);
			GameData.GetInstance().SaveAccount();

			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblRemember(rememberCredits.ToString());
		}
		else
		{
			totalCredits += delta;
			rememberCredits = 0;
			if (coinNum > 0)
			{
				GameData.GetInstance().zongTou += coinNum;
				revCoinCount += coinNum;
				GameEventManager.OnStopWatch(0, 0, coinNum, 0);
			}
			else
			{
				GameData.GetInstance().zongShang += delta;
				GameData.GetInstance().AppendKeyinKeoutRecords(delta, 0, 0, 0, 0);
				GameEventManager.OnStopWatch(delta, 0, 0, 0);
			}
			GameData.GetInstance().totalWin = (GameData.GetInstance().zongShang + GameData.GetInstance().zongTou) - (GameData.GetInstance().zongXia + GameData.GetInstance().zongTui);
			GameData.GetInstance().SaveAccount();

			ui.RefreshLblCredits(totalCredits.ToString());
			ui.RefreshLblRemember(string.Empty);
		}
    }

    protected void CleanAll()
    {
        betFields.Clear();
    }

	/// <summary>
	/// 计算限注
	/// </summary>
	/// <returns>能押注的金额.</returns>
	/// <param name="maxVal">最大押注金额.</param>
	/// <param name="originalVal">已经押注金额.</param>
	/// <param name="betVal">欲单次押注金额.</param>
	protected int MaxBet(int maxVal, int originalVal, int betVal)
	{
		if (originalVal + betVal > maxVal)
			betVal = maxVal - originalVal;
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

	protected IEnumerator AfterConnHID()
	{
		// 发送guid给加密片验证
		SendCheckInfo();
		yield return new WaitForSeconds(3);
		if (GameData.GetInstance().deviceIndex == 1)
		{
			bFirstOpenGate = true;
			FirstOpenGate();
		}
	}
	
	// 第一次连上机芯应打开一次门
	protected void FirstOpenGate()
	{
		if (GameData.GetInstance().deviceIndex == 1)
		{
			OpenGate();
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
			ui.ShowWarning(Notifies.usbDisconnected[GameData.GetInstance().language]);
		}
	}

    protected void AppendLast10(int startCredit, int endCredit, int bet, int win, int luckyWin, int ball_value)
    {
        BetRecord br = new BetRecord();
        br.startCredit = startCredit;
        br.endCredit = endCredit;
        br.bet = bet;
        br.win = win;
		br.luckyWin = luckyWin;
		br.ballValue = ball_value;
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

	protected void AppendLastBets(int totalBetCredit)
	{
		if (betFields.Count > 0)
		{
			GameData.GetInstance().RemoveLastBet();
			GameData.GetInstance().SaveLastBet(ref betFields, totalBetCredit);
		}
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
		UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.Loading);
    }

	public void SetPause(bool pause)
	{
		isPause = pause;
	}

	public bool IsPause()
	{
		return isPause;
	}

	private void OpenGate()
	{
		#if UNITY_STANDALONE_LINUX
		hidUtils.OpenGate();
		#endif
		#if UNITY_ANDROID
//		goldfingerUtils.OpenGate();
        Utils.Seed(System.DateTime.Now.Millisecond + System.DateTime.Now.Second + System.DateTime.Now.Minute + System.DateTime.Now.Hour);
        int time = GameData.GetInstance().gameDifficulty + Utils.GetRandom(1200, 3000);
        StartCoroutine(goldfingerUtils.BlowBall(time));
		#endif
	}
	
	private void StopPayCoin()
	{
		#if UNITY_STANDALONE_LINUX
		hidUtils.StopPayCoin();
		#endif
		#if UNITY_ANDROID
		goldfingerUtils.StopPayCoin();
		#endif
	}
	
	private void PayCoin(int coinNum)
	{
		#if UNITY_STANDALONE_LINUX
		hidUtils.PayCoin(coinNum);
		#endif
		#if UNITY_ANDROID
        goldfingerUtils.PayCoin(coinNum);
		#endif
	}
	
	private void SendCheckInfo()
	{
		#if UNITY_STANDALONE_LINUX
		hidUtils.SendCheckInfo();
		#endif
		#if UNITY_ANDROID
		goldfingerUtils.SendCheckInfo();
		#endif
	}
	
	public void BlowBall(int time)
	{
		#if UNITY_STANDALONE_LINUX
		hidUtils.BlowBall(time);
		#endif
		#if UNITY_ANDROID
		StartCoroutine(goldfingerUtils.BlowBall(time));
		#endif
	}
}
