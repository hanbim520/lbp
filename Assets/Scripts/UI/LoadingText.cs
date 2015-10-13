using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingText : MonoBehaviour
{
	public string baseText;
	public float interval = 0.5f;
	public int periodNumber = 3;
	
	private Text txtComponent;
	private float timerInterval = 0f;
	private string strPeriod = ".";

	void OnEnable()
	{
		txtComponent = GetComponent<Text>();
		txtComponent.text = baseText + strPeriod;
	}
	
	void Update()
	{
		if (txtComponent.enabled)
		{
			timerInterval += Time.deltaTime;
			if (timerInterval >= interval)
			{
				timerInterval = 0;
				strPeriod += ".";
				if (strPeriod.Length > periodNumber)
				{
					strPeriod = ".";
				}
				txtComponent.text = baseText + strPeriod;
			}
		}
	}

	public void Visualized(bool v)
	{
		txtComponent.enabled = v;
	}
}
