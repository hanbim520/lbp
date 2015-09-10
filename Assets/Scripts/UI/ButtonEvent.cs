using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonEvent : MonoBehaviour
{
	public GameObject receiver;
	public string inputUpEvent;
	public string inputDownEvent;
	public Sprite normalSprite;
	public Sprite pressedSprite;

	public void OnInputUp(Transform hitObject)
	{
		if (receiver != null && !string.IsNullOrEmpty(inputUpEvent))
		{
			receiver.SendMessage(inputUpEvent, hitObject);
		}
		if (normalSprite != null)
		{
			hitObject.GetComponent<Image>().sprite = normalSprite;
		}
	}

	public void OnInputDown(Transform hitObject)
	{
		if (receiver != null && !string.IsNullOrEmpty(inputDownEvent))
		{
			receiver.SendMessage(inputDownEvent, hitObject);
		}
		if (pressedSprite != null)
		{
			hitObject.GetComponent<Image>().sprite = pressedSprite;
		}
	}
}
