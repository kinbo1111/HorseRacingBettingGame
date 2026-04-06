using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionHandler : MonoBehaviour
{
    [SerializeField]
    int targetWidth = 1920, targetHeight = 1080;

    float displayScaleRatio
    {
        get { return (float)Display.main.renderingWidth / (float)Display.main.renderingHeight; }
    }
    float targetScaleRatio
    {
        get { return (float)targetWidth / (float)targetHeight; }
    }

    void Awake()
    {
        AdjustCameraViewport();
        AdjustCanvas();
    }

    void Start()
    {
        
    }

    void AdjustCameraViewport()
    {
        Rect camRect = new Rect();
        if (displayScaleRatio > targetScaleRatio)
        {
            float targetVSdisplayRatio = targetScaleRatio / displayScaleRatio;
            camRect.width = targetVSdisplayRatio;
            camRect.height = 1f;
            camRect.x = (1f - targetVSdisplayRatio) / 2f;
            camRect.y = 0f;
        }
        else
        {
            float displayVStargetRatio = displayScaleRatio / targetScaleRatio;
            camRect.width = 1f;
            camRect.height = displayVStargetRatio;
            camRect.x = 0f;
            camRect.y = (1f - displayVStargetRatio) / 2f;
        }

        List<Camera> allCam_Cps = new List<Camera>(FindObjectsOfType<Camera>());
        for (int i = 0; i < allCam_Cps.Count; i++)
        {
            allCam_Cps[i].rect = camRect;
        }
    }

    void AdjustCanvas()
    {
        List<Canvas> allCanvas = new List<Canvas>(FindObjectsOfType<Canvas>());
        List<Canvas> overlayCanvas = new List<Canvas>();
        for (int i = 0; i < allCanvas.Count; i++)
        {
            if (allCanvas[i].renderMode == RenderMode.ScreenSpaceOverlay)
            {
                overlayCanvas.Add(allCanvas[i]);
            }
        }
        for (int i = 0; i < overlayCanvas.Count; i++)
        {
            CanvasScaler cScaler = overlayCanvas[i].GetComponent<CanvasScaler>();
            cScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cScaler.referenceResolution = new Vector2(targetWidth, targetHeight);

            //
            RectTransform canvasGroup_RT = overlayCanvas[i].transform.GetChild(0).GetComponent<RectTransform>();
            if (displayScaleRatio > targetScaleRatio)
            {
                float targetVSdisplayRatio = targetScaleRatio / displayScaleRatio;
                canvasGroup_RT.anchorMin = new Vector2((1f - targetVSdisplayRatio) / 2f, 0f);
                canvasGroup_RT.anchorMax = new Vector2((1f + targetVSdisplayRatio) / 2f, 1f);
            }
            else
            {
                float displayVStargetRatio = displayScaleRatio / targetScaleRatio;
                canvasGroup_RT.anchorMin = new Vector2(0f, (1f - displayVStargetRatio) / 2f);
                canvasGroup_RT.anchorMax = new Vector2(1f, (1f + displayVStargetRatio) / 2f);
            }
            canvasGroup_RT.pivot = new Vector2(0.5f, 0.5f);
            canvasGroup_RT.offsetMin = new Vector2(0f, 0f);
            canvasGroup_RT.offsetMax = new Vector2(0f, 0f);
        }
    }

}