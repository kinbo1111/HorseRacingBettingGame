using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikersHandler : MonoBehaviour
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
        PlayerTracking,
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    [SerializeField] int trackStartTime;
    [SerializeField] GameObject biker_Pf;
    [SerializeField] Transform bikerSpawnPointsGroup_Tf;
    [SerializeField] List<float> bikerSpeeds = new List<float>();
    [SerializeField] List<float> bikerAngSpeeds = new List<float>();
    [SerializeField] List<int> bikersCount = new List<int>();
    [SerializeField] List<float> bikerSpawnIntervals = new List<float>();
    [SerializeField] List<int> damages = new List<int>();

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public int gLevelId;

    //-------------------------------------------------- private fields
    Controller_Delivering controller_Cp;
    Player player_Cp;
    GameLevelHandler gLevelHandler_Cp;
    TimerHandler_Delivering timer_Cp;
    List<Transform> bikerSpawnPoint_Tfs = new List<Transform>();
    List<BikerAI> biker_Cps = new List<BikerAI>();

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
    public int damage { get { return damages[gLevelId]; } }

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
        GetGameLevel();
        InitVariables();

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Delivering>();
        player_Cp = controller_Cp.player_Cp;
        timer_Cp = controller_Cp.timer_Cp;
        gLevelHandler_Cp = controller_Cp.gLevelHandler_Cp;
    }

    void GetGameLevel()
    {
        gLevelId = gLevelHandler_Cp.gLevelId;
    }

    void InitVariables()
    {
        for (int i = 0; i < bikerSpawnPointsGroup_Tf.childCount; i++)
        {
            bikerSpawnPoint_Tfs.Add(bikerSpawnPointsGroup_Tf.GetChild(i));
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    public void Play()
    {
        StartCoroutine(Corou_Play());
    }

    IEnumerator Corou_Play()
    {
        TrackPlayer();

        yield return null;
    }

    public void OnCollideBiker(BikerAI biker_Cp_tp)
    {
        biker_Cps.Remove(biker_Cp_tp);

        biker_Cp_tp.StopTrackPlayer();
        biker_Cp_tp.Die();
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Track Play
    //////////////////////////////////////////////////////////////////////
    #region Track Play

    public void TrackPlayer()
    {
        corouTrackPlayer = StartCoroutine(Corou_TrackPlayer());
    }

    Coroutine corouTrackPlayer;
    IEnumerator Corou_TrackPlayer()
    {
        AddGameStates(GameState_En.PlayerTracking);

        yield return new WaitUntil(() => timer_Cp.restTime > trackStartTime);

        while (true)
        {
            if (biker_Cps.Count < bikersCount[gLevelId])
            {
                SpawnRandomBiker();
                yield return new WaitForSeconds(bikerSpawnIntervals[gLevelId]);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }

    }

    void SpawnRandomBiker()
    {
        int randId = Random.Range(0, bikerSpawnPoint_Tfs.Count);
        GameObject biker_GO_tp = Instantiate(biker_Pf, bikerSpawnPoint_Tfs[randId].position,
            bikerSpawnPoint_Tfs[randId].rotation);

        BikerAI biker_Cp_tp = biker_GO_tp.GetComponent<BikerAI>();
        biker_Cp_tp.Init();
        biker_Cp_tp.SetSpeed(bikerSpeeds[gLevelId], bikerAngSpeeds[gLevelId]);
        biker_Cps.Add(biker_Cp_tp);
        biker_Cp_tp.StartTrackPlayer();
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void FinishPlaying()
    {
        mainGameState = GameState_En.Finished;

        StopCoroutine(corouTrackPlayer);
        for (int i = 0; i < biker_Cps.Count; i++)
        {
            if (biker_Cps[i].isPlayerTracking)
            {
                biker_Cps[i].StopTrackPlayer();
            }
        }

        RemoveGameStates(GameState_En.PlayerTracking);
    }

    #endregion
}
