using UnityEngine;
using System.Collections;

public enum ConnectionState
{
	Disconnected, Connecting, Connected
}

public enum GamePhase
{
	GameStart, SCountdown, ECountdown, SRun, ERun, SShowResult, EShowResult, SCompensate, ECompensate, GameEnd
}