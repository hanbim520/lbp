using UnityEngine;  
using UnityEngine.UI;  
using System.Collections;  

public class LaunchCamera : MonoBehaviour  
{  
	public Transform leftTop;
	public Transform rightBottom;
	private WebCamTexture cameraTexture;  
	private string cameraName 	= string.Empty;  
	private bool isPlay 		= false;  
	
	private float wctWidth;
	private float wctHeight;
	private float wctX;
	private float wctY;
	private CanvasScaler canvasScaler;
	
	// 允许先开启程序 后连接摄像头
	private const float checkInterval 	= 3.0f;
	private float checkElapsed 			= 0f;
	private bool bFoundWebcam			= false;	// 找到摄像头
	private bool bTryingWebcam			= true;		// 正在尝试打开摄像头
	
	void Start()  
	{  
		canvasScaler = GetComponent<CanvasScaler>();
//		SetResolution();
//		SetWCTProperties();
        Screen.SetResolution(1920, 1080, true);
        SetWCTProperties2();
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

    private void SetWCTProperties2()
    {
        float ratioWidth = Mathf.Abs(leftTop.localPosition.x - rightBottom.localPosition.x) / canvasScaler.referenceResolution.x;
        float ratioHeight = Mathf.Abs(leftTop.localPosition.y - rightBottom.localPosition.y) / canvasScaler.referenceResolution.y;
        wctWidth = ratioWidth * canvasScaler.referenceResolution.x;
        wctHeight = ratioHeight * canvasScaler.referenceResolution.y;

        float ratioPosX = (leftTop.localPosition.x - (-canvasScaler.referenceResolution.x / 2.0f)) / canvasScaler.referenceResolution.x;
        float ratioPosY = Mathf.Abs(leftTop.localPosition.y - (canvasScaler.referenceResolution.y / 2.0f)) / canvasScaler.referenceResolution.y;
        wctX = ratioPosX * canvasScaler.referenceResolution.x;
        wctY = ratioPosY * canvasScaler.referenceResolution.y;
    }

	//  尝试打开摄像头
	private IEnumerator RequestWebCam()  
	{  
		bTryingWebcam = true;
		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);  
		if (Application.HasUserAuthorization(UserAuthorization.WebCam))  
		{  
			WebCamDevice[] devices = WebCamTexture.devices;  
			if (devices != null && devices.Length > 0)
			{
				bFoundWebcam = true;
				cameraName = devices[0].name;  
//				cameraTexture = new WebCamTexture(cameraName, 640, 480, 30);  
				cameraTexture = new WebCamTexture(cameraName, 1280, 720, 60);  
				cameraTexture.Play();  
				isPlay = true;  
			}
			else
			{
				bFoundWebcam = false;
			}
		}  
		else
		{
			bFoundWebcam = false;
		}
		bTryingWebcam = false;
	}  
	
	void Update()
	{
		if (!bFoundWebcam &&
		    !bTryingWebcam)
		{
			checkElapsed += Time.deltaTime;
			if (checkElapsed > checkInterval)
			{
				checkElapsed = 0;
				StartCoroutine(RequestWebCam());  
			}
		}
		// 摄像头断开后 尝试重新打开摄像头
		if (bFoundWebcam &&
		    cameraTexture != null &&
		    !cameraTexture.isPlaying)
		{
			isPlay = false;
			WebCamTexture.Destroy(cameraTexture);
			cameraTexture = null;
			bFoundWebcam = false;
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
