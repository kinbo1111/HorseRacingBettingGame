using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_Ending : MonoBehaviour
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
    [SerializeField] UIManager_Ending ui_Cp;
    [SerializeField] string mainSceneName;
    [SerializeField] List<Sprite> resultSprites = new List<Sprite>();
    [SerializeField] List<string> resultTexts = new List<string>();
    [SerializeField] List<float> resultThresholds = new List<float>();
    [SerializeField] AudioSource resultAudioS_Cp;
    [SerializeField] AudioClip succAudioClip, failedAudioClip;

    //-------------------------------------------------- public fields
    [ReadOnly] public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public int gResultLevel;

    //-------------------------------------------------- private fields
    DataManager dataManager_Cp;

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
        InitGameResultLevel();
        InitUI();
        InitAudio();

        mainGameState = GameState_En.Inited;

        curtain_Cp.CurtainOpen(() => { Play(); });
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

    void InitGameResultLevel()
    {
        GameData_St gameData_tp = dataManager_Cp.gameData;
        if (gameData_tp.totalBettingCount > (dataManager_Cp.endAge - dataManager_Cp.startAge) * 12 / 4)
        {
            float succPercent = (float)gameData_tp.successBettingCount / (float)gameData_tp.totalBettingCount * 100f;
            for (int i = 0; i < resultThresholds.Count; i++)
            {
                if (succPercent >= resultThresholds[i])
                {
                    gResultLevel = i;
                    break;
                }
            }
        }
        else
        {
            gResultLevel = resultThresholds.Count - 1;
        }
    }

    void InitUI()
    {
        ui_Cp.Init();
        GameData_St gameData_tp = dataManager_Cp.gameData;
        ui_Cp.totalBettingCount = gameData_tp.totalBettingCount;
        ui_Cp.bettingSuccCountText = gameData_tp.successBettingCount;
        ui_Cp.resultSprite = resultSprites[gResultLevel];
        ui_Cp.resultText = resultTexts[gResultLevel];
    }

    void InitAudio()
    {
        resultAudioS_Cp.loop = false;
        if (gResultLevel < resultThresholds.Count - 1)
        {
            resultAudioS_Cp.clip = succAudioClip;
        }
        else
        {
            resultAudioS_Cp.clip = failedAudioClip;
        }
    }

    #endregion

    void Play()
    {
        audio_Cp.Play();

        Invoke("Invoke_PlayGameResultAudio", 1f);
    }

    void Invoke_PlayGameResultAudio()
    {
        resultAudioS_Cp.Play();
    }

    void ResetAndLoadData()
    {
        dataManager_Cp.ResetAndLoadData();
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void Escape()
    {
        ResetAndLoadData();
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
