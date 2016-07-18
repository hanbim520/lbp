using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugLogUtil : MonoBehaviour
{
	public Text[] debuglog;
	void Start()
	{
		GameEventManager.DebugLog += DebugLog;
	}

	void OnDestroy()
	{
		GameEventManager.DebugLog -= DebugLog;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			bool visible = debuglog[2].gameObject.activeSelf;
			debuglog[2].gameObject.SetActive(!visible);
		}
	}

	void DebugLog(int eventId, string log)
	{
		debuglog[eventId].text = log;
	}
}
