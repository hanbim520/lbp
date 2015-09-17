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
            PlayerPrefs.SetInt("totalCredits", _totalCredits);
            PlayerPrefs.SetInt("currentBet", currentBet);
            PlayerPrefs.Save();
        }
    }
    public int currentBet = 0;
    public int lastWin = 0;

    protected int _totalCredits;
}
