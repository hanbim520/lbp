using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

// 通过串口与金手指通讯
public class GoldfingerUtils : MonoBehaviour
{
	private AndroidSerialPort sp;		// 金手指通讯

	private const float kRevGoldFingerDataInterval	= 0.1f;
	private float revGoldFingerDataElapsed			= 0f;
	private const float kParseDataInterval			= 0.05f;
	private float parseDataElapsed					= 0f;

	private int phase = 0; 
	public const int kPhaseStartBlowBall 			= 1;
	public const int kPhaseEndBlowBall 				= 2;
	public const int kPhaseOpenGate 				= 3;
	public const int kPhaseCloseGate 				= 4;
	public const int kPhaseDetectBallValue 			= 5;
	public const int kPhaseDetectBallValue2			= 6;	// 第2次认球

	private const int kMaxCOM2DataLength			= 18;
	private bool bBlowedBall = false;
	private bool bOpenGate = false;
	private bool bEnterBackend = false;
	private bool bOpenKey = false;
	private bool bKeyout  = false;

	private int iHight 			= 0;
	private int iLow 			= 0;
	private int iBlowOrDoor 	= 0;
	private int iCellNum 		= 0;
	private int iLight			= 0;		// 中奖灯
	private int iPayCoin 		= 0;		// 退币信号
	private int iPayCoinHight 	= 0;		// 退币高位
	private int iPayCoinLow		= 0;		// 退币低位
    private int iLastPaySum     = 0;

    private int iRevCoin        = 0;        // 停止收币信号
    private int iLastRevCoin    = 0;

	private	int iRevValRound	= 0;		// 第1次收到球号时的圈数
	private int iRevValCell		= 0;		// 第1次收到球号时的格数
	private int iFirstBallVal	= 0;		// 第1次认球的球号

	private Timer timerHeartBeat;			// 用来检测断连的计时器
	private float lightElapsed	= 0;		// 中奖灯闪烁计时
	private const float lightDuration = 0.4f; // 中奖灯闪烁间隔
	private bool bTurnOnLight	= false;

	private int realtimeBallVal = 0;		// 大于0表示孔里有球
	private bool bCheckBallFall = false;	// 检查轨道上是否有球

	private float kHoldKeyinDur = 0.4f;		// 长按上分时间
	private float holdKeyinDelta= 0.0f;		// 长按上分键计时
	private bool bHoldKeyin		= false;	// 长按上分键成立

	bool isConntected = false;

	void Start()
	{
		DontDestroyOnLoad(this);
		OpenCOM();
		GameEventManager.WinLightSignal += WinLightSignal;
		GameEventManager.StartCountdown += StartCountdown;
		GameEventManager.EndCountdown += EndCountdown;
	}

	void OnDestroy()
	{
		GameEventManager.WinLightSignal -= WinLightSignal;
		GameEventManager.StartCountdown -= StartCountdown;
		GameEventManager.EndCountdown -= EndCountdown;
		CloseCOM();
	}

	private void OpenCOM()
	{
		sp = new AndroidSerialPort("/dev/ttyS2", 115200, Parity.None, 8, StopBits.One);
		sp.Open();
		if (sp.GetPortId() >= 0)
		{
			isConntected = true;
			StartCoroutine(AfterConnHID());
		}
		else
		{
			GameEventManager.OnHIDDisconnected();
		}
	}

	public void CloseCOM()
	{
		isConntected = false;
		if (sp != null)
			sp.Close();
	}

	private IEnumerator AfterConnHID()
	{
		string name = GameData.GetInstance().deviceIndex == 1 ? "ServerLogic" : "ClientLogic";
		GameObject logic = GameObject.Find(name);
		while(logic == null)
		{
			// 等待进入Main场景
			logic = GameObject.Find(name);
			yield return new WaitForSeconds(1);
		}
		// 执行验证和开门操作
		GameEventManager.OnHIDConnected();
	}

	void Update()
	{
		if (!isConntected)
			return;

		UpdateTimers();

		parseDataElapsed += Time.deltaTime;
		if (parseDataElapsed > kParseDataInterval)
		{
			ParseGoldfingerData();
			parseDataElapsed = 0;
		}

		revGoldFingerDataElapsed += Time.deltaTime;
		if (revGoldFingerDataElapsed > kRevGoldFingerDataInterval)
		{
			RevGoldfingerData();
			revGoldFingerDataElapsed = 0;
		}

		FlashWinLight();
	}

	private void UpdateTimers()
	{
		// 检测断连
		if (timerHeartBeat != null)
			timerHeartBeat.Update(Time.deltaTime);
	}

	// 已经与COM2断开
	private void HeartBeatOver()
	{
		timerHeartBeat = null;
		GameEventManager.OnBreakdownTip(BreakdownType.USBDisconnect);
	}

	private void ParseGoldfingerData()
	{
		int[] data = sp.ReadData();
		if (data == null || data[0] == -1)
		{
			if (timerHeartBeat == null &&
			    GameData.GetInstance().deviceIndex == 1)
			{
				timerHeartBeat = new Timer(10.0f, 0);
				timerHeartBeat.Tick += HeartBeatOver;
				timerHeartBeat.Start();
			}
			return;
		}

		if (timerHeartBeat != null)
		{
			timerHeartBeat.Stop();
			timerHeartBeat = null;
		}

		if (data.Length >= kMaxCOM2DataLength)
		{
			if (data[0] == 0xA5 && data[1] == 0x58 && data[2] == 0x57)
			{
				PrintData(ref data, true);
				// 校验数据
				int[] temp = new int[24];
				System.Array.Copy(data, 1, temp, 0, 24);
				if (data[25] != Utils.CrcAddXor(temp, 24))
				{
					// 校验不通过
//					DebugConsole.Log("校验不通过");
					return;
				}

				if (bCheckBallFall)
				{
					realtimeBallVal = data[9];
				}
				// 吹风
				if (data[4] == 0x55)
				{
					if (!bBlowedBall)
					{
						phase = kPhaseStartBlowBall;
						bBlowedBall = true;
						PrintData(ref data);
					}
				}
				else
				{
					if (bBlowedBall && phase == kPhaseStartBlowBall)
					{
						phase = kPhaseEndBlowBall;
						bBlowedBall = false;
						GameEventManager.OnEBlowBall();
					}
				}
				// 开门
				if (data[5] == 0x55)
				{
					if (!bOpenGate)
					{
						phase = kPhaseOpenGate;
						bOpenGate = true;
						PrintData(ref data);
					}
				}
				else
				{
					if (bOpenGate && phase == kPhaseOpenGate)
					{
						phase = kPhaseCloseGate;
						bOpenGate = false;
						realtimeBallVal = 0;
						GameEventManager.OnCloseGate();
					}
				}
				// 认球
				if (phase == kPhaseEndBlowBall)
				{
					// 结果
					int idx = data[6];
					if (idx > 0)
					{
						PrintData(ref data);
						idx -= 1;
						phase = kPhaseDetectBallValue;
//						if (GameData.GetInstance().maxNumberOfFields == 38)
//							GameEventManager.OnBallValue(GameData.GetInstance().ballValue38[idx]);
//						else if (GameData.GetInstance().maxNumberOfFields == 37)
//							GameEventManager.OnBallValue(GameData.GetInstance().ballValue37[idx]);

						iRevValRound = data[7];
						iRevValCell = data[8];
						if (GameData.GetInstance().maxNumberOfFields == 38)
							iFirstBallVal = GameData.GetInstance().ballValue38[idx];
						else if (GameData.GetInstance().maxNumberOfFields == 37)
							iFirstBallVal = GameData.GetInstance().ballValue37[idx];
					}
				}
                // 二次认球
				if (phase == kPhaseDetectBallValue)
				{
					int deltaCell = data[8] - iRevValCell;
                    int deltaRound = Mathf.Abs(data[7] - iRevValRound);
                    if ((deltaRound >= 1 && deltaCell >= 0) ||
                        (deltaRound >= 2))
					{
						int idx = data[6];
						if (idx > 0)
						{
							idx -= 1;
							phase = kPhaseDetectBallValue2;
							int secondBallVal = -1;
							if (GameData.GetInstance().maxNumberOfFields == 38)
								secondBallVal = GameData.GetInstance().ballValue38[idx];
							else if (GameData.GetInstance().maxNumberOfFields == 37)
								secondBallVal = GameData.GetInstance().ballValue37[idx];
							if (secondBallVal == iFirstBallVal)
							{
								if (GameData.GetInstance().maxNumberOfFields == 38)
									GameEventManager.OnBallValue(GameData.GetInstance().ballValue38[idx]);
								else if (GameData.GetInstance().maxNumberOfFields == 37)
									GameEventManager.OnBallValue(GameData.GetInstance().ballValue37[idx]);
							}
							else
							{
								GameEventManager.OnBreakdownTip(BreakdownType.RecognizeBall);
							}
						}
					}
				}
//				if (data[10] != 0)	// 投币
//				{
//					GameEventManager.OnReceiveCoin(data[10]);
//				}
//				if (data[11] != 0)	// 退币
//				{
//					GameEventManager.OnPayCoinCallback(data[11]);
//				}
				if ((data[12] & 0x01) == 0x01)	// 物理退币按键按下
				{
					GameEventManager.OnPayCoin();
				}
//				if (!bOpenKey && data[13] == 0x20)			// 功能菜单(原上分)
				if ((data[13] & 0x20) == 0x20)						// 物理键上分(17A)
				{
					bOpenKey = true;
					// 弹出功能菜单
//					GameEventManager.OnOpenKey();
					if (!bHoldKeyin)
					{
						holdKeyinDelta += Time.deltaTime;
//						DebugConsole.Log(holdKeyinDelta.ToString());
						if (holdKeyinDelta > kHoldKeyinDur)
						{
							bHoldKeyin = true;
							// 长按上分
							GameEventManager.OnKeyinHold();
						}
					}
				}
				else if (bOpenKey && ((data[13] & 0x20) == 0))
				{
					bOpenKey = false;

					holdKeyinDelta = 0;
					if (!bHoldKeyin)
					{
						// 短按上分
						GameEventManager.OnKeyinOnce();
					}
					else
					{
						bHoldKeyin = false;
						// 长按上分
//						GameEventManager.OnKeyinHold();
					}
				}
                if(!bKeyout && ((data[14] & 0x20) == 0x20))			// 物理键下分(25A)
				{
					bKeyout = true;
					GameEventManager.OnKeout();
				}
				else if (bKeyout && data[14] == 0)
				{
					bKeyout = false;
				}

				if (!bEnterBackend && ((data[13] & 0x40) == 0x40))		// 设置按键(原触屏版设置)
				{
					// 进后台前输入密码
//					GameEventManager.OnEnterBackend();
					// 弹出功能菜单
					GameEventManager.OnOpenKey();
				}
				else if (bEnterBackend && ((data[13] & 0x40) == 0))
				{
					bEnterBackend = false;
				}
				if (data[15] != 0)							// 拨码开关sw1
				{

				}
				if (data[16] != 0)							// 拨码开关sw2
				{

				}
                int totalRevCoin = data[17] | (data[18] << 8) | (data[19] << 16) | (data[20] << 24);
                if (totalRevCoin > iLastRevCoin)
                {
                    int deltaRev = totalRevCoin - iLastRevCoin;
                    GameEventManager.OnReceiveCoin(deltaRev);       // 投币
                }
                else
                {
                    GameEventManager.OnDetectRevCoinError();
                }
                iLastRevCoin = totalRevCoin;

                int totalPayCoin = data[21] | (data[22] << 8) | (data[23] << 16) | (data[24] << 24);
                if (iPayCoin == 1)                                  // 退币
                {
                    int deltaPay = totalPayCoin - iLastPaySum;
                    if (deltaPay > 0)
                    {
                        GameEventManager.OnPayCoinCallback(deltaPay);
                    }
                    else
                    {
                        GameEventManager.OnDetectPayCoinError();
                    }
                }
                iLastPaySum = totalPayCoin;

			}
			else
			{
//				DebugConsole.Log("不合格");
			}
		}
	}

	private void RevGoldfingerData()
	{
		int[] outData = new int[]{
			0xD5, 0x58, 0x57, 14, iBlowOrDoor,
			iHight, iLow, iCellNum, iPayCoin, iPayCoinHight,
			iPayCoinLow, 0, 0, 0, 0,
            iRevCoin, 0, iLight, 0, 0, 0, 0, 0, 0};
		int[] temp = new int[20];
		System.Array.Copy(outData, 1, temp, 0, 20);
		int crc = Utils.CrcAddXor(temp, 20);
		outData[21] = crc;
		sp.WriteData(ref outData);
//        DebugConsole.Log(Utils.ToString(ref outData));

		iHight = 0;
		iLow = 0;
		iBlowOrDoor = 0;
		iCellNum = 0;
		iPayCoinHight = 0;
		iPayCoinLow = 0;
        iRevCoin = 0;
	}

	// 开门
	public void OpenGate()
	{
		iBlowOrDoor = 2;
		iHight = 0x0E;
		iLow = 0xA6;
	}

	// 吹风
	public void BlowBall(int blowTime)
	{
		iBlowOrDoor = 3;
		iHight = blowTime >> 8 & 0xff;
		iLow = blowTime & 0xff;
		Utils.Seed(System.DateTime.Now.Millisecond + System.DateTime.Now.Second);
		// 控制吹风在轮盘转到第几个格子后启动
		iCellNum = Utils.GetRandom(1, GameData.GetInstance().maxNumberOfFields);

//		DebugConsole.Log("BlowBall: " + blowTime);
	}

	public void PayCoin(int coinNum)
	{
		iPayCoin = 1;
		iPayCoinHight = coinNum >> 8 & 0xff;
		iPayCoinLow = coinNum & 0xff;
	}

	// 没有加密芯片 不用验证
	public void SendCheckInfo()
	{

	}

	public void StopPayCoin()
	{
		iPayCoin = 0;
		iPayCoinHight = 0;
		iPayCoinLow = 0;
	}

	public void WinLightSignal(int signal)
	{
		iLight = signal;
		bTurnOnLight = signal == 1;
	}

	// 闪烁中奖灯
	private void FlashWinLight()
	{
		if (bTurnOnLight)
		{
			lightElapsed += Time.deltaTime;
			if (lightElapsed > lightDuration)
			{
				lightElapsed = 0;
				iLight = iLight == 1 ? 0 : 1;
			}
		}
	}

	public int GetRealtimeBallVal()
	{
		return realtimeBallVal;
	}

	private void StartCountdown()
	{
		bCheckBallFall = true;
	}

	private void EndCountdown()
	{
		bCheckBallFall = false;
	}

	private void PrintData(ref int[] data, bool bEvent = false)
	{
//		string log = "data.Length:" + data.Length + "--";
//		for (int i = 0; i < data.Length; ++i)
//		{
//			if (i > 0 && i % 20 == 0)
//				log += "\n";
//			log += string.Format("{0:X}", data[i]) + ", ";
//		}
//		if (!bEvent)
//		DebugConsole.Log(log);
//		else
//		GameEventManager.OnDebugLog(0, log);
	}

    public void StopRevCoin()
    {
        iRevCoin = 1;
    }

//	void OnGUI()
//	{
//		if (GUI.Button(new Rect(250, 200, 150, 100), "开门"))
//		{
//			OpenGate();
//		}
//		
//		if (GUI.Button(new Rect(250, 350, 150, 100), "吹风"))
//		{
//			Utils.Seed(System.DateTime.Now.Millisecond + System.DateTime.Now.Second + System.DateTime.Now.Minute + System.DateTime.Now.Hour);
//			int time = GameData.GetInstance().gameDifficulty + Utils.GetRandom(1200, 3000);
//			BlowBall(time);
//		}
//	}
}
