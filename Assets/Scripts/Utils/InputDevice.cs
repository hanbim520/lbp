using UnityEngine;
using System.Collections;

// 加载处理相应输入设备的脚本: 触摸屏 串口鼠标
public class InputDevice : MonoBehaviour
{
	void Start()
	{
		DontDestroyOnLoad(this);
	}

	void OnLevelWasLoaded(int level)
	{
		LoadDeviceComponent();
	}

	// 判断加载的脚本
	private void LoadDeviceComponent()
	{
		SerialMousePort mouse = gameObject.GetComponent<SerialMousePort>();
		TouchScreenPort touchScreen = gameObject.GetComponent<TouchScreenPort>();
		if (GameData.GetInstance().inputDevice == 0)
		{
			if (mouse != null)
			{
				mouse.Close();
				Destroy(mouse);
			}
			if (touchScreen == null)
				gameObject.AddComponent<TouchScreenPort>();
		}
		else
		{
			if (touchScreen != null)
			{
				touchScreen.Close();
				Destroy(touchScreen);
			}
			if (mouse == null)
				gameObject.AddComponent<SerialMousePort>();
		}
	}

	void Update()
	{
		if (GameData.debug)
		{
			if (Input.GetKeyUp(KeyCode.S))			// Check touch
			{
				GameEventManager.OnChangeScene(Scenes.TouchCheck);
			}
			else if (Input.GetKeyUp(KeyCode.Space))	// Main menu
			{
				GameEventManager.OnOpenKey();
			}
			else if (Input.GetKeyUp(KeyCode.B))
			{
				GameEventManager.OnEnterBackend();
			}
		}
	}
}
