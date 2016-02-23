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
	}

	void OnDestroy()
	{
		GameEventManager.Prompt -= Prompt;
	}

	private void Prompt(int promptId)
	{
		if (promptId == PromptId.PleaseBet)
			StartCoroutine(SetPrompt(0));
		else if (promptId == PromptId.NoMoreBet)
			StartCoroutine(SetPrompt(1));
	}

	private IEnumerator SetPrompt(int arrayIdx)
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
		iTween.MoveTo(currentPrompt.gameObject,
		              iTween.Hash("x", 0.0f, "time", 2.5f, "islocal", true, "easetype", iTween.EaseType.linear));
	}

	private void Disappear()
	{
		float width = currentPrompt.rectTransform.rect.width;
		float pivodX = -294.0f;
		Vector3 pos = currentPrompt.rectTransform.localPosition;
		pos.x = pivodX - width * 0.5f;
		iTween.MoveTo(currentPrompt.gameObject,
		              iTween.Hash("x", pos.x, "time", 0.5f, "islocal", true, "easetype", iTween.EaseType.linear));
	}
}
