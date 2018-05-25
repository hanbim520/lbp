using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

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
	public GameObject[] objCircalRecords;	// 0:国际38孔排列 1:特殊38孔排列

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
	private bool hitDownOpt = false;
//    private List<Transform> lightEffects = new List<Transform>();	// 选中的压分区域(亮色)
	// 改版后：鼠标只要移到边或角 所有关联压分区显示亮色
	Dictionary<string, List<Transform>> lightEffects = new Dictionary<string, List<Transform>>();
	private Transform flashObject;
	private List<Transform> flashObjects = new List<Transform>();	// 显示压中结果的区域(亮色)
    private List<Transform> winChips = new List<Transform>();        // 赢的筹码
    private Timer timerHideWarning;
	private Timer timerCountdown;									// 倒计时
    private string[] strCardError = new string[]{"If you want to use Presented\nCredits, you have to use up all\ncredits, and then keyin agian.",
        "要使用优惠卡，\n须从0分开始充值。"};
	// 认球错误
	private string[] strRecognBallError = {"Communication failures.\nPlease reboot device!\nError code:" + BreakdownType.RecognizeBall, 
		"通讯故障，请重启机器！\n故障号：" + BreakdownType.RecognizeBall};
	private string[] strUSBDisconnError = {"Communication failures.\nPlease reboot device!\nError code:" + BreakdownType.USBDisconnect, 
		"通讯故障，请重启机器！\n故障号：" + BreakdownType.USBDisconnect};
	private string[] strTimeoutError = {"Communication failures.\nPlease reboot device!\nError code:" + BreakdownType.RecognizeBallTimeout, 
		"通讯故障，请重启机器！\n故障号：" + BreakdownType.RecognizeBallTimeout};
	private string[] strBallHaventFallError = {"Communication failures.\nPlease reboot device!\nError code:" + BreakdownType.BallHaventFall, 
		"通讯故障，请重启机器！\n故障号：" + BreakdownType.BallHaventFall};
	
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
		SetRouletteType();
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
		GameEventManager.ChooseFields += ChooseFields;
		GameEventManager.BreakdownTip += BreakdownTip;
		GameEventManager.SyncUI += SyncUI;
		GameEventManager.EnterBackend += EnterBackend;
    }

    private void UnregisterEvents()
    {
		GameEventManager.CloseGate -= StopFlash;
		GameEventManager.OpenKey -= OpenKey;
		GameEventManager.ChooseFields -= ChooseFields;
		GameEventManager.BreakdownTip -= BreakdownTip;
		GameEventManager.SyncUI -= SyncUI;
		GameEventManager.EnterBackend -= EnterBackend;
    }

	private void Init()
	{
		eraser = GameObject.Find("Canvas/eraser");
		eraser.SetActive(false);
		crown = GameObject.Find("Canvas/crown");
		crown.SetActive(false);
		mouseIcon = GameObject.Find("Canvas/mouse icon").GetComponent<RectTransform>();
		fieldChipsRoot = GameObject.Find("Canvas/FieldChipsRoot");
		countdown = GameObject.Find("Canvas/Countdown");
        lblCredit = GameObject.Find("Canvas/Credit/Credit").GetComponent<Text>();
        lblWin = GameObject.Find("Canvas/Credit/Win").GetComponent<Text>();
        lblBet = GameObject.Find("Canvas/Credit/Bet").GetComponent<Text>();
		lblRemember = GameObject.Find("Canvas/Credit/Remember").GetComponent<Text>();

		countdown.transform.Find("Text").GetComponent<Text>().text = GameData.GetInstance().betTimeLimit.ToString();
		GameObject demoGo = GameObject.Find("Canvas/Demo");
		if (demoGo != null)
		{
			if (GameData.isDemo)
				GameObject.Find("Canvas/Demo").SetActive(true);
			else
				GameObject.Find("Canvas/Demo").SetActive(false);
		}
		LoadEllipsePrefabs();
	}

	void LoadEllipsePrefabs()
	{
		Transform parent = GameObject.Find("Canvas/38 Fields/Ellipse").transform;
		if (parent == null)
			return;
		
		string prefabPath = "";
		if (GameData.rouletteType == RouletteType.Standard)
		{
			prefabPath = "Ellipse38/Standard/Choose Effect";
		}
		else if (GameData.rouletteType == RouletteType.Special1)
		{
			prefabPath = "Ellipse38/Speial1/Choose Effect";
		}
		Object prefab = (Object)Resources.Load(prefabPath);
		GameObject objChooseEffect = (GameObject)Instantiate(prefab);
		objChooseEffect.name = "Choose Effect";
		objChooseEffect.transform.SetParent(parent);
		objChooseEffect.transform.localPosition = Vector3.zero;
		objChooseEffect.transform.localScale = Vector3.one;

		GameObject uilogic = GameObject.Find("UILogic");
		for (int i = 0; i <= 37; ++i)
		{
			string name = i != 37 ? string.Format("e{0}", i) : "e00";
			objChooseEffect.transform.Find(name).GetComponent<ButtonEvent>().receiver = uilogic;
		}
	}

	public void ChangeLanguage(Transform hitObject)
	{
        if (gameLogic.isPayingCoin ||
            IsDlgActived())
            return;

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
		if (gameLogic.LogicPhase >= GamePhase.Run ||
            gameLogic.isPayingCoin ||
            IsDlgActived())
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
		Dictionary<string, int> betFields = gameLogic.betFields;
		
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
				foreach (KeyValuePair<string, int> item in betFields)
				{
					int count = GameData.GetInstance().betChipValues.Count;
					Transform target = root.transform.Find(item.Key);
					if (target != null)
					{
						int chipIdx = 0;
						for (int i = 0; i < count; ++i)
						{
							if (item.Value >= GameData.GetInstance().betChipValues[i])
								chipIdx = i;
						}
						string prefabPath = "BC" + chipIdx.ToString();
						GameObject chip = PoolManager.Pools["Stuff"].Spawn(prefabPath).gameObject;
						chip.transform.SetParent(fieldChipsRoot.transform);
						chip.transform.localScale = Vector3.one;
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
				int count = GameData.GetInstance().betChipValues.Count;
				foreach (KeyValuePair<string, int> item in betFields)
				{
					int chipIdx = 0;
					for (int i = 0; i < count; ++i)
					{
						if (item.Value >= GameData.GetInstance().betChipValues[i])
							chipIdx = i;
					}

					List<string> prefabPath = new List<string>();
					prefabPath.Add("SC" + chipIdx.ToString());
					List<Transform> target = new List<Transform>();
					int fieldName;
					List<string> name = new List<string>();
					name.Add(item.Key);

					// 设置小筹码
					Transform tmpTarget = vfRoot.transform.Find(item.Key);
					if (tmpTarget != null)
						target.Add(tmpTarget);

					// 设置大筹码
					if (int.TryParse(item.Key, out fieldName) || string.Equals(item.Key, "00"))
					{
						prefabPath.Add("BC" + chipIdx.ToString());
						string eName = "e" + item.Key;
						name.Add(eName);
						tmpTarget = ceRoot.transform.Find(eName);
						if (tmpTarget != null)
							target.Add(tmpTarget);
					}
					
					if (target.Count > 0)
					{
						for (int i = 0; i < prefabPath.Count; ++i)
						{
							GameObject chip = PoolManager.Pools["Stuff"].Spawn(prefabPath[i]).gameObject;
							chip.transform.SetParent(fieldChipsRoot.transform);
							chip.transform.localScale = Vector3.one;
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

	public void SetBetChips()
	{
		betChipsRoot = GameObject.Find("BetChips");
		foreach (Transform child in betChipsRoot.transform)
			Destroy(child.gameObject);
		string path = "Bet Chips/";
		float y = -502.0f;
		float start = 0.0f, dist = 0.0f;
		int num = GameData.GetInstance().maxNumberOfChips;
		if (num == 6)
		{
			start = -627.5f;
			dist = 86;
		}
		else if (num == 5)
		{
			start = -620.0f;
			dist = 100;
		}
		else if (num == 4)
		{
			start = -600.0f;
			dist = 130;
		}
		else if (num == 3)
		{
			start = -580.0f;
			dist = 160;
		}
		else if (num == 2)
		{
			start = -500.0f;
			dist = 160;
		}
		else if (num == 1)
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
					betChip.GetComponent<BreathyChip>().enabled  = true;
					if (curChipIdx < 0 || curChipIdx >= betChipsNum)
						curChipIdx = i;
					if (!chooseBetEffect.activeSelf) chooseBetEffect.SetActive(true);
					chooseBetEffect.transform.localPosition = betChip.transform.localPosition + new Vector3(0, 10f, 0);
				}
				betChip.transform.localScale = Vector3.one;
				betChip.GetComponent<ButtonEvent>().receiver = gameObject;
				betChip.GetComponent<ButtonEvent>().inputUpEvent = "ChipButtonEvent";
				betChip.transform.GetChild(0).GetComponent<Text>().text = value.ToString();
				prefab = null;
                ++j;
			}
		}
	}

	public void ClearEvent(Transform hitObject)
	{
        if (gameLogic.isPayingCoin ||
            IsDlgActived())
            return;

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
        if (gameLogic.isPayingCoin ||
            IsDlgActived())
            return;

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
		ClearSingleSigns();
	}

	// 清除桌面筹码 不返还给玩家
	public void CleanAll()
	{
		if (fieldChipsRoot.transform.childCount == 0)
			return;
		
		foreach (Transform child in fieldChipsRoot.transform)
			Destroy(child.gameObject);
		GameEventManager.OnCleanAll();
		ClearSingleSigns();
	}

	// 清除没有中的筹码
	public void ClearLoseChips()
	{
		if (gameLogic.betFields.Count == 0)
			return;

		int type = 0;
		int lineId = 0;
		bool line1 = false;
		bool line2 = false;
		float startX1 = 0;
		float startX2 = 0;
        foreach (Transform t in fieldChipsRoot.transform)
		{
			if (winChips.Contains(t))
				continue;
			if (t.localPosition.y > -96)
			{
				line1 = true;
				if (startX1 > t.localPosition.x)
					startX1 = t.localPosition.x;
			}
			else
			{
				line2 = true;
				if (startX2 > t.localPosition.x)
					startX2 = t.localPosition.x;
			}
		}
		if (line1 || line2)
			type = line1 && line2 ? 2 : 1;
		else
			return;
		if (type == 2)
			lineId = 1;
		else
			lineId = line1 ? 1 : 2;
		GameEventManager.OnRakeInit(type, lineId, startX1 - 95.0f, startX2 - 95.0f, ref winChips);
	}

    // 清除中的筹码
    public void ClearWinChips()
    {
//        foreach (Transform t in winChips)
//            Destroy(t.gameObject);
        winChips.Clear();
		foreach (Transform child in fieldChipsRoot.transform)
			Destroy(child.gameObject);
    }

    // 加中的筹码
    public void AddWinChip(string fieldName)
    {
//        if (string.Equals(fieldName.Substring(0, 1), "e"))
//            return;
        Transform t = fieldChipsRoot.transform.Find(fieldName);
        if (t != null)
            winChips.Add(t);
    }

	public void RepeatEvent()
	{
        int count = GameData.GetInstance().lastBets.Count;
		if (gameLogic.isPayingCoin || IsDlgActived() || count == 0 || gameLogic.LogicPhase != GamePhase.Countdown)
            return;

		int lastBetCredit = GameData.GetInstance().lastBetCredit;
		if (lastBetCredit > gameLogic.totalCredits)
            return;

		// 检查是否超过限注
		string checkLimitMsg = NetInstr.CheckRepeatAble.ToString();
		foreach (KeyValuePair<string, int> item in gameLogic.betFields)
		{
			checkLimitMsg += string.Format(":{0}:{1}", item.Key, item.Value);
		}
		checkLimitMsg += string.Format(":{0}:{1}", "repeats", -1);
		foreach (KeyValuePair<string, int> item in GameData.GetInstance().lastBets)
		{
			checkLimitMsg += string.Format(":{0}:{1}", item.Key, item.Value);
		}
		if (GameData.GetInstance().deviceIndex == 1)
		{
			char[] delimiterChars = {':'};
			string[] words = checkLimitMsg.Split(delimiterChars);
			((ServerLogic)gameLogic).HandleCheckRepeatAble(false, 0, ref words);
		}
		else
		{
			((ClientLogic)gameLogic).clientSocket.SendToServer(checkLimitMsg);
		}
    }

	public void RepeatEventCB()
	{
		int betChipCount = GameData.GetInstance().betChipValues.Count;
		if (displayClassic.activeSelf)
		{
			string rootPath = "Canvas/38 Fields/Classic/Valid Fields";
			if (GameData.GetInstance().maxNumberOfFields == 37)
			{
				rootPath = "Canvas/37 Fields/Classic/Valid Fields";
			}
			GameObject root = GameObject.Find(rootPath);
			string prefabPath = "BC";
			if (root != null)
			{
				int betValue = 0;
				ClearAllEvent(null);
				foreach (KeyValuePair<string, int> info in GameData.GetInstance().lastBets)
				{
					if (info.Key == "00" && fields37.activeSelf)
						continue;
					
					ShowSingleSign(info.Key);
					Transform target = root.transform.Find(info.Key);
					if (target != null)
					{
						int chipIdx = 0;
						for (int i = 0; i < betChipCount; ++i)
						{
							if (info.Value >= GameData.GetInstance().betChipValues[i])
								chipIdx = i;
						}
						
						GameObject chip = PoolManager.Pools["Stuff"].Spawn(prefabPath + chipIdx.ToString()).gameObject;
						chip.transform.SetParent(fieldChipsRoot.transform);
						chip.transform.localScale = Vector3.one;
						Vector3 targetPos = new Vector3(target.localPosition.x * target.parent.localScale.x,
						                                target.localPosition.y * target.parent.localScale.y,
						                                0);
						chip.transform.localPosition = targetPos;
						chip.transform.GetChild(0).GetComponent<Text>().text = info.Value.ToString();
						chip.name = info.Key;
						betValue += info.Value;
						gameLogic.betFields.Add(info.Key, info.Value);
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
				foreach (KeyValuePair<string, int> info in GameData.GetInstance().lastBets)
				{
					if (info.Key == "00" && fields37.activeSelf)
						continue;
					
					ShowSingleSign(info.Key);
					string prefabPath = "SC";
					Transform target1 = null;
					Transform target2 = null;	// 为了恢复小的单点筹码
					int fieldName;
					string name = info.Key;
					if (int.TryParse(info.Key, out fieldName) || string.Equals(info.Key, "00"))
					{
						prefabPath = "BC";
						name = "e" + name;
						target1 = ceRoot.transform.Find(name);
						target2 = vfRoot.transform.Find(info.Key);
					}
					else
					{
						target1 = vfRoot.transform.Find(info.Key);
					}
					
					int chipIdx = 0;
					if (target1 != null)
					{
						for (int i = 0; i < betChipCount; ++i)
						{
							if (info.Value >= GameData.GetInstance().betChipValues[i])
								chipIdx = i;
						}
						
						GameObject chip = PoolManager.Pools["Stuff"].Spawn(prefabPath + chipIdx.ToString()).gameObject;
						chip.transform.SetParent(fieldChipsRoot.transform);
						chip.transform.localScale = Vector3.one;
						Vector3 targetPos = new Vector3(target1.localPosition.x * target1.parent.localScale.x,
						                                target1.localPosition.y * target1.parent.localScale.y,
						                                0);
						chip.transform.localPosition = targetPos;
						chip.transform.GetChild(0).GetComponent<Text>().text = info.Value.ToString();
						chip.name = name;
						betValue += info.Value;
						gameLogic.betFields.Add(info.Key, info.Value);
					}
					if (target2 != null)
					{
						GameObject chip = PoolManager.Pools["Stuff"].Spawn("SC" + chipIdx.ToString()).gameObject;
						chip.transform.SetParent(fieldChipsRoot.transform);
						chip.transform.localScale = Vector3.one;
						Vector3 targetPos = new Vector3(target2.localPosition.x * target2.parent.localScale.x,
						                                target2.localPosition.y * target2.parent.localScale.y,
						                                0);
						chip.transform.localPosition = targetPos;
						chip.transform.GetChild(0).GetComponent<Text>().text = info.Value.ToString();
						chip.name = info.Key;
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
        if (gameLogic.isPayingCoin ||
            IsDlgActived())
            return;

		if (gameLogic.totalCredits == 0)
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
			cardExplains[0].SetActive(false);
			cardExplains[1].SetActive(false);
		}
		else
		{
			// Blue button
			GameObject.Find("Canvas/Buttons/CN/Card CN").transform.GetChild(0).gameObject.SetActive(true);
			cardExplains[0].SetActive(false);
			cardExplains[1].SetActive(false);
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

	private void ChooseFields(Transform hitObject)
	{
		if (gameLogic.LogicPhase >= GamePhase.ShowResult)
			return;
		if (hitObject == null)
		{
			foreach (KeyValuePair<string, List<Transform>> item in lightEffects)
			{
				foreach (Transform t in item.Value)
				{
					Color c = t.GetComponent<Image>().color;
					c.a = 0;
					t.GetComponent<Image>().color = c;
				}
			}
			lightEffects.Clear();
			return;
		}
		if (lightEffects.ContainsKey(hitObject.name))
		{
			return;
		}
		else
		{
			foreach (KeyValuePair<string, List<Transform>> item in lightEffects)
			{
				foreach (Transform t in item.Value)
				{
					Color c = t.GetComponent<Image>().color;
					c.a = 0;
					t.GetComponent<Image>().color = c;
				}
			}
			lightEffects.Clear();

			string filedName = hitObject.name;
			if (string.Equals(filedName.Substring(0, 1), "e"))
			{
                lightEffects.Add(filedName, new List<Transform>(){hitObject});
                Color c = hitObject.GetComponent<Image>().color;
                c.a = 255;
                hitObject.GetComponent<Image>().color = c;
            }
            else
            {
                Transform effectRoot = hitObject.parent.parent.Find("Choose Effect");
				if (effectRoot != null)
				{
					char[] separater = {'-'};
					string[] names = hitObject.name.Split(separater);

					List<Transform> effects = new List<Transform>();
					foreach (string str in names)
					{
						Transform effect = effectRoot.Find(str);
						if (effect != null)
						{
							Color c = effect.GetComponent<Image>().color;
							c.a = 255;
							effect.GetComponent<Image>().color = c;
							effects.Add(effect);
						}
					}
					if (effects.Count > 0)
					{
						lightEffects.Add(filedName, effects);
					}
				}
			}
		}
	}

	public void FieldDownEvent(Transform hitObject)
	{
		if (IsDlgActived() || eraser.activeSelf || curChipIdx == -1 || 
		    gameLogic.LogicPhase != GamePhase.Countdown ||
            !gameLogic.bCanBet)
			return;
    }

	public void FieldClickEvent(Transform hitObject)
	{
		try
		{
            if (gameLogic.isPayingCoin)
                return;

            if (IsDlgActived())
                return;

			if (eraser.activeSelf)
			{
				Destroy(fieldChipsRoot.transform.Find(hitObject.name).gameObject);
				GameEventManager.OnClear(hitObject.name);
				eraser.SetActive(false);
				mouseIcon.gameObject.SetActive(true);

				string fieldName = hitObject.name;
				if (string.Equals(fieldName.Substring(0, 1), "e"))
				{
					fieldName = fieldName.Substring(1);
				}
				ClearSingleSign(fieldName);
				return;
			}
			
			if (curChipIdx == -1 || 
			    gameLogic.LogicPhase != GamePhase.Countdown ||
			    GameData.GetInstance().IsCardMode == CardMode.Ready ||
			    gameLogic.IsLock ||
                !gameLogic.bCanBet)
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
			if (bet <= 0)
				return;
			
			string prefabPath = "BC";
			if (string.Equals(hitObject.parent.name, "Valid Fields") && 
			    string.Equals(hitObject.parent.parent.name, "Ellipse"))
				prefabPath = "SC";
            int chipIdx = 0;
            if (gameLogic.betFields.ContainsKey(strField))
            {
                int credit = gameLogic.betFields[strField];
                int count = GameData.GetInstance().betChipValues.Count;
                for (int i = 0; i < count; ++i)
                {
                    if (credit >= GameData.GetInstance().betChipValues[i])
					{
                        chipIdx = i;
					}
                }
            }
			GameObject chip = PoolManager.Pools["Stuff"].Spawn(prefabPath + chipIdx.ToString()).gameObject;
			chip.transform.SetParent(fieldChipsRoot.transform);
			chip.transform.localPosition = betChipsRoot.transform.Find("BetChip" + curChipIdx + "(Clone)").localPosition;
			chip.transform.localScale = Vector3.one;
			chip.name = hitObject.name + " temp";
			
			chip.transform.GetChild(0).GetComponent<Text>().text = bet.ToString();
			
			Vector3 targetPos = new Vector3(hitObject.localPosition.x * hitObject.parent.localScale.x,
			                                hitObject.localPosition.y * hitObject.parent.localScale.y,
			                                0);
//			iTween.MoveTo(chip, iTween.Hash("time", 0.5, "islocal", true, "position", targetPos, 
//			                                "oncomplete", "FieldChipMoveComplete", "oncompletetarget", gameObject, "oncompleteparams", hitObject.name + ":" + bet.ToString()));
			chip.transform.localPosition = targetPos;
			FieldChipMoveComplete(hitObject.name + ":" + bet.ToString());

			if (isEllipse)
			{
				Transform targetObject = hitObject.parent.parent.Find("Valid Fields/" + strField);
				if (targetObject == null)
					return;
				prefabPath = "SC";
				chip = PoolManager.Pools["Stuff"].Spawn(prefabPath + chipIdx.ToString()).gameObject;
				chip.transform.SetParent(fieldChipsRoot.transform);
				chip.transform.localPosition = betChipsRoot.transform.Find("BetChip" + curChipIdx + "(Clone)").localPosition;
				chip.transform.localScale = Vector3.one;
				chip.name = targetObject.name + " temp";
				chip.transform.GetChild(0).GetComponent<Text>().text = bet.ToString();
				
				targetPos = new Vector3(targetObject.localPosition.x * targetObject.parent.localScale.x,
				                        targetObject.localPosition.y * targetObject.parent.localScale.y,
				                        0);
//				iTween.MoveTo(chip, iTween.Hash("time", 0.5, "islocal", true, "position", targetPos, 
//				                                "oncomplete", "FieldChipMoveComplete", "oncompletetarget", gameObject, "oncompleteparams", targetObject.name + ":" + bet.ToString()));
				chip.transform.localPosition = targetPos;
				FieldChipMoveComplete(targetObject.name + ":" + bet.ToString());
			}
			ShowSingleSign(strField);
		}
		catch(System.Exception ex)
		{
			Debug.Log(ex.ToString());
		}
	}

	public void FieldClickCB(string strField, int betVal)
	{
		try
		{
			bool isEllipse = GameData.GetInstance().displayType == 1;
			bool is38Type = GameData.GetInstance().maxNumberOfFields == 38;
			Transform hitObject;
			string rootPath = "Canvas/";
			if (is38Type)
				rootPath += "38 Fields/";
			else
				rootPath += "37 Fields/";
			if (!isEllipse)
				rootPath += "Classic/Valid Fields/";
			else
			{
				int num;
				if (int.TryParse(strField, out num))
					rootPath += "Ellipse/Choose Effect/e";
				else
					rootPath += "Ellipse/Valid Fields/";
			}
			rootPath += strField;
			hitObject = GameObject.Find(rootPath).transform;

			string prefabPath = "BC";
			if (string.Equals(hitObject.parent.name, "Valid Fields") && 
			    string.Equals(hitObject.parent.parent.name, "Ellipse"))
				prefabPath = "SC";
			int chipIdx = 0;
			if (gameLogic.betFields.ContainsKey(strField))
			{
				int credit = gameLogic.betFields[strField];
				int count = GameData.GetInstance().betChipValues.Count;
				for (int i = 0; i < count; ++i)
				{
					if (credit >= GameData.GetInstance().betChipValues[i])
					{
						chipIdx = i;
					}
				}
			}
			GameObject chip = PoolManager.Pools["Stuff"].Spawn(prefabPath + chipIdx.ToString()).gameObject;
			chip.transform.SetParent(fieldChipsRoot.transform);
			chip.transform.localPosition = betChipsRoot.transform.Find("BetChip" + curChipIdx + "(Clone)").localPosition;
			chip.transform.localScale = Vector3.one;
			chip.name = hitObject.name + " temp";
			
			chip.transform.GetChild(0).GetComponent<Text>().text = betVal.ToString();
			
			Vector3 targetPos = new Vector3(hitObject.localPosition.x * hitObject.parent.localScale.x,
			                                hitObject.localPosition.y * hitObject.parent.localScale.y,
			                                0);
			//			iTween.MoveTo(chip, iTween.Hash("time", 0.5, "islocal", true, "position", targetPos, 
			//			                                "oncomplete", "FieldChipMoveComplete", "oncompletetarget", gameObject, "oncompleteparams", hitObject.name + ":" + bet.ToString()));
			chip.transform.localPosition = targetPos;
			FieldChipMoveComplete(hitObject.name + ":" + betVal.ToString());

			if (isEllipse)
			{
				Transform targetObject = hitObject.parent.parent.Find("Valid Fields/" + strField);
				if (targetObject == null)
					return;
				prefabPath = "SC";
				chip = PoolManager.Pools["Stuff"].Spawn(prefabPath + chipIdx.ToString()).gameObject;
				chip.transform.SetParent(fieldChipsRoot.transform);
				chip.transform.localPosition = betChipsRoot.transform.Find("BetChip" + curChipIdx + "(Clone)").localPosition;
				chip.transform.localScale = Vector3.one;
				chip.name = targetObject.name + " temp";
				chip.transform.GetChild(0).GetComponent<Text>().text = betVal.ToString();
				
				targetPos = new Vector3(targetObject.localPosition.x * targetObject.parent.localScale.x,
				                        targetObject.localPosition.y * targetObject.parent.localScale.y,
				                        0);
				//				iTween.MoveTo(chip, iTween.Hash("time", 0.5, "islocal", true, "position", targetPos, 
				//				                                "oncomplete", "FieldChipMoveComplete", "oncompletetarget", gameObject, "oncompleteparams", targetObject.name + ":" + bet.ToString()));
				chip.transform.localPosition = targetPos;
				FieldChipMoveComplete(targetObject.name + ":" + betVal.ToString());
			}
			ShowSingleSign(strField);
			AudioController.Play("betClick");
		}
		catch(UnityException ex)
		{
			Debug.Log(ex.ToString());
		}
	}

	private void FieldChipMoveComplete(string param)
	{
		char[] separator = {':'};
		string[] str = param.Split(separator);
		Transform old = fieldChipsRoot.transform.Find(str[0]);
		Transform newOne = fieldChipsRoot.transform.Find(str[0] + " temp");

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
        if (IsDlgActived() ||
            gameLogic.isPayingCoin)
            return;

		if (eraser.activeSelf)
		{
			eraser.SetActive(false);
			mouseIcon.gameObject.SetActive(true);
		}

		int idx;
		if (int.TryParse(hitObject.name.Substring(7, 1), out idx))
        {
            foreach (Transform child in betChipsRoot.transform)
			{
                child.localScale = Vector3.one;
				child.GetComponent<BreathyChip>().enabled = false;
			}
			curChipIdx = idx;
			hitObject.GetComponent<BreathyChip>().enabled = true;
        }
		else
        {
			curChipIdx = -1;
        }
		
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

		if (GameData.debug)
		{
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				GameEventManager.OnKeyinOnce();
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				GameEventManager.OnKeout();
			}
			else if (Input.GetKeyDown(KeyCode.B))
			{
				GameEventManager.OnEnterBackend();
			}
			else if (Input.GetKeyDown(KeyCode.R))
			{
				GameEventManager.OnReceiveCoin(1000);
			}
			else if (Input.GetKeyDown(KeyCode.D))
			{
				DoubleEvent();
			}
			else if (Input.GetKeyDown(KeyCode.A))
			{
				BetSeries(SeriesName.SmallSeries);
			}
		}
	}

	private void DetectInputEvents()
	{
		if (InputEx.GetInputDown() && !hitDownOpt)
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
					if (hit[i].collider.tag == "Dialog")	// 弹出框按钮 相当于Dialog button
					{
						idx = i;
						break;
					}
					else if (hit[i].collider.gameObject.GetComponent<ButtonEvent>() != null)
						idx = i;
				}
			}
			if (hit[idx].collider != null)
			{
				hit[idx].collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit[idx].collider.transform);
				downHitObject = hit[idx].collider.gameObject;
			}

			mouseIcon.localPosition = new Vector3(pos.x, pos.y, 0);
			eraser.transform.localPosition = mouseIcon.localPosition;
			hitDownOpt = true;
		}
		else if (InputEx.GetInputUp() && hitDownOpt)
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
			hitDownOpt = false;
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
		timerCountdown = TimerManager.GetInstance().CreateTimer(1, TimerType.Loop, timeLimit);
		timerCountdown.Tick += CountdownTick;
		timerCountdown.OnComplete += CountdownComplete;
		iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", timeLimit,
		                                       "onupdate", "UpdateProgress", "onupdatetarget", gameObject));
		timerCountdown.Start();
		AudioController.Play("makeyourbets");
		GameEventManager.OnPrompt(PromptId.PleaseBet, -1);
        if (GameData.GetInstance().deviceIndex == 1)
		    AudioController.PlayMusic("bgMusic");
	}

	private void UpdateProgress(float value)
	{
		if (!gameLogic.IsPause())
			countdown.transform.Find("progress").GetComponent<Image>().fillAmount = value;
		if (Application.platform != RuntimePlatform.OSXEditor)
		{
			if (!gameLogic.IsPause() &&
				gameLogic.goldfingerUtils.GetRealtimeBallVal() > 0)
			{
				GameEventManager.OnBreakdownTip(BreakdownType.BallHaventFall);
			}
		}
	}

	private void CountdownTick()
	{
		--timeLimit;
		countdown.transform.Find("Text").GetComponent<Text>().text = timeLimit.ToString();
	}

	private void CountdownComplete()
	{
		Debug.Log("ui CountdownComplete");
        if (eraser.activeSelf)
        {
            eraser.SetActive(false);
            mouseIcon.gameObject.SetActive(true);
        }
		GameEventManager.OnEndCountdown();
		AudioController.Play("nomorebets");
        if (GameData.GetInstance().deviceIndex == 1)
		    AudioController.StopMusic(0.5f);
		GameEventManager.OnPrompt(PromptId.NoMoreBet, -1);
	}

	public void ResetCountdown()
	{
		countdown.transform.Find("Text").GetComponent<Text>().text = GameData.GetInstance().betTimeLimit.ToString();
		countdown.transform.Find("progress").GetComponent<Image>().fillAmount = 1;
	}

	// 生成中彩票的金币雨
	public void CreateGoldenRain()
	{
		GameObject root = GameObject.Find("Canvas/LotteryEffectRoot");
		Object prefab = (Object)Resources.Load("Effects/Golden Rain");
		GameObject golds = (GameObject)Instantiate(prefab);
		golds.transform.SetParent(root.transform);
		golds.transform.localPosition = Vector3.zero;
		golds.transform.localScale = Vector3.one;
		prefab = null;
	}

	// 显示彩金号码
	public IEnumerator FlashLotteries(List<int> lotteries)
	{
		Debug.Log("FlashLotteries");
		if (lotteries.Count == 0)
			yield break;

		GameObject root = GameObject.Find("Canvas/JackpotsRoot");
		string resPath = "Effects/";

		Object prefab = null;
		string prefabname = "Jackpot";
		foreach (int num in lotteries)
		{
			string refrenceName = num.ToString();
			if (num == 37)
			{
				refrenceName = "00";
			}
			prefab = (Object)Resources.Load(resPath + prefabname);
			GameObject jackpot = (GameObject)Instantiate(prefab);
			jackpot.transform.SetParent(root.transform);
			string chooseEffectPath = string.Empty;
			if (GameData.GetInstance().displayType == 0)	// 传统压分区
			{
				chooseEffectPath = GameData.GetInstance().maxNumberOfFields == 37 ? "Canvas/37 Fields/Jackpot Points Classic/" : "Canvas/38 Fields/Jackpot Points Classic/";
				jackpot.transform.localScale = Vector3.one;
			}
			else
			{
				chooseEffectPath = GameData.GetInstance().maxNumberOfFields == 37 ? "Canvas/37 Fields/Jackpot Points Ellipse/" : "Canvas/38 Fields/Jackpot Points Ellipse/";
				jackpot.transform.localScale = Vector3.one * 0.5f;
			}
			jackpot.transform.localPosition = GameObject.Find(chooseEffectPath + refrenceName).transform.localPosition;
			if (GameData.GetInstance().displayType == 1)	// 椭圆压分区
			{
				GameObject go = Instantiate(jackpot);
				go.transform.SetParent(root.transform);
				go.transform.localPosition = GameObject.Find(chooseEffectPath + "e" + refrenceName).transform.localPosition;
				go.transform.localScale = Vector3.one * 0.7f;
			}
		}
		prefab = null;

		GameEventManager.OnPrompt(PromptId.Jackpot, GameData.GetInstance().lotteryCondition);
	}

	public void StopFlashLotteries()
	{
		GameObject root = GameObject.Find("Canvas/JackpotsRoot");
		foreach (Transform t in root.transform)
		{
			t.gameObject.SetActive(false);
			Destroy(t.gameObject);
		}
	}

	public void FlashResult(int result)
	{
		if (lightEffects.Count > 0)
		{
			foreach (KeyValuePair<string, List<Transform>> item in lightEffects)
			{
				foreach (Transform t in item.Value)
				{
					Color c = t.GetComponent<Image>().color;
					c.a = 0;
					t.GetComponent<Image>().color = c;
				}
			}
			lightEffects.Clear();
		}
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

		Transform target = displayClassic.activeSelf ? displayClassic.transform.Find("Choose Effect/" + strResult) : 
						   displayEllipse.transform.Find("Choose Effect/" + "e" + strResult);
		if (target != null)
		{
			flashObject = target;
			FlashImage fo = target.gameObject.AddComponent<FlashImage>();
			fo.flashCount = 0;
			fo.interval = 0.5f;
		}
		// 除了号码外的其他区域也要闪烁
		if (result != 37 && result != 0)
		{
			List<string> flashAreas = new List<string>();
			if (result % 2 == 0)	
				flashAreas.Add("Even");
			else 				
				flashAreas.Add("odd");
			if (result >= 1 && result <= 12)
				flashAreas.Add("1st12");
			else if (result >= 13 && result <= 24)
				flashAreas.Add("2nd12");
			else if (result >= 25 && result <= 36)
				flashAreas.Add("3rd12");
			if (result >= 1 && result <= 18)
				flashAreas.Add("1to18");
			else if (result >= 19 && result <= 36)
				flashAreas.Add("19to36");
			if (GameData.GetInstance().colorTable[result] == ResultType.Red)
				flashAreas.Add("red");
			else if (GameData.GetInstance().colorTable[result] == ResultType.Black)
				flashAreas.Add("black");
			if (result % 3 == 0)
				flashAreas.Add("2to1 up");
			else if (result % 3 == 2)
				flashAreas.Add("2to1 middle");
			else if (result % 3 == 1)
				flashAreas.Add("2to1 down");

			if (displayEllipse.activeSelf)
				flashAreas.Add(strResult);
			
			foreach (string name in flashAreas)
			{
				Transform t = displayClassic.activeSelf ? displayClassic.transform.Find("Choose Effect/" + name) : 
							  displayEllipse.transform.Find("Choose Effect/" + name);
				if (t != null)
				{
					FlashImage fo = t.gameObject.AddComponent<FlashImage>();
					fo.flashCount = 0;
					fo.interval = 0.5f;
					flashObjects.Add(t);
				}
			}
		}
		else if (displayEllipse.activeSelf)
		{
			// 闪烁小区域里面的0/00
			Transform t = displayEllipse.transform.Find("Choose Effect/" + strResult); 
			if (t != null)
			{
				FlashImage fo = t.gameObject.AddComponent<FlashImage>();
				fo.flashCount = 0;
				fo.interval = 0.5f;
				flashObjects.Add(t);
			}
		}
		crown.SetActive(true);
		crown.transform.localPosition = target.localPosition;

		// 出提示语
		GameEventManager.OnPrompt(PromptId.Result, result);
		// Play sfx
		string promptSfx = result == 37 ? "00" : result.ToString();
		AudioController.Play(promptSfx);
	}

	public void StopFlash()
	{
		if (flashObject != null)
		{
			flashObject.GetComponent<FlashImage>().StopFlash();
			flashObject = null;
		}
		if (flashObjects.Count > 0)
		{
			foreach (Transform t in flashObjects)
				t.GetComponent<FlashImage>().StopFlash();
			flashObjects.Clear();
		}
        Transform transDisplay = displayClassic.activeSelf ? displayClassic.transform : displayEllipse.transform;
        if (transDisplay.gameObject.activeSelf)
        {
            Transform root = transDisplay.transform.Find("Choose Effect");
            FlashImage[] imgs = root.GetComponentsInChildren<FlashImage>();
            foreach (FlashImage img in imgs)
            {
                img.StopFlash();
            }
        }
		crown.SetActive(false);
		ClearSingleSigns();
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

	public void BreakdownTip(int breakdownType)
	{
		int language = GameData.GetInstance().language;
		gameLogic.SetPause(true);
		GameEventManager.OnPrompt(PromptId.Reboot, -1);
		if (breakdownType == BreakdownType.RecognizeBall)
		{
			ShowWarning(strRecognBallError[language]);
		}
		else if (breakdownType == BreakdownType.USBDisconnect)
		{
			ShowWarning(strUSBDisconnError[language]);
		}
		else if (breakdownType == BreakdownType.RecognizeBallTimeout)
		{
			ShowWarning(strTimeoutError[language]);
		}
		else if (breakdownType == BreakdownType.BallHaventFall)
		{
			ShowWarning(strBallHaventFallError[language]);
			TimerManager.GetInstance().RemoveTimer(timerCountdown.timerId);
		}
	}

	public void ShowWarning(string str, bool autoDisappear = false, float duration = 1.5f)
	{
		if (dlgWarning != null && !dlgWarning.activeSelf)
		{
			dlgWarning.SetActive(true);
			dlgWarning.transform.Find("Text").GetComponent<Text>().text = str;
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
        if (gameLogic.isPayingCoin)
            return;

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

	// 输入后台密码 准备进入后台
	public void EnterBackend()
	{
		ActiveDlgCard(true);
		dlgCard.GetComponent<CardDlg>().SetInputPW();
	}

    public void ActiveBackendTip(string tip)
    {
        backendTip.SetActive(true);
        backendTip.transform.Find("Text").GetComponent<Text>().text = tip;
    }

	private void SyncUI()
	{
//		SetDisplay();
//		SetBetChips();
	}

	// 清除所有单点押分标志
	private void ClearSingleSigns()
	{
		string path = GameData.GetInstance().maxNumberOfFields == 38 ? "38 Fields/DanDian" : "37 Fields/DanDian";
		GameObject root = GameObject.Find("Canvas/" + path);
		if (root != null)
		{
			int childCount = root.transform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				Transform child = root.transform.GetChild(i);
				Image img = child.GetComponent<Image>();
				Color c = img.color;
				c.a = 0;
				img.color = c;
			}
		}
	}

	// 清除单点押分标志
	private void ClearSingleSign(string fieldName)
	{
		string path = GameData.GetInstance().maxNumberOfFields == 38 ? "38 Fields/DanDian" : "37 Fields/DanDian";
		GameObject root = GameObject.Find("Canvas/" + path);
		if (root != null)
		{
			Transform child = root.transform.Find(fieldName);
			Image img = child.GetComponent<Image>();
			Color c = img.color;
			c.a = 0;
			img.color = c;
		}
	}

	// 显示单点押分标志
	private void ShowSingleSign(string fieldName)
	{
		int fieldVal;
		if (!int.TryParse(fieldName, out fieldVal))
			return;
		string path = GameData.GetInstance().maxNumberOfFields == 38 ? "38 Fields/DanDian" : "37 Fields/DanDian";
		GameObject root = GameObject.Find("Canvas/" + path);
		if (root != null)
		{
			Transform child = root.transform.Find(fieldName);
			Image img = child.GetComponent<Image>();
			Color c = img.color;
			c.a = 1;
			img.color = c;
		}
	}

	private void SetRouletteType()
	{
		if (GameData.GetInstance().maxNumberOfFields == 38)
		{
			string prefabPath = "";
			if (GameData.rouletteType == RouletteType.Standard)
			{
				objCircalRecords[0].SetActive(true);
				objCircalRecords[1].SetActive(false);
				prefabPath = "Ellipse38/Standard/DanDian";
			}
			else if (GameData.rouletteType == RouletteType.Special1)
			{
				objCircalRecords[0].SetActive(false);
				objCircalRecords[1].SetActive(true);
				prefabPath = "Ellipse38/Speial1/DanDian";
			}

			Object prefab = (Object)Resources.Load(prefabPath);
			GameObject objDandian = (GameObject)Instantiate(prefab);
			objDandian.name = "DanDian";
			objDandian.transform.SetParent(GameObject.Find("Canvas/38 Fields").transform);
			objDandian.transform.localPosition = Vector3.zero;
			objDandian.transform.localScale = Vector3.one;
		}
	}

	// 不检查全台限注
	public void RepeatEvent2()
	{
		int count = GameData.GetInstance().lastBets.Count;
		if (count == 0 || 
			gameLogic.isPayingCoin || 
			IsDlgActived() || 
			gameLogic.LogicPhase != GamePhase.Countdown || 
			gameLogic.IsLock ||
			!gameLogic.bCanBet)
			return;

		int lastBetCredit = GameData.GetInstance().lastBetCredit;
		if (lastBetCredit > gameLogic.totalCredits)
			return;

		RepeatEventCB();
	}

	// x2押分
	public void DoubleEvent()
	{
		if (gameLogic.isPayingCoin || 
			IsDlgActived() || 
			gameLogic.LogicPhase != GamePhase.Countdown || 
			gameLogic.IsLock ||
			!gameLogic.bCanBet)
			return;

		Dictionary<string, int> dict = new Dictionary<string, int>(gameLogic.betFields);
		foreach (KeyValuePair<string, int> item in dict)
			GameEventManager.OnFieldClick(item.Key, item.Value);
		SetDisplay();
	}

	string[] smallSeries37 = new string[]{ "5-8", "10-11", "13-16", "23-24", "27-30", "33-36" };			// 都1个注
	string[] orphaline37 = new string[]{ "1", "6-9", "14-17", "17-20", "31-34" };							// 都1个注
	string[] zeroSpiel37 = new string[]{ "0-3", "12-15", "26", "32-35" };									// 都1个注
	string[] biSeries37 = new string[]{ "0-2-3", "4-7", "12-15", "18-21", "19-22", "32-35", "25-26-28-29"}; // "0-2-3"和"25-26-28-29"投2个注，其他1个注
	// 区域押分(大区 小区 5/8区 其他区)
	public void BetSeries(SeriesName name)
	{
		if (gameLogic.isPayingCoin || 
			IsDlgActived() || 
			gameLogic.LogicPhase != GamePhase.Countdown || 
			gameLogic.IsLock ||
			!gameLogic.bCanBet)
			return;

		string[] series = new string[]{""};
//		if (GameData.GetInstance().maxNumberOfFields == 37)
//		{
//			
//		}
//		else
//		{
//			
//		}
		if (name == SeriesName.BigSeries)
			series = biSeries37;
		else if (name == SeriesName.SmallSeries)
			series = smallSeries37;
		else if (name == SeriesName.Orphaline)
			series = orphaline37;
		else if (name == SeriesName.ZeroSpiel)
			series = zeroSpiel37;
		
		int bet = GameData.GetInstance().betChipValues[curChipIdx];
		foreach (string key in series)
		{
			if (string.Compare("0-2-3", key) == 0 ||
				string.Compare("25-26-28-29", key) == 0)
			{
				GameEventManager.OnFieldClick(key, bet * 2);
				continue;
			}
			GameEventManager.OnFieldClick(key, bet);
		}
		SetDisplay();
	}
}
