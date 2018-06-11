using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestBillAcceptor : MonoBehaviour
{
	public InputField inputDenom;
	public InputField inputStatus;
	public Dropdown dropdownPort;

	string portName;
	BVAAndroid bvaAndroid;

	void Start()
	{
		#if UNITY_ANDROID || UNITY_LINUX
		string prefixPort = "ttyS";
		int startPort = 0;
		#else
		string prefixPort = "com";
		int startPort = 1;
		#endif
		List<string> portnames = new List<string>();
		for (int i = startPort; i <= 20; ++i)
			portnames.Add(string.Format("{0}{1}", prefixPort, i));
		portName = portnames[0];
		dropdownPort.AddOptions(portnames);

		bvaAndroid = GameObject.Find("BVA").GetComponent<BVAAndroid>();

		GameEventManager.ReceiveCoin += ReceiveCoin;
	}

	public void ReceiveCoin(int count)
	{
		inputDenom.text = string.Format("Receive: {0}", count);
	}
	
	void OnDestroy()
	{
		GameEventManager.ReceiveCoin -= ReceiveCoin;
	}

	public void OnClickQuit()
	{
		Application.Quit();
	}

	public void OnClickStart()
	{
		byte[] data = new byte[] { 0xfc, 0x5, 0x50 };
		byte[] ret = CRCUtils.JCMCRC(data, 3, 0);
		print(string.Format("{0:x}", ret[0]));
		print(string.Format("{0:x}", ret[1]));
	}

	public void OnClickAcceptBill()
	{

	}

	public void OnClickReturnBill()
	{

	}

	public void OnClicEnableBV()
	{

	}

	public void OnClickDisableBV()
	{

	}

	public void OnChangeBrand(int index)
	{
		GameData.GetInstance().billAcceptorType = index;
		bvaAndroid.LoadScript(portName);
	}

	public void OnChangePort(int index)
	{
		#if UNITY_ANDROID || UNITY_LINUX
		portName = string.Format("ttyS{0}", index);
		#else
		portName = string.Format("com{0}", ++index);
		#endif
	}
}
