using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardDlg : MonoBehaviour
{
	public GameObject cn;
	public GameObject en;
	private bool previousInputState;

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
		Image imgField = hitObject.GetComponent<Image>();
		Color c = imgField.color;
		c.a = 0;
		imgField.color = c;
		if (string.Equals(hitObject.name, "exit"))
			hitObject.parent.parent.gameObject.SetActive(false);
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
		Image imgField = hitObject.GetComponent<Image>();
		Color c = imgField.color;
		c.a = 255;
		imgField.color = c;
	}

	private void Keyin()
	{

	}

	private void Keout()
	{

	}

	private void System()
	{
		if (GameData.GetInstance().deviceIndex > 1)
			return;

	}

	private void Last10()
	{

	}

	private void Account()
	{

	}
}
