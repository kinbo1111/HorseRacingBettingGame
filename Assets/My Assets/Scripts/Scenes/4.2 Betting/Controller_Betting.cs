using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_Betting : MonoBehaviour
{

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Types
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Types

    public enum GameState_En
    {
        Nothing, Inited, Playing, Finished,
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
    [SerializeField] public UIManager_Betting ui_Cp;
    [SerializeField] public Data_Betting data_Cp;
    [SerializeField] public BettingHandler betHandler_Cp;
    [SerializeField] public CameraHandler_Betting camHandler_Cp;
    [SerializeField] string mainSceneName, nextSceneName;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    public DataManager dataManager_Cp;

    //-------------------------------------------------- private fields

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

        data_Cp.Init();
        yield return new WaitUntil(() => data_Cp.mainGameState == Data_Betting.GameState_En.Inited);

        ui_Cp.Init();
        yield return new WaitUntil(() => ui_Cp.mainGameState == UIManager_Betting.GameState_En.Inited);

        betHandler_Cp.Init();
        yield return new WaitUntil(() => betHandler_Cp.mainGameState == BettingHandler.GameState_En.Inited);

        camHandler_Cp.Init();
        yield return new WaitUntil(() => camHandler_Cp.mainGameState == CameraHandler_Betting.GameState_En.Inited);

        mainGameState = GameState_En.Inited;

        LookAroundRaceTrack();
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
            dataManager_Cp.playerRacerId = Random.Range(0, dataManager_Cp.totalRacerCount);
            dataManager_Cp.bettingMoney = Random.Range(0, dataManager_Cp.gameData.money + 1);
            dataManager_Cp.InitRacersPerformancesAndOdds_Test();
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
        betHandler_Cp.hasRacingNewspaper = gameData_tp.hasRacingNewspaper;

        gameData_tp.money -= dataManager_Cp.bettingMoney;
        gameData_tp.totalBettingCount += 1;
        int month = gameData_tp.month + 1;
        int age = gameData_tp.age;
        if (month == 13)
        {
            month = 1;
            age += 1;
        }
        gameData_tp.age = age;
        gameData_tp.month = month;
        gameData_tp.hasRacingNewspaper = false;
        dataManager_Cp.gameData = gameData_tp;
        dataManager_Cp.SaveGameData();
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    void LookAroundRaceTrack()
    {
        StartCoroutine(Corou_LookAroundRaceTrack());
    }

    IEnumerator Corou_LookAroundRaceTrack()
    {
        CurtainOpen();
        yield return new WaitForSeconds(curtain_Cp.curtainFadeDur);

        camHandler_Cp.LookAroundRaceTrack();
        yield return new WaitUntil(() => camHandler_Cp.ExistGameStates(CameraHandler_Betting.GameState_En.LookArounded));
        camHandler_Cp.RemoveGameStates(CameraHandler_Betting.GameState_En.LookArounded);

        CurtainClose();
        yield return new WaitForSeconds(curtain_Cp.curtainFadeDur);

        betHandler_Cp.startGate_GO.SetActive(false);

        CurtainOpen();
        yield return new WaitForSeconds(curtain_Cp.curtainFadeDur);

        Play();
    }

    void Play()
    {
        betHandler_Cp.Play();

        mainGameState = GameState_En.Playing;
    }

    #endregion

    void SaveFinalData()
    {
        dataManager_Cp.betResult = betHandler_Cp.IsBettingSuccess();
        GameData_St gameData_tp = dataManager_Cp.gameData;
        if (betHandler_Cp.IsBettingSuccess())
        {
            gameData_tp.successBettingCount += 1;
            gameData_tp.money += Mathf.RoundToInt((float)dataManager_Cp.bettingMoney
                * dataManager_Cp.racersOdds[betHandler_Cp.playerRacer_Cp.racerId]);
        }
        dataManager_Cp.gameData = gameData_tp;
        dataManager_Cp.SaveGameData();
    }

    //////////////////////////////////////////////////////////////////////
    /// Curtain
    //////////////////////////////////////////////////////////////////////
    #region Curtain

    void CurtainOpen()
    {
        curtain_Cp.CurtainOpen(() =>
        {
            audio_Cp.Play();
        });
    }

    void CurtainClose()
    {
        curtain_Cp.CurtainClose(() =>
        {

        });
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void OnRacingFinished()
    {
        mainGameState = GameState_En.Finished;
    }

    public void Escape()
    {
        if (mainGameState == GameState_En.Inited)
        {
            camHandler_Cp.StopLookAroundRaceTrack();
        }
        else if (mainGameState == GameState_En.Playing)
        {
            if (betHandler_Cp.playerRacer_Cp.mainGameState == RacerHandler.GameState_En.Finished)
            {
                SaveFinalData();
            }
            LoadNewScene(mainSceneName);
        }
        else if (mainGameState == GameState_En.Finished)
        {
            SaveFinalData();
            LoadNewScene(nextSceneName);
        }
    }

    public void LoadNewScene(string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }
        audio_Cp.Finish();
        curtain_Cp.CurtainClose(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    #endregion

}
