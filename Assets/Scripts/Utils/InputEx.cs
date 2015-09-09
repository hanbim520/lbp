using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InputEx : MonoBehaviour
{
	private static Queue<Vector2> touches;
	private static Queue<Vector2> mousePositions;

	void Start()
	{
		touches = new Queue<Vector2>();
		mousePositions = new Queue<Vector2>();
		RegisterEvents();
	}

	void OnDestroy()
	{
		UnregisterEvents();
	}
	
	void Update()
	{
//		if (touches > 5)
	}

	private void RegisterEvents()
	{
		GameEventManager.SMRBUp += SerialMouseButtonUp;
		GameEventManager.SMLBUp += SerialMouseButtonUp;
		GameEventManager.FingerUp += OnFingerUp;
	}

	private void UnregisterEvents()
	{
		GameEventManager.SMRBUp -= SerialMouseButtonUp;
		GameEventManager.SMLBUp -= SerialMouseButtonUp;
		GameEventManager.FingerUp -= OnFingerUp;
	}

	private void SerialMouseButtonUp()
	{
		float x = GameData.GetInstance().serialMouseX;
		float y = GameData.GetInstance().serialMouseY;
		mousePositions.Enqueue(new Vector2(x, y));
	}

	private void OnFingerUp(UInt16 x, UInt16 y)
	{
		touches.Enqueue(new Vector2(x, y));
	}

	public static bool GetButtonUp(out Vector2 mousePosition)
	{
		if (mousePositions.Count == 0)
		{
			mousePosition = Vector2.zero;
			return false;
		}

		Vector2 pos = mousePositions.Dequeue();
		mousePosition = new Vector2(pos.x, pos.y);
		return true;
	}

	public static bool GetTouchUp(out Vector2 touchPosition)
	{
		if (touches.Count == 0)
		{
			touchPosition = Vector2.zero;
			return false;
		}

		Vector2 pos = touches.Dequeue();
		float lcdx, lcdy;
		Utils.TouchScreenToLCD(pos.x, pos.y, out lcdx, out lcdy);
		touchPosition = new Vector2(lcdx, lcdy);
		return true;
	}

	public static void GetInputUp(out Vector2 inputPosition)
	{
		if (GetTouchUp(out inputPosition))
			return;
		else if (GetButtonUp(out inputPosition))
			return;
		else
		{
			if (Input.GetMouseButtonUp(0))
			{
				float lcdx, lcdy;
				Utils.ScreenSpaceToUISpace(Input.mousePosition.x, Input.mousePosition.y, out lcdx, out lcdy);
				inputPosition = new Vector2(lcdx, lcdy);
			}
		}
	}
}
