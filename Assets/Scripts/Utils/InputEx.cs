﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InputEx : MonoBehaviour
{
	public static bool inputEnable = true;
	private static Queue<InputInfo> touchUp;
	private static Queue<InputInfo> touchDown;
	private static Queue<InputInfo> mouseUp;
	private static Queue<InputInfo> mouseDown;

	private const int kMaxPointNumber = 5;
	private const float kDeleteTouchTime = 0.03f;
	private const float kDeleteMouseTime = 0.03f;

	void Start()
	{
		touchUp = new Queue<InputInfo>();
		touchDown = new Queue<InputInfo>();
		mouseUp = new Queue<InputInfo>();
		mouseDown = new Queue<InputInfo>();
		RegisterEvents();
	}

	void OnDestroy()
	{
		UnregisterEvents();
	}

	void Update()
	{
		CheckOutdated();
	}

	private void CheckOutdated()
	{
		float realTime = Time.realtimeSinceStartup;
		while (touchUp.Count > 0)
		{
			InputInfo info = touchUp.Peek();
			if (Mathf.Abs(realTime - info.time) > kDeleteTouchTime)
			{
				touchUp.Dequeue();
				continue;
			}
			break;
		}
		while (touchDown.Count > 0)
		{
			InputInfo info = touchDown.Peek();
			if (Mathf.Abs(realTime - info.time) > kDeleteTouchTime)
			{
				touchDown.Dequeue();
				continue;
			}
			break;
		}
		while (mouseUp.Count > 0)
		{
			InputInfo info = mouseUp.Peek();
			if (Mathf.Abs(realTime - info.time) > kDeleteMouseTime)
			{
				mouseUp.Dequeue();
				continue;
			}
			break;
		}
		while (mouseDown.Count > 0)
		{
			InputInfo info = mouseDown.Peek();
			if (Mathf.Abs(realTime - info.time) > kDeleteMouseTime)
			{
				mouseDown.Dequeue();
				continue;
			}
			break;
		}
	}

	private void RegisterEvents()
	{
		GameEventManager.SMRBUp += MouseButtonUp;
		GameEventManager.SMLBUp += MouseButtonUp;
		GameEventManager.SMRBDown += MouseButtonDown;
		GameEventManager.SMLBDown += MouseButtonDown;
		GameEventManager.FingerUp += FingerUp;
		GameEventManager.FingerDown += FingerDown;
	}

	private void UnregisterEvents()
	{
		GameEventManager.SMRBUp -= MouseButtonUp;
		GameEventManager.SMLBUp -= MouseButtonUp;
		GameEventManager.SMRBDown -= MouseButtonDown;
		GameEventManager.SMLBDown -= MouseButtonDown;
		GameEventManager.FingerUp -= FingerUp;
		GameEventManager.FingerDown -= FingerDown;
	}

	private void MouseButtonDown()
	{
		if (!inputEnable ||
		    mouseDown.Count >= kMaxPointNumber)
			return;

		float x = GameData.GetInstance().serialMouseX;
		float y = GameData.GetInstance().serialMouseY;
		InputInfo info = new InputInfo();
		info.x = x;
		info.y = y;
		info.time = Time.realtimeSinceStartup;
		info.state = 1;
		mouseDown.Enqueue(info);
	}

	private void MouseButtonUp()
	{
		if (!inputEnable ||
		    mouseUp.Count >= kMaxPointNumber)
			return;
		
		float x = GameData.GetInstance().serialMouseX;
		float y = GameData.GetInstance().serialMouseY;
		InputInfo info = new InputInfo();
		info.x = x;
		info.y = y;
		info.time = Time.realtimeSinceStartup;
		info.state = 0;
		mouseUp.Enqueue(info);
	}

	private void FingerUp(UInt16 x, UInt16 y)
	{
		if (!inputEnable ||
		    touchUp.Count >= kMaxPointNumber)
			return;

		InputInfo info = new InputInfo();
		info.x = x;
		info.y = y;
		info.time = Time.realtimeSinceStartup;
		info.state = 0;
		touchUp.Enqueue(info);
	}

	private void FingerDown(UInt16 x, UInt16 y)
	{
		if (!inputEnable ||
		    touchDown.Count >= kMaxPointNumber)
			return;
		
		InputInfo info = new InputInfo();
		info.x = x;
		info.y = y;
		info.time = Time.realtimeSinceStartup;
		info.state = 1;
		touchDown.Enqueue(info);
	}

	public static bool GetMouseUp()
	{
		return mouseUp.Count > 0;
	}

	public static bool GetMouseDown()
	{
		return mouseDown.Count > 0;
	}

	public static bool GetTouchUp()
	{
		return touchUp.Count > 0;
	}

	public static bool GetTouchDown()
	{
		return touchDown.Count > 0;
		
	}

	public static bool GetInputUp()
	{
		return (GetMouseUp() || GetTouchUp() || Input.GetMouseButtonUp(0));
	}

	public static bool GetInputDown()
	{
		return (GetMouseDown() || GetTouchDown() || Input.GetMouseButtonDown(0));
	}

	public static void MouseUpPosition(out Vector2 mousePosition)
	{
		if (mouseUp.Count == 0)
		{
			mousePosition = Vector2.zero;
			return;
		}

		InputInfo info = mouseUp.Peek();
		mousePosition = new Vector2(info.x, info.y);
	}

	public static void MouseDownPosition(out Vector2 mousePosition)
	{
		if (mouseDown.Count == 0)
		{
			mousePosition = Vector2.zero;
			return;
		}
		
		InputInfo info = mouseDown.Peek();
		mousePosition = new Vector2(info.x, info.y);
	}

	public static void TouchUpPosition(out Vector2 touchPosition)
	{
		if (touchUp.Count == 0)
		{
			touchPosition = Vector2.zero;
			return;
		}

		InputInfo pos = touchUp.Peek();
		float lcdx, lcdy;
		Utils.TouchScreenToLCD(pos.x, pos.y, out lcdx, out lcdy);
		touchPosition = new Vector2(lcdx, lcdy);
	}

	public static void TouchDownPosition(out Vector2 touchPosition)
	{
		if (touchDown.Count == 0)
		{
			touchPosition = Vector2.zero;
			return;
		}
		
		InputInfo pos = touchDown.Peek();
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
