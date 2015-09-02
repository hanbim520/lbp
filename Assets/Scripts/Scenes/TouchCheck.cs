using UnityEngine;
using UnityEngine.UI;
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

	void Start()
	{
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		remainTimes = 5;
		lcdPos.Add(new Vector2(0, 0));
		lcdPos.Add(new Vector2(-800, 400));
		lcdPos.Add(new Vector2(-800, -400));
		lcdPos.Add(new Vector2(800, -400));
		lcdPos.Add(new Vector2(800, 400));
		button.SetActive(false);
		txtFailure.SetActive(false);
		txtSuccess.SetActive(false);
		cross.SetActive(true);
	}
	
	void Update()
	{
		HandleInput();
	}

	private void LEqations3x3(double[,] det)
	{
		double x, y, z;
		Utils.LEqations3x3(det, out x, out y, out z);
	}

	private void HandleInput()
	{
		if (remainTimes > 0)
		{
			if (GetTouchUp())
			{
				Vector2 touchPos = Input.mousePosition;
				tsPos.Add(touchPos);
				--remainTimes;
				TranslateCross();
			}
		}
		else if (remainTimes < 0)
		{
			if (GetTouchUp())
			{
				Vector2 touchPos = Input.mousePosition;
				touchPos = ScreenSpaceToUISpace(touchPos);
				RectTransform rt = button.GetComponent<RectTransform>();
				if (Utils.PointInRect(touchPos, rt.rect))
				{
					txtSuccess.SetActive(true);
					StartCoroutine(RebotGame());
				}
				else
				{
					txtFailure.SetActive(true);
					Timer t = TimerManager.GetInstance().CreateTimer(1.5f);
					t.Tick += CheckAgain;
					t.Start();
				}
				button.SetActive(false);
			}
		}
	}

	public bool GetTouchUp()
	{
		#if UNITY_EDITOR
		return Input.GetMouseButtonUp(0);
		#endif

		#if UNITY_ANDROID
		#endif
	}

	private void TranslateCross()
	{
		if (remainTimes == 4)
		{
			iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", -800, "y", 400));
		}
		else if (remainTimes == 3)
		{
			iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", -800, "y", -400));
		}
		else if (remainTimes == 2)
		{
			iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", 800, "y", -400));
		}
		else if (remainTimes == 1)
		{
			iTween.MoveTo(cross, iTween.Hash("time", 0.6, "islocal", true, "x", 800, "y", 400));
		}
		else if (remainTimes == 0)
		{
			cross.SetActive(false);
			button.SetActive(true);
			--remainTimes;
		}
	}

	private Vector2 ScreenSpaceToUISpace(Vector2 pos)
	{
		return new Vector2(pos.x - screenWidth / 2, pos.y - screenHeight / 2);
	}

	private IEnumerator RebotGame()
	{
		yield return new WaitForSeconds(1.5f);
		StoreMatrix();
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

	}
}
