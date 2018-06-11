﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// 加载处理相应输入设备的脚本: 触摸屏 串口鼠标
public class InputDevice : MonoBehaviour
{
	void Start()
	{
		DontDestroyOnLoad(this);
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scence, LoadSceneMode mod)
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
}
