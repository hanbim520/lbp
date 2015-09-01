using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchCheck : MonoBehaviour
{
	public GameObject cross;
	public GameObject button;

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
		button.SetActive(false);
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
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			if (remainTimes > 0)
			{
				if (Input.GetMouseButtonUp(0))
				{
					Vector2 touchPos = Input.mousePosition;
					lcdPos.Add(touchPos);
					tsPos.Add(touchPos);
					--remainTimes;
					TranslateCross();
				}
			}
			else
			{
				Vector2 touchPos = Input.mousePosition;
//				if (Vector2.)
			}
		}
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
		}
	}
}
