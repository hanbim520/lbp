using UnityEngine;
using System.Collections;

public class MainLogic : MonoBehaviour 
{
    public MainSceneUI ui;

    private bool bEnableRun = false;

    private float timeInterval = 0;
    private float longPressTime = 3;
    private bool bLoadBackend = false;

	void Start() 
    {
        RegisterListener();
	}
	
    void OnDestroy()
    {
        UnregisterListener();
    }

    private void RegisterListener()
    {

    }

    private void UnregisterListener()
    {

    }

	void Update()
    {
        if (!bLoadBackend && Input.GetKey(KeyCode.Return))
        {
            timeInterval += Time.deltaTime;
            if (timeInterval > longPressTime)
            {
                timeInterval = 0;
                bLoadBackend = true;
                StartCoroutine(LoadBackend());
                ui.backendTip.SetActive(true);
            }
        }
	}

    private IEnumerator LoadBackend()
    {
        yield return new WaitForSeconds(2.0f);
        if (Config.Language == "CN")
            GameData.GetInstance().NextLevelName = "Backend CN";
        else if (Config.Language == "EN")
            GameData.GetInstance().NextLevelName = "Backend EN";
        Application.LoadLevel("Loading");
    }

    private void GameOver()
    {
        if (bLoadBackend)
            LoadBackend();
        else
            StartCoroutine(NextRound());
    }

    private IEnumerator NextRound()
    {
        yield return new WaitForSeconds(3.0f);
        bEnableRun = false;
        GameEventManager.TriggerGameStart();
    }

    private void GameStart()
    {
        // Client:
    }

    private void SCountdown()
    {

    }

    private void ECountdown()
    {

    }

    private void SRun()
    {

    }

    private void ERun()
    {

    }

    private void SCompensate()
    {

    }

    private void ECompensate()
    {

    }
}
