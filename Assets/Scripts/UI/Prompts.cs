using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Prompts : MonoBehaviour
{
	public Sprite[] promptImgs;
	private Image currentPrompt;
	void Start()
	{
		currentPrompt = GetComponent<Image>();
		GameEventManager.Prompt += Prompt;
		GameEventManager.ResultPrompt += ResultPrompt;
	}

	void OnDestroy()
	{
		GameEventManager.Prompt -= Prompt;
		GameEventManager.ResultPrompt -= ResultPrompt;
	}

	private void Prompt(int promptId)
	{
		StartCoroutine(SetPrompt(promptId));
	}

	private void ResultPrompt(int result)
	{
		StartCoroutine(SetPrompt(2, result));
	}

	private IEnumerator SetPrompt(int arrayIdx, int result = -1)
	{
		if (Mathf.Approximately(0, currentPrompt.rectTransform.localPosition.x))
		{
			Disappear();
			yield return new WaitForSeconds(0.5f);
		}
		if (GameData.GetInstance().language == 1)	// CN
			arrayIdx += promptImgs.Length / 2;
		currentPrompt.overrideSprite = promptImgs[arrayIdx];
		currentPrompt.SetNativeSize();
		float width = currentPrompt.rectTransform.rect.width;
		float pivodX = 323.0f;
		Vector3 pos = currentPrompt.rectTransform.localPosition;
		pos.x = pivodX + width * 0.5f;
		currentPrompt.rectTransform.localPosition = pos;
		if (result != -1)
			currentPrompt.transform.GetChild(0).GetComponent<Text>().text = result != 37 ? result.ToString() : "00";
		else
			currentPrompt.transform.GetChild(0).GetComponent<Text>().text = string.Empty;
		iTween.MoveTo(currentPrompt.gameObject,
//		              iTween.Hash("x", -120.0f, "time", 2.5f, "islocal", true, "easetype", iTween.EaseType.linear));
		              iTween.Hash("x", 15.0f, "time", 2.5f, "islocal", true, "easetype", iTween.EaseType.linear));
	}

	private void Disappear()
	{
		float width = currentPrompt.rectTransform.rect.width;
		float pivodX = -580.0f;
		Vector3 pos = currentPrompt.rectTransform.localPosition;
		pos.x = pivodX - width * 0.5f;
		iTween.MoveTo(currentPrompt.gameObject,
		              iTween.Hash("x", pos.x, "time", 0.5f, "islocal", true, "easetype", iTween.EaseType.linear));
	}
}
