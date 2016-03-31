using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardDlg : MonoBehaviour
{
	public GameObject cn;
	public GameObject en;
	public GameObject calc;
    public Transform mouseIcon;
	public Text calcTitle;
	public Text calcContent;
	public Text calcPassword;

	private bool previousInputState;
	private bool passwordMode;
	private int passwordType; // 0:none 1:system
	private string txtPassword; // Temp variable
	private Transform preSelected;
    private GameObject downHitObject;
	private bool hitDownOpt = false;
    
	private string[] strKeyin = new string[]{"Keyin", "上分"};
	private string[] strSysPassword = new string[]{"Input Sys-Password", "请输入系统密码"};
    private string[] strError = new string[]{"Error!", "密码错误!"};

	void OnEnable()
	{
		if (GameData.GetInstance().language == 0)
		{
			cn.SetActive(false);
			en.SetActive(true);
		}
		else
		{
			cn.SetActive(true);
			en.SetActive(false);
		}
		previousInputState = InputEx.inputEnable;
		InputEx.inputEnable = true;
		DisalbeCalc();
	}

	void OnDisenable()
	{
		InputEx.inputEnable = previousInputState;
	}

	private void DisalbeCalc()
	{
		calc.SetActive(false);
		if (cn.activeSelf)
		{
			int count = cn.transform.childCount;
			for (int i = 0; i < count; ++i)
				cn.transform.GetChild(i).gameObject.SetActive(false);
		}
		if (en.activeSelf)
		{
			int count = en.transform.childCount;
			for (int i = 0; i < count; ++i)
				en.transform.GetChild(i).gameObject.SetActive(false);
		}
	}

	private void EnableCalc()
	{
		calc.SetActive(true);
		if (GameData.GetInstance().language == 0)
		{
			if (en.activeSelf)
			{
				int count = en.transform.childCount;
				for (int i = 0; i < count; ++i)
					en.transform.GetChild(i).gameObject.SetActive(true);
			}
		}
		else
		{
			if (cn.activeSelf)
			{
				int count = cn.transform.childCount;
				for (int i = 0; i < count; ++i)
					cn.transform.GetChild(i).gameObject.SetActive(true);
			}
		}
	}

	public void FieldUpEvent(Transform hitObject)
	{
		SetAlpha(hitObject, 0);
		ClearCalc();
		passwordMode = false;
		passwordType = 0;
		txtPassword = null;
		if (string.Equals(hitObject.name, "exit"))
			gameObject.SetActive(false);
		else if (string.Equals(hitObject.name, "keyin"))
			Keyin();
		else if (string.Equals(hitObject.name, "keout"))
			Keout();
		else if (string.Equals(hitObject.name, "system"))
			System();
		else if (string.Equals(hitObject.name, "lastest10"))
			Last10();
		else if (string.Equals(hitObject.name, "account"))
			Account();
	}

	public void FieldDownEvent(Transform hitObject)
	{
		preSelected = hitObject;
		SetAlpha(hitObject, 255);
	}

	private void Keyin()
	{
		EnableCalc();
		int idx = GameData.GetInstance().language;
		SetCalcTitle(strKeyin[idx], Color.black);
	}

	private void Keout()
	{
		DisalbeCalc();
		GameEventManager.OnKeout();
	}

	private void System()
	{
		EnableCalc();
		passwordMode = true;
		passwordType = 1;
		int idx = GameData.GetInstance().language;
		SetCalcTitle(strSysPassword[idx], Color.black);
//		SetCalcTitle(TextDB.CardDlg_SysPassword[idx], Color.black);
	}

	private void Last10()
	{
        GameEventManager.OnChangeScene(Scenes.Last10);
        gameObject.SetActive(false);
	}

	private void Account()
	{
		DisalbeCalc();
        GameEventManager.OnChangeScene(Scenes.Account);
        gameObject.SetActive(false);
	}

	public void CalcDownEvent(Transform hitObject)
	{
		SetAlpha(hitObject, 255);
	}
	
	public void CalcUpEvent(Transform hitObject)
	{
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

	private void DelCalcContent()
	{
		Text target;
		if (passwordMode)
			target = calcPassword;
		else
			target = calcContent;
		string text = target.text;
		
		int length = text.Length;
		if (length > 1)
			text = text.Substring(0, length - 1);
		else if (length == 1)
		{
			if (passwordMode)
				text = string.Empty;
			else
				text = "0";
		}
		
		if (passwordMode)
		{
			if (txtPassword.Length > 1)
				txtPassword = txtPassword.Substring(0, txtPassword.Length - 1);
			else
				txtPassword = string.Empty;
			print(txtPassword);
		}
		SetCalcContent(text, Color.white);
	}

	private void CalcEnterEvent()
	{
		if (passwordMode)
		{
			if (passwordType == 1)
			{
                if (string.Equals(txtPassword, GameData.GetInstance().systemPassword))
                {
//                    print("sys p : " + GameData.GetInstance().systemPassword);
//                    print("txtpassword : " + txtPassword);
                    GameEventManager.OnChangeScene(Scenes.Backend);
                    gameObject.SetActive(false);
                }
                else
                {
                    int idx = GameData.GetInstance().language;
                    calcContent.text = strError[idx];
                    calcContent.color = Color.red;
                    calcPassword.text = string.Empty;
                }

                txtPassword = null;
			}
		}
		else
		{
			if (preSelected != null)
			{
				if (string.Equals(preSelected.name, "keyin"))
				{
                    int value;
                    if (int.TryParse(calcContent.text, out value))
                    {
                        GameEventManager.OnKeyin(value);
                    }
                    calcContent.text = string.Empty;
				}
			}
		}
	}

	private void AppendCalcContent(int num)
	{
        if (calcContent.color == Color.red)
            calcContent.text = string.Empty;

		if (passwordMode)
		{
			string text = calcPassword.text;
			if (text.Length < GameData.GetInstance().passwordLength)
			{
				text += "*";
				txtPassword += num.ToString();
//				print(txtPassword);
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

	private void SetCalcContent(string text, Color color)
	{
        if (string.IsNullOrEmpty(text))
        {
            calcPassword.text = string.Empty;
            calcContent.text = string.Empty;
        }
        else
        {
    		if (passwordMode)
    			calcPassword.text = text;
    		else
    			calcContent.text = text;
        }
		calcContent.color = color;
	}

	private void ClearCalc()
	{
		SetCalcContent(string.Empty, Color.white);
		SetCalcTitle(string.Empty, Color.black);
	}

    void Update()
    {
        DetectInputEvents();
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
            
            int idx = -1;
            if (hit.Length > 0)
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
            if (idx > -1 && hit[idx].collider != null)
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
}
