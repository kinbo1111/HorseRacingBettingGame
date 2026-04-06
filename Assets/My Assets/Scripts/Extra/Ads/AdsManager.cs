using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class AdsManager : MonoBehaviour
{

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Types
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Types

    public enum GameState_En
    {
        Nothing, Inited, Playing,
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    [SerializeField] Renderer adsTextureRenderer_Cp, adsVideoRenderer_Cp;
    [SerializeField] VideoPlayer videoPlayer_Cp, testVideoPlayer_Cp;
    [SerializeField] GameObject adsCancelBtn_GO;
    [SerializeField] float adsPresentPossibility = 0.5f, videoPresentPossibility = 0.5f;
    [SerializeField] bool isTestVideoMode;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public UnityEvent onAdsVideoPlaying, onAdsVideoStop;

    //-------------------------------------------------- private fields
    DataManager dataManager_Cp;
    AssetsLoader assetsLoader_Cp;

    bool presentAds, presentVideo;

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Properties
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Properties

    //-------------------------------------------------- public properties
    public GameState_En mainGameState
    {
        get { return gameStates[0]; }
        set { gameStates[0] = value; }
    }

    //-------------------------------------------------- private properties
    List<Texture2D> downloadedTextures
    {
        get { return assetsLoader_Cp.downloadedTextures; }
    }
    List<string> videoPlayUrls
    {
        get { return assetsLoader_Cp.downloadedVideoPaths; }
    }
    
    #endregion

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Methods
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        videoPlayer_Cp.playOnAwake = false;
        testVideoPlayer_Cp.playOnAwake = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Manage gameStates
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region ManageGameStates

    //--------------------------------------------------
    public void AddMainGameState(GameState_En value = GameState_En.Nothing)
    {
        if (gameStates.Count == 0)
        {
            gameStates.Add(value);
        }
    }

    //--------------------------------------------------
    public void AddGameStates(params GameState_En[] values)
    {
        foreach (GameState_En value_tp in values)
        {
            gameStates.Add(value_tp);
        }
    }

    //--------------------------------------------------
    public bool ExistGameStates(params GameState_En[] values)
    {
        bool result = true;
        foreach (GameState_En value in values)
        {
            if (!gameStates.Contains(value))
            {
                result = false;
                break;
            }
        }

        return result;
    }

    //--------------------------------------------------
    public bool ExistAnyGameStates(params GameState_En[] values)
    {
        bool result = false;
        foreach (GameState_En value in values)
        {
            if (gameStates.Contains(value))
            {
                result = true;
                break;
            }
        }

        return result;
    }

    //--------------------------------------------------
    public int GetExistGameStatesCount(GameState_En value)
    {
        int result = 0;

        for (int i = 0; i < gameStates.Count; i++)
        {
            if (gameStates[i] == value)
            {
                result++;
            }
        }

        return result;
    }

    //--------------------------------------------------
    public void RemoveGameStates(params GameState_En[] values)
    {
        foreach (GameState_En value in values)
        {
            gameStates.RemoveAll(gameState_tp => gameState_tp == value);
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Initialize
    //////////////////////////////////////////////////////////////////////
    #region Initialize

    public void Init()
    {
        AddGameStates(GameState_En.Nothing);

        SetComponents();
        InitComponents();

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        dataManager_Cp = FindObjectOfType<DataManager>();
        if (dataManager_Cp.assetsLoader_Cp == null)
        {
            assetsLoader_Cp = dataManager_Cp.gameObject.AddComponent<AssetsLoader>();
        }
    }

    void InitComponents()
    {
        adsTextureRenderer_Cp.enabled = false;
        adsVideoRenderer_Cp.enabled = false;
        adsCancelBtn_GO.SetActive(false);
    }

    #endregion

    public void PlayAds()
    {
        StartCoroutine(Corou_PlayAds());
    }

    IEnumerator Corou_PlayAds()
    {
        yield return new WaitUntil(() => assetsLoader_Cp.assetLoadDone);

        presentAds = Random.value <= adsPresentPossibility ? true : false;
        if (!presentAds)
        {
            yield break;
        }
        
        presentVideo = Random.value <= videoPresentPossibility ? true : false;
        if (!presentVideo)
        {
            if (downloadedTextures.Count > 0)
            {
                PresentTexture();
            }
        }
        else
        {
            if (isTestVideoMode)
            {
                Test_PlayVideo();

            }
            else
            {
                if (videoPlayUrls.Count > 0)
                {
                    PlayVideo();
                }
            }
        }

        mainGameState = GameState_En.Playing;
    }

    void PresentTexture()
    {
        adsTextureRenderer_Cp.enabled = true;
        adsTextureRenderer_Cp.material.mainTexture = downloadedTextures[Random.Range(0, downloadedTextures.Count)];
        adsCancelBtn_GO.SetActive(true);
    }

    void PlayVideo()
    {
        videoPlayer_Cp.source = VideoSource.Url;
        videoPlayer_Cp.url = videoPlayUrls[Random.Range(0, videoPlayUrls.Count)];
        videoPlayer_Cp.isLooping = true;
        videoPlayer_Cp.Prepare();
        videoPlayer_Cp.prepareCompleted += VideoPlayer_prepareCompleted;
    }

    void Test_PlayVideo()
    {
        testVideoPlayer_Cp.isLooping = true;
        testVideoPlayer_Cp.Prepare();
        testVideoPlayer_Cp.prepareCompleted += VideoPlayer_prepareCompleted;
    }

    void VideoPlayer_prepareCompleted(VideoPlayer vp)
    {
        onAdsVideoPlaying.Invoke();

        adsVideoRenderer_Cp.enabled = true;
        vp.Play();
        adsCancelBtn_GO.SetActive(true);
    }

    void DisableAdsPanels()
    {
        adsTextureRenderer_Cp.enabled = false;
        adsVideoRenderer_Cp.enabled = false;
        videoPlayer_Cp.Stop();
        adsCancelBtn_GO.SetActive(false);

        onAdsVideoStop.Invoke();
    }

    public void OnClick_CancelAdsBtn()
    {
        DisableAdsPanels();
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion
}
