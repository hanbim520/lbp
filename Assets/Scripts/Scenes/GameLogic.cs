using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour 
{
    public int totalCredits
    {
        get { return _totalCredits; }
        set
        {
            _totalCredits = value;
            // Save
            SaveTotalCredits();
        }
    }
    public int currentBet = 0;
    public int lastWin = 0;

    protected int _totalCredits;

    protected void FixExitAbnormally()
    {
        int lastBet = PlayerPrefs.GetInt("currentBet", 0);
        if (lastBet > 0)
        {
            _totalCredits = PlayerPrefs.GetInt("totalCredits", 0);
            _totalCredits += lastBet;
            currentBet = 0;
            SaveTotalCredits();
        }
    }

    public void SaveTotalCredits()
    {
        PlayerPrefs.SetInt("totalCredits", _totalCredits);
        PlayerPrefs.SetInt("currentBet", currentBet);
        PlayerPrefs.Save();
    }
}
