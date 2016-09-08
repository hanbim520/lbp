using UnityEngine;  
using UnityEngine.UI;  
using System.Collections;  

public class LaunchCamera : MonoBehaviour  
{  
	public Transform leftTop;
	public Transform rightBottom;
	private WebCamTexture cameraTexture;  
	private string cameraName = string.Empty;  
	private bool isPlay = false;  

	private float wctWidth;
	private float wctHeight;
	private float wctX;
	private float wctY;
	private CanvasScaler canvasScaler;

	void Start()  
	{  
		canvasScaler = GetComponent<CanvasScaler>();
		SetResolution();
		SetWCTProperties();
		StartCoroutine(RequestWebCam());  
	}  

	private void SetResolution()
	{
		float ratio = 1920.0f / 1080.0f;
		Resolution[] r = Screen.resolutions;
		for (int i = 0; i < r.Length; ++i)
		{
			float tmp = (float)r[i].width / r[i].height;
			if (Mathf.Approximately(ratio, tmp))
			{
				Screen.SetResolution(r[i].width, r[i].height, true);
				break;
			}
		}
	}

	private void SetWCTProperties()
	{
		float ratioWidth = Mathf.Abs(leftTop.localPosition.x - rightBottom.localPosition.x) / canvasScaler.referenceResolution.x;
		float ratioHeight = Mathf.Abs(leftTop.localPosition.y - rightBottom.localPosition.y) / canvasScaler.referenceResolution.y;
		wctWidth = ratioWidth * Screen.width;
		wctHeight = ratioHeight * Screen.height;

		float ratioPosX = (leftTop.localPosition.x - (-canvasScaler.referenceResolution.x / 2.0f)) / canvasScaler.referenceResolution.x;
		float ratioPosY = Mathf.Abs(leftTop.localPosition.y - (canvasScaler.referenceResolution.y / 2.0f)) / canvasScaler.referenceResolution.y;
		wctX = ratioPosX * canvasScaler.referenceResolution.x;
		wctY = ratioPosY * canvasScaler.referenceResolution.y;
	}
	
	private IEnumerator RequestWebCam()  
	{  
		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);  
		if (Application.HasUserAuthorization(UserAuthorization.WebCam))  
		{  
			WebCamDevice[] devices = WebCamTexture.devices;  
			cameraName = devices[0].name;  
			cameraTexture = new WebCamTexture(cameraName, 640, 480, 30);  
			cameraTexture.Play();  
			isPlay = true;  
		}  
	}  
	
	void OnGUI()  
	{  
		if (isPlay)  
		{  
			GUI.DrawTexture(new Rect(wctX, wctY, wctWidth, wctHeight), cameraTexture, ScaleMode.ScaleAndCrop);  
		}  
	}  
}  
