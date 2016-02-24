using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 显示压分区倍数
public class OddsTip : MonoBehaviour
{
	private Text txtTip;

	void Start()
	{
		txtTip = transform.GetChild(0).GetComponent<Text>();
		GameEventManager.OddsPrompt += ShowOdds;
		gameObject.SetActive(false);
	}
	
	void OnDestroy()
	{
		GameEventManager.OddsPrompt -= ShowOdds;
	}

	private void ShowOdds(int odds)
	{
		if (odds <= 0)
		{
			if (gameObject.activeSelf)
				gameObject.SetActive(false);
		}
		else
		{
			if (!gameObject.activeSelf) 
				gameObject.SetActive(true);
			txtTip.text = string.Format("x{0}", odds);
		}
	}
}
