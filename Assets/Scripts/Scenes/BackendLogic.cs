using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BackendLogic : MonoBehaviour
{
    public GameObject menuMain;
    public GameObject menuSetting;
    public GameObject menuAccount;
    public GameObject dlgPassword;
    public GameObject dlgYesNO;
    public GameObject warning;

    private RectTransform mouseIcon;
    private GameObject downHitObject;
    private Transform preSelected; // Only for setting menu
    private bool preInputState;
	private int passwordMode;	// 0:non-password 1:modify 2:enter account
    private int passwordType; // 0:none 1:system 2:account
	private int passwordPhase; // 0:original 1:new 2:check
    private string txtPassword; // Temp variable
	private string txtCheckPassword;
    private GameObject dlgCalc;
    private Text calcTitle;
    private Text calcContent;
    private Text calcPassword;
    private Timer timerHideWarning;
    
    private string[] strPassword = new string[]{"Please Input Password", "请输入原密码"};
    private string[] strNewPassword = new string[]{"Please Input New Password", "请输入新密码"};
    private string[] strAgain = new string[]{"Again!", "请再次输入!"};
	private string[] strOK = new string[]{"OK!", "修改成功!"};
    private string[] strCorrect = new string[]{"Correct!", "输入正确!"};
    private string[] strError = new string[]{"Error!", "输入错误!"};
	private string[] strAccountPassword = new string[]{"Input Account Password", "请输入账户密码"};

    void Start()
    {
        preInputState = InputEx.inputEnable;
        mouseIcon = GameObject.Find("Canvas/mouse icon").GetComponent<RectTransform>();
        calcTitle = GameObject.Find("Canvas/Calc/Input/Title").GetComponent<Text>();
        calcContent = GameObject.Find("Canvas/Calc/Input/Content").GetComponent<Text>();
        calcPassword = GameObject.Find("Canvas/Calc/Input/Password").GetComponent<Text>();
		dlgCalc = GameObject.Find("Canvas/Calc");
        calcPassword.text = calcTitle.text = calcContent.text = string.Empty;
        InitMain();
    }

    void OnDestroy()
    {
        InputEx.inputEnable = preInputState;
    }

    void Update()
    {
        UpdateTimer();
        DetectInputEvents();
    }
    
    private void DetectInputEvents()
    {
        if (InputEx.GetInputDown())
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
                }
            }
            if (hit[idx].collider != null)
            {
                hit[idx].collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit[idx].collider.transform);
                downHitObject = hit[idx].collider.gameObject;
            }
            
            mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
        }
        else if (InputEx.GetInputUp())
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
            downHitObject = null;
        }
    }

	public void SettingDownEvent(Transform hitObject)
    {
        if (IsSettingDlgActived())
            return;

        if (preSelected != null)
        {
            Transform t = preSelected.FindChild("Image");
            if (t != null)
                t.gameObject.SetActive(false);
            SetAlpha(preSelected, 0);
        }
        SetAlpha(hitObject, 255);

        Transform img = hitObject.FindChild("Image");
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
            InitMain();
        }
        else if (string.Equals(hitObject.name, "save"))
        {
            SaveSetting();
        }
        else if (string.Equals(hitObject.name, "password"))
        {
            passwordMode = 1;
            InitPasswordDlg();
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
        SetAlpha(hitObject, 255);
    }

    public void MainUpEvent(Transform hitObject)
    {
        SetAlpha(hitObject, 0);
        string name = hitObject.name;
        if (string.Equals(name, "setting"))
            InitSetting();
        else if (string.Equals(name, "account"))
		{
			dlgCalc.SetActive(true);
			dlgCalc.transform.localPosition = new Vector3(100, 200, 0);
			calcTitle.text = strAccountPassword[GameData.GetInstance().language];
			passwordMode = 2;
		}
        else if (string.Equals(name, "exit"))
		{
			GameData.GetInstance().NextLevelName = Scenes.Main;
            Application.LoadLevel(Scenes.Loading);
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
        if (string.Equals(name, "system"))
        {
            passwordType = 1;
        }
        else if (string.Equals(name, "account"))
        {
            passwordType = 2;
        }
        int idx = GameData.GetInstance().language;
        dlgPassword.SetActive(false);
        SetCalcTitle(strPassword[idx], Color.black);
        SetCalcContent(string.Empty, Color.white);
    }

    private void InitMain()
    {
        menuMain.SetActive(true);
        menuSetting.SetActive(false);
        menuAccount.SetActive(false);
		dlgCalc.SetActive(false);
        SetLanguage(menuMain);
    }

    private void InitSetting()
    {
        menuMain.SetActive(false);
        menuSetting.SetActive(true);
        menuAccount.SetActive(false);
		dlgCalc.SetActive(true);
		dlgCalc.transform.localPosition = Vector3.zero;

        SetLanguage(menuSetting);

        GameData ga = GameData.GetInstance();
        string[] datas = new string[]{ga.betTimeLimit.ToString(), ga.coinToScore.ToString(), ga.baoji.ToString(), ga.gameDifficulty.ToString(), 
            ga.betChipValues[0].ToString(), ga.betChipValues[1].ToString(), ga.betChipValues[2].ToString().ToString(), ga.betChipValues[3].ToString(), ga.betChipValues[4].ToString(), ga.betChipValues[5].ToString(),
            ga.max36Value.ToString(), ga.max18Value.ToString(), ga.max12Value.ToString(), ga.max9Value.ToString(), ga.max6Value.ToString(), ga.max3Value.ToString(), ga.max2Value.ToString(),
            ga.couponsStart.ToString(), ga.couponsKeyinRatio.ToString() + "%", ga.couponsKeoutRatio.ToString(), ga.beginSessions.ToString(), ga.maxNumberOfFields.ToString()};

        Transform root = menuSetting.transform.FindChild("Valid Fields");
        if (root != null)
        {
            int count = root.childCount;
            for (int i = 0; i < count; ++i)
            {
                Transform str = root.GetChild(i).FindChild("Text");
                if (str != null)
                {
                    str.GetComponent<Text>().text = datas[i];
                }
            }
        }
    }

    private void InitAccount()
    {
		print("InitAccount");
    }

    private void InitPasswordDlg()
    {
        passwordType = 0;
		passwordPhase = 0;
        dlgPassword.SetActive(true);
        SetLanguage(dlgPassword);
    }

    private void SetLanguage(GameObject menu)
    {
        GameObject en = menu.transform.FindChild("EN").gameObject;
        GameObject cn = menu.transform.FindChild("CN").gameObject;
        if (GameData.GetInstance().language == 0)
        {
            en.SetActive(true);
            cn.SetActive(false);
        }
        else
        {
            en.SetActive(false);
            cn.SetActive(true);
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
            if (txtPassword.Length > 1)
                txtPassword = txtPassword.Substring(0, txtPassword.Length - 1);
            else
                txtPassword = string.Empty;
//            print(txtPassword);
        }
        SetCalcContent(text, Color.white);
    }

    private void CalcEnterEvent()
    {
        if (passwordMode == 1)
        {
			int idx = GameData.GetInstance().language;
			if (passwordPhase == 0)
			{
				if (string.Equals(txtPassword, GameData.GetInstance().systemPassword))
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
			else if (passwordPhase == 1)
			{
				passwordPhase = 2;
				txtCheckPassword = txtPassword;
				calcPassword.text = string.Empty;
				calcContent.text = strAgain[idx];
				calcContent.color = Color.red;
				txtPassword = null;
			}
			else if (passwordPhase == 2)
			{
				if (string.Equals(txtPassword, txtCheckPassword))
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
		else if (passwordMode == 2)
		{
			if (string.Equals(txtPassword, GameData.GetInstance().accountPassword))
			{
				InitAccount();
			}
			else
			{
				calcContent.text = strError[GameData.GetInstance().language];
				calcContent.color = Color.red;
				calcPassword.text = string.Empty;
				txtPassword = null;
			}
		}
        else
        {
            if (preSelected != null)
            {
                Transform target = preSelected.FindChild("Text");
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
        return dlgPassword.activeSelf || dlgYesNO.activeSelf;
    }

    private void SaveSetting()
    {
        List<int> values = new List<int>();
        Transform root = menuSetting.transform.FindChild("Valid Fields");
        if (root != null)
        {
            int count = root.childCount;
            for (int i = 0; i < count; ++i)
            {
                Transform str = root.GetChild(i).FindChild("Text");
                if (str != null)
                {
                    int value;
                    string content = str.GetComponent<Text>().text;
                    if (content.Contains("%"))
                        content = content.Substring(0, content.Length - 1);
                    if (int.TryParse(content, out value))
                    {
                        values.Add(value);
                    }
                }
            }

            GameData.GetInstance().betTimeLimit = values[0];
            GameData.GetInstance().coinToScore = values[1];
            GameData.GetInstance().baoji = values[2];
            GameData.GetInstance().gameDifficulty = values[3];
            GameData.GetInstance().betChipValues[0] = values[4];
            GameData.GetInstance().betChipValues[1] = values[5];
            GameData.GetInstance().betChipValues[2] = values[6];
            GameData.GetInstance().betChipValues[3] = values[7];
            GameData.GetInstance().betChipValues[4] = values[8];
            GameData.GetInstance().betChipValues[5] = values[9];
            GameData.GetInstance().max36Value = values[10];
            GameData.GetInstance().max18Value = values[11];
            GameData.GetInstance().max12Value = values[12];
            GameData.GetInstance().max9Value = values[13];
            GameData.GetInstance().max6Value = values[14];
            GameData.GetInstance().max3Value = values[15];
            GameData.GetInstance().max2Value = values[16];
            GameData.GetInstance().couponsStart = values[17];
            GameData.GetInstance().couponsKeyinRatio = values[18];
            GameData.GetInstance().couponsKeoutRatio = values[19];
            GameData.GetInstance().beginSessions = values[20];
            GameData.GetInstance().maxNumberOfFields = values[21];
            GameData.GetInstance().SaveSetting();
            int idx = GameData.GetInstance().language;
            ShowWarning(Notifies.saveSuccess[idx], true);
        }
    }

    private void ShowWarning(string str, bool autoDisappear)
    {
        if (warning != null && !warning.activeSelf)
        {
            warning.SetActive(true);
            warning.transform.FindChild("Text").GetComponent<Text>().text = str;
        }
        if (autoDisappear)
        {
            timerHideWarning = new Timer(1.5f, 0);
            timerHideWarning.Tick += HideWarning;
            timerHideWarning.Start();
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
    }

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
            if (value > 300000)
                value = 300000;
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
                 string.Equals(name, "chip4") || string.Equals(name, "chip5"))
        {
            if (value > 100000)
                value = 100000;
        }
        else if (string.Equals(name, "x36") || string.Equals(name, "x18") || string.Equals(name, "x12") || string.Equals(name, "x9") ||
                 string.Equals(name, "x6") || string.Equals(name, "x3") || string.Equals(name, "x2"))
        {
            if (value > 100000)
                value = 100000;
            else if (value < 1)
                value = 1;
        }
        else if (string.Equals(name, "card1"))
        {
            if (value > 100000)
                value = 100000;
            else if (value < 1)
                value = 1;
        }
        else if (string.Equals(name, "card2"))
        {
            if (value > 100)
                value = 100;
            else if (value < 1)
                value = 1;
        }
        else if (string.Equals(name, "card3"))
        {
            if (value > 100)
                value = 100;
            else if (value < 1)
                value = 1;
        }
        else if (string.Equals(name, "card4"))
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
    }
}
