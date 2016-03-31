using UnityEngine;
using System.Collections;

public class BreathyChip : MonoBehaviour
{

	void OnEnable()
	{
		iTween.ScaleTo(gameObject, iTween.Hash("scale", new Vector3(1.2f, 1.2f, 1.2f), "time", 0.8,
		                                       "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.pingPong));
	}

	void OnDisable()
	{
		iTween.Stop(gameObject);
	}
}
