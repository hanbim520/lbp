using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoldenRain : MonoBehaviour
{
	void Start()
	{
		Launch();
	}

	private void Launch()
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			Transform t = transform.GetChild(i);
            Random.seed = i;
			iTween.MoveTo(t.gameObject, iTween.Hash("y", -460, "islocal", true, "time", Random.Range(2.0f, 4.0f), "easetype", iTween.EaseType.easeInCubic,
			                                "oncomplete", "OnComplete", "oncompletetarget", gameObject, "oncompleteparams", t.gameObject));
		}
	}

	private void OnComplete(Object target)
	{
		Destroy(target);
	}
}
