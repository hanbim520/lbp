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
	public GameObject dlgYesNO;
	public GameObject[] cardExplains;
	// Particles for lottery mode
	public GameObject win0;				// 大0中奖粒子
	public GameObject winSmall;			// 小00 0~36 中奖粒子
	public GameObject rectSpark0;		// 0框粒子
	public GameObject rectSpark00;		// 00框粒子
	public GameObject bigRectSpark;		// 大方框粒子
	public GameObject smallRectSpark;	// 小方框粒子

	public int CurChipIdx
	{
		get { return curChipIdx; }
	}

    public Text lblCredit;
    public Text lblWin;
    public Text lblBet;
    public Text lblRemember;

	private GameObject displayClassic;
	private GameObject displayEllipse;
	private GameObject eraser;
	private GameObject crown;
	private GameObject betChipsRoot;
	private GameObject fieldChipsRoot;
	private GameObject countdown;
	private RectTransform mouseIcon;
	private int curChipIdx = -1;
	private int timeLimit;
    private GameObject downHitObject;
    private List<Transform> lightEffects = new List<Transform>();
	private Transform flashObject;
    private Timer timerHideWarning;
    private string[] strCardError = new string[]{"If you want to use Presented\nCredits, you have to use up all\ncredits, and then keyin agian.",
        "要使用优惠卡，\n须从0分开始充值。"};
	
    void Awake()
    {
        string logicName = "";
        if (GameData.GetInstance().deviceIndex == 1)
            logicName = "ServerLogic";
        else
            logicName = "ClientLogic";
        Object prefab = (Object)Resources.Load("Logics/" + logicName);
        GameObject go = (GameObject)Instantiate(prefab);
        go.name = logicName;
        prefab = null;
        if (GameData.GetInstance().deviceIndex == 1)
            gameLogic = go.GetComponent<ServerLogic>();
        else
            gameLogic = go.GetComponent<ClientLogic>();
    }

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
		GameEventManager.OpenKey += OpenKey;
    }

    private void UnregisterEvents()
    {
		GameEventManager.CloseGate -= StopFlash;
		GameEventManager.OpenKey -= OpenKey;
    }

	private void Init()
	{
		eraser = GameObject.Find("Canvas/eraser");
		eraser.SetActive(false);
		crown = GameObject.Find("Canvas/crown");
		crown.SetActive(false);
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
		if (gameLogic.LogicPhase >= GamePhase.ShowResult)
			return;

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

		if (GameData.GetInstance().displayType == 0)	// 显示classic
		{
			if (displayClassic != null) displayClassic.SetActive(true);
			if (displayEllipse != null) displayEllipse.SetActive(false);
		}
		else if (GameData.GetInstance().displayType == 1)	// 显示ellipse
		{
			if (displayClassic != null) displayClassic.SetActive(false);
			if (displayEllipse != null) displayEllipse.SetActive(true);
		}

		// 还原压分区筹码
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
					continue;

				if (betFields.ContainsKey(name))
					betFields[name] += int.Parse(betValue);
				else
					betFields.Add(name, int.Parse(betValue));
			}

			foreach (Transform t in fieldChipsRoot.transform)
				Destroy(t.gameObject);

			// 还原经典压分区筹码
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
			// 还原椭圆压分区筹码
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
						List<string> prefabPath = new List<string>();
						prefabPath.Add("SmallChip/SC" + curChipIdx.ToString());
						List<Transform> target = new List<Transform>();
						int fieldName;
						List<string> name = new List<string>();
						name.Add(item.Key);

						// 设置小筹码
						Transform tmpTarget = vfRoot.transform.FindChild(item.Key);
						if (tmpTarget != null)
							target.Add(tmpTarget);

						// 设置大筹码
						if (int.TryParse(item.Key, out fieldName) || string.Equals(item.Key, "00"))
						{
							prefabPath.Add("BigChip/BC" + curChipIdx.ToString());
							string eName = "e" + item.Key;
							name.Add(eName);
							tmpTarget = ceRoot.transform.FindChild(eName);
							if (tmpTarget != null)
								target.Add(tmpTarget);
						}

						if (target.Count > 0)
						{
							for (int i = 0; i < prefabPath.Count; ++i)
							{
								Object prefab = (Object)Resources.Load(prefabPath[i]);
								GameObject chip = (GameObject)Instantiate(prefab);
								chip.transform.SetParent(fieldChipsRoot.transform);
								chip.transform.localScale = Vector3.one;
								prefab = null;
								Vector3 targetPos = new Vector3(target[i].localPosition.x * target[i].parent.localScale.x,
								                                target[i].localPosition.y * target[i].parent.localScale.y,
								                                0);
								chip.transform.localPosition = targetPos;
								chip.transform.GetChild(0).GetComponent<Text>().text = item.Value.ToString();
								chip.name = name[i];
							}
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
		float y = -502.0f;
		float start = 0.0f, dist = 0.0f;
		int num = GameData.GetInstance().maxNumberOfChips;
		if (num == 6)
		{
			start = -627.5f;
			dist = 86;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 5)
		{
			start = -620.0f;
			dist = 100;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 4)
		{
			start = -600.0f;
			dist = 130;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 3)
		{
			start = -580.0f;
			dist = 160;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 2)
		{
			start = -500.0f;
			dist = 160;
		}
		else if (GameData.GetInstance().maxNumberOfChips == 1)
		{
			start = -367.0f;
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
				if (j == 0)
				{
					// 默认第一个筹码是选中的
					curChipIdx = i;
					if (!chooseBetEffect.activeSelf) chooseBetEffect.SetActive(true);
					chooseBetEffect.transform.localPosition = betChip.transform.localPosition + new Vector3(0, 10f, 0);
				}
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
		if (eraser.activeSelf)
		{
			eraser.SetActive(false);
			mouseIcon.gameObject.SetActive(true);
			return;
		}
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
		if (eraser.activeSelf)
		{
			eraser.SetActive(false);
			mouseIcon.gameObject.SetActive(true);
		}
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
                GameData.GetInstance().ZongYa += betValue;
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
                GameData.GetInstance().ZongYa += betValue;
				gameLogic.currentBet += betValue;
				RefreshLblCredits(gameLogic.totalCredits.ToString());
				RefreshLblBet(gameLogic.currentBet.ToString());
            }
        }
    }

	// 退币按钮
	public void BackTicketEvent(Transform hitObject)
	{
		if (gameLogic.betFields.Count > 0 ||
            gameLogic.totalCredits == 0)
			return;

		ActiveDlgYesNO(true);
	}

	// 优惠卡按钮
	public void CardButtonEvent(Transform hitObject)
	{
		if (GameData.GetInstance().IsCardMode == CardMode.YES)
		{
			return;
		}

        if (gameLogic.totalCredits > 0)
        {
            ShowWarning(strCardError[GameData.GetInstance().language], true, 4f);
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
		try
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

			bool isEllipse = false;
			string strField = hitObject.name;
			int bet = GameData.GetInstance().betChipValues[curChipIdx];
			// Ellipse
			if (string.Equals(strField.Substring(0, 1), "e"))
			{
				isEllipse = true;
				strField = strField.Substring(1);
			}
			bet = GameEventManager.OnFieldClick(strField, bet);
			// Play sfx
			char[] splits = {'-'};
			string[] words = strField.Split(splits);
			int num;
			if (words.Length == 1 && int.TryParse(words[0], out num))
				AudioController.Play(words[0]);
			
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

			if (isEllipse)
			{
				Transform targetObject = hitObject.parent.parent.FindChild("Valid Fields/" + strField);
				if (targetObject == null)
					return;
				prefabPath = "SmallChip/SC";
				prefab = (Object)Resources.Load(prefabPath + curChipIdx);
				chip = (GameObject)Instantiate(prefab);
				chip.transform.SetParent(fieldChipsRoot.transform);
				chip.transform.localPosition = betChipsRoot.transform.Find("BetChip" + curChipIdx + "(Clone)").localPosition;
				chip.transform.localScale = Vector3.one;
				chip.name = targetObject.name + " temp";
				prefab = null;
				chip.transform.GetChild(0).GetComponent<Text>().text = bet.ToString();
				
				targetPos = new Vector3(targetObject.localPosition.x * targetObject.parent.localScale.x,
				                        targetObject.localPosition.y * targetObject.parent.localScale.y,
				                        0);
				iTween.MoveTo(chip, iTween.Hash("time", 0.5, "islocal", true, "position", targetPos, 
				                                "oncomplete", "FieldChipMoveComplete", "oncompletetarget", gameObject, "oncompleteparams", targetObject.name + ":" + bet.ToString()));
			}
		}
		catch(System.Exception ex)
		{
			Debug.Log(ex.ToString());
		}
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
		if (eraser.activeSelf)
		{
			eraser.SetActive(false);
			mouseIcon.gameObject.SetActive(true);
		}
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
		UpdateTimer();
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
			eraser.transform.localPosition = mouseIcon.localPosition;
		}
		else if (InputEx.GetInputUp())
		{
			Vector2 pos;
			InputEx.InputUpPosition(out pos);
			if (pos == new Vector2(-1, -1))
				return;

            mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
			eraser.transform.localPosition = mouseIcon.localPosition;

            if (downHitObject != null)
            {
                downHitObject.GetComponent<ButtonEvent>().OnInputUp(downHitObject.transform);
            }
            downHitObject = null;
        }
	}

	private void UpdateTimer()
	{
		if (timerHideWarning != null)
			timerHideWarning.Update(Time.deltaTime);
	}

	public void Countdown()
	{
		Debug.Log("ui Countdown");
		if (eraser.activeSelf)
		{
			eraser.SetActive(false);
			mouseIcon.gameObject.SetActive(true);
		}
		timeLimit = GameData.GetInstance().betTimeLimit;
		ResetCountdown();
		Timer t = TimerManager.GetInstance().CreateTimer(1, TimerType.Loop, timeLimit);
		t.Tick += CountdownTick;
		t.OnComplete += CountdownComplete;
		t.Start();
		AudioController.Play("makeyourbets");
		GameEventManager.OnPrompt(PromptId.PleaseBet);
	}

	private void CountdownTick()
	{
		--timeLimit;
		countdown.transform.FindChild("Text").GetComponent<Text>().text = timeLimit.ToString();
		countdown.transform.FindChild("progress").GetComponent<Image>().fillAmount = (float)timeLimit / GameData.GetInstance().betTimeLimit;
	}

	private void CountdownComplete()
	{
		Debug.Log("ui CountdownComplete");
		GameEventManager.OnEndCountdown();
		AudioController.Play("nomorebets");
		GameEventManager.OnPrompt(PromptId.NoMoreBet);
	}

	public void ResetCountdown()
	{
		countdown.transform.FindChild("Text").GetComponent<Text>().text = GameData.GetInstance().betTimeLimit.ToString();
		countdown.transform.FindChild("progress").GetComponent<Image>().fillAmount = 1;
	}

	// 显示彩金号码
	public void FlashLotteries(ref List<int> lotteries)
	{
		if (lotteries.Count == 0)
			return;

		List<GameObject> particles = new List<GameObject>();

	}

	public void StopFlashLotteries()
	{

	}

	public void FlashResult(int result)
	{
		if (eraser.activeSelf)
		{
			eraser.SetActive(false);
			mouseIcon.gameObject.SetActive(true);
		}
        string strResult = "";
        if (result != 37)
            strResult = result.ToString();
        else
            strResult = "00";
		if (displayClassic.activeSelf)
		{
			Transform target = displayClassic.transform.FindChild("Choose Effect/" + strResult);
			if (target != null)
			{
				flashObject = target;
				FlashImage fo = target.gameObject.AddComponent<FlashImage>();
				fo.flashCount = 0;
				fo.interval = 0.5f;
			}
			crown.SetActive(true);
			crown.transform.localPosition = target.localPosition;
		}
		else
		{
            Transform target = displayEllipse.transform.FindChild("Choose Effect/" + "e" + strResult);
			if (target != null)
			{
				flashObject = target;
				FlashImage fo = target.gameObject.AddComponent<FlashImage>();
				fo.flashCount = 0;
				fo.interval = 0.5f;
			}
			crown.SetActive(true);
			crown.transform.localPosition = target.localPosition;
		}
	}

	public void StopFlash()
	{
		if (flashObject != null)
		{
			flashObject.GetComponent<FlashImage>().StopFlash();
			flashObject = null;
		}
		crown.SetActive(false);
	}

	public void ChangeFlash()
	{
		if (flashObject != null)
		{
			string name = flashObject.name;
			Destroy(flashObject.GetComponent<FlashImage>());
			Color c = flashObject.GetComponent<Image>().color;
			c.a = 0;
			flashObject.GetComponent<Image>().color = c;
			if (string.Equals(name, "00") || string.Equals(name, "e00"))
			{
				FlashResult(37);
			}
			else
			{
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
	}

	public void ShowWarning(string str, bool autoDisappear = false, float duration = 1.5f)
	{
		if (dlgWarning != null && !dlgWarning.activeSelf)
		{
			dlgWarning.SetActive(true);
			dlgWarning.transform.FindChild("Text").GetComponent<Text>().text = str;
			if (autoDisappear)
			{
                timerHideWarning = new Timer(duration, 0);
				timerHideWarning.Tick += HideWarning;
				timerHideWarning.Start();
			}
		}
	}

	public void HideWarning()
	{
		timerHideWarning = null;
		if (dlgWarning != null && dlgWarning.activeSelf)
			dlgWarning.SetActive(false);
	}

    public bool IsDlgActived()
    {
		return dlgWarning.activeSelf || dlgCard.activeSelf || dlgYesNO.activeSelf;
    }

	public void ActiveDlgCard(bool active)
	{
		if (active)
		{
			if (!dlgCard.activeSelf)
				dlgCard.SetActive(true);
		}
		else
		{
			dlgCard.SetActive(false);
		}
	}

	public void ActiveDlgYesNO(bool active)
	{
		dlgYesNO.SetActive(active);
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

    // 旋转物理钥匙
	public void OpenKey()
	{
		ActiveDlgCard(true);
	}

    public void ActiveBackendTip(string tip)
    {
        backendTip.SetActive(true);
        backendTip.transform.FindChild("Text").GetComponent<Text>().text = tip;
    }
}
