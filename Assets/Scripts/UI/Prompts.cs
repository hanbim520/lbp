using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Prompts : MonoBehaviour
{
	public Sprite[] promptImgs;
	public Image secondPartImg;

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

	private void Prompt(PromptId promptId, int result)
	{
		StartCoroutine(SetPrompt(promptId, result));
	}

	private IEnumerator SetPrompt(PromptId promptId, int result = -1)
	{
		int arrayIdx = (int)promptId;
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
		float pivodX = 323.0f; // 边框的右侧
		Vector3 pos = currentPrompt.rectTransform.localPosition;
		pos.x = pivodX + width * 0.5f;
		currentPrompt.rectTransform.localPosition = pos;
		float destPosX = 15.0f;
		float moveDuration = 2.5f;
		if (result != -1)
		{
			Text content = currentPrompt.transform.GetChild(0).GetComponent<Text>();
			Vector3 contentPos = content.transform.localPosition;
			if (promptId == PromptId.Result)				// 球号
			{
				content.text = result != 37 ? result.ToString() : "00";
				contentPos.x = 211;

				float fontWidth = 36f;
				destPosX -= Utils.GetNumLength(result) * fontWidth / 2;
				secondPartImg.gameObject.SetActive(false);
			}
			else if (promptId == PromptId.Jackpot)			// 彩金玩法提示
			{
				secondPartImg.gameObject.SetActive(true);
				content.text = result.ToString();
				if (GameData.GetInstance().language == 0)	// EN
				{
					contentPos.x = 217;
				}
				else										// CN
				{
					contentPos.x = 143;
				}
				moveDuration = 2.5f;

				secondPartImg.overrideSprite = promptImgs[arrayIdx + 1];
				secondPartImg.SetNativeSize();
				float fontWidth = 36f;
				float space = 7f;							// 数字左右两侧与文字的间隔
				Vector3 spImgPos = secondPartImg.transform.localPosition;
				// Text的宽度可以装3个数字
				spImgPos.x = contentPos.x + ((float)Utils.GetNumLength(result) - 2.5f) * fontWidth + space * 2 + secondPartImg.rectTransform.rect.width / 2;
				secondPartImg.transform.localPosition = spImgPos;
				float leftPivodX = -296.0f;	// 边框的左侧
				destPosX = leftPivodX - (0.5f * currentPrompt.rectTransform.rect.width + Utils.GetNumLength(result) * fontWidth + secondPartImg.rectTransform.rect.width + space * 2);
				moveDuration = 10.0f;
			}
			content.transform.localPosition = contentPos;
		}
		else
		{
			currentPrompt.transform.GetChild(0).GetComponent<Text>().text = string.Empty;
			secondPartImg.gameObject.SetActive(false);
		}
		iTween.MoveTo(currentPrompt.gameObject,
		              iTween.Hash("x", destPosX, "time", moveDuration, "islocal", true, "easetype", iTween.EaseType.linear));
	}

	private void Disappear()
	{
		float width = currentPrompt.rectTransform.rect.width;
		float pivodX = -296.0f;	// 边框的左侧
		Vector3 pos = currentPrompt.rectTransform.localPosition;
		pos.x = pivodX - width * 0.5f;
		iTween.MoveTo(currentPrompt.gameObject,
		              iTween.Hash("x", pos.x, "time", 0.5f, "islocal", true, "easetype", iTween.EaseType.linear));
	}
}
