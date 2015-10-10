using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainUILogic : MonoBehaviour
{
    public GameLogic gameLogic;
	public GameObject setEN;
	public GameObject setCN;
	public GameObject fields37;
	public GameObject fields38;
	public GameObject backendTip;
	public GameObject chooseBetEffect;
	public GameObject cardEffect;
	public GameObject dlgWarning;
    public GameObject dlgCard;
	public GameObject[] cardExplains;

	public int CurChipIdx
	{
		get { return curChipIdx; }
	}

    private Text lblCredit;
    private Text lblWin;
    private Text lblBet;
	private Text lblRemember;

	private GameObject displayClassic;
	private GameObject displayEllipse;
	private GameObject eraser;
	private GameObject betChipsRoot;
	private GameObject fieldChipsRoot;
	private GameObject countdown;
	private RectTransform mouseIcon;
	private int curChipIdx = -1;
	private int timeLimit;
    private GameObject downHitObject;
    private List<Transform> lightEffects = new List<Transform>();
	private Transform flashObject;

	void Start()
	{
		Init();
		SetLanguage();
		SetDisplay();
		SetBetChips();
        RegisterEvents();
	}

    void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
		GameEventManager.CloseGate += StopFlash;
    }

    private void UnregisterEvents()
    {
		GameEventManager.CloseGate -= StopFlash;
    }

	private void Init()
	{
		eraser = GameObject.Find("Canvas/eraser");
		eraser.SetActive(false);
		mouseIcon = GameObject.Find("Canvas/mouse icon").GetComponent<RectTransform>();
		mouseIcon.localPosition = Vector3.zero;
		fieldChipsRoot = GameObject.Find("Canvas/FieldChipsRoot");
		countdown = GameObject.Find("Canvas/Countdown");
        lblCredit = GameObject.Find("Canvas/Credit/Credit").GetComponent<Text>();
        lblWin = GameObject.Find("Canvas/Credit/Win").GetComponent<Text>();
        lblBet = GameObject.Find("Canvas/Credit/Bet").GetComponent<Text>();
		lblRemember = GameObject.Find("Canvas/Credit/Remember").GetComponent<Text>();

		countdown.transform.FindChild("Text").GetComponent<Text>().text = GameData.GetInstance().betTimeLimit.ToString();
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
		// 设置优惠卡效果
		if (GameData.GetInstance().IsCardMode != CardMode.NO)
		{
			cardExplains[0].SetActive(false);
			cardExplains[1].SetActive(false);
			if (cardEffect != null) cardEffect.SetActive(true);
			if (setEN.activeSelf) setEN.transform.GetChild(4).GetChild(0).gameObject.SetActive(true);
			if (setCN.activeSelf) setCN.transform.GetChild(4).GetChild(0).gameObject.SetActive(true);
		}
		else
		{
			cardExplains[0].SetActive(false);
			cardExplains[1].SetActive(false);
			if (cardEffect != null) cardEffect.SetActive(false);
			if (setEN.activeSelf) setEN.transform.GetChild(4).GetChild(0).gameObject.SetActive(false);
			if (setCN.activeSelf) setCN.transform.GetChild(4).GetChild(0).gameObject.SetActive(false);
		}
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
		ChangeFlash();
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
			Dictionary<string, int> betFields = new Dictionary<string, int>();
			for (int i = 0; i < childCount; ++i)
			{
				Transform child = fieldChipsRoot.transform.GetChild(i);
				string betValue = child.GetChild(0).GetComponent<Text>().text;
				string name = child.name;
				if (string.Equals(name.Substring(0, 1), "e"))
					name = name.Substring(1);

				if (betFields.ContainsKey(name))
					betFields[name] += int.Parse(betValue);
				else
					betFields.Add(name, int.Parse(betValue));
			}

			foreach (Transform t in fieldChipsRoot.transform)
				Destroy(t.gameObject);

			if (displayClassic.activeSelf)
			{
				string rootPath = "Canvas/38 Fields/Classic/Valid Fields";
				if (GameData.GetInstance().maxNumberOfFields == 37)
				{
					rootPath = "Canvas/37 Fields/Classic/Valid Fields";
				}
				GameObject root = GameObject.Find(rootPath);
				if (root != null)
				{
					string prefabPath = "BigChip/BC" + curChipIdx;
					foreach (KeyValuePair<string, int> item in betFields)
					{
						Transform target = root.transform.FindChild(item.Key);
						if (target != null)
						{
							Object prefab = (Object)Resources.Load(prefabPath);
							GameObject chip = (GameObject)Instantiate(prefab);
							chip.transform.SetParent(fieldChipsRoot.transform);
							chip.transform.localScale = Vector3.one;
							prefab = null;
							Vector3 targetPos = new Vector3(target.localPosition.x * target.parent.localScale.x,
							                                target.localPosition.y * target.parent.localScale.y,
							                                0);
							chip.transform.localPosition = targetPos;
							chip.transform.GetChild(0).GetComponent<Text>().text = item.Value.ToString();
							chip.name = item.Key;
						}
					}
				}
			}
			else
			{
				string vfRootPath = "Canvas/38 Fields/Ellipse/Valid Fields";
				string ceRootPath = "Canvas/38 Fields/Ellipse/Choose Effect";
				if (GameData.GetInstance().maxNumberOfFields == 37)
				{
					vfRootPath = "Canvas/37 Fields/Ellipse/Valid Fields";
					ceRootPath = "Canvas/37 Fields/Ellipse/Choose Effect";
				}
				GameObject vfRoot = GameObject.Find(vfRootPath);
				GameObject ceRoot = GameObject.Find(ceRootPath);
				//Choose Effect
				if (vfRoot != null && ceRoot != null)
				{
					foreach (KeyValuePair<string, int> item in betFields)
					{
						string prefabPath = "SmallChip/SC" + curChipIdx;
						Transform target;
						int fieldName;
						string name = item.Key;
						if (int.TryParse(item.Key, out fieldName) || string.Equals(item.Key, "00"))
						{
							prefabPath = "BigChip/BC" + curChipIdx;
							name = "e" + name;
							target = ceRoot.transform.FindChild(name);
						}
						else
						{
							target = vfRoot.transform.FindChild(item.Key);
						}

						if (target != null)
						{
							Object prefab = (Object)Resources.Load(prefabPath);
							GameObject chip = (GameObject)Instantiate(prefab);
							chip.transform.SetParent(fieldChipsRoot.transform);
							chip.transform.localScale = Vector3.one;
							prefab = null;
							Vector3 targetPos = new Vector3(target.localPosition.x * target.parent.localScale.x,
							                                target.localPosition.y * target.parent.localScale.y,
							                                0);
							chip.transform.localPosition = targetPos;
							chip.transform.GetChild(0).GetComponent<Text>().text = item.Value.ToString();
							chip.name = name;
						}
					}
				}
			}
		}
	}

	public void SetBetChips()
	{
		betChipsRoot = GameObject.Find("BetChips");
		string path = "Bet Chips/";
		float y = -508.0f;
		float start = 0.0f, dist = 0.0f;
		int num = GameData.GetInstance().maxNumberOfChips;
		if (num == 6)
		{
			start = -577.0f;
			dist = 93;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 5)
		{
			start = -540.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 4)
		{
			start = -540.0f;
			dist = 130;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 3)
		{
			start = -500.0f;
			dist = 160;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 2)
		{
			start = -420.0f;
			dist = 160;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 1)
		{
			start = -330.0f;
		}

		int betChipsNum = GameData.GetInstance().betChipValues.Count;
		for (int i = 0, j = 0; i < betChipsNum; ++i)
		{
			int value = GameData.GetInstance().betChipValues[i];
			if (value > 0)
			{
				Object prefab = (Object)Resources.Load(path + "BetChip" + i);
				GameObject betChip = (GameObject)Instantiate(prefab);
				betChip.transform.SetParent(betChipsRoot.transform);
				betChip.transform.localPosition = new Vector3(start + j * dist, y, 0);
				++j;
				betChip.transform.localScale = Vector3.one;
				betChip.GetComponent<ButtonEvent>().receiver = gameObject;
				betChip.GetComponent<ButtonEvent>().inputUpEvent = "ChipButtonEvent";
				betChip.transform.GetChild(0).GetComponent<Text>().text = value.ToString();
				prefab = null;
			}
		}
	}

	public void ClearEvent(Transform hitObject)
	{
		if (fieldChipsRoot.transform.childCount == 0 || gameLogic.LogicPhase != GamePhase.Countdown)
			return;

		if (eraser != null) 
		{
			eraser.SetActive(true);
			eraser.transform.localPosition = hitObject.localPosition;
		}
		if (mouseIcon != null) mouseIcon.gameObject.SetActive(false);
	}

	// 清除桌面筹码 并返还给玩家
	public void ClearAllEvent(Transform hitObject)
	{
		if (fieldChipsRoot.transform.childCount == 0 || gameLogic.LogicPhase != GamePhase.Countdown)
			return;

        foreach (Transform child in fieldChipsRoot.transform)
            Destroy(child.gameObject);
		GameEventManager.OnClearAll();
	}

	// 清除桌面筹码 不返还给玩家
	public void CleanAll()
	{
		if (fieldChipsRoot.transform.childCount == 0)
			return;
		
		foreach (Transform child in fieldChipsRoot.transform)
			Destroy(child.gameObject);
		GameEventManager.OnCleanAll();
	}

	public void RepeatEvent()
	{
        int count = GameData.GetInstance().betRecords.Count;
		if (count == 0 || gameLogic.LogicPhase != GamePhase.Countdown)
            return;

        BetRecord lastRecord = GameData.GetInstance().betRecords[count - 1];
        if (lastRecord.bet > gameLogic.totalCredits)
            return;

        if (displayClassic.activeSelf)
        {
            string rootPath = "Canvas/38 Fields/Classic/Valid Fields";
            if (GameData.GetInstance().maxNumberOfFields == 37)
            {
                rootPath = "Canvas/37 Fields/Classic/Valid Fields";
            }
            GameObject root = GameObject.Find(rootPath);
            string prefabPath = "BigChip/BC0";
            if (root != null)
            {
				int betValue = 0;
				ClearAllEvent(null);
                foreach (BetInfo info in lastRecord.bets)
                {
                    if (info.betField == "00" && fields37.activeSelf)
                        continue;

                    Transform target = root.transform.FindChild(info.betField);
                    if (target != null)
                    {
                        Object prefab = (Object)Resources.Load(prefabPath);
                        GameObject chip = (GameObject)Instantiate(prefab);
                        chip.transform.SetParent(fieldChipsRoot.transform);
                        chip.transform.localScale = Vector3.one;
                        prefab = null;
                        Vector3 targetPos = new Vector3(target.localPosition.x * target.parent.localScale.x,
                                                        target.localPosition.y * target.parent.localScale.y,
                                                        0);
                        chip.transform.localPosition = targetPos;
                        chip.transform.GetChild(0).GetComponent<Text>().text = info.betValue.ToString();
                        chip.name = info.betField;
						betValue += info.betValue;
						gameLogic.betFields.Add(info.betField, info.betValue);
                    }
                }
				gameLogic.totalCredits -= betValue;
				gameLogic.currentBet += betValue;
				RefreshLblCredits(gameLogic.totalCredits.ToString());
				RefreshLblBet(gameLogic.currentBet.ToString());
            }
        }
        else
        {
            string vfRootPath = "Canvas/38 Fields/Ellipse/Valid Fields";
            string ceRootPath = "Canvas/38 Fields/Ellipse/Choose Effect";
            if (GameData.GetInstance().maxNumberOfFields == 37)
            {
                vfRootPath = "Canvas/37 Fields/Ellipse/Valid Fields";
                ceRootPath = "Canvas/37 Fields/Ellipse/Choose Effect";
            }
            GameObject vfRoot = GameObject.Find(vfRootPath);
            GameObject ceRoot = GameObject.Find(ceRootPath);
            //Choose Effect
            if (vfRoot != null && ceRoot != null)
            {
				int betValue = 0;
				ClearAllEvent(null);
                foreach (BetInfo info in lastRecord.bets)
                {
                    if (info.betField == "00" && fields37.activeSelf)
                        continue;

                    string prefabPath = "SmallChip/SC0";
                    Transform target;
                    int fieldName;
                    string name = info.betField;
                    if (int.TryParse(info.betField, out fieldName) || string.Equals(info.betField, "00"))
                    {
                        prefabPath = "BigChip/BC0";
                        name = "e" + name;
                        target = ceRoot.transform.FindChild(name);
                    }
                    else
                    {
                        target = vfRoot.transform.FindChild(info.betField);
                    }
                    
                    if (target != null)
                    {
                        Object prefab = (Object)Resources.Load(prefabPath);
                        GameObject chip = (GameObject)Instantiate(prefab);
                        chip.transform.SetParent(fieldChipsRoot.transform);
                        chip.transform.localScale = Vector3.one;
                        prefab = null;
                        Vector3 targetPos = new Vector3(target.localPosition.x * target.parent.localScale.x,
                                                        target.localPosition.y * target.parent.localScale.y,
                                                        0);
                        chip.transform.localPosition = targetPos;
                        chip.transform.GetChild(0).GetComponent<Text>().text = info.betValue.ToString();
                        chip.name = name;
						betValue += info.betValue;
						gameLogic.betFields.Add(info.betField, info.betValue);
                    }
                }
				gameLogic.totalCredits -= betValue;
				gameLogic.currentBet += betValue;
				RefreshLblCredits(gameLogic.totalCredits.ToString());
				RefreshLblBet(gameLogic.currentBet.ToString());
            }
        }
    }

	// 优惠卡按钮
	public void CardButtonEvent(Transform hitObject)
	{
		if (GameData.GetInstance().IsCardMode == CardMode.YES)
		{
			return;
		}

		if (GameData.GetInstance().IsCardMode == CardMode.NO)
		{
			GameData.GetInstance().IsCardMode = CardMode.Ready;
			// Explain
			if (GameData.GetInstance().language == 0)
			{
				cardExplains[0].SetActive(true);
				cardExplains[1].SetActive(false);
			}
			else
			{
				cardExplains[0].SetActive(false);
				cardExplains[1].SetActive(true);
			}
			// Effect
			if (cardEffect != null) cardEffect.SetActive(true);
			// Blue button
			hitObject.GetChild(0).gameObject.SetActive(true);
			// Red credit
			lblCredit.color = Color.red;
		}
		else if (GameData.GetInstance().IsCardMode == CardMode.Ready)
		{
			GameData.GetInstance().IsCardMode = CardMode.NO;
			// Disable explain
			cardExplains[0].SetActive(false);
			cardExplains[1].SetActive(false);
			// Disable effect
			if (cardEffect != null) cardEffect.SetActive(false);
			// Normal button
			hitObject.GetChild(0).gameObject.SetActive(false);
			// White credit
			lblCredit.color = Color.white;
			// Clear remember credits
			lblRemember.text = string.Empty;
		}
	}

	public void RecoverCardMode()
	{
		// Explain
		if (GameData.GetInstance().language == 0)
		{
			// Blue button
			GameObject.Find("Canvas/Buttons/EN/Card EN").transform.GetChild(0).gameObject.SetActive(true);
			cardExplains[0].SetActive(true);
			cardExplains[1].SetActive(false);
		}
		else
		{
			// Blue button
			GameObject.Find("Canvas/Buttons/CN/Card CN").transform.GetChild(0).gameObject.SetActive(true);
			cardExplains[0].SetActive(false);
			cardExplains[1].SetActive(true);
		}
		// Effect
		if (cardEffect != null) cardEffect.SetActive(true);
		// Red credit
		lblCredit.color = Color.red;
	}

	public void DisableCardMode()
	{
		GameData.GetInstance().IsCardMode = CardMode.NO;
		// Normal button
		GameObject.Find("Canvas/Buttons/EN/Card EN").transform.GetChild(0).gameObject.SetActive(false);
		GameObject.Find("Canvas/Buttons/CN/Card CN").transform.GetChild(0).gameObject.SetActive(false);
		// Disable explain
		cardExplains[0].SetActive(false);
		cardExplains[1].SetActive(false);
		// Disable effect
		if (cardEffect != null) cardEffect.SetActive(false);
		// White credit
		lblCredit.color = Color.white;
		// Clear remember credits
		lblRemember.text = string.Empty;
	}

	public void CardExplainUpEvent(Transform hitObject)
	{
		hitObject.gameObject.SetActive(false);
	}

	public void FieldDownEvent(Transform hitObject)
	{
		if (eraser.activeSelf || curChipIdx == -1 || 
		    gameLogic.LogicPhase != GamePhase.Countdown)
			return;

        if (string.Equals(hitObject.name.Substring(0, 1), "e"))
        {
            lightEffects.Add(hitObject);
        }
        else
        {
            Transform effectRoot = hitObject.parent.parent.FindChild("Choose Effect");
            if (effectRoot != null)
            {
                char[] separater = {'-'};
                string[] names = hitObject.name.Split(separater);
                foreach (string str in names)
                {
                    Transform effect = effectRoot.FindChild(str);
                    if (effect != null)
                    {
                        lightEffects.Add(effect);
                    }
                }
            }
        }
        foreach (Transform t in lightEffects)
        {
            Color c = t.GetComponent<Image>().color;
            c.a = 255;
            t.GetComponent<Image>().color = c;
        }
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

		// Clear light effects
		foreach (Transform t in lightEffects)
		{
			Color c = t.GetComponent<Image>().color;
			c.a = 0;
			t.GetComponent<Image>().color = c;
		}
		lightEffects.Clear();

		if (curChipIdx == -1 || 
		    gameLogic.LogicPhase != GamePhase.Countdown ||
		    GameData.GetInstance().IsCardMode == CardMode.Ready ||
		    gameLogic.IsLock)
			return;

        string strField = hitObject.name;
		int bet = GameData.GetInstance().betChipValues[curChipIdx];
        // Ellipse
        if (string.Equals(strField.Substring(0, 1), "e"))
        {
            strField = strField.Substring(1);
        }
		bet = GameEventManager.OnFieldClick(strField, bet);
		if (bet <= 0)
			return;

		string prefabPath = "BigChip/BC";
        if (string.Equals(hitObject.parent.name, "Valid Fields") && 
            string.Equals(hitObject.parent.parent.name, "Ellipse"))
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
	}

	public void ChipButtonEvent(Transform hitObject)
	{
		if (gameLogic.LogicPhase != GamePhase.Countdown)
			return;

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
        if (!IsDlgActived())
		    DetectInputEvents();
	}

	private void DetectInputEvents()
	{
		if (InputEx.GetInputDown())
		{
			Vector2 pos;
			InputEx.InputDownPosition(out pos);
			if (pos == new Vector2(-1, -1))
				return;

			float sx, sy;
			Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
			RaycastHit2D[] hit = Physics2D.RaycastAll(new Vector2(sx, sy), Vector2.zero);
			if (hit.Length == 0)
				return;
			
			int idx = 0;
			if (hit.Length > 1)
			{
				for (int i = 0; i < hit.Length; ++i)
				{
					if (hit[i].collider.tag == "Dialog")
					{
						idx = i;
						break;
					}
				}
			}
			if (hit[idx].collider != null)
			{
				hit[idx].collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit[idx].collider.transform);
				downHitObject = hit[idx].collider.gameObject;
			}

			mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
		}
		else if (InputEx.GetInputUp())
		{
			Vector2 pos;
			InputEx.InputUpPosition(out pos);
			if (pos == new Vector2(-1, -1))
				return;

            mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);

            if (downHitObject != null)
            {
                downHitObject.GetComponent<ButtonEvent>().OnInputUp(downHitObject.transform);
            }
            downHitObject = null;
        }
	}

	public void Countdown()
	{
		print("ui Countdown");
		timeLimit = GameData.GetInstance().betTimeLimit;
		ResetCountdown();
		Timer t = TimerManager.GetInstance().CreateTimer(1, TimerType.Loop, timeLimit);
		t.Tick += CountdownTick;
		t.OnComplete += CountdownComplete;
		t.Start();
	}

	private void CountdownTick()
	{
		--timeLimit;
		countdown.transform.FindChild("Text").GetComponent<Text>().text = timeLimit.ToString();
		countdown.transform.FindChild("progress").GetComponent<Image>().fillAmount = (float)timeLimit / GameData.GetInstance().betTimeLimit;
	}

	private void CountdownComplete()
	{
		print("ui CountdownComplete");
		GameEventManager.OnEndCountdown();
	}

	public void ResetCountdown()
	{
		countdown.transform.FindChild("Text").GetComponent<Text>().text = GameData.GetInstance().betTimeLimit.ToString();
		countdown.transform.FindChild("progress").GetComponent<Image>().fillAmount = 1;
	}

	public void FlashResult(int result)
	{
		if (displayClassic.activeSelf)
		{
			Transform target = displayClassic.transform.FindChild("Choose Effect/" + result.ToString());
			if (target != null)
			{
				flashObject = target;
				FlashImage fo = target.gameObject.AddComponent<FlashImage>();
				fo.flashCount = 0;
				fo.interval = 0.5f;
			}
		}
		else
		{
			Transform target = displayEllipse.transform.FindChild("Choose Effect/" + "e" + result.ToString());
			if (target != null)
			{
				flashObject = target;
				FlashImage fo = target.gameObject.AddComponent<FlashImage>();
				fo.flashCount = 0;
				fo.interval = 0.5f;
			}
		}
	}

	public void StopFlash()
	{
		if (flashObject != null)
		{
			flashObject.GetComponent<FlashImage>().StopFlash();
			flashObject = null;
		}
	}

	public void ChangeFlash()
	{
		if (flashObject != null)
		{
			string name = flashObject.name;
			Destroy(flashObject.GetComponent<FlashImage>());
			if (string.Equals(name.Substring(0, 1), "e"))
			{
				int value;
				string strField = name.Substring(1);
				if (int.TryParse(strField, out value))
				    FlashResult(value);
			}
			else
			{
				int value;
				if (int.TryParse(name, out value))
					FlashResult(value);
			}
		}
	}

	public void ShowWarning(string str)
	{
		if (dlgWarning != null && !dlgWarning.activeSelf)
		{
			dlgWarning.SetActive(true);
			dlgWarning.transform.FindChild("Text").GetComponent<Text>().text = str;
		}
	}

	public void HideWarning()
	{
		if (dlgWarning != null && dlgWarning.activeSelf)
			dlgWarning.SetActive(false);
	}

    public bool IsDlgActived()
    {
        return dlgWarning.activeSelf || dlgCard.activeSelf;
    }

	public void ActiveDlgCard(bool active)
	{
		dlgCard.SetActive(active);
	}

	public void RefreshLblWin(string str)
	{
		lblWin.text = str;
	}

	public void RefreshLblCredits(string str)
	{
		lblCredit.text = str;
	}

	public void RefreshLblBet(string str)
	{
		lblBet.text = str;
	}

	public void RefreshLblRemember(string str)
	{
		lblRemember.text = str;
	}
}
