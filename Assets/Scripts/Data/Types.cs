﻿using UnityEngine;
using System.Collections;

public enum ConnectionState
{
	Disconnected, Connecting, Connected
}

public struct GamePhase
{
	public const int GameStart = 1;
	public const int GameEnd = 2;
	public const int Countdown = 3;
    public const int Run = 4;
    public const int ShowResult = 5;
    public const int Compensate = 6;
}

public struct NetInstr
{
	public const int SynData = 1;
	public const int GamePhase = 2;
    public const int Bet = 3;
    public const int LimitBet = 4;
    public const int NoLimitBet = 5;
    public const int GetGamePhase = 6;
    public const int GameResult = 7;
}

public struct Fields
{
    public const int Even = 38;
    public const int Odd = 39;
    public const int Red = 40;
    public const int Black = 41;
    public const int Big = 42;
    public const int Small = 43;
}

public struct Notifies
{
    public static string LimitBet = "限红不能押注";
}

public enum ResultColor
{
	Green, Red, Black
}