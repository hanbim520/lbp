using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DlgPrintCode : MonoBehaviour
{
    public Text txtMachineId;
    public Text txtCurrentWin;
    public Text txtTotalWin;
    public Text txtPrintTimes;
    public Text txtCheckCode;
    public Text txtCalcInput;
    public Text txtInput;
    public Text lblInput;

    public GameObject en;
    public GameObject cn;

    private GameObject downHitObject;
    private AndroidJavaClass jc;
    private AndroidJavaObject jo;

    private string[] strMachineId = new string[]{"MachineId: ", "机台号: "};
    private string[] strCurrentWin = new string[]{"CurrentWin: ", "当次盈利: "};
    private string[] strTotalWin = new string[]{"TotalWin: ", "总盈利: "};
    private string[] strPrintTimes = new string[]{"PrintTimes: ", "打码次数: "};
    private string[] strCheckCode = new string[]{"CheckCode: ", "校验码: "};
    private string[] strPleaseInput = new string[]{"Input: ", "请输入: "};

	void OnEnable() 
    {
        Init();
	}

    private void Init()
    {
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
        txtMachineId.text = strMachineId[idx] + machineId.ToString();
        txtCurrentWin.text = strCurrentWin[idx] + currentWin.ToString();
        txtTotalWin.text = strTotalWin[idx] + totalWin.ToString();
        txtPrintTimes.text = strPrintTimes[idx] + printTimes.ToString();
        lblInput.text = strPleaseInput[idx];
        string checkCode = GetCheckCode(GameData.GetInstance().lineId, machineId, totalWin, currentWin, printTimes);
        if (checkCode != null)
        {
            int value;
            if (int.TryParse(checkCode, out value))
            {
                txtCheckCode.text = strCheckCode[idx] + value.ToString();
            }
        }
        txtInput.text = txtCalcInput.text = string.Empty;

        if (Application.platform == RuntimePlatform.Android)
        {
            jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        }
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
        print(txtInput.text);
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
        DetectInputEvents();
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

    public string GetCheckCode(long lineId, long machineId, long totalWin, long currentWin, long printTimes)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            IntPtr methodId = AndroidJNIHelper.GetMethodID(jo.GetRawClass(), "GetCheckCode");
            return AndroidJNI.CallStringMethod(jo.GetRawObject(), methodId, null);
        }
        return null;
    }
}
