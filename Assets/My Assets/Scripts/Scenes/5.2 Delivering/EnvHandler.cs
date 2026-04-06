using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvHandler : MonoBehaviour
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
    [SerializeField] Transform startPoints_Tf, endPoints_Tf;
    [SerializeField] List<GameObject> gLevelTarget_GOs = new List<GameObject>();
    [SerializeField] public Transform goal_Tf;
    [SerializeField] public Transform coinSpawnPoints_Tf, newspaperSpawnPoints_Tf;
    [SerializeField] GameObject coin_Pf, newspaper_Pf;
    [SerializeField] List<Material> skyboxes = new List<Material>();
    [SerializeField] public List<int> walkerDamages = new List<int>();

    //-------------------------------------------------- public fields
    [ReadOnly] public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public Transform startPoint_Tf, endPoint_Tf;

    //-------------------------------------------------- private fields
    Controller_Delivering controller_Cp;
    DataManager dataManager_Cp;
    BikersHandler bikersHandler_Cp;
    MapHandler mapHandler_Cp;
    TrafficHandler trafficHandler_Cp;
    Player player_Cp;

    List<Transform> coinSpawnPoint_Tfs = new List<Transform>();
    List<Transform> newspaperSpawnPoint_Tfs = new List<Transform>();

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

    private void Awake()
    {
        
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
        StartCoroutine(Corou_Init());
    }

    IEnumerator Corou_Init()
    {
        AddGameStates(GameState_En.Nothing);

        SetComponents();
        InitSky();
        InitGameLevel();
        InitStartPoint();
        SetPlayerToStartPoint();
        InitEndPoint();
        InitCoinSpawnPoints();
        SpawnCoins();
        InitNewspaperSpawnPoints();
        SpawnNewspaper();
        InitMap();

        bikersHandler_Cp.Init();
        yield return new WaitUntil(() => bikersHandler_Cp.mainGameState == BikersHandler.GameState_En.Inited);

        trafficHandler_Cp.Init();
        yield return new WaitUntil(() => trafficHandler_Cp.mainGameState == TrafficHandler.GameState_En.Inited);

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Delivering>();
        dataManager_Cp = controller_Cp.dataManager_Cp;
        bikersHandler_Cp = controller_Cp.bikersHandler_Cp;
        mapHandler_Cp = controller_Cp.mapHandler_Cp;
        trafficHandler_Cp = controller_Cp.trafficHandler_Cp;
        player_Cp = controller_Cp.player_Cp;
    }

    void InitSky()
    {
        if (dataManager_Cp.deliverSkyId == 0) // normal
        {
            RenderSettings.skybox = skyboxes[0];
            RenderSettings.ambientSkyColor = DataManager.HexToColor("#FFFFFF");
            RenderSettings.ambientEquatorColor = DataManager.HexToColor("#C8C8C8");
            RenderSettings.ambientGroundColor = DataManager.HexToColor("#646464");
            RenderSettings.fogColor = DataManager.HexToColor("#80FFFF");
            RenderSettings.fogStartDistance = 100f;
            RenderSettings.fogEndDistance = 1000f;
        }
        else if (dataManager_Cp.deliverSkyId == 1) // rain
        {
            RenderSettings.skybox = skyboxes[1];
            RenderSettings.ambientSkyColor = DataManager.HexToColor("#BEBEBE");
            RenderSettings.ambientEquatorColor = DataManager.HexToColor("#8C8C8C");
            RenderSettings.ambientGroundColor = DataManager.HexToColor("#323232");
            RenderSettings.fogColor = DataManager.HexToColor("#AAAAAA");
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 1000f;
        }
        else if (dataManager_Cp.deliverSkyId == 2) // snow
        {
            RenderSettings.skybox = skyboxes[2];
            RenderSettings.ambientSkyColor = DataManager.HexToColor("#BEBEBE");
            RenderSettings.ambientEquatorColor = DataManager.HexToColor("#8C8C8C");
            RenderSettings.ambientGroundColor = DataManager.HexToColor("#323232");
            RenderSettings.fogColor = DataManager.HexToColor("#FFFFFF");
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 1000f;
        }
    }

    void InitGameLevel()
    {
        int gLevelId = controller_Cp.gLevelHandler_Cp.gLevelId;
        for (int i = 0; i < gLevelTarget_GOs.Count; i++)
        {
            if (i >= gLevelId)
            {
                Destroy(gLevelTarget_GOs[i]);
            }
        }
    }

    void InitMap()
    {
        mapHandler_Cp.Init();
    }

    void InitStartPoint()
    {
        List<Transform> startPoint_Tfs_tp = new List<Transform>();
        for (int i = 0; i < startPoints_Tf.childCount; i++)
        {
            startPoint_Tfs_tp.Add(startPoints_Tf.GetChild(i).transform);
        }
        startPoint_Tf = startPoint_Tfs_tp[Random.Range(0, startPoint_Tfs_tp.Count)];
    }

    void SetPlayerToStartPoint()
    {
        player_Cp.transform.position = startPoint_Tf.position;
        player_Cp.transform.rotation = startPoint_Tf.rotation;
    }

    void InitEndPoint()
    {
        List<Transform> endPoint_Tfs_tp = new List<Transform>();
        for (int i = 0; i < endPoints_Tf.childCount; i++)
        {
            endPoint_Tfs_tp.Add(endPoints_Tf.GetChild(i).transform);
        }
        endPoint_Tf = endPoint_Tfs_tp[Random.Range(0, endPoint_Tfs_tp.Count)];
        goal_Tf.SetParent(endPoint_Tf, false);
        goal_Tf.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    void InitCoinSpawnPoints()
    {
        for (int i = 0; i < coinSpawnPoints_Tf.childCount; i++)
        {
            coinSpawnPoint_Tfs.Add(coinSpawnPoints_Tf.GetChild(i));
        }
    }

    void SpawnCoins()
    {
        for (int i = 0; i < coinSpawnPoint_Tfs.Count; i++)
        {
            Instantiate(coin_Pf, coinSpawnPoint_Tfs[i]);
        }
    }

    void InitNewspaperSpawnPoints()
    {
        for (int i = 0; i < newspaperSpawnPoints_Tf.childCount; i++)
        {
            newspaperSpawnPoint_Tfs.Add(newspaperSpawnPoints_Tf.GetChild(i));
        }
    }

    void SpawnNewspaper()
    {
        int index = Random.Range(0, newspaperSpawnPoint_Tfs.Count);
        Instantiate(newspaper_Pf, newspaperSpawnPoint_Tfs[index]);
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    public void Play()
    {
        mainGameState = GameState_En.Playing;

        bikersHandler_Cp.Play();
        mapHandler_Cp.ActivateMap();
        trafficHandler_Cp.Play();
    }

    public void OnPlayerGetCoin(GameObject coin_GO)
    {
        Destroy(coin_GO);
    }

    public void OnPlayerGetNewspaper(GameObject newspaper_GO_tp)
    {
        Destroy(newspaper_GO_tp);
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void FinishPlay()
    {
        mainGameState = GameState_En.Finished;

        bikersHandler_Cp.FinishPlaying();
    }

    #endregion
}
