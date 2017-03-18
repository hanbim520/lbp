using UnityEngine;
using System.Collections;

public class TestResolution : MonoBehaviour
{
	private bool hitDownOpt = false;
	
	void Start()
	{
//		Screen.SetResolution(1440, 1080, true);
	}

	void Update()
	{
//		DetectInputEvents();
	}

	private void DetectInputEvents()
	{
		if (InputEx.GetInputDown() && !hitDownOpt)
		{
			Vector2 pos;
			InputEx.InputDownPosition(out pos);
			if (pos == new Vector2(-1, -1))
				return;
			
			float sx, sy;
			Utils.UISpaceToScreenSpace(pos.x, pos.y, out sx, out sy);
			RaycastHit2D[] hit = Physics2D.RaycastAll(new Vector2(sx, sy), Vector2.zero);
			if (hit.Length == 0)
				return;
			
			int idx = 0;
			if (hit.Length > 1)
			{
				for (int i = 0; i < hit.Length; ++i)
				{
					if (hit[i].collider.tag == "Dialog")
					{
						idx = i;
						break;
					}
					else if (hit[i].collider.gameObject.GetComponent<ButtonEvent>() != null)
						idx = i;
				}
			}

			hitDownOpt = true;
		}
		else if (InputEx.GetInputUp() && hitDownOpt)
		{
			Vector2 pos;
			InputEx.InputUpPosition(out pos);
			if (pos == new Vector2(-1, -1))
				return;
			

			hitDownOpt = false;
		}
	}

//	void OnGUI()
//	{
//		if (GUI.Button(new Rect(10, 10, 100, 50), "Quit"))
//		{
//			Application.Quit();
//		}
//	}
}
