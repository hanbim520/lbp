using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StartInfo : MonoBehaviour
{
	public Text txtWarning;
	public GameObject objDlgWarning;
	public GameObject[] objCircalRecords;
	public Text tipPrintCode;
	public GameObject objPrinCode;

	float tipElapsed = 0;
	bool isTipShowed = false;

	void Start()
	{
		GameData.GetInstance().ReadDataFromDisk();
		if (GameData.GetInstance().deviceIndex < GameData.GetInstance().monitorDeviceIndex)
		{
			LoadNetwork();
			LoadHIDUtils();
		}

		GameEventManager.PrintCodeSuccess += PrintCodeSuccess;
		GameEventManager.PrintCodeFail += PrintCodeFail;
		SetRouletteType();
		int deviceIndex = GameData.GetInstance().deviceIndex;
		// 判断是否要打码
		if (GameData.controlCode)
		{
			int resetAccount = PlayerPrefs.GetInt("ResetAccount", 0);
			if (resetAccount > 0)
			{
				PlayerPrefs.SetInt("ResetAccount", 0);
				PlayerPrefs.Save();
			}

			long remain = ExpiredTicks();
			if (remain <= 0 ||
				resetAccount > 0)
			{
				tipPrintCode.text = string.Format("Machine Is Expired, Please Unlock!");
				if (deviceIndex == 1)
				{
					// 打码
					objPrinCode.SetActive(true);
					return;
				}
			}
			// 显示剩余时间(分钟)
			int mins = (int)new System.TimeSpan(remain).TotalMinutes;
			tipPrintCode.text = string.Format("Machine Expires After: {0} mins", mins);
		}

		InitLoading();
	}

	void OnDestroy()
	{
		GameEventManager.PrintCodeSuccess -= PrintCodeSuccess;
		GameEventManager.PrintCodeFail -= PrintCodeFail;
	}

	void PrintCodeSuccess(int type)
	{
		if (type == 40000)	// 清账
		{
			string msg = NetInstr.ClearAccount.ToString();
			UHost host = GameObject.Find("NetworkObject").GetComponent<UHost>();
			host.SendToAll(msg);
		}
		objPrinCode.SetActive(false);
		InitLoading();
	}

	void PrintCodeFail()
	{
		objDlgWarning.SetActive(true);
		txtWarning.text = "Unlock Machine Failed!";
		isTipShowed = true;
	}

	void Update()
	{
		if (!isTipShowed)
			return;

		tipElapsed += Time.deltaTime;
		if (tipElapsed > 2.0f)
		{
			isTipShowed = false;
			tipElapsed = 0;
			objDlgWarning.SetActive(false);
		}
	}

	// 初始化加载各种工具
	private void InitLoading()
	{
		LoadUpdateUtils();

		StartCoroutine(NextScene(GameData.controlCode ? 1.5f : 0));
	}

	IEnumerator NextScene(float delay)
	{
		yield return new WaitForSeconds(delay);
		//		GameData.GetInstance().deviceIndex = 101;
		if (GameData.GetInstance().deviceIndex > 0 && 
			GameData.GetInstance().deviceIndex < GameData.GetInstance().monitorDeviceIndex)
		{
			Screen.SetResolution(GameData.GetInstance().resolutionWidth, GameData.GetInstance().resolutionHeight, true);
			//			LoadInputDevice();
			//			TextDB.LoadFile();
			LoadBVA();

			GameData.GetInstance().NextLevelName = Scenes.Main;
			UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.Loading);
		}
		else if (GameData.GetInstance().deviceIndex >= GameData.GetInstance().monitorDeviceIndex)
		{
			GameData.GetInstance().NextLevelName = Scenes.TopStatistics;
			UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.Loading);
		}
		else
		{
			Screen.SetResolution(GameData.GetInstance().resolutionWidth, GameData.GetInstance().resolutionHeight, true);
			UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.Backend);
		}
	}

	private void LoadUpdateUtils()
	{
		if (GameObject.Find("UpdateUtils") == null)
		{
			Object prefab = (Object)Resources.Load("Update/UpdateUtils");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "UpdateUtils";
			prefab = null;
		}
	}

	private void LoadInputDevice()
	{
		if (GameObject.Find("InputDevice") == null)
		{
			Object prefab = (Object)Resources.Load("Input/InputDevice");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "InputDevice";
			prefab = null;
		}
	}

	private void LoadHIDUtils()
	{
#if UNITY_STANDALONE_LINUX
		if (GameObject.Find("HIDUtils") == null)
		{
			Object prefab = (Object)Resources.Load("Input/HIDUtils");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "HIDUtils";
			prefab = null;
		}
#endif
#if UNITY_ANDROID
		if (GameObject.Find("AndroidHIDUtils") == null)
		{
			Object prefab = (Object)Resources.Load("Input/AndroidHIDUtils");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "AndroidHIDUtils";
			prefab = null;
		}
#endif
	}

	private void LoadNetwork()
	{
		if (GameObject.Find("NetworkObject") == null)
		{
			Object prefab = (Object)Resources.Load("Network/NetworkObject");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "NetworkObject";
			prefab = null;
		}
	}

	// 显示正确Logo
	void SetRouletteType()
	{
		if (GameData.rouletteType == RouletteType.Standard)
		{
			objCircalRecords[0].SetActive(true);
			objCircalRecords[1].SetActive(false);
		}
		else if (GameData.rouletteType == RouletteType.Special1)
		{
			objCircalRecords[0].SetActive(false);
			objCircalRecords[1].SetActive(true);
		}
	}

	void LoadBVA()
	{
		#if UNITY_ANDROID
		if (GameObject.Find("BVAAndroid") == null)
		{
			Object prefab = (Object)Resources.Load("BVA/BVAAndroid");
			GameObject go = (GameObject)Instantiate(prefab);
			go.name = "BVAAndroid";
			prefab = null;
		}
		#endif
	}

	// 返回剩余的打码时间（单位tick)
	long ExpiredTicks()
	{
		string date = PlayerPrefs.GetString("ExpiredDate", string.Empty);
		if (string.IsNullOrEmpty(date))
		{
			System.TimeSpan ts = new System.TimeSpan(7, 0, 0, 0);
			long next = System.DateTime.Now.Ticks + ts.Ticks;
			PlayerPrefs.SetString("ExpiredDate", next.ToString());
			PlayerPrefs.Save();
			return ts.Ticks;
		}

		return long.Parse(date) - System.DateTime.Now.Ticks;
	}
}
