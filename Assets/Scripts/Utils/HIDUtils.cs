using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public class HIDUtils : MonoBehaviour
{
	private int vid = 0x0483;
	private int pid = 0x5750;

	public const int kPhaseStartBlowBall = 1;
	public const int kPhaseEndBlowBall = 2;
	public const int kPhaseOpenGate = 3;
	public const int kPhaseCloseGate = 4;
	public const int kPhaseDetectBallValue = 5;

#if UNITY_ANDROID
	private AndroidJavaClass jc;
	private AndroidJavaObject jo;
#endif
	private bool isReadThreadExit = false;
	private Thread tRead;
//	private Queue<int> readQueue = new Queue<int>();
	private SafedQueue<int> readQueue = new SafedQueue<int>();
	// In seconds
	private const float getDataTime = 0.1f;
	private float getDataTimeDelta = 0;
	private const float kReceiveFromHIDTime = 0.1f;
	private float receiveFromHIDInterver = 0.0f;
	private const int kReadDataLength = 59;
	private const int kBlowBall = 0x55;
	private const int kOpenGate = 0x55;
	private const int kPreviousValue = 0x55;
	private int flagPayCoin = 0;

	private Timer timerHeartBeat;		// 用来检测断连的计时器
	private Timer timerCheckInfo;		// 用来验证的计时器
	private bool bCheckInfo = true;		// 验证是否开始
	private bool bBlowedBall = false;
	private bool bOpenGate = false;
	private int phase = 0; 
	private bool isOpen = false;
	public bool IsOpen
	{
		get { return isOpen; }
		set { isOpen = value; }
	}

	void Start()
	{
		DontDestroyOnLoad(this);
		InitData();
	}

	void OnDestroy()
	{
		CloseUSB();
	}

	private void UpdateTimers()
	{
		// 检测断连
		if (timerHeartBeat != null)
			timerHeartBeat.Update(Time.deltaTime);
		// 等待加密片验证结束
		if (timerCheckInfo != null)
			timerCheckInfo.Update(Time.deltaTime);
	}

	void Update()
	{
		UpdateTimers();
		if (bCheckInfo)
			return;

		getDataTimeDelta += Time.deltaTime;
		if (getDataTimeDelta >= getDataTime)
		{
			getDataTimeDelta = 0;
			ReadUsbPort();
		}

		receiveFromHIDInterver += Time.deltaTime;
		if (receiveFromHIDInterver >= kReceiveFromHIDTime)
		{
			receiveFromHIDInterver = 0;
			ReceiveDataFromHID();
		}
	}

	private void InitData()
	{
	
#if UNITY_ANDROID
			jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
			jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
#endif
		
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
			OpenUSB();
#endif
	}

	public void SetState(string value)
	{
		Debug.Log("SetState:" + value);
		bool state;
		if (bool.TryParse(value, out state))
		{
			isOpen = state;
			if (isOpen)
			{
				StartCoroutine(AfterConnHID());
			}
			else
			{
				GameEventManager.OnHIDDisconnected();
			}
		}
	}

	protected IEnumerator AfterConnHID()
	{
//		if (GameData.GetInstance().deviceIndex <= 0)
//			yield break;

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

	public void OpenUSB()
	{
#if UNITY_ANDROID
		int ret = jo.Call("openUsb");
		if (ret > 0)
		{
			SetState("True");
		}
#endif

#if UNITY_STANDALONE_WIN
		int ret = WinUsbPortOpen();
		Debug.Log("WinUsbPortOpen:" + ret);
		Debug.Log("GUID:" + GameData.GetInstance().deviceGuid);
		if (ret == 0)
		{
			SetState("True");
			isReadThreadExit = false;
			tRead = new Thread(QueueStore);
			tRead.Start();
			SendCheckInfo();
		}
#endif

#if UNITY_STANDALONE_LINUX
		int ret = LinuxUsbPortOpen();
		Debug.Log("LinuxUsbPortOpen: " + ret);
		if (ret == 1)
		{
			SetState("True");
			isReadThreadExit = false;
			tRead = new Thread(QueueStore);
			tRead.Start();
		}
		else
		{
			SetState("False");
		}
#endif
	}

	public void CloseUSB()
	{
		isReadThreadExit = true;
		tRead.Abort();
#if UNITY_ANDROID
		jo.Call("closeUsb");
#endif

#if UNITY_STANDALONE_WIN
		WinUsbPortClose();
#endif

#if UNITY_STANDALONE_LINUX
		LinuxUsbPortClose();
#endif
	}

	public void DebugLog(string msg)
	{
		DebugConsole.Log(msg);
	}

	public int[] ReadData(string methodName)
	{
#if UNITY_ANDROID
			AndroidJavaObject rev = jo.Call<AndroidJavaObject>(methodName);
			return AndroidJNIHelper.ConvertFromJNIArray<int[]>(rev.GetRawObject());
#endif
		return null;
	}

	public void ReadUsbPort()
	{
#if UNITY_ANDROID
		int[] data = ReadData("readHID");
#endif

#if UNITY_STANDALONE_LINUX || UNITY_STANDALONE_WIN
		if (!isOpen)
			return;

		int[] data = QueueRead();
#endif
		if (data == null || data[0] == -1)
		{ 
			if (timerHeartBeat == null)
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

		if (data.Length >= kReadDataLength)
		{
			PrintData(ref data);
            // 机芯指令
            if (data[0] == 0x58 && data[1] == 0x57)
            {
                // 吹风
                if (data[2] == kBlowBall)
                {
                    if (!bBlowedBall)
                    {
                        phase = kPhaseStartBlowBall;
                        bBlowedBall = true;
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
                if (data[3] == kOpenGate)
                {
                    if (!bOpenGate)
                    {
                        phase = kPhaseOpenGate;
                        bOpenGate = true;
                    }
                }
                else
                {
                    if (bOpenGate && phase == kPhaseOpenGate)
                    {
                        phase = kPhaseCloseGate;
                        bOpenGate = false;
                        GameEventManager.OnCloseGate();
                    }
                }
                // 不是上轮结果
				if (data[5] != kPreviousValue && phase == kPhaseEndBlowBall)
                {
					// 结果
					int idx = data[4];
					if (idx == 0)
						return;
					else
						idx -= 1;
					phase = kPhaseDetectBallValue;
					if (GameData.GetInstance().maxNumberOfFields == 38)
						GameEventManager.OnBallValue(GameData.GetInstance().ballValue38[idx]);
					else if (GameData.GetInstance().maxNumberOfFields == 37)
						GameEventManager.OnBallValue(GameData.GetInstance().ballValue37[idx]);
                }
				else if (data[5] == kPreviousValue && phase == kPhaseEndBlowBall)
				{
					GameEventManager.OnBreakdownTip(BreakdownType.RecognizeBall);
				}
				if (data[9] != 0)	// 投币
				{
					GameEventManager.OnReceiveCoin(data[9]);
				}
				if (data[10] != 0)	// 退币
				{
					GameEventManager.OnPayCoinCallback(data[10]);
				}
				if (data[11] != 0)	// 投币
				{
					int coin = 0;
					int sign = data[11];
					if (sign == 0x40)
						coin = 1;
					else if (sign == 0x41)
						coin = 5;
					else if (sign == 0x42)
						coin = 10;
					else if (sign == 0x43)
						coin = 20;
					else if (sign == 0x44)
						coin = 100;
					else if (sign == 0x45)
						coin = 50;
					GameEventManager.OnReceiveCoin(coin);
				}
				if (data[12] != 0)	// 纸币器error号
				{
					Debug.Log("Bill Acceptor Happen Exception:" + data[12]);
				}
				if (data[54] != 0)	// 物理钥匙(原上分)
				{
					GameEventManager.OnOpenKey();
				}
				if (data[55] != 0)	// 触摸屏校验(原下分)
				{
					GameEventManager.OnChangeScene(Scenes.TouchCheck);
				}
            }
            // 报账指令
            else if (data[0] == 0x42 && data[1] == 0x5a)
            {
//                PrintData(ref data);
				List<byte> col = new List<byte>();
				for (int i = 3; i < 17; ++i)
					col.Add((byte)data[i]);
				byte[] sendData = col.ToArray();
#if UNITY_ANDROID
				IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(sendData);
                jvalue[] blah = new jvalue[1];
                blah[0].l = pArr;

                IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "GetCheckPWStringValue");
				// 获取打码结果
				string result = AndroidJNI.CallStringMethod(jo.GetRawObject(), methodId, blah);
#endif

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
				IntPtr ret = EncryChip.ParserCheckData(sendData);
				string result = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ret);
				EncryChip.FreeByteArray(ret);
#endif
				char[] split = {':'};
				string[] word = result.Split(split);
				if (word.Length >= 2)
				{
					int value, days;
					if (int.TryParse(word[0], out value))
					{
						// 打码正确
						if (value == 1)
						{
							if (int.TryParse(word[1], out days))
								GameData.GetInstance().remainMins = 24 * 60 * days;
							GameEventManager.OnPrintCodeSuccess();
						}
						else
							GameEventManager.OnPrintCodeFail();
					}
				}
            }
		}
		else
		{
			PrintData(ref data);
		}
	}

	public int WriteData(int[] data, string methodName)
	{
#if UNITY_ANDROID
		IntPtr pArr = AndroidJNIHelper.ConvertToJNIArray(data);
		jvalue[] blah = new jvalue[1];
		blah[0].l = pArr;
		
		IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), methodName);
		return AndroidJNI.CallIntMethod(jo.GetRawObject(), methodId, blah);
#endif

#if UNITY_STANDALONE_WIN
//		print("write:" + WinUsbPortWrite(data));
		if (!isOpen)
			return 0;
		return WinUsbPortWrite(data);
#endif

#if UNITY_STANDALONE_LINUX
		if (!isOpen)
			return -1;
		return LinuxUsbPortWrite(data);
#endif
	}

	public void OpenGate()
	{
		int[] data = new int[]{0x58, 0x57, 0x02, 0x0E, 0xA6, flagPayCoin, 0, 0, 
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0};
		WriteData(data, "writeUsbPort");
	}

	public void BlowBall(int blowTime)
	{
		int hight = blowTime >> 8 & 0xff;
		int low = blowTime & 0xff;

		Utils.Seed(System.DateTime.Now.Millisecond + System.DateTime.Now.Minute);
		// 控制吹风在轮盘转到第几个格子后启动
		int cellNum = Utils.GetRandom(1, GameData.GetInstance().maxNumberOfFields);

		int[] data = new int[]{0x58, 0x57, 0x03, hight, low, flagPayCoin, 0, 0, 
			cellNum, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0};
		WriteData(data, "writeUsbPort");
	}

	private void PrintData(ref int[] data)
	{
		string log = "data.Length:" + data.Length + "--";
		for (int i = 0; i < data.Length; ++i)
		{
			log += string.Format("{0:X}", data[i]) + ", ";
		}
//		Debug.Log(log);
//		DebugConsole.Log(log);
		GameEventManager.OnDebugLog(0, log);
	}

	public void SendCheckInfo()
	{
		int minCapacity = 64;
		char[] arr = GameData.GetInstance().deviceGuid.ToCharArray();
		List<int> dataList = new List<int>();
		// 0x595a 验证
		dataList.Add(0x59);
		dataList.Add(0x5a);
		dataList.Add(arr.Length);
		for (int i = 0; i < arr.Length; ++i)
		{
			dataList.Add((int)arr[i]);
		}
		while (dataList.Count < minCapacity)
		{
			dataList.Add(0);
		}
		int[] data = dataList.ToArray();
		WriteData(data, "writeUsbPort");

		bCheckInfo = true;
		timerCheckInfo = new Timer(2.0f, 0);
		timerCheckInfo.Tick += CheckInfoOver;
		timerCheckInfo.Start();
	}

	// timerCheckInfo 结束
	private void CheckInfoOver()
	{
		timerCheckInfo = null;
		bCheckInfo = false;
	}

	// timerHeartBeat 结束
	private void HeartBeatOver()
	{
		timerHeartBeat = null;
		GameEventManager.OnBreakdownTip(BreakdownType.USBDisconnect);
	}
	
	private void ReceiveDataFromHID()
	{
		int[] data = new int[]{0x58, 0x57, 0, 0, 0, flagPayCoin, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0};
		WriteData(data, "writeUsbPort");
	}

	// 设置退币指令位
	public void PayCoin(int count)
	{
		flagPayCoin = 1;
		int hight = count >> 8 & 0xff;
		int low = count & 0xff;
		int[] data = new int[]{0x58, 0x57, 0, 0, 0, flagPayCoin, hight, low,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0};
		WriteData(data, "writeUsbPort");
	}

	public void StopPayCoin()
	{
		flagPayCoin = 0;
	}

	// return: 0 success
	private int WinUsbPortOpen()
	{
		return WinHidPort.OpenHid(vid, pid);
	}

	private void QueueStore()
	{
		try
		{
			while(!isReadThreadExit)
			{
				#if UNITY_STANDALONE_LINUX
				int[] data = LinuxUsbPortRead();
				#endif
				#if UNITY_STANDALONE_WIN
				int[] data = WinUsbPortRead();
				#endif
				if (data == null || data[0] == -1)
					continue;
				foreach (int d in data)
				{
					readQueue.Enqueue(d);
				}
			}
		}
		catch(Exception ex)
		{
			Debug.Log(ex.ToString());
		}
	}

	private int[] QueueRead()
	{
		if (readQueue.Count > 0)
		{
			int[] data = readQueue.ToArray();
			readQueue.Clear();
			return data;
		}
		else
			return null;
	}

	private void WinUsbPortClose()
	{
		WinHidPort.CloseHid();
	}

	private int[] WinUsbPortRead()
	{
		byte[] data = WinHidPort.Read();
		int len = data.Length;
		int[] array = new int[len];
		for (int i = 0; i < len; ++i)
			array[i] = data[i];
		return array;
	}

	private int WinUsbPortWrite(int[] data)
	{
		int len = data.Length - 3;
		byte[] buffer = new byte[len];
		for (int i = 0; i < len; ++i)
			buffer[i] = (byte)data[i];
		return WinHidPort.Write(ref buffer, len);
	}

	private int LinuxUsbPortOpen()
	{
		return LinuxHidPort.Open(vid, pid);
	}

	private void LinuxUsbPortClose()
	{
		LinuxHidPort.Close();
	}

	private int[] LinuxUsbPortRead()
	{
		byte[] data = LinuxHidPort.Read();
		int len = data.Length;
		int[] array = new int[len];
		for (int i = 0; i < len; ++i)
			array[i] = data[i];
		return array;
	}

	private int LinuxUsbPortWrite(int[] data)
	{
		int len = data.Length - 3;
		byte[] buffer = new byte[len];
		for (int i = 0; i < len; ++i)
			buffer[i] = (byte)data[i];
		return LinuxHidPort.Write(ref buffer, len);
	}

//	void OnGUI()
//	{
//		if (GUI.Button(new Rect(10, 10, 100, 50), "Touch Check"))
//		{
//			Application.LoadLevel("TouchCheck");
//		}
//	}
}
