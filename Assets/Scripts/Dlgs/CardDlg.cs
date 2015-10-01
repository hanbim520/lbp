using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardDlg : MonoBehaviour
{
	public GameObject cn;
	public GameObject en;
	public Text calcTitle;
	public Text calcContent;
	public Text calcPassword;
	private bool previousInputState;
	private bool passwordMode;
	private int passwordType; // 0:none 1:system
	private string txtPassword; // Temp variable
	private Transform preSelected;

	private string[] strKeyin = new string[]{"Keyin", "上分"};
	private string[] strkeout = new string[]{"Keout", "下分"};
	private string[] strSysPassword = new string[]{"Please Input System Password", "请输入系统密码"};

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
	}

	void OnDisenable()
	{
		InputEx.inputEnable = previousInputState;
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
		int idx = GameData.GetInstance().language;
		SetCalcTitle(strKeyin[idx], Color.black);
	}

	private void Keout()
	{
		int idx = GameData.GetInstance().language;
		SetCalcTitle(strkeout[idx], Color.white);
	}

	private void System()
	{
		if (GameData.GetInstance().deviceIndex > 1)
			return;

		passwordMode = true;
		passwordType = 1;
		int idx = GameData.GetInstance().language;
		SetCalcTitle(strSysPassword[idx], Color.black);
	}

	private void Last10()
	{

	}

	private void Account()
	{

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
				//"Backend"
			}
		}
		else
		{
			if (preSelected != null)
			{
				if (string.Equals(preSelected.name, "keyin"))
				{

				}
				else if (string.Equals(preSelected.name, "keout"))
				{
					// TODO: keout
				}
			}
		}
	}

	private void AppendCalcContent(int num)
	{
		if (passwordMode)
		{
			string text = calcPassword.text;
			if (text.Length < GameData.GetInstance().passwordLength)
			{
				text += "*";
				txtPassword += num.ToString();
				print(txtPassword);
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
		if (passwordMode)
			calcPassword.text = text;
		else
			calcContent.text = text;
		calcContent.color = color;
	}

	private void ClearCalc()
	{
		SetCalcContent(string.Empty, Color.white);
		SetCalcTitle(string.Empty, Color.black);
	}
}
