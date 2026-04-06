using PathCreation;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrafficHandler : MonoBehaviour
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
    [SerializeField] GameObject vehiclesHolder_GO;
    [SerializeField] GameObject walkersHolder_GO;
    [SerializeField] GameObject walkerDieEff_Pf;
    [SerializeField] AudioClip crashToWalkerAudioClip;
    [SerializeField] float crashToWalkerAudioVolume = 0.5f;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

    //-------------------------------------------------- private fields
    Controller_Delivering controller_Cp;
    GameLevelHandler gLevelHandler_Cp;

    List<PathCreator> vehiclesPaths = new List<PathCreator>();
    List<List<VehicleAI>> vehicles_Cps = new List<List<VehicleAI>>();
    List<PathCreator> walkersPaths = new List<PathCreator>();
    List<List<WalkerAI>> walkers_Cps = new List<List<WalkerAI>>();

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
    int gLevelId { get { return gLevelHandler_Cp.gLevelId; } }

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
        StartCoroutine(Corou_Init());
    }

    IEnumerator Corou_Init()
    {
        AddGameStates(GameState_En.Nothing);

        SetComponents();
        InitVehicles();
        InitWalkers();

        mainGameState = GameState_En.Inited;

        yield return null;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Delivering>();
        gLevelHandler_Cp = controller_Cp.gLevelHandler_Cp;
    }

    void InitVehicles()
    {
        vehiclesPaths = new List<PathCreator>(vehiclesHolder_GO.GetComponentsInChildren<PathCreator>());
        for (int i = 0; i < vehiclesPaths.Count; i++)
        {
            vehicles_Cps.Add(new List<VehicleAI>(vehiclesPaths[i].GetComponentsInChildren<VehicleAI>()));
        }

        for (int i = 0; i < vehicles_Cps.Count; i++)
        {
            for (int j = 0; j < vehicles_Cps[i].Count; j++)
            {
                vehicles_Cps[i][j].Init();
            }
        }
    }

    void InitWalkers()
    {
        walkersPaths = new List<PathCreator>(walkersHolder_GO.GetComponentsInChildren<PathCreator>());
        for (int i = 0; i < walkersPaths.Count; i++)
        {
            walkers_Cps.Add(new List<WalkerAI>(walkersPaths[i].GetComponentsInChildren<WalkerAI>()));
        }

        for (int i = 0; i < walkers_Cps.Count; i++)
        {
            for (int j = 0; j < walkers_Cps[i].Count; j++)
            {
                walkers_Cps[i][j].Init();
                walkers_Cps[i][j].dieEff_Pf = walkerDieEff_Pf;
            }
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    public void Play()
    {
        for (int i = 0; i < vehicles_Cps.Count; i++)
        {
            for (int j = 0; j < vehicles_Cps[i].Count; j++)
            {
                vehicles_Cps[i][j].Play();
            }
        }
        for (int i = 0; i < walkers_Cps.Count; i++)
        {
            for (int j = 0; j < walkers_Cps[i].Count; j++)
            {
                walkers_Cps[i][j].Play();
            }
        }

        mainGameState = GameState_En.Playing;
    }

    public bool IsEqualPaths(PathCreator path1, PathCreator path2)
    {
        return path1 == path2;
    }

    public void OnCrashPlayerWithWalker(WalkerAI walker_Cp_tp)
    {
        AudioSource walkerAudioS_Cp_tp = walker_Cp_tp.AddComponent<AudioSource>();
        walkerAudioS_Cp_tp.clip = crashToWalkerAudioClip;
        walkerAudioS_Cp_tp.loop = false;
        walkerAudioS_Cp_tp.volume = crashToWalkerAudioVolume;
        walkerAudioS_Cp_tp.Play();

        walker_Cp_tp.isAlive = false;
        walker_Cp_tp.Die(() =>
        {
            Destroy(walker_Cp_tp.gameObject);
        });
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion
}
