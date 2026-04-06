using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_BetEvaluation : MonoBehaviour
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
    [SerializeField] AudioSource evaluationAudioS_Cp;
    [SerializeField] AudioClip succAudioClip, failedAudioClip;
    [SerializeField] UIManager_BetEvaluation ui_Cp;
    [SerializeField] string mainSceneName;
    [SerializeField] float evaluationAudioVolume = 0.4f;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public DataManager dataManager_Cp;

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

    void InitUI()
    {
        ui_Cp.Init();
    }

    void InitAudio()
    {
        if (evaluationAudioS_Cp == null)
        {
            return;
        }

        evaluationAudioS_Cp.volume = evaluationAudioVolume;
        evaluationAudioS_Cp.loop = false;

        if (dataManager_Cp.betResult)
        {
            if (succAudioClip != null)
            {
                evaluationAudioS_Cp.clip = succAudioClip;
            }
        }
        else
        {
            if (failedAudioClip != null)
            {
                evaluationAudioS_Cp.clip = failedAudioClip;
            }
        }
    }

    #endregion

    void Play()
    {
        mainGameState = GameState_En.Playing;

        audio_Cp.Play();
        PlayEvaluationAudio();
    }

    void PlayEvaluationAudio()
    {
        Invoke("Invoke_PlayEvaluationAudio", 1f);
    }

    void Invoke_PlayEvaluationAudio()
    {
        evaluationAudioS_Cp.Play();
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void Escape()
    {
        if (evaluationAudioS_Cp != null)
        {
            evaluationAudioS_Cp.Stop();
        }
        audio_Cp.Finish();

        LoadNewScene(mainSceneName);
    }

    public void LoadNewScene(string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
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
