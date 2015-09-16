using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainUILogic : MonoBehaviour
{
	public GameObject setEN;
	public GameObject setCN;
	public GameObject fields37;
	public GameObject fields38;
	public GameObject backendTip;
	public GameObject chooseBetEffect;

	private GameObject displayClassic;
	private GameObject displayEllipse;
	private GameObject eraser;
	private GameObject betChipsRoot;
	private GameObject fieldChipsRoot;
	private RectTransform mouseIcon;
	private int curChipIdx = -1;

	void Start()
	{
		GameData.GetInstance().DefaultSetting();
		Init();
		SetLanguage();
		SetDisplay();
		SetBetChips();
	}

	private void Init()
	{
		eraser = GameObject.Find("Canvas/eraser");
		eraser.SetActive(false);
		mouseIcon = GameObject.Find("Canvas/mouse icon").GetComponent<RectTransform>();
		mouseIcon.localPosition = Vector3.zero;
		fieldChipsRoot = GameObject.Find("Canvas/FieldChipsRoot");
	}

	public void ChangeLanguage(Transform hitObject)
	{
		if (GameData.GetInstance().language == 0)		// EN
		{
			GameData.GetInstance().language = 1;
		}
		else if (GameData.GetInstance().language == 1)	// CN
		{
			GameData.GetInstance().language = 0;
		}
		GameData.GetInstance().SaveLanguage();
		SetLanguage();
	}

	public void SetLanguage()
	{
		if (GameData.GetInstance().language == 0)		// EN
		{
			if (setEN != null) setEN.SetActive(true);
			if (setCN != null) setCN.SetActive(false);
		}
		else if (GameData.GetInstance().language == 1)	// CN
		{
			if (setEN != null) setEN.SetActive(false);
			if (setCN != null) setCN.SetActive(true);
		}
	}

	public void ChangeDisplay()
	{
		if (GameData.GetInstance().displayType == 0)	// classic
		{
			GameData.GetInstance().displayType = 1;
		}
		else if (GameData.GetInstance().displayType == 1)	// ellipse
		{
			GameData.GetInstance().displayType = 0;
		}
		GameData.GetInstance().SaveDisplayType();
		SetDisplay();
	}

	public void SetDisplay()
	{
		if (GameData.GetInstance().maxNumberOfFields == 37)
		{
			if (!fields37.activeSelf) fields37.SetActive(true);
			if (fields38.activeSelf) fields38.SetActive(false);
			if (displayClassic == null) displayClassic = GameObject.Find("Canvas/37 Fields/Classic");
			if (displayEllipse == null) displayEllipse = GameObject.Find("Canvas/37 Fields/Ellipse");
		}
		else if (GameData.GetInstance().maxNumberOfFields == 38)
		{
			if (fields37.activeSelf) fields37.SetActive(false);
			if (!fields38.activeSelf) fields38.SetActive(true);
			if (displayClassic == null) displayClassic = GameObject.Find("Canvas/38 Fields/Classic");
			if (displayEllipse == null) displayEllipse = GameObject.Find("Canvas/38 Fields/Ellipse");
		}

		if (GameData.GetInstance().displayType == 0)	// classic
		{
			if (displayClassic != null) displayClassic.SetActive(true);
			if (displayEllipse != null) displayEllipse.SetActive(false);
		}
		else if (GameData.GetInstance().displayType == 1)	// ellipse
		{
			if (displayClassic != null) displayClassic.SetActive(false);
			if (displayEllipse != null) displayEllipse.SetActive(true);
		}

		if (fieldChipsRoot.transform.childCount > 0)
		{
			int childCount = fieldChipsRoot.transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				Transform child = fieldChipsRoot.transform.GetChild(i);
				string betValue = child.GetChild(0).GetComponent<Text>().text;
				string name = child.name;
				if (string.Equals(name.Substring(0, 1), "e"))
					name = name.Substring(1);


				Destroy(child.gameObject);
			}
		}
	}

	public void SetBetChips()
	{
		betChipsRoot = GameObject.Find("BetChips");
		string path = "Bet Chips/";
		float y = -492.0f;
		float start = 0.0f, dist = 0.0f;
		int num = GameData.GetInstance().maxNumberOfChips;
		if (num == 6)
		{
			start = -644.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 5)
		{
			start = -594.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 4)
		{
			start = -544.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 3)
		{
			start = -494.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 2)
		{
			start = -544.0f;
			dist = 300;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 1)
		{
			start = -394.0f;
		}

		for (int i = 0; i < num; ++i)
		{
			Object prefab = (Object)Resources.Load(path + "BetChip" + i);
			GameObject betChip = (GameObject)Instantiate(prefab);
			betChip.transform.SetParent(betChipsRoot.transform);
			betChip.transform.localPosition = new Vector3(start + i * dist, y, 0);
			betChip.transform.localScale = Vector3.one;
			betChip.GetComponent<ButtonEvent>().receiver = gameObject;
			betChip.GetComponent<ButtonEvent>().inputUpEvent = "ChipButtonEvent";
			prefab = null;
		}
	}

	public void ClearEvent(Transform hitObject)
	{
		if (fieldChipsRoot.transform.childCount == 0)
			return;

		if (eraser != null) 
		{
			eraser.SetActive(true);
			eraser.transform.localPosition = hitObject.localPosition;
		}
		if (mouseIcon != null) mouseIcon.gameObject.SetActive(false);
	}

	public void ClearAllEvent(Transform hitObject)
	{
		if (fieldChipsRoot.transform.childCount == 0)
			return;

		fieldChipsRoot.transform.DetachChildren();
		GameEventManager.OnClearAll();
	}

	public void RepeatEvent()
	{
		
	}

	public void FieldDownEvent(Transform hitObject)
	{
		if (eraser.activeSelf || curChipIdx == -1)
			return;


	}

	public void FieldClickEvent(Transform hitObject)
	{
		if (eraser.activeSelf)
		{
			Destroy(fieldChipsRoot.transform.FindChild(hitObject.name).gameObject);
			GameEventManager.OnClear(hitObject.name);
			eraser.SetActive(false);
			mouseIcon.gameObject.SetActive(true);
			return;
		}

		if (curChipIdx == -1)
			return;

        string strField = hitObject.name;
		int bet = GameData.GetInstance().betChipValues[curChipIdx];
        // Ellipse
        if (string.Equals(strField.Substring(0, 1), "e"))
            strField = strField.Substring(1);
		GameEventManager.OnFieldClick(strField, bet);

		string prefabPath = "BigChip/BC";
		if (string.Equals(hitObject.parent.name, "Classic"))
			prefabPath = "SmallChip/SC";
		Object prefab = (Object)Resources.Load(prefabPath + curChipIdx);
		GameObject chip = (GameObject)Instantiate(prefab);
		chip.transform.SetParent(fieldChipsRoot.transform);
		chip.transform.localPosition = betChipsRoot.transform.Find("BetChip" + curChipIdx + "(Clone)").localPosition;
		chip.transform.localScale = Vector3.one;
		chip.name = hitObject.name + " temp";
		prefab = null;

		chip.transform.GetChild(0).GetComponent<Text>().text = bet.ToString();

		Vector3 targetPos = new Vector3(hitObject.localPosition.x * hitObject.parent.localScale.x,
		                                hitObject.localPosition.y * hitObject.parent.localScale.y,
		                                0);
		iTween.MoveTo(chip, iTween.Hash("time", 0.5, "islocal", true, "position", targetPos, 
		                                "oncomplete", "FieldChipMoveComplete", "oncompletetarget", gameObject, "oncompleteparams", hitObject.name + ":" + bet.ToString()));
		// TODO:Choose effect
	}

	private void FieldChipMoveComplete(string param)
	{
		char[] separator = {':'};
		string[] str = param.Split(separator);
		Transform old = fieldChipsRoot.transform.FindChild(str[0]);
		Transform newOne = fieldChipsRoot.transform.FindChild(str[0] + " temp");

		int betVal;
		if (old != null)
		{
			// Change bet text
			if (int.TryParse(old.GetChild(0).GetComponent<Text>().text, out betVal))
			{
				betVal = int.Parse(str[1]) + betVal;
				newOne.GetChild(0).GetComponent<Text>().text = betVal.ToString();
			}
			Destroy(old.gameObject);
		}
		newOne.name = str[0];
//		print("FieldChipMoveComplete:" + fieldName);
	}

	public void ChipButtonEvent(Transform hitObject)
	{
		int idx;
		if (int.TryParse(hitObject.name.Substring(7, 1), out idx))
			curChipIdx = idx;
		else
			curChipIdx = -1;
		
		if (curChipIdx == -1)
		{
			chooseBetEffect.SetActive(false);
			return;
		}

		if (!chooseBetEffect.activeSelf) chooseBetEffect.SetActive(true);
		chooseBetEffect.transform.localPosition = hitObject.localPosition + new Vector3(0, 10f, 0);
	}

	void Update()
	{
		DetectInputEvents();
	}

	private void DetectInputEvents()
	{
		if (InputEx.GetInputDown())
		{
			Vector2 pos;
			InputEx.InputDownPosition(out pos);

			float sx, sy;
			Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
			RaycastHit2D hit = Physics2D.Raycast(new Vector2(sx, sy), Vector2.zero);
			if (hit.collider != null)
			{
				hit.collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit.collider.transform);
			}

			mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
		}
		else if (InputEx.GetInputUp())
		{
			Vector2 pos;
			InputEx.InputUpPosition(out pos);
			
	        float sx, sy;
	        Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
	        RaycastHit2D hit = Physics2D.Raycast(new Vector2(sx, sy), Vector2.zero);
	        if (hit.collider != null)
	        {
				hit.collider.gameObject.GetComponent<ButtonEvent>().OnInputUp(hit.collider.transform);
	        }

			mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);}
	}
}
