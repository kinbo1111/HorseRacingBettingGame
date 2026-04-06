using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CurtainHandler : MonoBehaviour
{

    [SerializeField]
    RectTransform curtain_RT;

    [SerializeField]
    public float curtainFadeDur = 0.3f;

    CanvasRenderer curtainRenderer_Cp;

    private void Awake()
    {
        if (!curtain_RT.gameObject.activeSelf)
        {
            curtain_RT.gameObject.SetActive(true);
        }
        curtainRenderer_Cp = curtain_RT.GetComponent<CanvasRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CurtainOpen(TweenCallback cb)
    {
        if (!curtain_RT.gameObject.activeSelf)
        {
            curtain_RT.gameObject.SetActive(true);
        }
        curtainRenderer_Cp.SetAlpha(1f);
        DOTween.To(() => curtainRenderer_Cp.GetAlpha(), x => curtainRenderer_Cp.SetAlpha(x), 0f, curtainFadeDur)
            .OnComplete(() =>
            {
                curtain_RT.gameObject.SetActive(false);
                cb.Invoke();
            });
    }

    public void CurtainClose(TweenCallback cb)
    {
        if (!curtain_RT.gameObject.activeSelf)
        {
            curtain_RT.gameObject.SetActive(true);
        }
        curtainRenderer_Cp.SetAlpha(0f);
        DOTween.To(() => curtainRenderer_Cp.GetAlpha(), x => curtainRenderer_Cp.SetAlpha(x), 1f, curtainFadeDur)
            .OnComplete(cb);
    }

}
