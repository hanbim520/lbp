using UnityEngine;
using System.Collections;

public enum ConnectionState
{
	Disconnected, Connecting, Connected
}

public struct GamePhase
{
	public const int GameStart = 0x01;
	public const int GameEnd = 0x02;
	public const int SCountdown = 0x01;
	public const int ECountdown = 0x01;
	public const int SRun = 0x01;
	public const int ERun = 0x01;
	public const int SShowResult = 0x01;
	public const int EShowResult = 0x01;
	public const int SCompensate = 0x01;
	public const int ECompensate = 0x01;
}

public struct NetInstr
{
	public const int SynData = 0x01;
	public const int GamePhase = 0x02;
}