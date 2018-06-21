﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BackendLogic : MonoBehaviour
{
    public GameObject menuMain;
    public GameObject menuSetting;
    public GameObject menuAccount;
	public GameObject menuLottery;
    public GameObject dlgPassword;
    public GameObject dlgYesNO;
    public GameObject dlgPrintCode;
    public GameObject warning;
	public Text[] deviceId;				// 机台序号 0:en 1:cn
	public GameObject[] btnMouse;
	public GameObject[] btnTouchScreen;
	public Dropdown billAcceptorList;

    private RectTransform mouseIcon;
    private GameObject downHitObject;
	private bool hitDownOpt = false;
    private Transform preSelected; 		// Only for setting menu
    private bool preInputState;
	private int passwordMode;			// 0:non-password 1:modify password 2:enter account 3:enter system
	private int passwordType; 			// 密码类型,修改密码时用到.	0:none 1:system 2:account 3:admin
	private int passwordPhase; 			// 修改密码的步骤序号			0:original 1:new 2:check
    private string txtPassword; 		// Temp variable
	private string txtCheckPassword;
    private GameObject dlgCalc;
    private Text calcTitle;
    private Text calcContent;
    private Text calcPassword;
    private Timer timerHideWarning;
	private Timer timerRefreshAccounts;
	private UHost host;
	private Dictionary<int, AccountItem> otherDevice = new Dictionary<int, AccountItem>();
	private Transform accountItemRoot;
	private Transform loadingRoot;
    private int printCodeTimes;
	private int iYesNoType = 0;	// Yes No对话框的类型: 0--none 1--清账
    
	private string[] strPassword = new string[]{"Input Original/Admin Password", "输入原密码,或管理员密码!"};
	private string[] strNewPassword = new string[]{"Input New Password", "输入新密码"};
    private string[] strAgain = new string[]{"Again!", "请再次输入!"};
	private string[] strOK = new string[]{"OK!", "修改成功!"};
    private string[] strCorrect = new string[]{"Correct!", "输入正确!"};
    private string[] strError = new string[]{"Error!", "输入错误!"};
	private string[] strAccountPassword = new string[]{"Input Account Password", "请输入查账密码"};
	private string[] strSysPassword = new string[]{"Input Sys-Password", "请输入设置密码"};
	private string[] strAdminPassword = new string[]{"Input Admin Password", "请输入管理员密码"};
	private string[] strDeviceId = new string[]{"Device Id", "请输入设备Id"};
	private string[] strSetError = new string[]{"Only host device can enter.", "只有主机可以进入"};
	private string[] strErrorPW = new string[]{"Incorrect Password!", "密码输入错误!"};

    void Start()
    {
		GameData.GetInstance().ReadDataFromDisk();
        printCodeTimes = 0;
        preInputState = InputEx.inputEnable;
        mouseIcon = GameObject.Find("Canvas/mouse icon").GetComponent<RectTransform>();
        calcTitle = GameObject.Find("Canvas/Calc/Input/Title").GetComponent<Text>();
        calcContent = GameObject.Find("Canvas/Calc/Input/Content").GetComponent<Text>();
        calcPassword = GameObject.Find("Canvas/Calc/Input/Password").GetComponent<Text>();
		dlgCalc = GameObject.Find("Canvas/Calc");
        calcPassword.text = calcTitle.text = calcContent.text = string.Empty;
		InitMain();

		if (GameData.GetInstance().deviceIndex == 1)
		{
			host = GameObject.Find("NetworkObject").GetComponent<UHost>();
		}

        GameEventManager.PrintCodeSuccess += PrintCodeSuccess;
        GameEventManager.PrintCodeFail += PrintCodeFail;
    }

    void OnDestroy()
    {
        InputEx.inputEnable = preInputState;

        GameEventManager.PrintCodeSuccess -= PrintCodeSuccess;
        GameEventManager.PrintCodeFail -= PrintCodeFail;
    }

    void Update()
    {
        if (!IsDlgActived())
       		DetectInputEvents();
        UpdateTimer();
    }

	public bool IsDlgActived()
	{
		return warning.activeSelf;
	}
    
    private void DetectInputEvents()
    {
		if (InputEx.GetInputDown() && !hitDownOpt)
        {
            Vector2 pos;
            InputEx.InputDownPosition(out pos);
            if (pos == new Vector2(-1, -1))
                return;
            
            float sx, sy;
            Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
            RaycastHit2D[] hit = Physics2D.RaycastAll(new Vector2(sx, sy), Vector2.zero);
            if (hit.Length == 0)
                return;

            int idx = 0;
            if (hit.Length > 1)
            {
                for (int i = 0; i < hit.Length; ++i)
                {
					if (hit[i].collider.tag == "Dialog")
                    {
                        idx = i;
                        break;
                    }
					else if (hit[i].collider.gameObject.GetComponent<ButtonEvent>() != null)
						idx = i;
                }
            }
            if (hit[idx].collider != null)
            {
                hit[idx].collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit[idx].collider.transform);
                downHitObject = hit[idx].collider.gameObject;
            }
            
            mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
			hitDownOpt = true;
        }
		else if (InputEx.GetInputUp() && hitDownOpt)
        {
            Vector2 pos;
            InputEx.InputUpPosition(out pos);
            if (pos == new Vector2(-1, -1))
                return;
            
            mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
            
            if (downHitObject != null)
            {
                downHitObject.GetComponent<ButtonEvent>().OnInputUp(downHitObject.transform);
            }
			hitDownOpt = false;
        }
    }

	public void SettingDownEvent(Transform hitObject)
    {
        if (IsSettingDlgActived())
            return;

        if (preSelected != null)
        {
            Transform t = preSelected.Find("Image");
            if (t != null)
                t.gameObject.SetActive(false);
            SetAlpha(preSelected, 0);
        }
        SetAlpha(hitObject, 255);

        Transform img = hitObject.Find("Image");
        if (img != null)
        {
            img.gameObject.SetActive(true);
        }
        preSelected = hitObject;
    }

    public void SettingUpEvent(Transform hitObject)
    {
        if (IsSettingDlgActived())
            return;

        ClearCalc();
        passwordMode = 0;
        passwordType = 0;
		passwordPhase = 0;
        txtPassword = null;
        if (string.Equals(hitObject.name, "exit"))
        {
			SetAlpha(preSelected, 0);
			preSelected = null;
            InitMain();
        }
        else if (string.Equals(hitObject.name, "save"))
        {
			if (menuSetting.activeSelf)
            	SaveSetting();
			else if (menuLottery.activeSelf)
				SaveLottery();
			GameEventManager.OnSyncData();
        }
        else if (string.Equals(hitObject.name, "reset"))
        {
            GameData.GetInstance().DefaultSetting();
            InitSetting();
        }
    }

    public void CalcDownEvent(Transform hitObject)
    {
        if (IsSettingDlgActived())
            return;
        SetAlpha(hitObject, 255);
    }

    public void CalcUpEvent(Transform hitObject)
    {
        if (IsSettingDlgActived())
            return;
        SetAlpha(hitObject, 0);
        string name = hitObject.name;
        if (string.Equals(name, "del"))
        {
            DelCalcContent();
        }
        else if (string.Equals(name, "enter2"))
        {
            CalcEnterEvent();
        }
        else
        {
            int value;
            if (int.TryParse(name, out value))
            {
                AppendCalcContent(value);
            }
        }
    }

    public void MainDownEvent(Transform hitObject)
    {
		if (IsMainDlgActived())
			return;

        SetAlpha(hitObject, 255);
    }

    public void MainUpEvent(Transform hitObject)
    {
		if (IsMainDlgActived())
			return;

        SetAlpha(hitObject, 0);
        string name = hitObject.name;
		if (string.Compare(name, "setting2") == 0)
		{
			if (GameData.GetInstance().deviceIndex == 1)
			{
				dlgCalc.SetActive(true);
				dlgCalc.transform.localPosition = new Vector3(100, 200, 0);
				calcTitle.text = strSysPassword[GameData.GetInstance().backendLanguage];
				calcContent.text = string.Empty;
				calcPassword.text = string.Empty;
				passwordMode = 3;
			}
			else
				ShowWarning(strSetError[GameData.GetInstance().backendLanguage], true);
		}
		else if (string.Compare(name, "account2") == 0)
		{
			if (GameData.GetInstance().deviceIndex == 1)
			{
				dlgCalc.SetActive(true);
				dlgCalc.transform.localPosition = new Vector3(100, 200, 0);
				calcTitle.text = strAccountPassword[GameData.GetInstance().backendLanguage];
				calcContent.text = string.Empty;
				calcPassword.text = string.Empty;
				passwordMode = 2;
			}
			else
				ShowWarning(strSetError[GameData.GetInstance().backendLanguage], true);
		}
		else if (string.Compare(name, "lottery2") == 0)
		{
			if (GameData.GetInstance().deviceIndex == 1)
				InitLotterySetting();
			else
				ShowWarning(strSetError[GameData.GetInstance().backendLanguage], true);
		}
		else if (string.Compare(name, "exit2") == 0)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.StartInfo);
		}
		else if (string.Compare(name, "device2 id") == 0)
		{
			dlgCalc.SetActive(true);
			dlgCalc.transform.localPosition = new Vector3(100, 40, 0);
			calcTitle.text = strDeviceId[GameData.GetInstance().backendLanguage];
			calcContent.text = string.Empty;
			calcPassword.text = string.Empty;
			passwordMode = 0;
		}
		else if (string.Compare(name, "ts2") == 0)
		{
			foreach (GameObject i in btnMouse)
				i.SetActive(true);
			foreach (GameObject i in btnTouchScreen)
				i.SetActive(false);
			GameData.GetInstance().inputDevice = 0;
			GameData.GetInstance().SaveInputDevice();
			StartCoroutine(ChangeInputHanlde());
		}
		else if (string.Compare(name, "mouse2") == 0)
		{
			foreach (GameObject i in btnMouse)
				i.SetActive(false);
			foreach (GameObject i in btnTouchScreen)
				i.SetActive(true);
			GameData.GetInstance().inputDevice = 1;
			GameData.GetInstance().SaveInputDevice();
			StartCoroutine(ChangeInputHanlde());
		}
		else if (string.Compare(hitObject.name, "password") == 0)
		{
			passwordMode = 1;
			passwordType = 0;
			passwordPhase = 0;
			InitPasswordDlg();
		}
    }

	private IEnumerator ChangeInputHanlde()
	{
		GameObject go = GameObject.Find("InputDevice");
		if (go != null)
		{
			string[] msg = {"Setting device, please wait.", "正在设置，请稍后！"};
			ShowWarning(msg[GameData.GetInstance().backendLanguage], true, 4.0f);

			SerialMousePort mouse = go.GetComponent<SerialMousePort>();
			TouchScreenPort touchScreen = go.GetComponent<TouchScreenPort>();
			if (GameData.GetInstance().inputDevice == 0)
			{
				if (mouse != null)
				{
					mouse.Close();
					Destroy(mouse);
				}
				yield return new WaitForSeconds(2.0f);
				if (touchScreen == null)
				{
					go.AddComponent<TouchScreenPort>();
				}
			}
			else
			{
				if (touchScreen != null)
				{
					touchScreen.Close();
					Destroy(touchScreen);
				}
				yield return new WaitForSeconds(2.0f);
				if (mouse == null)
				{
					go.AddComponent<SerialMousePort>();
				}
			}
			GameEventManager.OnSyncInputDevice();
		}
	}

    public void DlgPasswordDownEvent(Transform hitObject)
    {
        SetAlpha(hitObject, 255);
    }

    public void DlgPasswordUpEvent(Transform hitObject)
    {
        SetAlpha(hitObject, 0);
        string name = hitObject.name;
		if (string.Compare(name, "system") == 0)
        {
            passwordType = 1;
        }
		else if (string.Compare(name, "account") == 0)
        {
            passwordType = 2;
        }
		else if (string.Compare(name, "admin") == 0)
		{
			passwordType = 3;
		}
		dlgCalc.SetActive(true);
		dlgCalc.transform.localPosition = new Vector3(100, 40, 0);
        int idx = GameData.GetInstance().backendLanguage;
        dlgPassword.SetActive(false);
        SetCalcTitle(strPassword[idx], Color.black);
        SetCalcContent(string.Empty, Color.white);
    }

	public void DlgYesNoDownEvent(Transform hitObject)
	{
		SetAlpha(hitObject, 255);
		iYesNoType = 1;
	}

	public void DlgYesNoUpEvent(Transform hitObject)
	{
		SetAlpha(hitObject, 0);
		iYesNoType = 0;
		string name = hitObject.name;
		if (Utils.StringIsEquals(name, "Yes"))
		{
			passwordMode = 4;
			passwordPhase = 0;
			passwordType = 0;
			calcTitle.text = strAdminPassword[GameData.GetInstance().backendLanguage];
			calcTitle.color = Color.black;
			calcPassword.text = calcContent.text = string.Empty;
			dlgCalc.SetActive(true);
			dlgCalc.transform.localPosition = Vector3.zero;
//			ClearAccount();
		}
		dlgYesNO.SetActive(false);
	}

    private void InitMain()
    {
        menuMain.SetActive(true);
        menuSetting.SetActive(false);
        menuAccount.SetActive(false);
		menuLottery.SetActive(false);
		dlgCalc.SetActive(false);

		calcContent.text = calcPassword.text = calcTitle.text = string.Empty;
		passwordType = 0;
		passwordMode = 0;
		passwordPhase = 0;

        SetLanguage(menuMain);
		SetInputDevice();

		// Device id
		int idx = GameData.GetInstance().backendLanguage;
		if (GameData.GetInstance().deviceIndex <= 0)
			deviceId[idx].text = string.Empty;
		else
        {
            deviceId[0].text = GameData.GetInstance().deviceIndex.ToString();
			deviceId[1].text = GameData.GetInstance().deviceIndex.ToString();
        }
    }

	private void InitLotterySetting()
	{
		if (GameData.GetInstance().deviceIndex > 1)
		{
			ShowWarning(strSetError[GameData.GetInstance().backendLanguage], true);
			return;
		}

		menuMain.SetActive(false);
		menuSetting.SetActive(false);
		menuAccount.SetActive(false);
		menuLottery.SetActive(true);
		dlgCalc.SetActive(true);
		dlgCalc.transform.localPosition = Vector3.zero;
		calcContent.text = string.Empty;
		calcPassword.text = string.Empty;

		SetLanguage(menuLottery);

		GameData ga = GameData.GetInstance();
		string[] datas = new string[]{ga.lotteryCondition.ToString(), ga.lotteryBase.ToString(), ga.lotteryRate.ToString(), ga.lotteryAllocation.ToString()};

		Transform root = menuLottery.transform.Find("Valid Fields");
		if (root != null)
		{
			int count = root.childCount;
			for (int i = 0; i < count; ++i)
			{
				Transform str = root.GetChild(i).Find("Text");
				if (str != null)
				{
					str.GetComponent<Text>().text = datas[i];
				}
			}
		}
	}

    private void InitSetting()
    {
		if (GameData.GetInstance().deviceIndex > 1)
		{
			ShowWarning(strSetError[GameData.GetInstance().backendLanguage], true);
			return;
		}
        menuMain.SetActive(false);
        menuSetting.SetActive(true);
        menuAccount.SetActive(false);
		menuLottery.SetActive(false);
		dlgCalc.SetActive(true);
		dlgCalc.transform.localPosition = new Vector3(0, -30, 0);
		calcContent.text = string.Empty;
		calcPassword.text = string.Empty;
		calcTitle.text = string.Empty;

//        SetLanguage(menuSetting);

        GameData ga = GameData.GetInstance();
		string[] datas = new string[]{ga.betTimeLimit.ToString(), ga.coinToScore.ToString(), ga.baoji.ToString(), ga.gameDifficulty.ToString(), 
			ga.betChipValues[0].ToString(), ga.betChipValues[1].ToString(), ga.betChipValues[2].ToString().ToString(), ga.betChipValues[3].ToString(), ga.betChipValues[4].ToString(), ga.betChipValues[5].ToString(),
			ga.max36Value.ToString(), ga.max18Value.ToString(), ga.max12Value.ToString(), ga.max9Value.ToString(), ga.max6Value.ToString(), ga.max3Value.ToString(), ga.max2Value.ToString(),
			ga.lotteryLv.ToString(), ga.lotteryCondition.ToString(), ga.lotteryBase.ToString(), ga.lotteryRate.ToString(), ga.lotteryAllocation.ToString(),
			ga.beginSessions.ToString(), ga.maxNumberOfFields.ToString(), ga.topScreenLanguage.ToString()};

        Transform root = menuSetting.transform.Find("Valid Fields");
        if (root != null)
        {
			int pageIdx = 0;
			int count = datas.Length - 1;
            for (int i = 0; i <= count; ++i)
            {
                Transform str = root.GetChild(pageIdx).GetChild(i).Find("Text");
                if (str != null)
                {
                    str.GetComponent<Text>().text = datas[i];
                }
            }
			pageIdx = 1;
			root.GetChild(pageIdx).GetChild(0).Find("Text").GetComponent<Text>().text = ga.allMax36Val.ToString();
			root.GetChild(pageIdx).GetChild(1).Find("Text").GetComponent<Text>().text = ga.allMax18Val.ToString();
			root.GetChild(pageIdx).GetChild(2).Find("Text").GetComponent<Text>().text = ga.allMax12Val.ToString();
			root.GetChild(pageIdx).GetChild(3).Find("Text").GetComponent<Text>().text = ga.allMax9Val.ToString();
			root.GetChild(pageIdx).GetChild(4).Find("Text").GetComponent<Text>().text = ga.allMax6Val.ToString();
			root.GetChild(pageIdx).GetChild(5).Find("Text").GetComponent<Text>().text = ga.allMax3Val.ToString();
			root.GetChild(pageIdx).GetChild(6).Find("Text").GetComponent<Text>().text = ga.allMax2Val.ToString();
			root.GetChild(pageIdx).GetChild(7).Find("Text").GetComponent<Text>().text = ga.blowTiming.ToString();
        }
		billAcceptorList.value = GameData.GetInstance().billAcceptorType;
    }

    private void InitAccount()
    {
		host.SendToAll(NetInstr.CheckAccount.ToString());
		if (accountItemRoot == null)
			accountItemRoot = menuAccount.transform.Find("ItemsRoot");
		if (loadingRoot == null)
			loadingRoot = menuAccount.transform.Find("LoadingTextRoot");

        menuMain.SetActive(false);
        menuSetting.SetActive(false);
        menuAccount.SetActive(true);
		menuLottery.SetActive(false);
        dlgCalc.SetActive(false);

        SetLanguage(menuAccount.gameObject);

		otherDevice.Clear();
		UpdateHostAccount();
		CalcTotalAccount();

		if (loadingRoot != null)
		{
			float basePosY = 325;
			float dist = 45;
			for (int i = 0; i < GameData.GetInstance().MaxNumOfPlayers; ++i)
			{
				Object prefab = (Object)Resources.Load("Account/LoadingText");
				GameObject go = (GameObject)Instantiate(prefab);
				go.transform.SetParent(loadingRoot);
				go.transform.localScale = Vector3.one;
				go.transform.localPosition = new Vector3(-85, basePosY - dist * i, 0);
				go.GetComponent<LoadingText>().baseText = "Device " + (i + 1) + " is loading";
				go.name = "LoadingText" + i;
				prefab = null;
			}
		}

		timerRefreshAccounts = new Timer(5, 1, TimerType.Loop);
		timerRefreshAccounts.Tick += RefreshAccounts;
		timerRefreshAccounts.Start();
    }

	private void RefreshAccounts()
	{
		foreach (KeyValuePair<int, AccountItem> item in otherDevice)
		{
			int id = item.Key;
			if (loadingRoot != null)
			{
				Transform t = loadingRoot.Find("LoadingText" + (id - 1));
				if (t != null)
				{
					string prefabName = "Account/AccountItem NCC";
					Object prefab = (Object)Resources.Load(prefabName);
					GameObject go = (GameObject)Instantiate(prefab);
					go.transform.SetParent(accountItemRoot);
					go.transform.localScale = Vector3.one;
					go.transform.localPosition = new Vector3(0, t.localPosition.y, 0);
					go.transform.Find("idx").GetComponent<Text>().text = item.Value.deviceIndex.ToString();
					go.transform.Find("keyin").GetComponent<Text>().text = item.Value.keyin.ToString();
					go.transform.Find("keout").GetComponent<Text>().text = item.Value.keout.ToString();
					go.transform.Find("tou").GetComponent<Text>().text = item.Value.receiveCoin.ToString();
					go.transform.Find("tui").GetComponent<Text>().text = item.Value.payCoin.ToString();
					go.transform.Find("total winnings").GetComponent<Text>().text = item.Value.totalWinnings.ToString();
					go.transform.Find("card").GetComponent<Text>().text = item.Value.card.ToString();
					go.name = "AccountItem NCC" + (id - 1);
					prefab = null;
					Destroy(t.gameObject);
				}
				else
				{
					string name = "AccountItem NCC" + (id - 1);
					t = accountItemRoot.Find(name);
					t.Find("idx").GetComponent<Text>().text = item.Value.deviceIndex.ToString();
					t.Find("keyin").GetComponent<Text>().text = item.Value.keyin.ToString();
					t.Find("keout").GetComponent<Text>().text = item.Value.keout.ToString();
					t.Find("tou").GetComponent<Text>().text = item.Value.receiveCoin.ToString();
					t.Find("tui").GetComponent<Text>().text = item.Value.payCoin.ToString();
					t.Find("total winnings").GetComponent<Text>().text = item.Value.totalWinnings.ToString();
					t.Find("card").GetComponent<Text>().text = item.Value.card.ToString();
				}
			}
		}
		CalcTotalAccount();
		host.SendToAll(NetInstr.CheckAccount.ToString());
	}

	public void AccountExitEvent(Transform hitObject)
	{
		otherDevice.Clear();
		if (accountItemRoot == null)
			accountItemRoot = menuAccount.transform.Find("ItemsRoot");
		if (accountItemRoot != null)
		{
			foreach (Transform t in accountItemRoot)
				Destroy(t.gameObject);
		}

		Transform loadingRoot = menuAccount.transform.Find("LoadingTextRoot");
		if (loadingRoot != null)
		{
			foreach (Transform t in loadingRoot)
				Destroy(t.gameObject);
		}

        if (timerRefreshAccounts != null)
        {
            timerRefreshAccounts.Stop();
            timerRefreshAccounts = null;
        }

		menuMain.SetActive(true);
		menuSetting.SetActive(false);
		menuAccount.SetActive(false);
		menuLottery.SetActive(false);
		dlgCalc.SetActive(false);
		dlgPrintCode.SetActive(false);
	}

    private void InitPasswordDlg()
    {
        dlgPassword.SetActive(true);
        SetLanguage(dlgPassword);
    }

    private GameObject SetLanguage(GameObject menu)
    {
        GameObject en = menu.transform.Find("EN").gameObject;
        GameObject cn = menu.transform.Find("CN").gameObject;
        if (GameData.GetInstance().backendLanguage == 0)
        {
            en.SetActive(true);
            cn.SetActive(false);
            return en;
        }
        else
        {
            en.SetActive(false);
            cn.SetActive(true);
            return cn;
        }
    }

	private void SetInputDevice()
	{
		if (GameData.GetInstance().inputDevice == 0)
		{
			foreach (GameObject i in btnMouse)
				i.SetActive(true);
			foreach (GameObject i in btnTouchScreen)
				i.SetActive(false);
		}
		else
		{
			foreach (GameObject i in btnMouse)
				i.SetActive(false);
			foreach (GameObject i in btnTouchScreen)
				i.SetActive(true);
		}
	}

    private void SetAlpha(Transform target, int alpha)
    {
        Image field = target.GetComponent<Image>();
        Color c = field.color;
        c.a = alpha;
        field.color = c;
    }

    private void SetCalcTitle(string title, Color color)
    {
        calcTitle.text = title;
        calcTitle.color = color;
    }

    private void AppendCalcContent(int num)
    {
		if (calcContent.color == Color.red)
			calcContent.text = string.Empty;

        if (passwordMode != 0)
        {
            string text = calcPassword.text;
            if (text.Length < GameData.GetInstance().passwordLength)
            {
                text += "*";
                txtPassword += num.ToString();
//                print(txtPassword);
                SetCalcContent(text, Color.white);
            }
        }
        else
        {
            string text = calcContent.text;
            if (string.Equals(text, "0"))
                text = num.ToString();
            else
                text = text + num.ToString();
            SetCalcContent(text, Color.white);
        }
    }

    private void DelCalcContent()
    {
        Text target;
        if (passwordMode != 0)
            target = calcPassword;
        else
            target = calcContent;
        string text = target.text;

        int length = text.Length;
        if (length > 1)
            text = text.Substring(0, length - 1);
        else if (length == 1)
        {
            if (passwordMode != 0)
                text = string.Empty;
            else
                text = "0";
        }

        if (passwordMode != 0)
        {
			if (txtPassword != null)
			{
				if (txtPassword.Length > 1)
					txtPassword = txtPassword.Substring(0, txtPassword.Length - 1);
				else
					txtPassword = string.Empty;
//              print(txtPassword);
			}
        }
        SetCalcContent(text, Color.white);
    }

    private void CalcEnterEvent()
    {
        if (passwordMode == 1)				// 修改密码
        {
			int idx = GameData.GetInstance().backendLanguage;
			if (passwordPhase == 0)			// 输入原密码
			{
				if (((Utils.StringIsEquals(txtPassword, GameData.GetInstance().systemPassword) || 
				    Utils.StringIsEquals(txtPassword, GameData.GetInstance().adminPassword)) && passwordType == 1) ||
				    ((Utils.StringIsEquals(txtPassword, GameData.GetInstance().accountPassword) || 
				  	Utils.StringIsEquals(txtPassword, GameData.GetInstance().adminPassword)) && passwordType == 2) ||
				    (Utils.StringIsEquals(txtPassword, GameData.GetInstance().adminPassword) && passwordType == 3))
				{
					passwordPhase = 1;
					SetCalcTitle(strNewPassword[idx], Color.black);
					calcPassword.text = string.Empty;
				}
				else
				{
					calcPassword.text = string.Empty;
					calcContent.text = strError[idx];
					calcContent.color = Color.red;
					print(GameData.GetInstance().systemPassword);
				}
				txtPassword = null;
			}
			else if (passwordPhase == 1)	// 输入新密码
			{
				passwordPhase = 2;
				txtCheckPassword = txtPassword;
				calcPassword.text = string.Empty;
				calcContent.text = strAgain[idx];
				calcContent.color = Color.red;
				txtPassword = null;
			}
			else if (passwordPhase == 2)	// 再次输入新密码
			{
				if (string.Compare(txtPassword, txtCheckPassword) == 0)
				{
					if (passwordType == 1)
					{
						GameData.GetInstance().systemPassword = txtPassword;
						GameData.GetInstance().SaveSysPassword();
					}
					else if (passwordType == 2)
					{
						GameData.GetInstance().accountPassword = txtPassword;
						GameData.GetInstance().SaveAccountPassword();
					}
					else if (passwordType == 3)
					{
						GameData.GetInstance().adminPassword = txtPassword;
						GameData.GetInstance().SaveAdminPassword();
					}
					calcTitle.text = string.Empty;
					calcPassword.text = string.Empty;
					calcContent.text = strOK[idx];
					calcContent.color = Color.red;

					passwordMode = 0;
					passwordType = 0;
					passwordPhase = 0;
					txtPassword = null;
				}
				else
				{
					calcPassword.text = string.Empty;
					calcContent.text = strError[idx];
					calcContent.color = Color.red;
					txtCheckPassword = txtPassword = null;
					passwordPhase = 1;
				}
			}
        }
		else if (passwordMode == 2)		// 输入查账密码
		{
			if (Utils.StringIsEquals(txtPassword, GameData.GetInstance().accountPassword))
			{
				txtPassword = "";
				InitAccount();
			}
			else
			{
				calcContent.text = strError[GameData.GetInstance().backendLanguage];
				calcContent.color = Color.red;
				calcPassword.text = string.Empty;
				txtPassword = null;
			}
		}
		else if (passwordMode == 3)		// 输入设置密码
		{
			if (Utils.StringIsEquals(txtPassword, GameData.GetInstance().systemPassword))
			{
				txtPassword = "";
				InitSetting();
			}
			else
			{
				calcContent.text = strError[GameData.GetInstance().backendLanguage];
				calcContent.color = Color.red;
				calcPassword.text = string.Empty;
				txtPassword = null;
			}
		}
		else if (passwordMode == 4)		// 清帐
		{
			if (Utils.StringIsEquals(txtPassword, GameData.GetInstance().adminPassword))
			{
				ClearAccount();
			}
			else
			{
				ShowWarning(strErrorPW[GameData.GetInstance().backendLanguage], true);
			}
			passwordMode = 0;
			calcPassword.text = string.Empty;
			dlgCalc.SetActive(false);
			txtPassword = null;
		}
        else 							// 输入参数值
        {
			// Set setting-menu item
			if (preSelected != null)
            {
                Transform target = preSelected.Find("Text");
                if (target != null)
                {
                    int value;
                    if (int.TryParse(calcContent.text, out value))
                    {
                        LimitValue(preSelected.name, ref value);
                        string str = value.ToString();
                        if (preSelected.name == "card2")
                            str += "%";
                        target.GetComponent<Text>().text = str;
                    }
                }
            }
			else if (menuMain.activeSelf)
			{
				// Set device index
				int value;
				if (int.TryParse(calcContent.text, out value))
				{
					if (value == 0)
						deviceId[GameData.GetInstance().backendLanguage].text = string.Empty;
					else
					{
                        deviceId[0].text = value.ToString();
						deviceId[1].text = value.ToString();
						GameData.GetInstance().deviceIndex = value;
						GameData.GetInstance().SaveDeviceIndex();
					}
				}
			}
        }
    }

    private void SetCalcContent(string text, Color color)
    {
		if (string.IsNullOrEmpty(text))
		{
			calcPassword.text = string.Empty;
			calcContent.text = string.Empty;
		}
		else
		{
			if (passwordMode != 0)
				calcPassword.text = text;
			else
				calcContent.text = text;
		}
       
        calcContent.color = color;
    }

    private void ClearCalc()
    {
        SetCalcTitle(string.Empty, Color.black);
        SetCalcContent(string.Empty, Color.white);
    }

    private bool IsSettingDlgActived()
    {
        return dlgYesNO.activeSelf;
    }

	private bool IsMainDlgActived()
	{
		return dlgPassword.activeSelf;
	}

	private void SaveLottery()
	{
		List<int> values = new List<int>();
		Transform root = menuLottery.transform.Find("Valid Fields");
		if (root != null)
		{
			int count = root.childCount;
			for (int i = 0; i < count; ++i)
			{
				Transform str = root.GetChild(i).Find("Text");
				if (str != null)
				{
					int value;
					string content = str.GetComponent<Text>().text;
					if (int.TryParse(content, out value))
					{
						values.Add(value);
					}
				}
			}

			GameData.GetInstance().lotteryCondition = values[0];
			GameData.GetInstance().lotteryBase = values[1];
			GameData.GetInstance().lotteryRate = values[2];
			GameData.GetInstance().lotteryAllocation = values[3];
			GameData.GetInstance().SaveSetting();
            int idx = GameData.GetInstance().backendLanguage;
			ShowWarning(Notifies.saveSuccess[idx], true);
		}
	}

    private void SaveSetting()
    {
        List<int> page1Val = new List<int>();
        List<int> page2Val = new List<int>();
        Transform root = menuSetting.transform.Find("Valid Fields");
        if (root != null)
        {
            int count = root.GetChild(0).childCount;
            for (int i = 0; i < count; ++i)
            {
                Transform str = root.GetChild(0).GetChild(i).Find("Text");
                if (str != null)
                {
                    int value;
                    string content = str.GetComponent<Text>().text;
                    if (content.Contains("%"))
                        content = content.Substring(0, content.Length - 1);
                    if (int.TryParse(content, out value))
                    {
                        page1Val.Add(value);
                    }
                }
            }

            GameData.GetInstance().betTimeLimit = page1Val[0];
            GameData.GetInstance().coinToScore = page1Val[1];
            GameData.GetInstance().baoji = page1Val[2];
            GameData.GetInstance().gameDifficulty = page1Val[3];
            GameData.GetInstance().betChipValues[0] = page1Val[4];
            GameData.GetInstance().betChipValues[1] = page1Val[5];
            GameData.GetInstance().betChipValues[2] = page1Val[6];
            GameData.GetInstance().betChipValues[3] = page1Val[7];
            GameData.GetInstance().betChipValues[4] = page1Val[8];
            GameData.GetInstance().betChipValues[5] = page1Val[9];
            GameData.GetInstance().max36Value = page1Val[10];
            GameData.GetInstance().max18Value = page1Val[11];
            GameData.GetInstance().max12Value = page1Val[12];
            GameData.GetInstance().max9Value = page1Val[13];
            GameData.GetInstance().max6Value = page1Val[14];
            GameData.GetInstance().max3Value = page1Val[15];
            GameData.GetInstance().max2Value = page1Val[16];
			GameData.GetInstance().lotteryLv = page1Val[17];
			GameData.GetInstance().lotteryCondition = page1Val[18];
            if (page1Val[19] != GameData.GetInstance().lotteryBase)
            {
                GameData.GetInstance().lotteryDigit = page1Val[19];
                CryptoPrefs.SetInt("lotteryDigit", page1Val[19]);
                CryptoPrefs.Save();
            }
			GameData.GetInstance().lotteryBase = page1Val[19];
			GameData.GetInstance().lotteryRate = page1Val[20];
			GameData.GetInstance().lotteryAllocation = page1Val[21];
			GameData.GetInstance().beginSessions = page1Val[22];
			GameData.GetInstance().maxNumberOfFields = page1Val[23];

			count = root.GetChild(1).childCount;
			for (int i = 0; i < count; ++i)
			{
				Transform str = root.GetChild(1).GetChild(i).Find("Text");
				if (str != null)
				{
					int value;
					string content = str.GetComponent<Text>().text;
					if (content.Contains("%"))
						content = content.Substring(0, content.Length - 1);
					if (int.TryParse(content, out value))
					{
						page2Val.Add(value);
					}
				}
			}
			GameData.GetInstance().allMax36Val = page2Val[0];
			GameData.GetInstance().allMax18Val = page2Val[1];
			GameData.GetInstance().allMax12Val = page2Val[2];
			GameData.GetInstance().allMax9Val = page2Val[3];
			GameData.GetInstance().allMax6Val = page2Val[4];
			GameData.GetInstance().allMax3Val = page2Val[5];
			GameData.GetInstance().allMax2Val = page2Val[6];
			GameData.GetInstance().blowTiming = page2Val[7];
			GameData.GetInstance().billAcceptorType = billAcceptorList.value;
			GameData.GetInstance().SaveSetting();

			GameObject objBVA = GameObject.Find("BVAAndroid");
			if (objBVA != null)
			{
				BVAAndroid bva = objBVA.GetComponent<BVAAndroid>();
				bva.LoadScript(BVAAndroid.PortName);
			}
			
            int idx = GameData.GetInstance().backendLanguage;
            ShowWarning(Notifies.saveSuccess[idx], true);
        }
    }

	private void ShowWarning(string str, bool autoDisappear, float duration = 2.0f)
    {
        if (warning != null && !warning.activeSelf)
        {
            warning.SetActive(true);
            warning.transform.Find("Text").GetComponent<Text>().text = str;
			if (autoDisappear)
			{
				timerHideWarning = new Timer(duration, 0);
				timerHideWarning.Tick += HideWarning;
				timerHideWarning.Start();
			}
        }
    }

    private void HideWarning()
    {
        timerHideWarning = null;
        if (warning != null && warning.activeSelf)
        {
            warning.SetActive(false);
        }
    }

    private void UpdateTimer()
    {
        if (timerHideWarning != null)
            timerHideWarning.Update(Time.deltaTime);
		if (timerRefreshAccounts != null)
			timerRefreshAccounts.Update(Time.deltaTime);
    }

	// 限制参数的范围
    private void LimitValue(string name, ref int value)
    {
        if (string.Equals(name, "time"))
        {
            if (value > 90)
                value = 90;
            else if (value < 5)
                value = 5;
        }
        else if (string.Equals(name, "coin"))
        {
            if (value > 100000)
                value = 100000;
            else if (value < 1)
                value = 1;
        }
        else if (string.Equals(name, "baoji"))
        {
            if (value > 30000000)
                value = 30000000;
            else if (value < 10000)
                value = 10000;
        }
        else if (string.Equals(name, "difficulty"))
        {
            if (value > 480)
                value = 480;
            else if (value < 1)
                value = 1;
        }
        else if (string.Equals(name, "chip0") || string.Equals(name, "chip1") || string.Equals(name, "chip2") || string.Equals(name, "chip3") ||
		         string.Equals(name, "chip4") || string.Equals(name, "chip5") || string.Equals(name, "bonus limit"))
        {
			value = Mathf.Clamp(value, 0, 100000);
        }
        else if (string.Equals(name, "x36") || string.Equals(name, "x18") || string.Equals(name, "x12") || string.Equals(name, "x9") ||
		         string.Equals(name, "x6") || string.Equals(name, "x3") || string.Equals(name, "x2"))
        {
            if (value > 100000)
                value = 100000;
            else if (value < 1)
                value = 1;
        }
		else if (string.Equals(name, "device id"))
        {
            if (value > 100)
                value = 100;
            else if (value < 1)
                value = 1;
        }
		else if (string.Equals(name, "beginSession"))
        {
            if (value > 100000)
                value = 100000;
            else if (value < 1)
                value = 1;
        }
        else if (string.Equals(name, "type"))
        {
            if (value > 38)
                value = 38;
            else if (value < 37)
                value = 37;
        }
		else if (string.Equals(name, "bet limit"))
		{
			value = Mathf.Clamp(value, 1, 100000);
		}
		else if (string.Equals(name, "rate"))
		{
			value = Mathf.Clamp(value, 1, 100);
		}
		else if (string.Equals(name, "allocation") || Utils.StringIsEquals(name, "jackpot lv"))
		{
			value = Mathf.Clamp(value, 0, 100);
		}
		else if (Utils.StringIsEquals(name, "monitor language"))
		{
			value = Mathf.Clamp(value, 0, 1);
		}
		else if (Utils.StringIsEquals(name, "blow timing"))
		{
			value = Mathf.Clamp(value, 0, 7);
		}
    }

	public void HandleRecData(ref string[] words, int connectionId)
	{
		int instr;
		if (!int.TryParse(words[0], out instr))
			return;

		if (instr == NetInstr.CheckAccount)
		{
			int deviceIndex;
			if (int.TryParse(words[1], out deviceIndex))
			{
				AccountItem item = otherDevice.ContainsKey(deviceIndex) ? otherDevice[deviceIndex] : new AccountItem();
				item.deviceIndex = deviceIndex;
				int zongShang, zongXia, zongTou, zongTui, winnings, totalWinnings, card;
				if (int.TryParse(words[2], out zongShang))
					item.keyin = zongShang;
				if (int.TryParse(words[3], out zongXia))
					item.keout = zongXia;
				if (int.TryParse(words[4], out zongTou))
					item.receiveCoin = zongTou;
				if (int.TryParse(words[5], out zongTui))
					item.payCoin = zongTui;
				if (int.TryParse(words[6], out winnings))
					item.winnings = winnings;
				if (int.TryParse(words[7], out totalWinnings))
					item.totalWinnings = totalWinnings;
				if (int.TryParse(words[8], out card))
					item.card = card;
				if (!otherDevice.ContainsKey(deviceIndex))
					otherDevice.Add(deviceIndex, item);
			}
		}
	}

	// 清账按钮
	public void PrintCodeEvent(Transform hitObject)
	{
		if (GameData.controlCode)
		{
			PlayerPrefs.SetInt("ResetAccount", 1);
			PlayerPrefs.Save();
			GameData.GetInstance().NextLevelName = Scenes.StartInfo;
			UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.Loading);
		}
		else
		{
			dlgYesNO.SetActive(true);
		}
	}

	private void PrintCodeSuccess(int type)
    {
        dlgPrintCode.SetActive(false);
        printCodeTimes += 1;
        if (printCodeTimes >= 2)
        {
            ClearAccount();
        }
		else
		{
//			GameData.GetInstance().currentWin = 0;
//			GameData.GetInstance().currentZongShang = 0;
//			GameData.GetInstance().currentZongXia = 0;
//			GameData.GetInstance().SaveAccount();
			UpdateHostAccount();
			string msg = NetInstr.ClearCurrentWin.ToString();
			host.SendToAll(msg);
		}
    }

    private void PrintCodeFail()
    {
        Debug.Log("PrintCodeFail");
		ShowWarning(strError[GameData.GetInstance().backendLanguage], true);
    }

	// 账目归零
    private void ClearAccount()
    {
		GameData.GetInstance().ClearAccount();
		UpdateHostAccount();
		string msg = NetInstr.ClearAccount.ToString();
		host.SendToAll(msg);
    }

	private void UpdateHostAccount()
	{
		int deviceIndex = GameData.GetInstance().deviceIndex;

		AccountItem item = otherDevice.ContainsKey(deviceIndex) ? otherDevice[deviceIndex] : new AccountItem();
		item.deviceIndex = deviceIndex;
		item.keyin = GameData.GetInstance().zongShang;
		item.keout = GameData.GetInstance().zongXia;
		item.receiveCoin = GameData.GetInstance().zongTou;
		item.payCoin = GameData.GetInstance().zongTui;
		item.winnings = GameData.GetInstance().totalWin;
		item.totalWinnings = GameData.GetInstance().totalWin;
		item.card = GameData.GetInstance().cardCredits;
		if (!otherDevice.ContainsKey(deviceIndex))
			otherDevice.Add(deviceIndex, item);
	}

	private void CalcTotalAccount()
	{
		string goName = "TotalNum NCC";
		Transform totalNum = accountItemRoot.Find(goName);
		if (totalNum == null)
		{
			string prefabName = "Account/TotalNum NCC";
			Object prefab = (Object)Resources.Load(prefabName);
			GameObject go = (GameObject)Instantiate(prefab);
			go.transform.SetParent(accountItemRoot);
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = new Vector3(0, -60, 0);
			go.name = goName;
			prefab = null;
			totalNum = go.transform;
		}
		int keyin = 0, keout = 0, tou = 0, tui = 0, winnings = 0, totalWin = 0, card = 0;
		foreach (AccountItem item in otherDevice.Values)
		{
			keyin += item.keyin;
			keout += item.keout;
			tou += item.receiveCoin;
			tui += item.payCoin;
			winnings += item.winnings;
			totalWin += item.totalWinnings;
			card += item.card;
		}
		totalNum.Find("keyin").GetComponent<Text>().text = keyin.ToString();
		totalNum.Find("keout").GetComponent<Text>().text = keout.ToString();
		totalNum.Find("tou").GetComponent<Text>().text = tou.ToString();
		totalNum.Find("tui").GetComponent<Text>().text = tui.ToString();
		totalNum.Find("total winnings").GetComponent<Text>().text = totalWin.ToString();
		totalNum.Find("card").GetComponent<Text>().text = card.ToString();
	}

	private void SetBackendLanguage()
	{
		int lang = GameData.GetInstance().backendLanguage;
		lang = lang == 0 ? 1 : 0;
		GameData.GetInstance().backendLanguage = lang;
		SetLanguage(menuMain);
		GameData.GetInstance().SaveBackendLanguage();
	}
}
