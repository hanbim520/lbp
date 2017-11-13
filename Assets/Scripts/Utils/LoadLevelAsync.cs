using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadLevelAsync : MonoBehaviour
{
    public Image progressBar;
	public GameObject[] objCircalRecords;
	
    private AsyncOperation async;

    void Start()
    {
		SetRouletteType();
        progressBar.fillAmount = 0.05f;
        StartCoroutine(LoadLevel());
    }

    private IEnumerator LoadLevel()
    {
		async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GameData.GetInstance().NextLevelName);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
			if (progressBar.fillAmount < async.progress ||
                Mathf.Approximately(async.progress, 0))
            {
				progressBar.fillAmount += Time.fixedDeltaTime;
				if (progressBar.fillAmount > 0.8f || Mathf.Approximately(progressBar.fillAmount, 0.8f))
                {
                    break;
                }
            }

            yield return null;
        }

		if (progressBar.fillAmount < 1)
        {
            float delta = 0.01f;
            while (!async.allowSceneActivation)
            {
				progressBar.fillAmount += delta;
				if (progressBar.fillAmount > 0.9f || Mathf.Approximately(progressBar.fillAmount, 0.9f))
                {
					progressBar.fillAmount = 1;
                    async.allowSceneActivation = true;
                }
            }
        }

        yield return async;
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
