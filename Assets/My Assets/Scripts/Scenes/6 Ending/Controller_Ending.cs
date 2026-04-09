using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Controller_Ending : MonoBehaviour
{
    #region Types

    public enum GameState_En
    {
        Nothing, Inited, Playing,
    }

    #endregion

    #region Fields

    [SerializeField] CurtainHandler curtain_Cp;
    [SerializeField] AudioHandler audio_Cp;
    [SerializeField] UIManager_Ending ui_Cp;
    [SerializeField] string mainSceneName;
    [SerializeField] List<Sprite> resultSprites = new List<Sprite>();
    [SerializeField] List<string> resultTexts = new List<string>();
    [SerializeField] List<float> resultThresholds = new List<float>();
    [SerializeField] AudioSource resultAudioS_Cp;
    [SerializeField] AudioClip succAudioClip, failedAudioClip;
    [SerializeField] List<VideoClip> resultVideos = new List<VideoClip>();
    [Header("Debug Ending (Editor Test)")]
    [SerializeField] bool useDebugMoney = false;
    [SerializeField] int debugMoney = -1;

    [ReadOnly] public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public int gResultLevel;

    DataManager dataManager_Cp;

    #endregion

    #region Properties

    public GameState_En mainGameState
    {
        get { return gameStates[0]; }
        set { gameStates[0] = value; }
    }

    #endregion

    void Start()
    {
        Init();
    }

    void Update()
    {

    }

    #region ManageGameStates

    public void AddMainGameState(GameState_En value = GameState_En.Nothing)
    {
        if (gameStates.Count == 0)
        {
            gameStates.Add(value);
        }
    }

    public void AddGameStates(params GameState_En[] values)
    {
        foreach (GameState_En value_tp in values)
        {
            gameStates.Add(value_tp);
        }
    }

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

    public void RemoveGameStates(params GameState_En[] values)
    {
        foreach (GameState_En value in values)
        {
            gameStates.RemoveAll(gameState_tp => gameState_tp == value);
        }
    }

    #endregion

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
        // Determine ending level based on final money.
        // Mapping:
        // 0: 収支マイナス -> use 1.mp4
        // 1: 1円～1000万円未満 -> use 2.mp4
        // 2: 2000万円～1億円 -> use 3.mp4
        // 3: 1億円以上 -> use 4.mp4
        GameData_St gameData_tp = dataManager_Cp.gameData;
        int finalMoney = useDebugMoney ? debugMoney : gameData_tp.money;

        if (finalMoney < 0)
        {
            gResultLevel = 0; // 収支マイナス
        }
        else if (finalMoney < 10000000) // < 10,000,000 (1円～1000万円未満)
        {
            gResultLevel = 1;
        }
        else if (finalMoney >= 20000000 && finalMoney < 100000000) // 20,000,000 ～ 100,000,000 (2000万円～1億円)
        {
            gResultLevel = 2;
        }
        else if (finalMoney >= 100000000) // >= 1億円
        {
            gResultLevel = 3;
        }
        else
        {
            // Fallback: treat values between 10,000,000 and 19,999,999 as the lower tier (1)
            gResultLevel = 1;
        }
    }

    void InitUI()
    {
        ui_Cp.Init();
        GameData_St gameData_tp = dataManager_Cp.gameData;
        ui_Cp.totalBettingCount = gameData_tp.totalBettingCount;
        ui_Cp.bettingSuccCountText = gameData_tp.successBettingCount;
        ui_Cp.resultSprite = resultSprites.Count > gResultLevel ? resultSprites[gResultLevel] : null;

        // Fixed ending text per result level.
        // This is always used so each ending shows the intended sentence block.
        List<string> fixedResultTexts = new List<string>()
        {
            "【収支マイナス】\n財政的に大きな失敗をし、老後には全財産を失ってしまいます。絶望の中で生きる希望を見失い、最終的には自らの命を絶つことを選ぶという悲劇的な結末です。プレイヤーにとっては、人生における金銭管理の重要さと失敗のリスクを強調した教訓的なエンディングです。",
            "【1円～1000万円未満】\n老後にはなんとか生き延びるだけの資産を持っていますが、その生活は非常に厳しく、古くてボロボロの家に住むことを余儀なくされます。医療や生活費に困り、日々の生活は常に苦労を伴いますが、なんとかやりくりしながら生活を続けていくという、少し報われない結末です。",
            "【2000万円～1億円】\nそれなりに成功を収めた老後生活が待っています。大きな贅沢はできませんが、生活に余裕があり、旅行や趣味を楽しむことも可能です。老後資金を計画的に準備していたプレイヤーは、安心感のある暮らしを送ることができ、家族や友人との時間をゆっくりと過ごすことができる穏やかなエンディングです。",
            "【1億円以上】\n財産を築き上げ、非常に裕福な老後を迎えることができます。豪華客船での世界一周旅行や、誰もが憧れるような贅沢な暮らしを満喫します。何不自由ない生活を送るだけでなく、周りからも尊敬され、老後の時間を最大限に楽しむことができる、最も理想的で華やかな結末です。"
        };

        string assignText = fixedResultTexts[gResultLevel];

        ui_Cp.resultText = assignText;
        ui_Cp.SetFinalMoney(gameData_tp.money);

        if (resultVideos != null && resultVideos.Count > gResultLevel && resultVideos[gResultLevel] != null)
        {
            ui_Cp.PlayVideo(resultVideos[gResultLevel]);
        }
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
