using UnityEngine;
using System.Collections;

public static class GameEventManager 
{
    public delegate void GameEvent();
    public static event GameEvent ObtainInput;

    public static void TriggerObtainInput()
    {
        if (ObtainInput != null)
        {
            ObtainInput();
        }
    }
}
