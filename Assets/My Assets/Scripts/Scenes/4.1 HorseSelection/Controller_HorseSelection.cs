using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_HorseSelection : MonoBehaviour
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
    [SerializeField] CurtainHandler curtain_Cp;
    [SerializeField] AudioHandler audio_Cp;
    [SerializeField] Data_HorseSelection data_Cp;
    [SerializeField] AdsManager adsManager_Cp;
    [SerializeField] public UIManager_HorseSelection ui_Cp;

    [SerializeField] string mainSceneName, bettingSceneName;

    //-------------------------------------------------- public fields
    [ReadOnly] public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public DataManager dataManager_Cp;
    [ReadOnly] public int selectedRacerIndex;
    [ReadOnly] public int inputBettingMoney;

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

    public void Init()
    {
        AddGameStates(GameState_En.Nothing);

        InitDataManager();
        GenerateRaceWinnerCandidateId();
        InitRacersOddsAndPerformances();
        InitUI();
        InitAdsManager();

        mainGameState = GameState_En.Inited;

        curtain_Cp.CurtainOpen(() => { audio_Cp.Play(); Play(); });
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

    void GenerateRaceWinnerCandidateId()
    {
        dataManager_Cp.raceWinnerCandidateIds.Clear();
        List<int> candIds_tp = new List<int>();
        if (dataManager_Cp.gameData.hasRacingNewspaper)
        {
            for (int i = 0; i < 3; i++)
            {
                int candId_tp = 0;
                do
                {
                    candId_tp = Random.Range(0, dataManager_Cp.totalRacerCount);
                }
                while (candIds_tp.Contains(candId_tp));

                candIds_tp.Add(candId_tp);
                dataManager_Cp.raceWinnerCandidateIds.Add(candId_tp);
            }
        }
    }

    void InitRacersOddsAndPerformances()
    {
        dataManager_Cp.racersOdds.Clear();
        float oddsInterval = (dataManager_Cp.maxOdds - dataManager_Cp.minOdds) / dataManager_Cp.totalRacerCount;
        List<int> randomIds = new List<int>();
        for (int i = 0; i < dataManager_Cp.totalRacerCount; i++)
        {
            int randomId = 0;
            do
            {
                randomId = Random.Range(0, dataManager_Cp.totalRacerCount);
            } while (randomIds.Contains(randomId));

            randomIds.Add(randomId);
            float odds = Random.Range(dataManager_Cp.minOdds + oddsInterval * randomId,
                dataManager_Cp.minOdds + (oddsInterval) * (randomId + 1));
            odds = Mathf.Round(odds * 10f) / 10f;
            dataManager_Cp.racersOdds.Add(odds);
        }

        dataManager_Cp.racersPerformances.Clear();
        for (int i = 0; i < dataManager_Cp.totalRacerCount; i++)
        {
            dataManager_Cp.racersPerformances.Add(dataManager_Cp.racersOdds[i] / 100f
                * dataManager_Cp.maxRacerPerformance);
        }
    }

    void InitUI()
    {
        ui_Cp.Init();
        ui_Cp.InitGameDataUI();
        ui_Cp.InitRacersPanel();
        ui_Cp.InitBettingMoneyInputPanel();
        ui_Cp.InitNewspaperModal();
    }

    void InitAdsManager()
    {
        adsManager_Cp.onAdsVideoPlaying.AddListener(OnAdsVideoPlaying);
        adsManager_Cp.onAdsVideoStop.AddListener(OnAdsVideoStop);
        adsManager_Cp.Init();
    }

    #endregion

    void Play()
    {
        mainGameState = GameState_En.Playing;

        adsManager_Cp.PlayAds();
    }

    bool IsBettingAvailable()
    {
        bool result = false;

        if (selectedRacerIndex >= 0 && selectedRacerIndex < dataManager_Cp.totalRacerCount
            && inputBettingMoney >= dataManager_Cp.minBettingMoney
            && inputBettingMoney <= dataManager_Cp.gameData.money)
        {
            result = true;
        }

        return result;
    }

    public void OnClick_BettingBtn()
    {
        if (!IsBettingAvailable())
        {
            return;
        }
        dataManager_Cp.playerRacerId = selectedRacerIndex;
        dataManager_Cp.bettingMoney = inputBettingMoney;

        LoadNewScene(bettingSceneName);
    }

    void OnAdsVideoPlaying()
    {
        audio_Cp.Pause();
    }

    void OnAdsVideoStop()
    {
        audio_Cp.Resume();
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void Escape()
    {
        LoadNewScene(mainSceneName);
    }

    void LoadNewScene(string sceneName = null)
    {
        audio_Cp.Finish();
        curtain_Cp.CurtainClose(() =>
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        });
    }

    #endregion
}
