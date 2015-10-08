using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecordBarText : MonoBehaviour
{
	private Image bar;

	void Start()
	{
		bar = transform.parent.GetComponent<Image>();
	}

	public void Refresh(int persentage)
	{
		if (persentage > 0)
			GetComponent<Text>().text = persentage.ToString() + "%";
		else
		{
			GetComponent<Text>().text = string.Empty;
			return;
		}

		if (bar == null)
			bar = transform.parent.GetComponent<Image>();

		float fillAccount = bar.fillAmount;
		float parentWidth = transform.parent.GetComponent<RectTransform>().rect.width;
		if (bar.gameObject.name == "Red")
		{
			transform.localPosition = new Vector3(-parentWidth * (1.0f - fillAccount) * 0.50f, 0, 0);
		}
		else if (bar.gameObject.name == "Blue")
		{
			transform.localPosition = new Vector3(parentWidth * (1.0f - fillAccount) * 0.50f, 0, 0);
		}
		else if (bar.gameObject.name == "Green")
		{
			float redFillAcount = transform.parent.parent.FindChild("Red").GetComponent<Image>().fillAmount;
			float x = -parentWidth * (1.0f - redFillAcount) * 0.50f + fillAccount * 0.50f * parentWidth;
			transform.localPosition = new Vector3(x, 0, 0);
		}
	}
}
