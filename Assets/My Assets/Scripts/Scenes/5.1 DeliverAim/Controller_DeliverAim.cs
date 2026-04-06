using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_DeliverAim : MonoBehaviour
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
    [SerializeField] public UIManager_DeliverAim ui_Cp;
    [SerializeField] List<string> deliveringScenes = new List<string>();
    [SerializeField] string mainSceneName;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public DataManager dataManager_Cp;

    //-------------------------------------------------- private fields
    int deliverSceneId, deliverSkyId;

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
        ui_Cp.Init();
        InitRandomMapAndEnv();
        dataManager_Cp.gLevelId = ui_Cp.gLevelId;

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

        if (dataManager_Cp.gameStates.Count > 0 && dataManager_Cp.mainGameState == DataManager.GameState_En.Inited)
        {
            return;
        }

        GameObject.DontDestroyOnLoad(dataManager_Cp.gameObject);

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

    void InitRandomMapAndEnv()
    {
        deliverSceneId = Random.Range(0, deliveringScenes.Count);
        deliverSkyId = Random.Range(0, 3); // 0 normal, 1 rain, 2 snow
    }

    #endregion

    void Play()
    {
        mainGameState = GameState_En.Playing;
    }

    public void OnToggle_GameLevel(int index)
    {
        dataManager_Cp.gLevelId = index;
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void Escape()
    {
        LoadNewScene(mainSceneName);
    }

    public void OnClick_DeliverBtn()
    {
        if (dataManager_Cp.gLevelId == 0)
        {
            if (deliverSkyId == 2)
            {
                deliverSkyId = 0;
            }
        }
        dataManager_Cp.deliverSkyId = deliverSkyId;

        LoadNewScene(deliveringScenes[deliverSceneId]);
    }

    void LoadNewScene(string sceneName = null)
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
