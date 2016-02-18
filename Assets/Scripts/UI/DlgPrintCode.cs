using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class DlgPrintCode : MonoBehaviour
{
    public Text txtMachineId;
    public Text txtCurrentWin;
    public Text txtTotalWin;
    public Text txtPrintTimes;
    public Text txtCheckCode;
    public Text txtCalcInput;
    public Text txtInput;

    public GameObject en;
    public GameObject cn;

    private GameObject downHitObject;
#if UNITY_ANDROID
    private AndroidJavaClass jc;
    private AndroidJavaObject jo;
#endif
	private HIDUtils hidUtils;
	private int checkCodeNum;

	void OnEnable() 
    {
        Init();
		hidUtils = GameObject.Find("HIDUtils").GetComponent<HIDUtils>();
	}

    private void Init()
    {
#if UNITY_ANDROID
		jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
#endif

        int idx = GameData.GetInstance().language;
        if (idx == 0)
        {
            en.SetActive(true);
            cn.SetActive(false);
        }
        else
        {
            en.SetActive(false);
            cn.SetActive(true);
        }

        int machineId = GameData.GetInstance().machineId;
        int currentWin = GameData.GetInstance().currentWin;
        int totalWin = GameData.GetInstance().totalWin;
        int printTimes = GameData.GetInstance().printTimes;
        txtMachineId.text = machineId.ToString();
        txtCurrentWin.text = currentWin.ToString();
        txtTotalWin.text = totalWin.ToString();
        txtPrintTimes.text = printTimes.ToString();
        string checkCode = GetCheckCode(GameData.GetInstance().lineId, machineId, totalWin, currentWin, printTimes);
        if (checkCode != null)
			txtCheckCode.text = checkCode.ToString();
        txtInput.text = txtCalcInput.text = string.Empty;
    }

    public void CalcDownEvent(Transform hitObject)
    {
        SetAlpha(hitObject, 255);
    }

    public void CalcUpEvent(Transform hitObject)
    {
        SetAlpha(hitObject, 0);
        string name = hitObject.name;
        if (string.Equals(name, "del"))
        {
            DelCalcContent();
        }
        else if (string.Equals(name, "enter2"))
        {
            CalcEnterEvent();
        }
        else
        {
            int value;
            if (int.TryParse(name, out value))
            {
                AppendCalcContent(value);
            }
        }
    }

    private void DelCalcContent()
    {
        string text = txtCalcInput.text;

        if (string.IsNullOrEmpty(text))
            return;
        if (text.Length > 1)
            text = text.Substring(0, text.Length - 1);
        else
            text = string.Empty;
        SetCalcContent(text);
    }

    private void CalcEnterEvent()
    {
        Debug.Log(txtInput.text);
		txtCalcInput.text = string.Empty;
		int userInput;
		if (int.TryParse(txtInput.text, out userInput))
		{
			int lineId = GameData.GetInstance().lineId;
			int machineId = GameData.GetInstance().machineId;
			int currentWin = GameData.GetInstance().currentWin;
			int totalWin = GameData.GetInstance().totalWin;
			int printTimes = GameData.GetInstance().printTimes;
			CheckUserInput(lineId, machineId, totalWin, currentWin, printTimes, checkCodeNum, userInput);
		}
    }

    private void AppendCalcContent(int num)
    {
        SetCalcContent(txtCalcInput.text + num.ToString());
    }

    private void SetCalcContent(string text)
    {
        txtCalcInput.text = text;
        txtInput.text = text;
    }

    private void SetAlpha(Transform target, int alpha)
    {
        Image field = target.GetComponent<Image>();
        Color c = field.color;
        c.a = alpha;
        field.color = c;
    }
	
	void Update ()
    {
//        DetectInputEvents();
	}
    
    private void DetectInputEvents()
    {
        if (InputEx.GetInputDown())
        {
            Vector2 pos;
            InputEx.InputDownPosition(out pos);
            if (pos == new Vector2(-1, -1))
                return;
            
            float sx, sy;
            Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
            RaycastHit2D[] hit = Physics2D.RaycastAll(new Vector2(sx, sy), Vector2.zero);
            if (hit.Length == 0)
                return;
            
            int idx = -1;
            if (hit.Length > 0)
            {
                for (int i = 0; i < hit.Length; ++i)
                {
                    if (hit[i].collider.tag == "Dialog")
                    {
                        idx = i;
                        break;
                    }
                }
            }
            if (idx > -1 && hit[idx].collider != null)
            {
                hit[idx].collider.gameObject.GetComponent<ButtonEvent>().OnInputDown(hit[idx].collider.transform);
                downHitObject = hit[idx].collider.gameObject;
            }
        }
        else if (InputEx.GetInputUp())
        {
            Vector2 pos;
            InputEx.InputUpPosition(out pos);
            if (pos == new Vector2(-1, -1))
                return;
            
            if (downHitObject != null)
            {
                downHitObject.GetComponent<ButtonEvent>().OnInputUp(downHitObject.transform);
            }
            downHitObject = null;
        }
    }

	public string GetCheckCode(int lineId, int machineId, int totalWin, int currentWin, int printTimes)
    {
#if UNITY_ANDROID
		string strCrc = jo.Call<string>("GetPWCheckValue4", (long)lineId, (long)machineId, (long)totalWin, (long)currentWin, (long)printTimes);
#endif

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
//		string strCrc = "";
		IntPtr ret = EncryChip.ReturnCheckCode(lineId, machineId, totalWin, currentWin, printTimes);
		string strCrc = System.Runtime.InteropServices.Marshal.PtrToStringAuto(ret);
		EncryChip.FreeByteArray(ret);
#endif
		int value;
		if (int.TryParse(strCrc, out value))
		{
			checkCodeNum = value;
			return value.ToString();
		}
        return null;
    }

	public void CheckUserInput(int lineId, int machineId, int totalWin, int currentWin, int printTimes, int crc, int userInput)
	{
		int dataSize = 32;
#if UNITY_ANDROID
		AndroidJavaObject rev = jo.Call<AndroidJavaObject>("CreateCheckPWString", 
		                                                   (long)lineId, (long)machineId, (long)totalWin, (long)currentWin, (long)printTimes, (long)crc, (long)userInput);
		byte[] buf = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(rev.GetRawObject());
#endif

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
//		byte[] buf = new byte[61];
		IntPtr ret = EncryChip.CreateReportBytes(lineId, machineId, totalWin, currentWin, printTimes, crc, userInput);
		byte[] buf = new byte[dataSize];
		System.Runtime.InteropServices.Marshal.Copy(ret, buf, 0, dataSize);
		EncryChip.FreeByteArray(ret);
#endif

		List<int> data = new List<int>();
		data.Add(0x42);
		data.Add(0x5a);
		data.Add(dataSize);	// 数据长度
		foreach(byte b in buf)
			data.Add((int)b);
		while (data.Count < 64)
			data.Add(0);
		hidUtils.WriteData(data.ToArray(), "writeUsbPort");
	}
}
