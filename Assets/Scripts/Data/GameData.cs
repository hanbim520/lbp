using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
{0,13,1,00,27,10,25,29,12,8,19,31,18,6,21,33,16,4,23,35,14,2,0,28,9,26,30,11,7,20,32,17,5,22,34,15,3,24,36}
{36,13,1,00,27,10,25,29,12,8,19,31,18,6,21,33,16,4,23,35,14,2,0,28,9,26,30,11,7,20,32,17,5,22,34,15,3,24,0}
{36,11,30,8,23,10,37,5,24,16,33,1,20,14,31,9,22,18,29,7,28,12,35,3,26,0,32,15,19,4,21,2,25,17,34,6,27,13}
 */
public class GameData
{
	public static bool debug 		= false;		// 是否模拟出球
	public static bool controlCode	= true;		// 是否打码
	public static bool isDemo		= false;		// 演示版本(总出彩金)
	public static RouletteType rouletteType = RouletteType.Standard;	// 轮盘数字排列类型
	public static string version 	= "4.3.7";

    // Setting menu
    public int betTimeLimit;
    public int coinToScore;
	public int gameDifficulty;
    public int baoji;
	public List<int> betChipValues = new List<int>();
	public int max36Value;			// 单台限注36倍
	public int max18Value;			// 单台限注18倍
	public int max12Value;			// 单台限注12倍
	public int max9Value;			// 单台限注9倍
	public int max6Value;			// 单台限注6倍
	public int max3Value;			// 单台限注3倍
	public int max2Value;			// 单台限注2倍
	public int allMax36Val;			// 全台限注36倍
	public int allMax18Val;			// 全台限注18倍
	public int allMax12Val;			// 全台限注12倍
	public int allMax9Val;			// 全台限注9倍
	public int allMax6Val;			// 全台限注6倍
	public int allMax3Val;			// 全台限注3倍
	public int allMax2Val;			// 全台限注2倍
	public int blowTiming;			// 提前X秒吹球 [0, 5]
	public int powerOffCompensate;	// 0--断电吃分 1--断电赔付
	// 优惠卡分限
	public int couponsStart;
	public int couponsKeyinRatio;	// 1%~100%
	public int couponsKeoutRatio;	
	public int maxNumberOfFields; 	// 37 or 38
	public int beginSessions;		// 起始场次
	public int maxNumberOfChips;	// 1 ~ 6
	public int lotteryLv;			// 彩金档位(1-100)
	public int lotteryCondition;	// 彩金押分起始分值(1-100000)
	public int lotteryBase;			// 起始彩金(0-100000)
	public int lotteryRate;			// 彩金累计千分比(1-100)
	public int lotteryAllocation; 	// 彩金分配(可设置范围0-100)
	public int topScreenLanguage;	// 顶部路单屏的语言  0:EN 1:CN
	public int billAcceptorType;	// 0:JCM 1:ICT002 2:ICT104 3:ICT106
	// 纸币值
	public int FirstBill;
	public int SecondBill;
	public int ThirdBill;
	public int FourthBill;
	public int FifthBill;
	public int SixthBill;
	public int SeventhBill;
	public int EighthBill;
	public int NinthBill;
	public int TeenthBill;
	public int EleventhBill;
	public int TwelfthBill;

    // Account
    public int zongShang;	// 总上分
    public int zongXia;		// 总下分
    public int zongTou;		// 总投币
    public int zongTui;		// 总退币
    public int zongYa;      // 总押
    public int zongPei;     // 总赔
	public int totalWin;			// 总赢分
	public int cardCredits;			// 优惠卡送的分
	public int totalZongYa;			// 机台的总押
	public int totalZongPei;		// 机台的总赔
	public Queue<KeyinKeoutRecord> keyinKeoutRecords = new Queue<KeyinKeoutRecord>();		// 上下分 投退币流水账
    private int _printTimes;   // 打码次数
    public int printTimes
	{
		get { return _printTimes; }
		set
		{
			_printTimes = value;
			SavePrintTimes();
		}
	}
    public int ZongYa
    {
        get { return zongYa; }
        set
        {
			zongYa = value;
            CryptoPrefs.SetInt("zongYa", zongYa);
            CryptoPrefs.Save();
        }
    }
    public int ZongPei
    {
        get { return zongPei; }
        set
        {
			zongPei = value;
            CryptoPrefs.SetInt("zongPei", zongPei);
            CryptoPrefs.Save();
        }
    }
	public int TotalZongYa
	{
		get { return totalZongYa; }
		set
		{
			totalZongYa = value;
			CryptoPrefs.SetInt("totalZongYa", totalZongYa);
			CryptoPrefs.Save();
		}

	}
	public int TotalZongPei
	{
		get { return totalZongPei; }
		set
		{
			totalZongPei = value;
			CryptoPrefs.SetInt("totalZongPei", totalZongPei);
			CryptoPrefs.Save();
		}
	}

	private int _lotteryCredits;
	public int lotteryCredits		// 彩金送的分 显示在总账上面
	{
		get { return _lotteryCredits; }
		set
		{
			_lotteryCredits = value;
			CryptoPrefs.SetInt("lotteryCredits", _lotteryCredits);
			CryptoPrefs.Save();
		}
	}
	private int _jackpotDaybook;	// 彩金送的分 显示在流水账上面
	public int jackpotDaybook
	{
		get { return _jackpotDaybook; }
		set
		{
			_jackpotDaybook = value;
			CryptoPrefs.SetInt("jackpotDaybook", _jackpotDaybook);
			CryptoPrefs.Save();
		}
	}
	private int _vipCredits;		// vip卡送出的分 显示在总账上面
	public int vipCredits
	{
		get { return _vipCredits; }
		set
		{
			_vipCredits = value;
			CryptoPrefs.SetInt("vipCredits", _vipCredits);
			CryptoPrefs.Save();
		}
	}
	private int _vipDaybook;		// vip卡送出的分 显示在流水账上面
	public int vipDaybook
	{
		get { return _vipDaybook; }
		set
		{
			_vipDaybook = value;
			CryptoPrefs.SetInt("vipDaybook", _vipDaybook);
			CryptoPrefs.Save();
		}
	}
	

    private int _lineId = 111;        // 线号
    public int lineId
    {
        get 
		{
			if (_lineId == 0)
			{
				_lineId = PlayerPrefs.GetInt("lineId");
			}
			return _lineId; 
		}
        set
        {
            _lineId = value;
            PlayerPrefs.SetInt("lineId", _lineId);
            PlayerPrefs.Save();
        }
    }

	private int _machineId = 88111127;		// 机台号 8位
    public int machineId
    {
        get 
		{
			if (_machineId == 0)
			{
				_machineId = PlayerPrefs.GetInt("machineId");
			}
			return _machineId;
		}
        set 
        { 
            _machineId = value;
            PlayerPrefs.SetInt("machineId", _machineId);
            PlayerPrefs.Save();
        }
    }

	#region 带控制的彩金参数
	private int _lotteryMatchCount;		// 彩金场次记数
	public int lotteryMatchCount
	{
		get { return _lotteryMatchCount; }
		set
		{
			_lotteryMatchCount = value;
			PlayerPrefs.SetInt("lotteryMatchCount", _lotteryMatchCount);
			PlayerPrefs.Save();
		}
	}
	public int lotteryMaxMatch;			// 彩金场次的最大序号
	public List<int> lotteryWinIdx = new List<int>();	// 会中的彩金场次
	#endregion
	
	#region 不带控制的彩金参数
	// 彩金场次计数
	private int _jackpotMatchCount;	
	public int jackpotMatchCount
	{
		get { return _jackpotMatchCount; }
		set
		{
			_jackpotMatchCount = value;
			CryptoPrefs.SetInt("jackpotMatchCount", _jackpotMatchCount);
			CryptoPrefs.Save();
		}
	}
	// 中彩金的场次
	private int _jackpotBingoIdx;
	public int jackpotBingoIdx
	{
		get { return _jackpotBingoIdx; }
		set
		{
			_jackpotBingoIdx = value;
			CryptoPrefs.SetInt("jackpotBingoIdx", _jackpotBingoIdx);
			CryptoPrefs.Save();
		}
	}
	#endregion

    public int deviceIndex;     // 机台序号 1, 2, 3...
	public string deviceGuid;	// 发给加密芯片做验证
	public string EncryChipUUID // android板加密芯片的uuid
	{
		get 
		{
			return CryptoPrefs.GetString("EncryChipUUID", string.Empty);
		}
		set
		{
			CryptoPrefs.SetString("EncryChipUUID", value);
			CryptoPrefs.Save();
		}
	}

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
	public int backendLanguage;		// 后台语言 0:EN 1:CN
	public int displayType; 		// 0:classic 1:ellipse
	public string systemPassword;	// 设置密码
	public string accountPassword;	// 查询密码
	public string adminPassword;	// 管理员密码
    public int passwordLength = 6;
	public int inputDevice;			// 0:touch screen 1:serial mouse
	public int lotteryDigit;		// 累计彩金
	public int lotteryBetPool;		// 累积到彩金池的压分

    // 记录最近10场押分情况
    public List<BetRecord> betRecords = new List<BetRecord>();
	public Queue<int> records = new Queue<int>();	// 100场记录		00:用37表示
    public Dictionary<int, ResultType> colorTable = new Dictionary<int, ResultType>();
	public Dictionary<string, int> lastBets = new Dictionary<string, int>();	// 最近一场压分记录
	public int lastBetCredit = 0;					// 最近一场压分总数
    public List<int> hotValues = new List<int>();   // 出现最多5个号
    public List<int> coldValues = new List<int>();  // 出现最少5个号 不包括出现0次的号

	public int[] ballValue38 = new int[]{36,13,1,37,27,10,25,29,12,8,19,31,18,6,21,33,16,4,23,35,14,2,0,28,9,26,30,11,7,20,32,17,5,22,34,15,3,24};
    public int[] ballValue37 = new int[]{0,32,15,19,4,21,2,25,17,34,6,27,13,36,11,30,8,23,10,5,24,16,33,1,20,14,31,9,22,18,29,7,28,12,35,3,26};
	private int[,] ballValues = new int[,]{
		{36,13,1,37,27,10,25,29,12,8,19,31,18,6,21,33,16,4,23,35,14,2,0,28,9,26,30,11,7,20,32,17,5,22,34,15,3,24},
		{36,11,30,8,23,10,37,5,24,16,33,1,20,14,31,9,22,18,29,7,28,12,35,3,26,0,32,15,19,4,21,2,25,17,34,6,27,13}
	};

	// For host
	private int connectClientsTime = 10;
	public int ConnectClientsTime
	{
		get { return connectClientsTime; }
	}

	private const int maxNumOfPlayers = 9;
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

	private int isCardMode = 0; // 0:false 1:ready 2:true
	public int IsCardMode
	{
		get { return isCardMode; }
		set 
		{
			isCardMode = value;
			PlayerPrefs.SetInt("isCardMode", isCardMode);
			PlayerPrefs.Save();
		}
	}

	// 彩金功能开关
	public bool lotteryEnable
	{
		get { return lotteryAllocation > 0 && lotteryLv > 0; }
	}

	public int monitorDeviceIndex = 101;	// 路单屏的机器id (大于等于这个号都是路单屏)

	private GameData()
	{
		// GUID
		deviceGuid = PlayerPrefs.GetString("deviceGuid", string.Empty);
		if (string.IsNullOrEmpty(deviceGuid))
		{
			deviceGuid = Utils.GuidTo16String();
			PlayerPrefs.SetString("deviceGuid", deviceGuid);
			PlayerPrefs.Save();
		}
        deviceIndex = PlayerPrefs.GetInt("deviceIndex", 0);
//		deviceIndex = 2;

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
       
		int ballValCount = ballValue38.Length;
		int rtype = (int)rouletteType;
		for (int i = 0; i < ballValCount; ++i)
			ballValue38[i] = ballValues[rtype, i];
		
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
		PlayerPrefs.SetInt("allMax36Val", allMax36Val);
		PlayerPrefs.SetInt("allMax18Val", allMax18Val);
		PlayerPrefs.SetInt("allMax12Val", allMax12Val);
		PlayerPrefs.SetInt("allMax9Val", allMax9Val);
		PlayerPrefs.SetInt("allMax6Val", allMax6Val);
		PlayerPrefs.SetInt("allMax3Val", allMax3Val);
		PlayerPrefs.SetInt("allMax2Val", allMax2Val);
		PlayerPrefs.SetInt("blowTiming", blowTiming);
		PlayerPrefs.SetInt("couponsStart", couponsStart);
		PlayerPrefs.SetInt("couponsKeyinRatio", couponsKeyinRatio);
		PlayerPrefs.SetInt("couponsKeoutRatio", couponsKeoutRatio);
		PlayerPrefs.SetInt("beginSessions", beginSessions);
		PlayerPrefs.SetInt("lotteryLv", lotteryLv);
		PlayerPrefs.SetInt("lotteryCondition", lotteryCondition);
		PlayerPrefs.SetInt("lotteryBase", lotteryBase);
		PlayerPrefs.SetInt("lotteryRate", lotteryRate);
		PlayerPrefs.SetInt("lotteryAllocation", lotteryAllocation);
		PlayerPrefs.SetInt("powerOffCompensate", powerOffCompensate);
		PlayerPrefs.SetInt("topScreenLanguage", topScreenLanguage);
		PlayerPrefs.SetInt("billAcceptorType", billAcceptorType);
		PlayerPrefs.SetInt("FirstBill", FirstBill);
		PlayerPrefs.SetInt("SecondBill", SecondBill);
		PlayerPrefs.SetInt("ThirdBill", ThirdBill);
		PlayerPrefs.SetInt("FourthBill", FourthBill);
		PlayerPrefs.SetInt("FifthBill", FifthBill);
		PlayerPrefs.SetInt("SixthBill", SixthBill);
		PlayerPrefs.SetInt("SeventhBill", SeventhBill);
		PlayerPrefs.SetInt("EighthBill", EighthBill);
		PlayerPrefs.SetInt("NinthBill", NinthBill);
		PlayerPrefs.SetInt("TeenthBill", TeenthBill);
		PlayerPrefs.SetInt("EleventhBill", EleventhBill);
		PlayerPrefs.SetInt("TwelfthBill", TwelfthBill);
        PlayerPrefs.Save();
    }

    public void DefaultSetting ()
    {
        betTimeLimit = 30; //30
        coinToScore = 1;
		gameDifficulty = 1;
        baoji = 30000;
		maxNumberOfFields = 38;
		maxNumberOfChips = 6;
		betChipValues.Clear();
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
		allMax36Val = 800;
		allMax18Val = 800;
		allMax12Val = 800;
		allMax9Val = 800;
		allMax6Val = 800;
		allMax3Val = 800;
		allMax2Val = 800;
		blowTiming = 0;
		couponsStart = 100;
		couponsKeyinRatio = 10;	// 1%~100%
		couponsKeoutRatio = 4;	
		beginSessions = 100;
		lotteryLv = 50;
		lotteryCondition = 100;
		lotteryBase = 1000;
		lotteryRate = 10;
		lotteryAllocation = 40;
		powerOffCompensate = 1;
		topScreenLanguage = 0;
		billAcceptorType = 0;
		FirstBill = 1;
		SecondBill = 5;
		ThirdBill = 10;
		FourthBill = 20;
		FifthBill = 50;
		SixthBill = 100;
		SeventhBill = 200;
		EighthBill = 250;
		NinthBill = 300;
		TeenthBill = 500;
		EleventhBill = 1000;
		TwelfthBill = 2000;
    }

	public void DefaultCustom()
	{
		language = 0;		// EN
		backendLanguage = 0;// EN
		displayType = 0;	// classic
		systemPassword = "888888";
		accountPassword = "888888";
		adminPassword = "888888";
		isCardMode = CardMode.NO;
		inputDevice = 1;
		lotteryDigit = 1000;
		lotteryBetPool = 0;
	}

	public void SaveCustom()
	{
		CryptoPrefs.SetString("systemPassword", systemPassword);
		CryptoPrefs.SetString("accountPassword", accountPassword);
		CryptoPrefs.SetString("adminPassword", adminPassword);
		PlayerPrefs.SetInt("language", language);
		PlayerPrefs.SetInt("backendLanguage", backendLanguage);
		PlayerPrefs.SetInt("displayType", displayType);
		PlayerPrefs.SetInt("isCardMode", isCardMode);
		PlayerPrefs.SetInt("inputDevice", inputDevice);
		CryptoPrefs.SetInt("lotteryDigit", lotteryDigit);
		CryptoPrefs.SetInt("lotteryBetPool", lotteryBetPool);
		PlayerPrefs.Save();
	}

    public void SaveAccount()
    {
        CryptoPrefs.SetInt("zongShang", zongShang);
        CryptoPrefs.SetInt("zongXia", zongXia);
        CryptoPrefs.SetInt("zongTou", zongTou);
        CryptoPrefs.SetInt("zongTui", zongTui);
        CryptoPrefs.SetInt("zongYa", zongYa);
        CryptoPrefs.SetInt("zongPei", zongPei);
		CryptoPrefs.SetInt("totalWin", totalWin);
		CryptoPrefs.SetInt("totalZongYa", totalZongYa);
		CryptoPrefs.SetInt("totalZongPei", totalZongPei);
		CryptoPrefs.SetInt("cardCredits", cardCredits);
		CryptoPrefs.SetInt("lotteryCredits", _lotteryCredits);
		CryptoPrefs.SetInt("jackpotDaybook", _jackpotDaybook);
		CryptoPrefs.SetInt("vipCredits", _vipCredits);
		CryptoPrefs.SetInt("vipDaybook", _vipDaybook);
        CryptoPrefs.Save();
    }

    public void DefaultAccount()
    {
        zongShang = 0;
        zongXia = 0;
        zongTou = 0;
        zongTui = 0;
        zongYa = 0;
        zongPei = 0;
		totalZongPei = 0;
		totalZongYa = 0;
		totalWin = 0;
		cardCredits = 0;
		printTimes = 0;
		_lotteryCredits = 0;
		_jackpotDaybook = 0;
		_vipCredits = 0;
		_vipDaybook = 0;
    }

    public void ReadDataFromDisk()
    {
//    	PlayerPrefs.DeleteAll();
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
			RandomRecords();
        }
        else
        {
            // Setting menu
			betTimeLimit = PlayerPrefs.GetInt("betTimeLimit");
            coinToScore = PlayerPrefs.GetInt("coinToScore");
			baoji = PlayerPrefs.GetInt("baoji");
			gameDifficulty = PlayerPrefs.GetInt("gameDifficulty");

			maxNumberOfChips = 0;
			if (betChipValues.Count > 0)
			{
				foreach (int item in betChipValues)
				{
					if (item > 0)
						++maxNumberOfChips;
				}
			}
			else
			{
				for (int i = 0; i < 6; ++i)
				{
					int value = PlayerPrefs.GetInt("betChipValues" + i, 0);
					betChipValues.Add(value);
					if (value > 0)
						++maxNumberOfChips;
				}
			}
            max36Value = PlayerPrefs.GetInt("max36Value");
			max18Value = PlayerPrefs.GetInt("max18Value");
			max12Value = PlayerPrefs.GetInt("max12Value");
			max9Value = PlayerPrefs.GetInt("max9Value");
			max6Value = PlayerPrefs.GetInt("max6Value");
			max3Value = PlayerPrefs.GetInt("max3Value");
			max2Value = PlayerPrefs.GetInt("max2Value");
            allMax36Val = PlayerPrefs.GetInt("allMax36Val");
            allMax18Val = PlayerPrefs.GetInt("allMax18Val");
            allMax12Val = PlayerPrefs.GetInt("allMax12Val");
			allMax9Val = PlayerPrefs.GetInt("allMax9Val");
			allMax6Val = PlayerPrefs.GetInt("allMax6Val");
			allMax3Val = PlayerPrefs.GetInt("allMax3Val");
			allMax2Val = PlayerPrefs.GetInt("allMax2Val");
			blowTiming = PlayerPrefs.GetInt("blowTiming");
			couponsStart = PlayerPrefs.GetInt("couponsStart");
			couponsKeyinRatio = PlayerPrefs.GetInt("couponsKeyinRatio");
			couponsKeoutRatio = PlayerPrefs.GetInt("couponsKeoutRatio");
			beginSessions = PlayerPrefs.GetInt("beginSessions");
			maxNumberOfFields = PlayerPrefs.GetInt("maxNumberOfFields");
            lotteryLv = PlayerPrefs.GetInt("lotteryLv");
			lotteryCondition = PlayerPrefs.GetInt("lotteryCondition");
			lotteryBase = PlayerPrefs.GetInt("lotteryBase");
			lotteryRate = PlayerPrefs.GetInt("lotteryRate");
			lotteryAllocation = PlayerPrefs.GetInt("lotteryAllocation");
			powerOffCompensate = PlayerPrefs.GetInt("powerOffCompensate", 1);
			topScreenLanguage = PlayerPrefs.GetInt("topScreenLanguage", 0);
			billAcceptorType = PlayerPrefs.GetInt("billAcceptorType", 0);
			FirstBill = PlayerPrefs.GetInt("FirstBill", 1);
			SecondBill = PlayerPrefs.GetInt("SecondBill", 5);
			ThirdBill = PlayerPrefs.GetInt("ThirdBill", 10);
			FourthBill = PlayerPrefs.GetInt("FourthBill", 20);
			FifthBill = PlayerPrefs.GetInt("FifthBill", 50);
			SixthBill = PlayerPrefs.GetInt("SixthBill", 100);
			SeventhBill = PlayerPrefs.GetInt("SeventhBill", 200);
			EighthBill = PlayerPrefs.GetInt("EighthBill", 250);
			NinthBill = PlayerPrefs.GetInt("NinthBill", 300);
			TeenthBill = PlayerPrefs.GetInt("TeenthBill", 500);
			EleventhBill = PlayerPrefs.GetInt("EleventhBill", 1000);
			TwelfthBill = PlayerPrefs.GetInt("TwelfthBill", 2000);

            // Check account menu 
            zongShang = CryptoPrefs.GetInt("zongShang");
            zongXia = CryptoPrefs.GetInt("zongXia");
            zongTou = CryptoPrefs.GetInt("zongTou");
            zongTui = CryptoPrefs.GetInt("zongTui");
            zongYa = CryptoPrefs.GetInt("zongYa");
            zongPei = CryptoPrefs.GetInt("zongPei");
			totalZongYa = CryptoPrefs.GetInt("totalZongYa");
			totalZongPei = CryptoPrefs.GetInt("totalZongPei");
			totalWin = CryptoPrefs.GetInt("totalWin");
			cardCredits = CryptoPrefs.GetInt("cardCredits");
			_printTimes = CryptoPrefs.GetInt("printTimes");
			_lotteryCredits = CryptoPrefs.GetInt("lotteryCredits", 0);
			_jackpotDaybook = CryptoPrefs.GetInt("jackpotDaybook", 0);
			_vipCredits = CryptoPrefs.GetInt("vipCredits", 0);
			_vipDaybook = CryptoPrefs.GetInt("vipDaybook", 0);

			// Custom setting
			language = PlayerPrefs.GetInt("language");
			backendLanguage = PlayerPrefs.GetInt("backendLanguage", 0);
			displayType = PlayerPrefs.GetInt("displayType");
			systemPassword = CryptoPrefs.GetString("systemPassword");
			accountPassword = CryptoPrefs.GetString("accountPassword");
			adminPassword = CryptoPrefs.GetString("adminPassword");
			isCardMode = PlayerPrefs.GetInt("isCardMode");
			inputDevice = PlayerPrefs.GetInt("inputDevice");
			lotteryDigit = CryptoPrefs.GetInt("lotteryDigit");
			lotteryBetPool = CryptoPrefs.GetInt("lotteryBetPool");
        }
        ReadTouchMatrix();
        ReadRecords();
        ReadBetRecords();
		ReadLastBets();
		ReadKeyinKeoutRecords();
		ReadLotteryParams();
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
		if (records.Count > 0)
			records.Clear();

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
        PlayerPrefs.SetFloat("TF", TF);
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

	public void SaveBackendLanguage()
	{
		PlayerPrefs.SetInt("backendLanguage", backendLanguage);
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
			int oldNum = PlayerPrefs.GetInt("br_bets" + idx + "_num", 0);
			if (oldNum > 0)
			{
				for (int i = 0; i < oldNum; ++i)
				{
					PlayerPrefs.DeleteKey("br_bets" + idx + "_field" + i);
					PlayerPrefs.DeleteKey("br_bets" + idx + "_value" + i);
				}
				PlayerPrefs.DeleteKey("br_bets" + idx + "_num");
			}

			PlayerPrefs.SetInt("br_bets" + idx + "_num", betsNum);
            for (int i = 0; i < betsNum; ++i)
            {
                PlayerPrefs.SetString("br_bets" + idx + "_field" + i, betRecords[idx].bets[i].betField);
                PlayerPrefs.SetInt("br_bets" + idx + "_value" + i, betRecords[idx].bets[i].betValue);
            }
            PlayerPrefs.SetInt("br_startCredit" + idx, betRecords[idx].startCredit);
            PlayerPrefs.SetInt("br_endCredit" + idx, betRecords[idx].endCredit);
            PlayerPrefs.SetInt("br_bet" + idx, betRecords[idx].bet);
            PlayerPrefs.SetInt("br_win" + idx, betRecords[idx].win);
            PlayerPrefs.SetInt("br_luckyWin" + idx, betRecords[idx].luckyWin);
			PlayerPrefs.SetInt("br_ballValue" + idx, betRecords[idx].ballValue);
        }
        PlayerPrefs.Save();
    }

	// 最近10场押分记录
    private void ReadBetRecords()
    {
		if (betRecords.Count > 0)
			betRecords.Clear();

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
			br.luckyWin = PlayerPrefs.GetInt("br_luckyWin" + idx);
			br.ballValue = PlayerPrefs.GetInt("br_ballValue" + idx);
            br.bets = new List<BetInfo>();
			int numOfbets = PlayerPrefs.GetInt("br_bets" + idx + "_num");
			for (int i = 0; i < numOfbets; ++i)
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

	public void SaveAdminPassword()
	{
		CryptoPrefs.SetString("adminPassword", adminPassword);
		CryptoPrefs.Save();
	}

	public void SaveDeviceIndex()
	{
		PlayerPrefs.SetInt("deviceIndex", deviceIndex);
		PlayerPrefs.Save();
	}

	public void SaveLotteryDigit()
	{
		CryptoPrefs.SetInt("lotteryDigit", lotteryDigit);
		CryptoPrefs.Save();
	}

	public void SaveLotteryBetPool()
	{
		CryptoPrefs.SetInt("lotteryBetPool", lotteryBetPool);
		CryptoPrefs.Save();
	}

	public void AppendKeyinKeoutRecords(int keyin, int keout, int receiveCoin, int payCoin, int card)
	{
		if (keyin == 0 && keout == 0 && 
			receiveCoin == 0 && payCoin == 0 && 
			card == 0)
			return;
		
		KeyinKeoutRecord record = new KeyinKeoutRecord();
		record.time = Utils.GetSystemTime();
		record.keyin = keyin;
		record.keout = keout;
		record.toubi = receiveCoin;
		record.tuibi = payCoin;
		record.card = card;

		keyinKeoutRecords.Enqueue(record);
		while (keyinKeoutRecords.Count > 20)
			keyinKeoutRecords.Dequeue();

		int count = 0;
		foreach (KeyinKeoutRecord item in keyinKeoutRecords)
		{
			PlayerPrefs.SetString("daybook_time" + count, item.time);
			PlayerPrefs.SetInt("daybook_keyin" + count, item.keyin);
			PlayerPrefs.SetInt("daybook_keout" + count, item.keout);
			PlayerPrefs.SetInt("daybook_toubi" + count, item.toubi);
			PlayerPrefs.SetInt("daybook_tuibi" + count, item.tuibi);
			PlayerPrefs.SetInt("daybook_card" + count, item.card);
			++count;
		}

		PlayerPrefs.Save();
	}

	// 读取流水账
	public void ReadKeyinKeoutRecords()
	{
		if (keyinKeoutRecords.Count > 0)
			keyinKeoutRecords.Clear();

		for (int i = 0; i < 20; ++i)
		{
			string time = PlayerPrefs.GetString("daybook_time" + i);
			if (string.IsNullOrEmpty(time))
				break;
			KeyinKeoutRecord record = new KeyinKeoutRecord();
			record.time = time;
			record.keyin = PlayerPrefs.GetInt("daybook_keyin" + i);
			record.keout = PlayerPrefs.GetInt("daybook_keout" + i);
			record.toubi = PlayerPrefs.GetInt("daybook_toubi" + i);
			record.tuibi = PlayerPrefs.GetInt("daybook_tuibi" + i);
			record.card = PlayerPrefs.GetInt("daybook_card" + i);
			keyinKeoutRecords.Enqueue(record);
		}
	}

	// 读取彩金相关参数
	public void ReadLotteryParams()
	{
		_lotteryMatchCount = PlayerPrefs.GetInt("lotteryMatchCount", 0);
		lotteryMaxMatch = CryptoPrefs.GetInt("lotteryMaxMatch", 0);
		lotteryWinIdx.Clear();
		for (int i = 0; i < 20; ++i)
		{
			string key = "lotteryMatchIdx" + i.ToString();
			if (CryptoPrefs.HasKey(key))
				lotteryWinIdx.Add(CryptoPrefs.GetInt(key));
		}
		if (lotteryEnable && 
		    lotteryWinIdx.Count == 0)
		{
			CalcLotteryIdx();
			lotteryMatchCount = 0;
		}
		_jackpotMatchCount = CryptoPrefs.GetInt("jackpotMatchCount", 0);
		_jackpotBingoIdx = CryptoPrefs.GetInt("jackpotBingoIdx", 0);
	}

	public void SavePrintTimes()
	{
		CryptoPrefs.SetInt("printTimes", _printTimes);
		CryptoPrefs.Save();
	}

	public void SaveInputDevice()
	{
		PlayerPrefs.SetInt("inputDevice", inputDevice);
		PlayerPrefs.Save();
	}

    public void ClearAccount()
    {
        zongShang = 0;
        zongXia = 0;
        zongYa = 0;
        zongPei = 0;
        zongTou = 0;
        zongTui = 0;
        totalWin = 0;
        cardCredits = 0;
		totalZongYa = 0;
		totalZongPei = 0;
		_lotteryCredits = 0;
		_jackpotDaybook = 0;
		_vipDaybook = 0;
		_vipCredits = 0;
        SaveAccount();
    }

	// 开始场次加一
	public void AddBeginSessions()
	{
		++GameData.GetInstance().beginSessions;
		if (GameData.GetInstance().beginSessions > 100000)
			GameData.GetInstance().beginSessions = 1;
		PlayerPrefs.SetInt("beginSessions", beginSessions);
		PlayerPrefs.Save();
	}

	// 计算会中彩金的场次序号
	public void CalcLotteryIdx()
	{
		Utils.Seed(System.DateTime.Now.Millisecond);
		int count = Utils.GetRandom(9, 12);					// 共有多少场出彩金
		int sumMatch = 2400;								// 2天内的场次数 (一场90秒 每天15个小时)
		lotteryWinIdx.Clear();
		for (int i = 0; i < 20; ++i)
		{
			string key = "lotteryMatchIdx" + i.ToString();
			if (CryptoPrefs.HasKey(key))
				CryptoPrefs.DeleteKey(key);
		}
		while (lotteryWinIdx.Count < count)
		{
			int index = Utils.GetRandom(1, sumMatch);
			if (!lotteryWinIdx.Contains(index))
				lotteryWinIdx.Add(index);
		}
		// Save to disk
		for (int i = 0; i < lotteryWinIdx.Count; ++i)
		{
			string key = "lotteryMatchIdx" + i.ToString();
			CryptoPrefs.SetInt(key, lotteryWinIdx[i]);
		}
		lotteryMaxMatch = sumMatch;
		CryptoPrefs.SetInt("lotteryMaxMatch", lotteryMaxMatch);
		CryptoPrefs.Save();
	}

	// 统计各个号码出现的次数
	public void StatisticBall(int ballValue)
	{
		int count = PlayerPrefs.GetInt("ballValue" + ballValue.ToString(), 0);
		++count;
		PlayerPrefs.SetInt("ballValue" + ballValue.ToString(), count);
		PlayerPrefs.Save();
	}

    // 删除各个号码出现的次数
    public void RemoveStatisticBalls()
    {
        int count = maxNumberOfFields;
        for (int i = 0; i < maxNumberOfFields; ++i)
            PlayerPrefs.SetInt("ballValue" + i, 0);
        PlayerPrefs.Save();
    }

	public void RemoveLastBet()
	{
		int count = PlayerPrefs.GetInt("lastBet_count");
		PlayerPrefs.DeleteKey("lastBet_count");
		for (int i = 0; i < count; ++i)
		{
			PlayerPrefs.DeleteKey("lastBet_field" + i);
			PlayerPrefs.DeleteKey("lastBet_value" + i);
		}
		lastBets.Clear();
		PlayerPrefs.Save();
	}

	public void SaveLastBet(ref Dictionary<string, int> betFields, int totalBetCredit)
	{
		int count = betFields.Count;
		if (count > 0)
		{
			PlayerPrefs.SetInt("lastBet_count", count);
			PlayerPrefs.SetInt("lastBet_credit", totalBetCredit);
			lastBetCredit = totalBetCredit;
			int idx = 0;
			foreach (KeyValuePair<string, int> item in betFields)
			{
				PlayerPrefs.SetString("lastBet_field" + idx, item.Key);
				PlayerPrefs.SetInt("lastBet_value" + idx, item.Value);
				lastBets.Add(item.Key, item.Value);
				++idx;
			}
			PlayerPrefs.Save();
		}
	}

	// 读取最近一次压分
	public void ReadLastBets()
	{
		int count = PlayerPrefs.GetInt("lastBet_count", 0);
		if (count <= 0)
			return;

		lastBetCredit = PlayerPrefs.GetInt("lastBet_credit");
		if (lastBets.Count > 0)
			lastBets.Clear();
		for (int i = 0; i < count; ++i)
		{
			string field = PlayerPrefs.GetString("lastBet_field" + i);
			int value = PlayerPrefs.GetInt("lastBet_value" + i);
			lastBets.Add(field, value);
		}
	}

    public void SaveMonitorLanguage()
    {
        PlayerPrefs.SetInt("topScreenLanguage", topScreenLanguage);
        PlayerPrefs.Save();
    }

	private void RandomRecords()
	{
		SaveRecord(12);
		SaveRecord(12);
		SaveRecord(12);
		SaveRecord(12);
		SaveRecord(12);
		SaveRecord(14);
		SaveRecord(23);
		SaveRecord(14);
		SaveRecord(23);
		SaveRecord(23);
		
		SaveRecord(1);
		SaveRecord(2);
		SaveRecord(4);
		SaveRecord(6);
		SaveRecord(4);
		SaveRecord(13);
		SaveRecord(11);
		SaveRecord(10);
		SaveRecord(9);
		SaveRecord(2);
		
		SaveRecord(0);
		SaveRecord(12);
		SaveRecord(4);
		SaveRecord(15);
		SaveRecord(16);
		SaveRecord(13);
		SaveRecord(11);
		SaveRecord(10);
		SaveRecord(0);
		SaveRecord(2);
		
		SaveRecord(0);
		SaveRecord(2);
		SaveRecord(4);
		SaveRecord(6);
		SaveRecord(0);
		SaveRecord(13);
		SaveRecord(11);
		SaveRecord(10);
		SaveRecord(9);
		SaveRecord(2);
		
		SaveRecord(1);
		SaveRecord(2);
		SaveRecord(4);
		SaveRecord(6);
		SaveRecord(4);
		SaveRecord(13);
		SaveRecord(11);
		SaveRecord(10);
		SaveRecord(9);
		SaveRecord(2);
		
		SaveRecord(9);
		SaveRecord(8);
		SaveRecord(7);
		SaveRecord(6);
		SaveRecord(5);
		SaveRecord(14);
		SaveRecord(19);
		SaveRecord(17);
		SaveRecord(9);
		SaveRecord(2);
		
		SaveRecord(21);
		SaveRecord(22);
		SaveRecord(24);
		SaveRecord(26);
		SaveRecord(24);
		SaveRecord(23);
		SaveRecord(11);
		SaveRecord(10);
		SaveRecord(3);
		SaveRecord(1);
		
		SaveRecord(31);
		SaveRecord(32);
		SaveRecord(34);
		SaveRecord(36);
		SaveRecord(4);
		SaveRecord(13);
		SaveRecord(35);
		SaveRecord(10);
		SaveRecord(9);
		SaveRecord(2);
		
		SaveRecord(1);
		SaveRecord(3);
		SaveRecord(7);
		SaveRecord(6);
		SaveRecord(5);
		SaveRecord(13);
		SaveRecord(11);
		SaveRecord(10);
		SaveRecord(9);
		SaveRecord(2);
		
		SaveRecord(1);
		SaveRecord(32);
		SaveRecord(4);
		SaveRecord(6);
		SaveRecord(4);
		SaveRecord(13);
		SaveRecord(11);
		SaveRecord(10);
		SaveRecord(9);
		SaveRecord(2);
	}
}