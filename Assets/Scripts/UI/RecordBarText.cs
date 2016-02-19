using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecordBarText : MonoBehaviour
{
	public Image bar;

	public void Refresh(int persentage)
	{
        // 更新数字
        GetComponent<Text>().text = string.Format("{0}%", Mathf.Max(persentage, 0));

        // 更新数字位置
        float minAccount = 0.16f;
        float fillAccount = Mathf.Max(bar.fillAmount, minAccount);
		float parentWidth = transform.parent.GetComponent<RectTransform>().rect.width;
//		if (bar.gameObject.name == "Red")
//		{
//			transform.localPosition = new Vector3(-parentWidth * (1.0f - fillAccount) * 0.50f, 0, 0);
//		}
//		else if (bar.gameObject.name == "Blue")
//		{
//			transform.localPosition = new Vector3(parentWidth * (1.0f - fillAccount) * 0.50f, 0, 0);
//		}
//        else if (bar.gameObject.name == "Green")
		if (bar.gameObject.name == "Green")
		{
            float redFillAcount = Mathf.Max(transform.parent.parent.FindChild("Red").GetComponent<Image>().fillAmount, minAccount);
			float x = -parentWidth * (1.0f - redFillAcount) * 0.50f + fillAccount * 0.50f * parentWidth;
			transform.localPosition = new Vector3(x, 0, 0);
		}
	}
}
