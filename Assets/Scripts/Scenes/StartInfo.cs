using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartInfo : MonoBehaviour
{
	public Text txtWarning;
	public GameObject objDlgWarning;
	public GameObject[] objCircalRecords;
	
	void Start()
	{
		SetRouletteType();
		InitLoading();
//		CheckEncryChip();
	}

	// 校验加密芯片
	private void CheckEncryChip()
	{
		// 获取加密芯片的UUID
		EncryptChip.encryption_chip_open ();
		byte[] uuidArray = new byte[255];
		int uuidLen = EncryptChip.encryption_chip_get_uuid (uuidArray, 255);
		if (uuidLen <= 0)
		{
			// 没有检测到加密芯片
			objDlgWarning.SetActive(true);
			txtWarning.text = "Error: Cannot find encry chip!";
		}
		else 
		{
			string localUUID = GameData.GetInstance().EncryChipUUID;
			string temp = "";
			for (int i = 0; i < uuidLen; i++) 
			{
				temp += string.Format ("{0:X2}", uuidArray [i]);
			}
			if (string.IsNullOrEmpty(localUUID))
			{
				GameData.GetInstance().EncryChipUUID = temp;
				InitLoading();
			}
			else
			{
				if (string.Compare(temp, localUUID) == 0)
				{
					InitLoading();
				}
				else
				{
					objDlgWarning.SetActive(true);
					txtWarning.text = "Error: Encry chip is invalid!";
				}
			}
		}
	}

	// 初始化加载各种工具
	private void InitLoading()
	{
		GameData.GetInstance().ReadDataFromDisk();
		LoadUpdateUtils();
		//		GameData.GetInstance().deviceIndex = 101;
		if (GameData.GetInstance().deviceIndex > 0 && 
		    GameData.GetInstance().deviceIndex < GameData.GetInstance().monitorDeviceIndex)
		{
			Screen.SetResolution(GameData.GetInstance().resolutionWidth, GameData.GetInstance().resolutionHeight, true);
			//			LoadInputDevice();
			LoadHIDUtils();
			LoadNetwork();
			//			TextDB.LoadFile();
			
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
}
