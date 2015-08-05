using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadLevelAsync : MonoBehaviour
{
    public Slider progressBar;

    private AsyncOperation async;

    void Start()
    {
        progressBar.value = 0;
        StartCoroutine(LoadLevel());
    }

    private IEnumerator LoadLevel()
    {
        async = Application.LoadLevelAsync(GameData.GetInstance().NextLevelName);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            if (progressBar.value < async.progress ||
                Mathf.Approximately(async.progress, 0))
            {
                progressBar.value += Time.fixedDeltaTime;
                if (progressBar.value > 0.8f || Mathf.Approximately(progressBar.value, 0.8f))
                {
                    break;
                }
            }

            yield return null;
        }

        if (progressBar.value < 1)
        {
            float delta = 0.01f;
            while (!async.allowSceneActivation)
            {
                progressBar.value += delta;
                if (progressBar.value > 0.9f || Mathf.Approximately(progressBar.value, 0.9f))
                {
                    progressBar.value = 1;
                    async.allowSceneActivation = true;
                }
            }
        }

        yield return async;
    }
}
