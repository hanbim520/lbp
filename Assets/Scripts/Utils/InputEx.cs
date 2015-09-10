using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InputEx : MonoBehaviour
{
	public static bool inputEnable = true;
	private static Queue<Vector2> touches;
	private static Queue<Vector2> mousePositions;
//	private const float
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

	}

	private void RegisterEvents()
	{
		GameEventManager.SMRBUp += SerialMouseButtonEvent;
		GameEventManager.SMLBUp += SerialMouseButtonEvent;
		GameEventManager.SMRBDown += SerialMouseButtonEvent;
		GameEventManager.SMLBDown += SerialMouseButtonEvent;
		GameEventManager.FingerUp += OnFingerEvent;
		GameEventManager.FingerDown += OnFingerEvent;
	}

	private void UnregisterEvents()
	{
		GameEventManager.SMRBUp -= SerialMouseButtonEvent;
		GameEventManager.SMLBUp -= SerialMouseButtonEvent;
		GameEventManager.SMRBDown -= SerialMouseButtonEvent;
		GameEventManager.SMLBDown -= SerialMouseButtonEvent;
		GameEventManager.FingerUp -= OnFingerEvent;
		GameEventManager.FingerDown -= OnFingerEvent;
	}

	private void SerialMouseButtonEvent()
	{
		if (!inputEnable) return;
		float x = GameData.GetInstance().serialMouseX;
		float y = GameData.GetInstance().serialMouseY;
		if (mousePositions.Count > 0)
			mousePositions.Clear();
		mousePositions.Enqueue(new Vector2(x, y));
	}

	private void OnFingerEvent(UInt16 x, UInt16 y)
	{
		if (!inputEnable) return;
		if (touches.Count > 0)
			touches.Clear();
		touches.Enqueue(new Vector2(x, y));
	}

	public static bool GetMouseUp()
	{
		return mousePositions.Count > 0;
	}

	public static bool GetTouchUp()
	{
		return touches.Count > 0;
	}

	public static bool GetInputUp()
	{
		return (GetMouseUp() || GetTouchUp() || Input.GetMouseButtonUp(0));
	}

	public static bool GetMouseDown()
	{
		return mousePositions.Count > 0;
	}

	public static bool GetTouchDown()
	{
		return touches.Count > 0;
	}

	public static bool GetInputDown()
	{
		return (GetMouseDown() || GetTouchDown() || Input.GetMouseButtonDown(0));
	}

	public static void MouseUpPosition(out Vector2 mousePosition)
	{
		if (mousePositions.Count == 0)
		{
			mousePosition = Vector2.zero;
			return;
		}

		Vector2 pos = mousePositions.Dequeue();
		mousePosition = new Vector2(pos.x, pos.y);
	}

	public static void MouseDownPosition(out Vector2 mousePosition)
	{
		if (mousePositions.Count == 0)
		{
			mousePosition = Vector2.zero;
			return;
		}
		
		Vector2 pos = mousePositions.Dequeue();
		mousePosition = new Vector2(pos.x, pos.y);
	}

	public static void TouchUpPosition(out Vector2 touchPosition)
	{
		if (touches.Count == 0)
		{
			touchPosition = Vector2.zero;
			return;
		}

		Vector2 pos = touches.Dequeue();
		float lcdx, lcdy;
		Utils.TouchScreenToLCD(pos.x, pos.y, out lcdx, out lcdy);
		touchPosition = new Vector2(lcdx, lcdy);
	}

	public static void TouchDownPosition(out Vector2 touchPosition)
	{
		if (touches.Count == 0)
		{
			touchPosition = Vector2.zero;
			return;
		}
		
		Vector2 pos = touches.Dequeue();
		float lcdx, lcdy;
		Utils.TouchScreenToLCD(pos.x, pos.y, out lcdx, out lcdy);
		touchPosition = new Vector2(lcdx, lcdy);
	}

	public static void InputUpPosition(out Vector2 inputPosition)
	{
		inputPosition = new Vector2(-1, -1);
		if (GetTouchUp())
			TouchUpPosition(out inputPosition);
		else if (GetMouseUp())
			MouseUpPosition(out inputPosition);
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

	public static void InputDownPosition(out Vector2 inputPosition)
	{
		inputPosition = new Vector2(-1, -1);
		if (GetTouchDown())
			TouchDownPosition(out inputPosition);
		else if (GetMouseDown())
			MouseDownPosition(out inputPosition);
		else
		{
			if (Input.GetMouseButtonDown(0))
			{
				float lcdx, lcdy;
				Utils.ScreenSpaceToUISpace(Input.mousePosition.x, Input.mousePosition.y, out lcdx, out lcdy);
				inputPosition = new Vector2(lcdx, lcdy);
			}
		}
	}
}
