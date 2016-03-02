using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoldenRain : MonoBehaviour
{
	private List<GameObject> golds = new List<GameObject>();

	void Start()
	{
//		Generator();
		Launch();
	}

	private void Generator()
	{
		Object prefab = (Object)Resources.Load("Effects/Gold");
		int maxnum = Random.Range(30, 40);
		for (int i = 0; i < maxnum; ++i)
		{
			GameObject gold = (GameObject)Instantiate(prefab);
			gold.transform.SetParent(transform);
			gold.transform.localScale = Vector3.one;
			float x = Utils.GetRandom(-664, 664);
			float y = Random.Range(320.0f, 480.0f);
			gold.transform.localPosition = new Vector3(x, y, 0);
			golds.Add(gold);
			iTween.MoveTo(gold, iTween.Hash("y", -460, "islocal", true, "time", Random.Range(2.0f, 4.0f), "easetype", iTween.EaseType.easeInCubic,
			                                "oncomplete", "OnComplete", "oncompletetarget", gameObject, "oncompleteparams", gold));
		}
		prefab = null;
	}

	private void Launch()
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			Transform t = transform.GetChild(i);
			iTween.MoveTo(t.gameObject, iTween.Hash("y", -460, "islocal", true, "time", Random.Range(3.0f, 5.0f), "easetype", iTween.EaseType.easeInCubic,
			                                "oncomplete", "OnComplete", "oncompletetarget", gameObject, "oncompleteparams", t.gameObject));
		}
	}

	private void OnComplete(Object target)
	{
		Destroy(target);
	}
}
