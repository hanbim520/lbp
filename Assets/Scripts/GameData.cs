using UnityEngine;
using System.Collections;

public class GameData
{
    /*
     押分时间                                10-120（每次加5）
		投币代分								1-1000
		彩票代分
		最小押分
		单点限注                                1000-30000（每次加1000）
		大小限红								1000-30000（每次加1000）
		单双限红								1000-30000（每次加1000）
		颜色限红                                1000-30000（每次加1000）
		全台爆机分         
		单台爆机分   
		恢复出厂设置
		保存退出
不保存退出
*/

    // Setting menu
    public int betTimeLimit;
    public int coinToScore;
    public int ticketToScore;
    public int minBet;
    public int danXianZhu;
    public int daXiaoXianHong;
    public int danShuangXianHong;
    public int yanSeXianHong;
    public int quanTaiBaoJi;
    public int danTaiBaoJi;

    // Account
    public int[] zongShang;
    public int[] zongXia;
    public int[] zongTou;
    public int[] zongTui;
    public int[] zongYa;
    public int[] zongYing;
	public int[] caiPiao;

	private const int maxNumOfPlayers = 8;
	public int MaxNumOfPlayers
	{
		get { return maxNumOfPlayers; }
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
        PlayerPrefs.SetInt("daXiaoXianHong", daXiaoXianHong);
        PlayerPrefs.SetInt("danShuangXianHong", danShuangXianHong);
        PlayerPrefs.SetInt("yanSeXianHong", yanSeXianHong);
        PlayerPrefs.SetInt("quanTaiBaoJi", quanTaiBaoJi);
        PlayerPrefs.SetInt("danTaiBaoJi", danTaiBaoJi);
        PlayerPrefs.Save();
    }

    public void DefaultSetting ()
    {
        betTimeLimit = 15;
        coinToScore = 1;
        ticketToScore = 1;
        minBet = 1;
        danXianZhu = 20000;
        daXiaoXianHong = 1000;
        danShuangXianHong = 1000;
        yanSeXianHong = 1000;
        quanTaiBaoJi = 20000;
        danTaiBaoJi = 20000;
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
            daXiaoXianHong = PlayerPrefs.GetInt("daXiaoXianHong");
            danShuangXianHong = PlayerPrefs.GetInt("danShuangXianHong");
            yanSeXianHong = PlayerPrefs.GetInt("yanSeXianHong");
            quanTaiBaoJi = PlayerPrefs.GetInt("quanTaiBaoJi");
            danTaiBaoJi = PlayerPrefs.GetInt("danTaiBaoJi");

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
