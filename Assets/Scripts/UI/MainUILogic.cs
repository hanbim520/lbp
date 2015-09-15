using UnityEngine;
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
	private RectTransform mouseIcon;
	private int curChipIdx = 0;

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
			fields37.SetActive(true);
			fields38.SetActive(false);
			displayClassic = GameObject.Find("Canvas/37 Fields/Classic");
			displayEllipse = GameObject.Find("Canvas/37 Fields/Ellipse");
		}
		else if (GameData.GetInstance().maxNumberOfFields == 38)
		{
			fields37.SetActive(false);
			fields38.SetActive(true);
			displayClassic = GameObject.Find("Canvas/38 Fields/Classic");
			displayEllipse = GameObject.Find("Canvas/38 Fields/Ellipse");
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
	}

	public void SetBetChips()
	{
		GameObject root = GameObject.Find("BetChips");
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
			betChip.transform.SetParent(root.transform);
			betChip.transform.localPosition = new Vector3(start + i * dist, y, 0);
			betChip.transform.localScale = Vector3.one;
			betChip.GetComponent<ButtonEvent>().receiver = gameObject;
			betChip.GetComponent<ButtonEvent>().inputUpEvent = "ChipButtonEvent";
			prefab = null;
		}
	}

	public void ClearEvent()
	{
		if (eraser != null) eraser.SetActive(true);
	}

	public void ClearAllEvent()
	{

	}

	public void RepeatEvent()
	{
		
	}

	public void FieldClickEvent(Transform hitObject)
	{

	}

	public void ChipButtonEvent(Transform hitObject)
	{
		if (!chooseBetEffect.activeSelf) chooseBetEffect.SetActive(true);
		chooseBetEffect.transform.localPosition = hitObject.localPosition + new Vector3(0, 10f, 0);

		int idx;
		if (int.TryParse(hitObject.name.Substring(7, 1), out idx))
			curChipIdx = idx;
		else
			curChipIdx = 0;
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
