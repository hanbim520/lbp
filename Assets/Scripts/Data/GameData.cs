using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameData
{
    // Setting menu
    public int betTimeLimit;
    public int coinToScore;
    public int ticketToScore;
    public int minBet;
    public int danXianZhu;
	public int gameDifficulty;
    public int quanTaiBaoJi;

    // Account
    public int[] zongShang;
    public int[] zongXia;
    public int[] zongTou;
    public int[] zongTui;
    public int[] zongYa;
    public int[] zongYing;
	public int[] caiPiao;

	// Odds
    public float yanseOdds = 1.97f;
    public float shuangOdds = 1.97f;
    public float danOdds = 1.97f;
    public float daOdds = 1.97f;
    public float xiaoOdds = 1.97f;
    public float duOdds = 36.97f;

	public string deviceId; // unique string
	public int deviceIndex;	// 1, 2, 3...
	public Dictionary<int, string> clientsId;

	// Serial mouse coordinates
	public int serialMouesX;
	public int serialMouseY;

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
        PlayerPrefs.SetInt("ticketToScore", ticketToScore);
        PlayerPrefs.SetInt("minBet", minBet);
        PlayerPrefs.SetInt("danXianZhu", danXianZhu);
		PlayerPrefs.SetInt("gameDifficulty", gameDifficulty);
        PlayerPrefs.SetInt("quanTaiBaoJi", quanTaiBaoJi);
        PlayerPrefs.Save();
    }

    public void DefaultSetting ()
    {
        betTimeLimit = 15;
        coinToScore = 1;
        ticketToScore = 1;
        minBet = 1;
        danXianZhu = 20000;
		gameDifficulty = 1;
        quanTaiBaoJi = 20000;
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
        zongShang = new int[maxNumOfPlayers];
        zongXia = new int[maxNumOfPlayers];
        zongTou = new int[maxNumOfPlayers];
        zongTui = new int[maxNumOfPlayers];
        zongYa = new int[maxNumOfPlayers];
        zongYing = new int[maxNumOfPlayers];
		caiPiao = new int[maxNumOfPlayers];
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
        PlayerPrefs.DeleteAll();
        int firstWrite = PlayerPrefs.GetInt("FirstWrite", 0);
        if (firstWrite == 0)
        {
            DefaultSetting();
            SaveSetting();
            DefaultAccount();
            SaveAccount();
            PlayerPrefs.SetInt("FirstWrite", 1);
        }
        else
        {
            // Setting menu
            betTimeLimit = PlayerPrefs.GetInt("betTimeLimit");
            coinToScore = PlayerPrefs.GetInt("coinToScore");
            ticketToScore = PlayerPrefs.GetInt("ticketToScore");
            minBet = PlayerPrefs.GetInt("minBet");
            danXianZhu = PlayerPrefs.GetInt("danXianZhu");
			gameDifficulty = PlayerPrefs.GetInt("gameDifficulty");
            quanTaiBaoJi = PlayerPrefs.GetInt("quanTaiBaoJi");

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
        }
    }
}