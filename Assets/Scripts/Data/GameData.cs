using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
{0,13,1,00,27,10,25,29,12,8,19,31,18,6,21,33,16,4,23,35,14,2,0,28,9,26,30,11,7,20,32,17,5,22,34,15,3,24,36}
{36,13,1,00,27,10,25,29,12,8,19,31,18,6,21,33,16,4,23,35,14,2,0,28,9,26,30,11,7,20,32,17,5,22,34,15,3,24,0}
 */
public class GameData
{
	public static bool debug = true;

    // Setting menu
    public int betTimeLimit;
    public int coinToScore;
	public int gameDifficulty;
    public int baoji;
	public List<int> betChipValues = new List<int>();
	public int max36Value;
	public int max18Value;
	public int max12Value;
	public int max9Value;
	public int max6Value;
	public int max3Value;
	public int max2Value;
	// 优惠卡分限
	public int couponsStart;
	public int couponsKeyinRatio;	// 1%~100%
	public int couponsKeoutRatio;	
	public int maxNumberOfFields; 	// 37 or 38
	public int beginSessions;	// 起始场次
	public int maxNumberOfChips;	// 1 ~ 6
	
    // Account
    public int[] zongShang;
    public int[] zongXia;
    public int[] zongTou;
    public int[] zongTui;
    public int[] zongYa;
    public int[] zongYing;
	public int[] caiPiao;

	// Odds
    public float yanseOdds = 1.0f;
    public float shuangOdds = 1.97f;
    public float danOdds = 1.97f;
    public float daOdds = 1.97f;
    public float xiaoOdds = 1.97f;
    public float duOdds = 36.97f;

	public string deviceId; // unique string
	public int deviceIndex;	// 1, 2, 3...
	public Dictionary<int, string> clientsId;

	// Serial mouse coordinates
	public float serialMouseX;
    public float serialMouseY;

    public int resolutionWidth = 1440;
    public int resolutionHeight = 1080;

	// Det(determinant) for Touch-screen coordinate to UI coordinate
	public float TA;
	public float TB;
	public float TC;
	public float TD;
	public float TE;
	public float TF;

	// Custom setting
	public int language;			// 0:EN 1:CN
	public int displayType; 		// 0:classic 1:ellipse
    public string systemPassword;
    public string accountPassword;
    public int passwordLength = 6;

    // 记录最近10场押分情况
    public List<BetRecord> betRecords = new List<BetRecord>();
	public Queue<int> records = new Queue<int>();	// 00:用37表示
    public Dictionary<int, ResultType> colorTable = new Dictionary<int, ResultType>();

	public int[] ballValue38 = new int[]{36,13,1,37,27,10,25,29,12,8,19,31,18,6,21,33,16,4,23,35,14,2,0,28,9,26,30,11,7,20,32,17,5,22,34,15,3,24};
	public int[] ballValue37 = new int[]{36,13,1,37,27,10,25,29,12,8,19,31,18,6,21,33,16,4,23,35,14,2,0,28,9,26,30,11,7,20,32,17,5,22,34,15,3,24};

	// For host
	private int connectClientsTime = 15;
	public int ConnectClientsTime
	{
		get { return connectClientsTime; }
	}

	private const int maxNumOfPlayers = 8;
	public int MaxNumOfPlayers
	{
		get { return maxNumOfPlayers; }
	}

	private string nextLevelName;
	public string NextLevelName
	{
		get { return nextLevelName; }
		set { nextLevelName = value; }
	}

	private GameData()
	{
		deviceId = PlayerPrefs.GetString("deviceId", string.Empty);
		if (deviceId == string.Empty)
		{
			deviceId = Guid.NewGuid().ToString();
			PlayerPrefs.SetString("deviceId", deviceId);
			PlayerPrefs.Save();
		}
        deviceIndex = PlayerPrefs.GetInt("deviceIndex", 0);
		deviceIndex = 1;

        colorTable.Add(37, ResultType.Green);    // 37: 00
		colorTable.Add(0, ResultType.Green);
		colorTable.Add(1, ResultType.Red);
		colorTable.Add(3, ResultType.Red);
		colorTable.Add(5, ResultType.Red);
		colorTable.Add(7, ResultType.Red);
		colorTable.Add(9, ResultType.Red);
		colorTable.Add(12, ResultType.Red);
		colorTable.Add(14, ResultType.Red);
		colorTable.Add(16, ResultType.Red);
		colorTable.Add(18, ResultType.Red);
		colorTable.Add(19, ResultType.Red);
		colorTable.Add(21, ResultType.Red);
		colorTable.Add(23, ResultType.Red);
		colorTable.Add(25, ResultType.Red);
		colorTable.Add(27, ResultType.Red);
		colorTable.Add(30, ResultType.Red);
		colorTable.Add(32, ResultType.Red);
		colorTable.Add(34, ResultType.Red);
		colorTable.Add(36, ResultType.Red);

		colorTable.Add(2, ResultType.Black);
		colorTable.Add(4, ResultType.Black);
		colorTable.Add(6, ResultType.Black);
		colorTable.Add(8, ResultType.Black);
		colorTable.Add(10, ResultType.Black);
		colorTable.Add(11, ResultType.Black);
		colorTable.Add(13, ResultType.Black);
		colorTable.Add(15, ResultType.Black);
		colorTable.Add(17, ResultType.Black);
		colorTable.Add(20, ResultType.Black);
		colorTable.Add(22, ResultType.Black);
		colorTable.Add(24, ResultType.Black);
		colorTable.Add(26, ResultType.Black);
		colorTable.Add(28, ResultType.Black);
		colorTable.Add(29, ResultType.Black);
		colorTable.Add(31, ResultType.Black);
        colorTable.Add(33, ResultType.Black);
        colorTable.Add(35, ResultType.Black);

        zongShang = new int[maxNumOfPlayers];
        zongXia = new int[maxNumOfPlayers];
        zongTou = new int[maxNumOfPlayers];
        zongTui = new int[maxNumOfPlayers];
        zongYa = new int[maxNumOfPlayers];
        zongYing = new int[maxNumOfPlayers];
        caiPiao = new int[maxNumOfPlayers];
	}

    private static GameData instance;
    public static GameData GetInstance()
    {
        if (instance == null)
        {
            instance = new GameData();
        }
        return instance;
    }

    public void SaveSetting()
    {
        PlayerPrefs.SetInt("betTimeLimit", betTimeLimit);
        PlayerPrefs.SetInt("coinToScore", coinToScore);
		PlayerPrefs.SetInt("gameDifficulty", gameDifficulty);
        PlayerPrefs.SetInt("baoji", baoji);
		PlayerPrefs.SetInt("maxNumberOfFields", maxNumberOfFields);
		maxNumberOfChips = 0;
		for (int i = 0; i < betChipValues.Count; ++i)
		{
			int value = betChipValues[i];
			PlayerPrefs.SetInt("betChipValues" + i, value);
			if (value > 0)
				++maxNumberOfChips;
		}
		PlayerPrefs.SetInt("max36Value", max36Value);
		PlayerPrefs.SetInt("max18Value", max18Value);
		PlayerPrefs.SetInt("max12Value", max12Value);
		PlayerPrefs.SetInt("max9Value", max9Value);
		PlayerPrefs.SetInt("max6Value", max6Value);
		PlayerPrefs.SetInt("max3Value", max3Value);
		PlayerPrefs.SetInt("max2Value", max2Value);
		PlayerPrefs.SetInt("couponsStart", couponsStart);
		PlayerPrefs.SetInt("couponsKeyinRatio", couponsKeyinRatio);
		PlayerPrefs.SetInt("couponsKeoutRatio", couponsKeoutRatio);
		PlayerPrefs.SetInt("beginSessions", beginSessions);
        PlayerPrefs.Save();
    }

    public void DefaultSetting ()
    {
        betTimeLimit = 5; //30
        coinToScore = 1;
		gameDifficulty = 1;
        baoji = 20000;
		maxNumberOfFields = 38;
		maxNumberOfChips = 6;
		betChipValues.Add(1);
		betChipValues.Add(10);
		betChipValues.Add(50);
		betChipValues.Add(100);
		betChipValues.Add(500);
		betChipValues.Add(1000);
		max36Value = 100;
		max18Value = 100;
		max12Value = 100;
		max9Value = 100;
		max6Value = 100;
		max3Value = 100;
		max2Value = 100;
		couponsStart = 100;
		couponsKeyinRatio = 10;	// 1%~100%
		couponsKeoutRatio = 4;	
    }

	public void DefaultCustom()
	{
		language = 0;		// EN
		displayType = 0;	// classic
		systemPassword = "888888";
		accountPassword = "888888";
	}

	public void SaveCustom()
	{
		CryptoPrefs.SetString("systemPassword", systemPassword);
		CryptoPrefs.SetString("accountPassword", accountPassword);
		CryptoPrefs.Save();
		PlayerPrefs.SetInt("language", language);
		PlayerPrefs.SetInt("displayType", displayType);
		PlayerPrefs.Save();
	}

    public void SaveAccount()
    {
        for (int i = 0; i < maxNumOfPlayers; ++i)
        {
            CryptoPrefs.SetInt("zongShang" + i, zongShang[i]);
            CryptoPrefs.SetInt("zongXia" + i, zongXia[i]);
            CryptoPrefs.SetInt("zongTou" + i, zongTou[i]);
            CryptoPrefs.SetInt("zongTui" + i, zongTui[i]);
            CryptoPrefs.SetInt("zongYa" + i, zongYa[i]);
            CryptoPrefs.SetInt("zongYing" + i, zongYing[i]);
			CryptoPrefs.SetInt("caiPiao" + i, caiPiao[i]);
        }
        CryptoPrefs.Save();
    }

    public void DefaultAccount()
    {
        for (int i = 0; i < maxNumOfPlayers; ++i)
        {
            zongShang[i] = 0;
            zongXia[i] = 0;
            zongTou[i] = 0;
            zongTui[i] = 0;
            zongYa[i] = 0;
            zongYing[i] = 0;
			caiPiao[i] = 0;
        }
    }

    public void ReadDataFromDisk()
    {
//        PlayerPrefs.DeleteAll();
        int firstWrite = PlayerPrefs.GetInt("FirstWrite", 0);
        if (firstWrite == 0)
        {
            DefaultSetting();
            SaveSetting();
            DefaultAccount();
            SaveAccount();
			DefaultCustom();
			SaveCustom();
            PlayerPrefs.SetInt("FirstWrite", 1);
        }
        else
        {
            // Setting menu
            betTimeLimit = PlayerPrefs.GetInt("betTimeLimit");
            coinToScore = PlayerPrefs.GetInt("coinToScore");
			baoji = PlayerPrefs.GetInt("baoji");
			gameDifficulty = PlayerPrefs.GetInt("gameDifficulty");
			maxNumberOfChips = 0;
			for (int i = 0; i < maxNumberOfChips; ++i)
			{
				int value = PlayerPrefs.GetInt("betChipValues" + i, 0);
				betChipValues.Add(value);
				if (value > 0)
					++maxNumberOfChips;
			}
			max36Value = PlayerPrefs.GetInt("max36Value");
			max18Value = PlayerPrefs.GetInt("max18Value");
			max12Value = PlayerPrefs.GetInt("max12Value");
			max9Value = PlayerPrefs.GetInt("max9Value");
			max6Value = PlayerPrefs.GetInt("max6Value");
			max3Value = PlayerPrefs.GetInt("max3Value");
			max2Value = PlayerPrefs.GetInt("max2Value");
			couponsStart = PlayerPrefs.GetInt("couponsStart");
			couponsKeyinRatio = PlayerPrefs.GetInt("couponsKeyinRatio");
			couponsKeoutRatio = PlayerPrefs.GetInt("couponsKeoutRatio");
			beginSessions = PlayerPrefs.GetInt("beginSessions");
			maxNumberOfFields = PlayerPrefs.GetInt("maxNumberOfFields");

            // Check account menu 
            for (int i = 0; i < maxNumOfPlayers; ++i)
            {
                zongShang[i] = CryptoPrefs.GetInt("zongShang" + i);
                zongXia[i] = CryptoPrefs.GetInt("zongXia" + i);
                zongTou[i] = CryptoPrefs.GetInt("zongTou" + i);
                zongTui[i] = CryptoPrefs.GetInt("zongTui" + i);
                zongYa[i] = CryptoPrefs.GetInt("zongYa" + i);
                zongYing[i] = CryptoPrefs.GetInt("zongYing" + i);
				caiPiao[i] = CryptoPrefs.GetInt("caiPiao" + i);
            }

			// Custom setting
			language = PlayerPrefs.GetInt("language");
			displayType = PlayerPrefs.GetInt("displayType");
			systemPassword = CryptoPrefs.GetString("systemPassword");
			accountPassword = CryptoPrefs.GetString("accountPassword");
        }
        ReadTouchMatrix();
        ReadRecords();
        ReadBetRecords();
    }

	private void SaveRecords()
	{
		if (records.Count > 0)
		{
			int idx = 0;
			foreach (int r in records)
			{
				PlayerPrefs.SetInt("R" + idx, r);
				++idx;
			}
			PlayerPrefs.Save();
		}
	}

	public void SaveRecord(int result)
	{
        if (records.Count < 100)
        {
            records.Enqueue(result);
            int idx = records.Count - 1;
            PlayerPrefs.SetInt("R" + idx, result);
            PlayerPrefs.Save();
        }
        else
        {
            records.Enqueue(result);
            while (records.Count > 100)
                records.Dequeue();
            SaveRecords();
        }
	}

	// 100场记录
	public void ReadRecords()
	{
		for (int i = 0; i < 100; ++i)
		{
			int record = PlayerPrefs.GetInt("R" + i, -1);
			if (record > -1)
				records.Enqueue(record);
			else
				break;
		}
	}

    public void SaveTouchMatrix()
    {
        PlayerPrefs.SetFloat("TA", TA);
        PlayerPrefs.SetFloat("TB", TB);
        PlayerPrefs.SetFloat("TC", TC);
        PlayerPrefs.SetFloat("TD", TD);
        PlayerPrefs.SetFloat("TE", TE);
        PlayerPrefs.SetFloat("TE", TF);
        PlayerPrefs.Save();
    }

    public void ReadTouchMatrix()
    {
        TA = PlayerPrefs.GetFloat("TA", 0.0f);
        TB = PlayerPrefs.GetFloat("TB", 0.0f);
        TC = PlayerPrefs.GetFloat("TC", 0.0f);
        TD = PlayerPrefs.GetFloat("TD", 0.0f);
        TE = PlayerPrefs.GetFloat("TE", 0.0f);
        TF = PlayerPrefs.GetFloat("TF", 0.0f);
    }

	public void SaveLanguage()
	{
		PlayerPrefs.SetInt("language", language);
		PlayerPrefs.Save();
	}

	public void SaveDisplayType()
	{
		PlayerPrefs.SetInt("displayType", displayType);
		PlayerPrefs.Save();
	}

    public void SaveBetRecords()
    {
        int recordsNum = betRecords.Count;
        for (int idx = 0; idx < recordsNum; ++idx)
        {
            int betsNum = betRecords[idx].bets.Count;
            for (int i = 0; i < betsNum; ++i)
            {
                PlayerPrefs.SetString("br_bets" + idx + "_field" + i, betRecords[idx].bets[i].betField);
                PlayerPrefs.SetInt("br_bets" + idx + "_value" + i, betRecords[idx].bets[i].betValue);
            }
            PlayerPrefs.SetInt("br_startCredit" + idx, betRecords[idx].startCredit);
            PlayerPrefs.SetInt("br_endCredit" + idx, betRecords[idx].endCredit);
            PlayerPrefs.SetInt("br_bet" + idx, betRecords[idx].bet);
            PlayerPrefs.SetInt("br_win" + idx, betRecords[idx].win);
        }
        PlayerPrefs.Save();
    }

	// 最近10场押分记录
    private void ReadBetRecords()
    {
        for (int idx = 0; idx < 10; ++idx)
        {
            int startCredit = PlayerPrefs.GetInt("br_startCredit" + idx, -1);
            if (startCredit < 0)
                break;
            BetRecord br = new BetRecord();
            br.startCredit = startCredit;
            br.endCredit = PlayerPrefs.GetInt("br_endCredit" + idx);
            br.bet = PlayerPrefs.GetInt("br_bet" + idx);
            br.win = PlayerPrefs.GetInt("br_win" + idx);
            br.bets = new List<BetInfo>();
            for (int i = 0; i < 10; ++i)
            {
                string str = PlayerPrefs.GetString("br_bets" + idx + "_field" + i, string.Empty);
                if (string.IsNullOrEmpty(str))
                    break;
                BetInfo betInfo = new BetInfo();
                betInfo.betField = str;
                betInfo.betValue = PlayerPrefs.GetInt("br_bets" + idx + "_value" + i);
                br.bets.Add(betInfo);
            }
            betRecords.Add(br);
        }
    }

    public void SaveSysPassword()
    {
        CryptoPrefs.SetString("systemPassword", systemPassword);
        CryptoPrefs.Save();
    }

    public void SaveAccountPassword()
    {
        CryptoPrefs.SetString("accountPassword", accountPassword);
        CryptoPrefs.Save();
    }
}