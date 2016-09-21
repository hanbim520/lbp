using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ConnectionState
{
	Disconnected, Connecting, Connected
}

public struct GamePhase
{
	public const int GameStart          = 1;
	public const int Countdown          = 2;
    public const int Run                = 3;
    public const int ShowResult         = 4;
    public const int Compensate         = 5;
	public const int GameEnd            = 6;
}

public struct NetInstr
{
	public const int SyncData           = 1; // sync system setting
	public const int GamePhase          = 2;
    public const int Bet                = 3;
    public const int GetGamePhase       = 4;
	public const int CheckAccount       = 5;
	public const int ClearAccount       = 6;
	public const int ClearCurrentWin    = 7;
	public const int GetBetRecords      = 8;
    public const int LotteryNum         = 9;  // 同步彩金号码
    public const int SyncRecords        = 10; // sync last 100 records
	public const int LuckSum			= 11; // 同步当前局压中彩金的总筹码数
	public const int SyncLottery		= 13; // 同步彩金池
	public const int GetTotalBet		= 14; // 获取分机的总压分
	public const int SyncInputDevice	= 15; // 同步输入设备
    public const int SyncTSLanguage     = 16; // 同步路单的语言
	public const int IndicateIdentity	= 17; // 1:分机 2:路单屏幕
}

// Use for network
public struct Fields
{
    // 0...36
    public const int Even = 38;
    public const int Odd = 39;
    public const int Red = 40;
    public const int Black = 41;
    public const int Big = 42;
    public const int Small = 43;
}

public struct Notifies
{
	public static string[] LimitBet = new string[]{"限红不能押注"};
	public static string[] usbDisconnected = new string[]{"Disconnect from the usb.\nCheck usb port please.", "Usb断开连接。\n请检查Usb接口。"};
    public static string[] saveSuccess = new string[]{"Saved successfully.", "保存成功。"};
}

public enum ResultType
{
    Green, Red, Black, Even, Odd, Big, Small, Serial1_18, Serial19_36
}

public struct InputInfo
{
	public float x;
	public float y;
	public float time;
	public byte state;	// 0:up 1:down
}

public struct BetRecord
{
    public List<BetInfo> bets;
    public int startCredit;
    public int endCredit;
    public int bet;
    public int win;
	public int luckyWin;		// 赢取的彩金数
	public int ballValue;		// 球的号码
}

public struct BetInfo
{
    public string betField;
    public int betValue;
}

public struct Scenes
{
    public static string Main           = "Main";
    public static string Loading        = "Loading";
    public static string StartInfo      = "StartInfo";
    public static string Backend        = "Backend";
	public static string Last10         = "LastTen";
	public static string Account        = "Account";
	public static string TouchCheck     = "TouchCheck";
	public static string TopStatistics  = "TopStatistics";
}

public struct CardMode
{
	public static int YES = 2;
	public static int Ready = 1;
	public static int NO = 0;
}

// 时间 上下分 投退币
public struct KeyinKeoutRecord
{
    public string time; // 20151010-23:19:40
    public int keyin;
    public int keout;
    public int toubi;
    public int tuibi;
	public int card;	// VIP卡送分(原优惠卡分)
}

// 分机账目
public class AccountItem
{
	public int deviceIndex;
	public int keyin;
	public int keout;
	public int receiveCoin;	// 总投
	public int payCoin;		// 总退
	public int winnings;
	public int totalWinnings;
	public int card;
}

public enum PromptId
{
	None 		= -1,
	PleaseBet 	= 0,
	NoMoreBet 	= 1,
	Result 		= 2,
	Reboot		= 3,
	Jackpot		= 4
}

public struct BreakdownType
{
	public static int RecognizeBall = 1;	// 两次认球结果不同
	public static int USBDisconnect = 2;	// 与HID通讯不上
	public static int RecognizeBallTimeout = 3;	// 认球超时
}