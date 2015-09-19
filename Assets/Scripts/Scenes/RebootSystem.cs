using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RebootSystem : MonoBehaviour
{
    private float timeInterval;
    private float countdown = 5;
    private float updateInterval;
    private float updateTime = 1;
    private int updateNum = 5;
    private bool timeUp = false;

    void Update()
    {
        if (!timeUp)
        {
            updateInterval += Time.deltaTime;
            if (updateInterval > updateTime)
            {
                updateInterval = 0;
                --updateNum;
                UpdateMessage(updateNum);
            }
            timeInterval += Time.deltaTime;
            if (timeInterval > countdown)
            {
                timeUp = true;
                --updateNum;
                UpdateMessage(updateNum);
                Reboot();
            }
        }
    }

    void OnEnable()
    {
        UpdateMessage(updateNum);
    }

    private void Reboot()
    {
        Debug.Log("Reboot");
        // TODO: Save all settings
        // TODO: Reboot system
        Application.LoadLevel("StartInfo");
    }

    private void UpdateMessage(int countdown)
    {
        if (countdown < 0)
            countdown = 0;
        if (GameData.GetInstance().language == 1)
            transform.GetChild(0).GetComponent<Text>().text = string.Format("参数已保存，{0} 秒后重启系统。", countdown);
        else
            transform.GetChild(0).GetComponent<Text>().text = string.Format("Saving all parameters, \nreboot system after {0} seconds.", countdown);
    }
}
