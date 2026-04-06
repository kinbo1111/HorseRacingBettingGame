using GlobalSnowEffect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_Delivering : MonoBehaviour
{

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Types
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Types

    public enum GameState_En
    {
        Nothing, Inited, Prepared, Playing, Finished,
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    [SerializeField] public CurtainHandler curtain_Cp;
    [SerializeField] AudioHandler audio_Cp;
    [SerializeField] public UIManager_Delivering ui_Cp;
    [SerializeField] public Data_Delivering data_Cp;
    [SerializeField] public BikersHandler bikersHandler_Cp;
    [SerializeField] public EnvHandler envHandler_Cp;
    [SerializeField] public CameraHandler_Delivering camHandler_Cp;
    [SerializeField] public TimerHandler_Delivering timer_Cp;
    [SerializeField] public MapHandler mapHandler_Cp;
    [SerializeField] public Player player_Cp;
    [SerializeField] public TrafficHandler trafficHandler_Cp;
    [SerializeField] public GameLevelHandler gLevelHandler_Cp;
    [SerializeField] GameObject rain_GO;
    [SerializeField] GlobalSnow snow_Cp;
    [SerializeField] string mainSceneName, evaluationSceneName;
    [SerializeField] AudioSource resultAudioS_Cp;
    [SerializeField] AudioClip succResultAudioClip, failedResultAudioClip;
    [SerializeField] float resultAudioVolume = 0.6f;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public DataManager dataManager_Cp;

    //-------------------------------------------------- private fields
    [SerializeField][ReadOnly] bool deliverResult;

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

    #endregion

    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Methods
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    // Start is called before the first frame update
    void Start()
    {
        Init();
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

    void Init()
    {
        StartCoroutine(Corou_Init());
    }

    IEnumerator Corou_Init()
    {
        AddGameStates(GameState_En.Nothing);

        InitDataManager();
        SaveCurrentGameData();
        InitSky();

        data_Cp.Init();
        yield return new WaitUntil(() => data_Cp.mainGameState == Data_Delivering.GameState_En.Inited);

        ui_Cp.Init();
        yield return new WaitUntil(() => ui_Cp.mainGameState == UIManager_Delivering.GameState_En.Inited);

        gLevelHandler_Cp.Init();
        yield return new WaitUntil(() => gLevelHandler_Cp.mainGameState == GameLevelHandler.GameState_En.Inited);

        envHandler_Cp.Init();
        yield return new WaitUntil(() => envHandler_Cp.mainGameState == EnvHandler.GameState_En.Inited);

        player_Cp.Init();
        yield return new WaitUntil(() => player_Cp.mainGameState == Player.GameState_En.Inited);

        camHandler_Cp.Init();
        yield return new WaitUntil(() => camHandler_Cp.mainGameState == CameraHandler_Delivering.GameState_En.Inited);

        timer_Cp.Init();
        timer_Cp.timerEndEvent.AddListener(OnEndTime);

        ui_Cp.SetActivePopupPanels(false);

        InitAudio();

        mainGameState = GameState_En.Inited;

        Prepare();
    }

    void InitDataManager()
    {
        bool isTestMode = false;
        if (dataManager_Cp == null)
        {
            dataManager_Cp = FindObjectOfType<DataManager>();
            if (dataManager_Cp == null)
            {
                dataManager_Cp = new GameObject().AddComponent<DataManager>();
                isTestMode = true;
            }
        }
        GameObject.DontDestroyOnLoad(dataManager_Cp.gameObject);

        if (dataManager_Cp.gameStates.Count > 0 && dataManager_Cp.mainGameState == DataManager.GameState_En.Inited)
        {
            return;
        }

        if (isTestMode)
        {
            dataManager_Cp.Init(DataManager.InitMode_En.Test);
            dataManager_Cp.deliverSkyId = Random.Range(0, 3);
        }
        else if (dataManager_Cp.IsExistGameData())
        {
            dataManager_Cp.Init(DataManager.InitMode_En.Saved);
        }
        else
        {
            dataManager_Cp.Init(DataManager.InitMode_En.Start);
        }
    }

    void SaveCurrentGameData()
    {
        GameData_St gameData_tp = dataManager_Cp.gameData;
        int month = gameData_tp.month + 1;
        int age = gameData_tp.age;
        if (month == 13)
        {
            month = 1;
            age += 1;
        }
        gameData_tp.age = age;
        gameData_tp.month = month;
        dataManager_Cp.gameData = gameData_tp;
        dataManager_Cp.SaveGameData();
    }

    void InitSky()
    {
        if (dataManager_Cp.deliverSkyId == 0)
        {
            SetActiveRain(false);
            SetActiveSnow(false);
        }
        else if (dataManager_Cp.deliverSkyId == 1)
        {
            SetActiveRain(true);
            SetActiveSnow(false);
        }
        else if (dataManager_Cp.deliverSkyId == 2)
        {
            SetActiveRain(false);
            SetActiveSnow(true);
        }
    }

    void SetActiveRain(bool active)
    {
        rain_GO.SetActive(active);
    }

    void SetActiveSnow(bool active)
    {
        snow_Cp.enabled = active;
    }

    void InitAudio()
    {
        resultAudioS_Cp.loop = false;
        resultAudioS_Cp.volume = resultAudioVolume;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Prepare
    //////////////////////////////////////////////////////////////////////
    #region Prepare

    void Prepare()
    {
        StartCoroutine(Corou_Prepare());
    }

    IEnumerator Corou_Prepare()
    {
        curtain_Cp.CurtainOpen(() => { });

        mainGameState = GameState_En.Prepared;

        Play();

        yield return null;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    void Play()
    {
        StartCoroutine(Corou_Play());
    }

    IEnumerator Corou_Play()
    {
        mainGameState = GameState_En.Playing;

        audio_Cp.Play();
        timer_Cp.PlayTimer();
        envHandler_Cp.Play();
        player_Cp.Play();

        yield return null;
    }

    void SaveData()
    {
        GameData_St gameData_tp = dataManager_Cp.gameData;
        dataManager_Cp.deliverResult = deliverResult;
        if (deliverResult)
        {
            if (player_Cp.gotNewspaper)
            {
                gameData_tp.hasRacingNewspaper = player_Cp.gotNewspaper;
            }
            dataManager_Cp.gotCoin = player_Cp.coin;
            dataManager_Cp.gotBonus = dataManager_Cp.deliverSuccBonus[gLevelHandler_Cp.gLevelId];
            gameData_tp.money += (player_Cp.coin + dataManager_Cp.deliverSuccBonus[gLevelHandler_Cp.gLevelId]);
        }
        else
        {
            dataManager_Cp.gotCoin = 0;
            dataManager_Cp.gotBonus = 0;
        }
        dataManager_Cp.gameData = gameData_tp;
        dataManager_Cp.SaveGameData();
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    void OnEndTime()
    {
        OnFailed();        
    }

    public void OnSuccess()
    {
        resultAudioS_Cp.clip = succResultAudioClip;
        resultAudioS_Cp.Play();

        deliverResult = true;
        FinishPlaying();
        ui_Cp.SetActiveSuccessPanel(true);        
    }

    public void OnFailed()
    {
        resultAudioS_Cp.clip = failedResultAudioClip;
        resultAudioS_Cp.Play();

        deliverResult = false;
        FinishPlaying();
        ui_Cp.SetActiveFailedPanel(true);
    }

    public void FinishPlaying()
    {
        mainGameState = GameState_En.Finished;
        player_Cp.FinishPlay();
        envHandler_Cp.FinishPlay();
    }

    public void Escape()
    {
        SaveData();
        if (audio_Cp != null)
        {
            audio_Cp.Finish();
        }

        if (mainGameState == GameState_En.Playing)
        {
            LoadNewScene(mainSceneName);
        }
        else if (mainGameState == GameState_En.Finished)
        {
            LoadNewScene(evaluationSceneName);
        }
    }

    public void LoadNewScene(string sceneName = null)
    {
        if (sceneName == null)
        {
            return;
        }
        
        curtain_Cp.CurtainClose(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    #endregion
}
