using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Backend : MonoBehaviour
{
    public GameObject txtMenuGuide;
    public GameObject mainMenu;
    public GameObject settingMenu;
    public GameObject checkAccountMenu;
    public GameObject exitMenu;

    private enum BackMenus
    {
        Main = 0, Setting, CheckAccount, Exit
    }
    private BackMenus menusType;
    private GameObject[] menus;
    private int numOfItems;
    private int curItemId;
    private bool bEnableInput = false;
    private int intervalValue = 1;

    public int ItemId
    {
        set
        {
            curItemId = value;
            if (curItemId < 0)
            {
                curItemId = NumOfItems - 1;
            }
            else if (curItemId >= NumOfItems)
            {
                curItemId = 0;
            }
            if (NumOfItems >= 0)
            {
                Transform mark = GetCurMenu().transform.GetChild(NumOfItems);
                float posY = GetCurMenu().transform.GetChild(curItemId).localPosition.y;
                Vector3 pos = mark.localPosition;
                pos.y = posY;
                mark.localPosition = pos;
            }
        }
        get { return curItemId; }
    }

    public int NumOfItems
    {
        get { return numOfItems; }
        set { numOfItems = value - 1; }
    }

    void Start()
    {
        menusType = BackMenus.Main;
        menus = new GameObject[4] { mainMenu, settingMenu, checkAccountMenu, exitMenu };
        SetMenuEnable(menusType);
        RegisterListener();
    }

    void OnDestroy()
    {
        UnregisterListener();
    }

    private void RegisterListener()
    {
        GameEventManager.ObtainInput += ObtainInput;
    }

    private void UnregisterListener()
    {
        GameEventManager.ObtainInput -= ObtainInput;
    }

    private void ObtainInput()
    {
        bEnableInput = true;
    }

    void Update()
    {
        if (bEnableInput)
        {
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                --ItemId;
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                ++ItemId;
            }
            else if (Input.GetKeyUp(KeyCode.Return))
            {
                ItemOnClicked(ItemId);
            }
        }
    }

    private void SetMenuEnable(BackMenus menu)
    {
        int idx = (int)menu;
        menus[idx].SetActive(true);
        NumOfItems = menus[idx].transform.childCount;

        if (menusType != menu)
        {
            idx = (int)menusType;
            menus[idx].SetActive(false);
            menusType = menu;
        }
        if (menusType != BackMenus.Exit)
        {
            bEnableInput = true;
        }
        else
        {
            bEnableInput = false;
        }
        InitMenu();
        ItemId = 0;
    }

    private GameObject GetCurMenu()
    {
        int idx = (int)menusType;
        return menus[idx];
    }

    private void InitMenu()
    {
        if (menusType == BackMenus.Main)
            InitMainMenu();
        else if (menusType == BackMenus.Setting)
            InitSettingMenu();
        else if (menusType == BackMenus.CheckAccount)
            InitCheckAccount();
    }

    private void InitMainMenu()
    {
        if (GameData.GetInstance().language == 1)
            txtMenuGuide.GetComponent<Text>().text = "后台";
        else if (GameData.GetInstance().language == 0)
            txtMenuGuide.GetComponent<Text>().text = "Backend";
    }

    private void InitSettingMenu()
    {
        if (GameData.GetInstance().language == 1)
            txtMenuGuide.GetComponent<Text>().text = "后台 | 游戏设置";
        else if (GameData.GetInstance().language == 0)
            txtMenuGuide.GetComponent<Text>().text = "Backend | Setting";

        Transform item = settingMenu.transform.GetChild(0).GetChild(1);
        item.GetComponent<Text>().text = GameData.GetInstance().betTimeLimit.ToString();
        item = settingMenu.transform.GetChild(1).GetChild(1);
        item.GetComponent<Text>().text = GameData.GetInstance().coinToScore.ToString();
        item = settingMenu.transform.GetChild(2).GetChild(1);
        item.GetComponent<Text>().text = GameData.GetInstance().ticketToScore.ToString();
        item = settingMenu.transform.GetChild(3).GetChild(1);
        item.GetComponent<Text>().text = GameData.GetInstance().minBet.ToString();
        item = settingMenu.transform.GetChild(4).GetChild(1);
        item.GetComponent<Text>().text = GameData.GetInstance().danXianZhu.ToString();
		item = settingMenu.transform.GetChild(5).GetChild(1);
		item.GetComponent<Text>().text = GameData.GetInstance().gameDifficulty.ToString();
        item = settingMenu.transform.GetChild(6).GetChild(1);
        item.GetComponent<Text>().text = GameData.GetInstance().quanTaiBaoJi.ToString();
    }

    private void InitCheckAccount()
    {

        if (GameData.GetInstance().language == 1)
            txtMenuGuide.GetComponent<Text>().text = "后台 | 账目查询";
        else if (GameData.GetInstance().language == 0)
            txtMenuGuide.GetComponent<Text>().text = "Backend | Check Account";

		for (int i = 0; i < GameData.GetInstance().MaxNumOfPlayers; ++i)
		{
			int[] v = new int[7]{	GameData.GetInstance().zongShang[i], GameData.GetInstance().zongXia[i],
									GameData.GetInstance().zongYa[i], GameData.GetInstance().zongYing[i],
									GameData.GetInstance().zongTou[i], GameData.GetInstance().zongTui[i],
									GameData.GetInstance().caiPiao[i]};
			for (int j = 1; j <= 7; ++j)
			{
				Transform item = checkAccountMenu.transform.GetChild(i + 1).GetChild(j);
				item.GetComponent<Text>().text = v[j - 1].ToString();
			}
		}
	}
	
    private void ItemOnClicked(int idx)
    {
        if (menusType == BackMenus.Main)
            HandleMainMenu(idx);
        else if (menusType == BackMenus.Setting)
            HandleSettingMenu(idx);
        else if (menusType == BackMenus.CheckAccount)
            HandleCheckAccount(idx);
    }

    private void HandleMainMenu(int idx)
    {
        if (idx == 0)
            SetMenuEnable(BackMenus.Setting);
        else if (idx == 1)
            SetMenuEnable(BackMenus.CheckAccount);
        else if (idx == 2)
        {
            if (GameData.GetInstance().language == 1)
                txtMenuGuide.GetComponent<Text>().text = "后台 | 退出";
            else if (GameData.GetInstance().language == 0)
                txtMenuGuide.GetComponent<Text>().text = "Backend | Exit";
            SetMenuEnable(BackMenus.Exit);
        }
    }

    /*
押分时间                                10-120（每次加5）
投币代分								1-1000
彩票代分
最小押分
单点限注                                1000-30000（每次加1000）
游戏难度
全台爆机分         
恢复出厂设置
保存退出
不保存退出
*/
    private void HandleSettingMenu(int idx)
    {
        Transform item = null;
        if (idx < 7)
            item = settingMenu.transform.GetChild(idx).GetChild(1);
        if (idx == 0)
        {
            intervalValue = 5;
            GameData.GetInstance().betTimeLimit += intervalValue;
            if (GameData.GetInstance().betTimeLimit > 120)
                GameData.GetInstance().betTimeLimit = 10;
            item.GetComponent<Text>().text = GameData.GetInstance().betTimeLimit.ToString();
        }
        else if (idx == 1)
        {
            GameData.GetInstance().coinToScore *= 10;
            if (GameData.GetInstance().coinToScore > 1000)
                GameData.GetInstance().coinToScore = 1;
            item.GetComponent<Text>().text = GameData.GetInstance().coinToScore.ToString();
        }
        else if (idx == 2)
        {
            GameData.GetInstance().ticketToScore *= 10;
            if (GameData.GetInstance().ticketToScore > 1000)
                GameData.GetInstance().ticketToScore = 1;
            item.GetComponent<Text>().text = GameData.GetInstance().ticketToScore.ToString();
        }
        else if (idx == 3)
        {
            intervalValue = 1;
            GameData.GetInstance().minBet += intervalValue;
            if (GameData.GetInstance().minBet > 10000)
                GameData.GetInstance().minBet = 1;
            item.GetComponent<Text>().text = GameData.GetInstance().minBet.ToString();
        }
        else if (idx == 4)
        {
            intervalValue = 1000;
            GameData.GetInstance().danXianZhu += intervalValue;
            if (GameData.GetInstance().danXianZhu > 30000)
                GameData.GetInstance().danXianZhu = 1000;
            item.GetComponent<Text>().text = GameData.GetInstance().danXianZhu.ToString();
        }
		else if (idx == 5)
		{
			intervalValue = 1;
			GameData.GetInstance().gameDifficulty += intervalValue;
			if (GameData.GetInstance().gameDifficulty > 480)
				GameData.GetInstance().gameDifficulty = 1;
			item.GetComponent<Text>().text = GameData.GetInstance().gameDifficulty.ToString();
		}
        else if (idx == 6)
        {
            intervalValue = 1000;
            GameData.GetInstance().quanTaiBaoJi += intervalValue;
            if (GameData.GetInstance().quanTaiBaoJi > 30000)
                GameData.GetInstance().quanTaiBaoJi = 1000;
			item.GetComponent<Text>().text = GameData.GetInstance().quanTaiBaoJi.ToString();
        }
        else if (idx == 7)
        {
            GameData.GetInstance().DefaultSetting();
            InitSettingMenu();
        }
        else if (idx == 8)
        {
            GameData.GetInstance().SaveSetting();
            SetMenuEnable(BackMenus.Main);
        }
        else if (idx == 9)
        {
            GameData.GetInstance().ReadDataFromDisk();
            SetMenuEnable(BackMenus.Main);
        }
    }

    private void HandleCheckAccount(int idx)
    {
		SetMenuEnable(BackMenus.Main);
    }
}
