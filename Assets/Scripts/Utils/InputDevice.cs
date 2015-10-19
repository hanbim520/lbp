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
		if (GameData.GetInstance().inputDevice == 0)
		{
			if (gameObject.GetComponent<TouchScreenPort>() == null)
				gameObject.AddComponent<TouchScreenPort>();
			if (gameObject.GetComponent<SerialMousePort>() != null)
				Destroy(gameObject.GetComponent<SerialMousePort>());
		}
		else
		{
			if (gameObject.GetComponent<SerialMousePort>() == null)
				gameObject.AddComponent<SerialMousePort>();
			if (gameObject.GetComponent<TouchScreenPort>() != null)
				Destroy(gameObject.GetComponent<TouchScreenPort>());
		}
	}
}
