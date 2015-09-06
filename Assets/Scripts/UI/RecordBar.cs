using UnityEngine;
using System.Collections;

public class RecordBar : MonoBehaviour
{
    public ResultType redBar;

	void Start ()
    {
        RegisterEvents();
	}

    void OnDestroy()
    {
        UnregisterEvents();
    }
	
	void Update () 
    {
	
	}

    private void RegisterEvents()
    {
        GameEventManager.RefreshRecord += RefreshRecord;
    }

    private void UnregisterEvents()
    {
        GameEventManager.RefreshRecord -= RefreshRecord;
    }

    private void RefreshRecord(int result)
    {

    }
}
