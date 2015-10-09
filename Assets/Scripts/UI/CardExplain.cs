using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardExplain : MonoBehaviour
{
	public Text ratio;
	public Text multiple;
	public Text start;

	void OnEnable()
	{
		ratio.text = GameData.GetInstance().couponsKeyinRatio.ToString();
		multiple.text = GameData.GetInstance().couponsKeoutRatio.ToString();
		start.text = GameData.GetInstance().couponsStart.ToString();
	}
}
