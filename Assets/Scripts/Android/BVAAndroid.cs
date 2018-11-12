using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVAAndroid : MonoBehaviour
{
	public static string PortName = "ttyS1";

	int type = -1;

	void Start()
	{
		DontDestroyOnLoad(this);
		LoadScript(PortName);
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
		else if (type == 1)		// ICT002
		{
			gameObject.AddComponent<ICTBVAndroid>();
			ICTBVAndroid bva = gameObject.GetComponent<ICTBVAndroid>();
			bva.OpenCOM(port);
		}
		else if (type == 2)		// ICT104
		{
			gameObject.AddComponent<ICT104>();
			ICT104 bva = gameObject.GetComponent<ICT104>();
			bva.OpenCOM(port);
		}
		else if (type == 3)		// ICT106
		{
			gameObject.AddComponent<ICT106>();
			ICT106 bva = gameObject.GetComponent<ICT106>();
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
		else if (type == 1)		// ICT002
		{
			ICTBVAndroid bva = gameObject.GetComponent<ICTBVAndroid>();
			if (bva == null)
				return;
			bva.CloseCOM();
			Destroy(gameObject.GetComponent<ICTBVAndroid>());
		}
		else if (type == 2)		// ICT104
		{
			ICT104 bva = gameObject.GetComponent<ICT104>();
			if (bva == null)
				return;
			bva.CloseCOM();
			Destroy(gameObject.GetComponent<ICT104>());
		}
		else if (type == 3)		// ICT106
		{
			ICT106 bva = gameObject.GetComponent<ICT106>();
			if (bva == null)
				return;
			bva.CloseCOM();
			Destroy(gameObject.GetComponent<ICT106>());
		}
	}
}
