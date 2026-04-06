using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_Main : MonoBehaviour
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
    [SerializeField] public CurtainHandler curtainHandler_Cp;
    [SerializeField] AudioHandler audio_Cp;
    [SerializeField] public UIManager_Main ui_Cp;

    [SerializeField] public string startSceneName, continueSceneName, optionsSceneName, aboutSceneName,
        endingSceneName;

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
        if (gameStates.Count > 0 && mainGameState == GameState_En.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Backdoor_SetHasNewspaper(true);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Backdoor_SetMoney(dataManager_Cp.minBettingMoney);
            }
        }
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
        InitUI();

        curtainHandler_Cp.CurtainOpen(() =>
        {
            audio_Cp.Play();
        });

        mainGameState = GameState_En.Inited;

        Play();
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

        // check the age
        if (dataManager_Cp.gameData.age > dataManager_Cp.endAge)
        {
            SceneManager.LoadScene(endingSceneName);
        }

        // check the dataManager is already inited
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

    void InitUI()
    {        
        ui_Cp.Init();
        if (dataManager_Cp.IsExistGameData())
        {
            ui_Cp.SetActiveContinueBtn(true);
        }
        else
        {
            ui_Cp.SetActiveContinueBtn(false);
        }
    }

    #endregion

    void Play()
    {
        mainGameState = GameState_En.Playing;
    }

    public void OnClick_StartScene()
    {
        dataManager_Cp.ResetAndLoadData();
        LoadNewScene(startSceneName);
    }

    //////////////////////////////////////////////////////////////////////
    /// Backdoor
    //////////////////////////////////////////////////////////////////////
    #region Backdoor

    void Backdoor_SetHasNewspaper(bool active)
    {
        GameData_St gameData_tp = dataManager_Cp.gameData;
        gameData_tp.hasRacingNewspaper = active;
        dataManager_Cp.gameData = gameData_tp;
        dataManager_Cp.SaveGameData();
    }

    void Backdoor_SetMoney(int amount)
    {
        GameData_St gameData_tp = dataManager_Cp.gameData;
        gameData_tp.money += amount;
        dataManager_Cp.gameData = gameData_tp;
        dataManager_Cp.SaveGameData();
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void Escape()
    {
        LoadNewScene(string.Empty);
    }

    public void OnClick_LoadScene(string sceneName)
    {
        LoadNewScene(sceneName);
    }

    void LoadNewScene(string sceneName = null)
    {
        audio_Cp.Finish();
        curtainHandler_Cp.CurtainClose(() =>
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Application.Quit();
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
