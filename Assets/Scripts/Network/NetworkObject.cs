using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkObject : MonoBehaviour
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
		UClient networkClient = gameObject.GetComponent<UClient>();
		UHost networkHost = gameObject.GetComponent<UHost>();
		if (GameData.GetInstance().deviceIndex == 1)
		{
			if (networkHost == null)
				gameObject.AddComponent<UHost>();
			if (networkClient != null)
				Destroy(networkClient);
		}
		else
		{
			if (networkHost != null)
				Destroy(networkHost);
			if (networkClient == null)
				gameObject.AddComponent<UClient>();
		}
	}
}
