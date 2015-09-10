using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class TouchCheck : MonoBehaviour
{
	public GameObject cross;
	public GameObject button;
	public GameObject txtFailure;
	public GameObject txtSuccess;

	private int rebootDuration = 5;
	private int remainTimes;
	private int screenWidth;
	private int screenHeight;
	// LCD Device positions
	private List<Vector2> lcdPos = new List<Vector2>();
	// Touch-screen positions
	private List<Vector2> tsPos = new List<Vector2>();
	private Queue<Vector2> touches = new Queue<Vector2>();

	void Start()
	{
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		remainTimes = 5;
		lcdPos.Add(new Vector2(0, 0));
		lcdPos.Add(new Vector2(-500, 400));
		lcdPos.Add(new Vector2(-500, -400));
		lcdPos.Add(new Vector2(500, -400));
		lcdPos.Add(new Vector2(500, 400));
		button.SetActive(false);
		txtFailure.SetActive(false);
		txtSuccess.SetActive(false);
		cross.SetActive(true);
		RegisterEvents();
	}

	void OnDestroy()
	{
		UnregisterEvents();
	}

	void Update()
	{
		HandleInput();
	}

	private void RegisterEvents()
	{
		GameEventManager.FingerUp += FingerUp;
	}

	private void UnregisterEvents()
	{
		GameEventManager.FingerUp -= FingerUp;
	}

	private void LEqations3x3(double[,] det)
	{
		double x, y, z;
		Utils.LEqations3x3(det, out x, out y, out z);
	}

	private void HandleInput()
	{
		if (touches.Count == 0)
			return;

		Vector2 touch = touches.Dequeue();
		float x = touch.x;
		float y = touch.y;
		if (remainTimes > 0)
		{
			// Check points
			tsPos.Add(new Vector2(x, y));
			--remainTimes;
			TranslateCross();
		}
		else if (remainTimes < 0)
		{
			float lx, ly;
			Utils.TouchScreenToLCD(x, y, out lx, out ly);
			Debug.Log("lx:" + lx + ", ly:" + ly);
			Vector2 touchPos = new Vector2(lx, ly);
			RectTransform rt = button.GetComponent<RectTransform>();
			// Hit in button field
			if (Utils.PointInRect(touchPos, rt))
			{
				txtSuccess.SetActive(true);
				// Reboot system
				StartCoroutine(RebootGame());
			}
			else
			{
				// Check again
				txtFailure.SetActive(true);
				Timer t = TimerManager.GetInstance().CreateTimer(1.5f);
				t.Tick += CheckAgain;
				t.Start();
			}
			button.SetActive(false);
		}
	}

	private void TranslateCross()
	{
		if (remainTimes == 4)
		{
			iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", -500, "y", 400));
		}
		else if (remainTimes == 3)
		{
			iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", -500, "y", -400));
		}
		else if (remainTimes == 2)
		{
			iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", 500, "y", -400));
		}
		else if (remainTimes == 1)
		{
			iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", 500, "y", 400));
		}
		else if (remainTimes == 0)
		{
			cross.SetActive(false);
			button.SetActive(true);
			--remainTimes;
			CalcDet();
		}
	}

	private Vector2 ScreenSpaceToUISpace(Vector2 pos)
	{
		return new Vector2(pos.x - screenWidth / 2, pos.y - screenHeight / 2);
	}

	private IEnumerator RebootGame()
	{
		StoreMatrix();
		yield return new WaitForSeconds(1.5f);
		rebootDuration = 5;
		Timer t = TimerManager.GetInstance().CreateTimer(1.0f, TimerType.Loop, rebootDuration);
		t.Tick += RebootTick;
		t.OnComplete += StartScene;
		t.Start();
	}

	private void RebootTick()
	{
		txtSuccess.GetComponent<Text>().text = string.Format("Reboot system after {0} seconds", rebootDuration);
		--rebootDuration;
	}

	private void StartScene()
	{
		Application.LoadLevel("StartInfo");
	}

	private void CheckAgain()
	{
		cross.SetActive(true);
		button.SetActive(false);
		txtFailure.SetActive(false);
		txtSuccess.SetActive(false);
		remainTimes = 5;
		tsPos.Clear();
		iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", -0, "y", 0));
	}

	private void StoreMatrix()
	{
        GameData.GetInstance().SaveTouchMatrix();
	}

	private void FingerUp(UInt16 x, UInt16 y)
	{
		Debug.Log("tx:" + x + ", ty:" + y);
		touches.Enqueue(new Vector2(x, y));
	}

	private void CalcDet()
	{
		double a1 = 0, b1 = 0, c1 = 5, d1 = 0;
		double a2 = 0, b2 = 0, c2 = 0, d2 = 0;
		double a3 = 0, b3 = 0, c3 = 0, d3 = 0;

		double a4 = 0, b4 = 0, c4 = 5, d4 = 0;
		double a5 = 0, b5 = 0, c5 = 0, d5 = 0;
		double a6 = 0, b6 = 0, c6 = 0, d6 = 0;

		for (int i = 0; i < 5; ++i)
		{
			a1 += tsPos[i].x;
			b1 += tsPos[i].y;
			d1 += lcdPos[i].x;

			a2 += Mathf.Pow(tsPos[i].x, 2);
			b2 += tsPos[i].x * tsPos[i].y;
			c2 = a1;
			d2 += lcdPos[i].x * tsPos[i].x;

			a3 = b2;
			b3 += Mathf.Pow(tsPos[i].y, 2);
			c3 = b1;
			d3 += lcdPos[i].x * tsPos[i].y;

			a4 = a1;
			b4 = b1;
			d4 += lcdPos[i].y;

			a5 = a2;
			b5 = b2;
			c5 = c2;
			d5 += lcdPos[i].y * tsPos[i].x;

			a6 = a3;
			b6 = b3;
			c6 = c3;
			d6 += lcdPos[i].y * tsPos[i].y;
		}

		double[,] det1 = new double[,]{{a1, b1, c1, d1}, {a2, b2, c2, d2}, {a3, b3, c3, d3}};
		double[,] det2 = new double[,]{{a4, b4, c4, d4}, {a5, b5, c5, d5}, {a6, b6, c6, d6}};
		double ta, tb, tc;
		double td, te, tf;
		Utils.LEqations3x3(det1, out ta, out tb, out tc);
		Utils.LEqations3x3(det2, out td, out te, out tf);
		GameData.GetInstance().TA = (float)ta;
		GameData.GetInstance().TB = (float)tb;
		GameData.GetInstance().TC = (float)tc;
		GameData.GetInstance().TD = (float)td;
		GameData.GetInstance().TE = (float)te;
		GameData.GetInstance().TF = (float)tf;
	}
}
