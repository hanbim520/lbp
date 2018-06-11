using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVAAndroid : MonoBehaviour
{
	int type = -1;

	void Start()
	{
		DontDestroyOnLoad(this);
		LoadScript("ttyS3");
	}

	public void LoadScript(string port)
	{
		if (type == GameData.GetInstance().billAcceptorType)
			return;

		DetachScript();
		type = GameData.GetInstance().billAcceptorType;
		if (type == 0)			// JCM
		{
			gameObject.AddComponent<JCMBVAndroid>();
			JCMBVAndroid bva = gameObject.GetComponent<JCMBVAndroid>();
			bva.OpenCOM(port);
		}
		else if (type == 1)		// ICT
		{
			gameObject.AddComponent<ICTBVAndroid>();
			ICTBVAndroid bva = gameObject.GetComponent<ICTBVAndroid>();
			bva.OpenCOM(port);
		}
	}

	void DetachScript()
	{
		if (type == 0)			// JCM
		{
			JCMBVAndroid bva = gameObject.GetComponent<JCMBVAndroid>();
			if (bva == null)
				return;
			bva.CloseCOM();
			Destroy(gameObject.GetComponent<JCMBVAndroid>());
		}
		else if (type == 1)		// ICT
		{
			ICTBVAndroid bva = gameObject.GetComponent<ICTBVAndroid>();
			if (bva == null)
				return;
			bva.CloseCOM();
			Destroy(gameObject.GetComponent<ICTBVAndroid>());
		}
	}
}
