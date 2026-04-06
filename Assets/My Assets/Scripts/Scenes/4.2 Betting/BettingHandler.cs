using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct RacerRealTimeData_St
{
    public int id;
    public int rank;

    public RacerRealTimeData_St(int id_tp, int rank_tp)
    {
        id = id_tp;
        rank = rank_tp;
    }
}

public class BettingHandler : MonoBehaviour
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
        IsRankingAble,
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    [SerializeField] GameObject racer_Pf;
    [SerializeField] Transform startPoints_Tf, passPoints_Tf;
    [SerializeField] public List<GameObject> passPointCollider_GOs;
    [SerializeField] public GameObject startGate_GO;
    [SerializeField] float normalSpeed = 30f, normalSpeedChange = 3f,
        playerSpeedBonus = 0.1f;
    [SerializeField] List<float> newspaperPlayerSpeedBonus = new List<float>();

    //-------------------------------------------------- public fields
    [ReadOnly] public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public List<RacerHandler> racer_Cps = new List<RacerHandler>();
    [ReadOnly] public RacerHandler playerRacer_Cp;
    [ReadOnly] public List<Transform> startPoint_Tfs = new List<Transform>();
    [ReadOnly] public List<Transform> passPoint_Tfs = new List<Transform>();
    [ReadOnly] public List<RacerRealTimeData_St> racerRTDatas = new List<RacerRealTimeData_St>();
    [ReadOnly] public bool hasRacingNewspaper;

    //-------------------------------------------------- private fields
    Controller_Betting controller_Cp;
    DataManager dataManager_Cp;
    UIManager_Betting ui_Cp;
    Data_Betting data_Cp;
    CameraHandler_Betting camHandler_Cp;

    List<Animator> startGateDoorAnim_Cps = new List<Animator>();
    List<RacerHandler> enteredFinalRacer_Cps = new List<RacerHandler>();
    List<RacerHandler> restRacer_Cps = new List<RacerHandler>();

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
    List<RacerBaseData_St> racerBDatas
    {
        get { return data_Cp.racerBDatas; }
    }

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
        if (ExistGameStates(GameState_En.IsRankingAble))
        {
            CalculateCurrentRanking();
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

        SetComponents();
        InitVariables();
        InstantRacers();
        InitRacers();
        InitRacerRTDatas();
        playerRacer_Cp = racer_Cps[data_Cp.playerRacerId];

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Betting>();
        data_Cp = controller_Cp.data_Cp;
        dataManager_Cp = controller_Cp.dataManager_Cp;
        ui_Cp = controller_Cp.ui_Cp;
        camHandler_Cp = controller_Cp.camHandler_Cp;
    }

    void InitVariables()
    {
        for (int i = 0; i < startPoints_Tf.childCount; i++)
        {
            startPoint_Tfs.Add(startPoints_Tf.GetChild(i));
        }
        for (int i = 0; i < passPoints_Tf.childCount; i++)
        {
            passPoint_Tfs.Add(passPoints_Tf.GetChild(i));
        }
        startGateDoorAnim_Cps = new List<Animator>(startGate_GO.GetComponentsInChildren<Animator>());
    }

    void InstantRacers()
    {
        for (int i = 0; i < racerBDatas.Count; i++)
        {
            GameObject racer_GO_tp = Instantiate(racer_Pf, startPoint_Tfs[i].position, startPoint_Tfs[i].rotation);
            racer_Cps.Add(racer_GO_tp.GetComponent<RacerHandler>());
        }
    }

    void InitRacers()
    {
        for (int i = 0; i < racer_Cps.Count; i++)
        {
            racer_Cps[i].racerBData = racerBDatas[i];
            racer_Cps[i].Init();
        }
    }

    void InitRacerRTDatas()
    {
        for (int i = 0; i < racerBDatas.Count; i++)
        {
            RacerRealTimeData_St racerRTData_tp = new RacerRealTimeData_St();
            racerRTData_tp.id = 0;
            racerRTData_tp.rank = 0;
            racerRTDatas.Add(racerRTData_tp);
        }
    }

    public void OpenStartGateDoors()
    {
        for (int i = 0; i < startGateDoorAnim_Cps.Count; i++)
        {
            startGateDoorAnim_Cps[i].SetBool("Open", true);
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
        mainGameState = GameState_En.Playing;

        for (int i = 0; i < racer_Cps.Count; i++)
        {
            racer_Cps[i].Play();
        }
        AddGameStates(GameState_En.IsRankingAble);

        while (mainGameState == GameState_En.Playing)
        {
            for (int i = 0; i < racer_Cps.Count; i++)
            {
                if (racer_Cps[i].mainGameState == RacerHandler.GameState_En.Playing)
                {
                    float speedChangeAmount = Random.Range(-normalSpeedChange, normalSpeedChange);
                    racer_Cps[i].maxSpeed = normalSpeed + speedChangeAmount;
                    racer_Cps[i].maxSpeed += dataManager_Cp.racersPerformances[i];
                    if (racer_Cps[i] == playerRacer_Cp)
                    {
                        racer_Cps[i].maxSpeed += playerSpeedBonus;
                    }
                    if (dataManager_Cp.raceWinnerCandidateIds.Contains(i))
                    {
                        racer_Cps[i].maxSpeed += newspaperPlayerSpeedBonus[
                            dataManager_Cp.raceWinnerCandidateIds.IndexOf(i)];
                    }
                    yield return new WaitForSeconds(Time.deltaTime);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    float maxRemainDist = 5000f;
    void CalculateCurrentRanking()
    {
        int prevPlayerRacerRanking = racerRTDatas[playerRacer_Cp.racerBData.id].rank;

        restRacer_Cps.Clear();
        for (int i = 0; i < racer_Cps.Count; i++)
        {
            if (!enteredFinalRacer_Cps.Contains(racer_Cps[i]))
            {
                restRacer_Cps.Add(racer_Cps[i]);
            }
        }

        List<float> runDists = new List<float>();
        for (int i = 0; i < restRacer_Cps.Count; i++)
        {
            float runDist = ((restRacer_Cps[i].lapId - 1) * passPoint_Tfs.Count
                + (restRacer_Cps[i].targetPassPointId + 2) % 3) * maxRemainDist
                + (maxRemainDist - Vector3.Distance(restRacer_Cps[i].transform.position,
                restRacer_Cps[i].agent_Cp.destination));
            runDists.Add(runDist);
        }
        for (int i = 0; i < runDists.Count; i++)
        {
            int id_tp = restRacer_Cps[i].racerBData.id;
            racerRTDatas[id_tp] = new RacerRealTimeData_St(id_tp, enteredFinalRacer_Cps.Count + 1);

            for (int j = 0; j < runDists.Count; j++)
            {
                if (i != j)
                {
                    if (runDists[i] < runDists[j])
                    {
                        racerRTDatas[id_tp] = new RacerRealTimeData_St(id_tp, racerRTDatas[id_tp].rank + 1);
                    }
                }
            }
        }

        ui_Cp.RefreshRacerRankingData();

        int curPlayerRacerRanking = racerRTDatas[playerRacer_Cp.racerBData.id].rank;
        if(curPlayerRacerRanking < prevPlayerRacerRanking)
        {
            playerRacer_Cp.InstantRankingUpEffect();
        }
    }

    public bool IsBettingSuccess()
    {
        bool result = false;

        if (racerRTDatas[playerRacer_Cp.racerBData.id].rank == 1)
        {
            result = true;
        }

        return result;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void RacerEnteredFinal(RacerHandler racer_Cp_tp)
    {
        if (racer_Cp_tp == playerRacer_Cp)
        {
            camHandler_Cp.StopFollowingPlayer();
        }
        enteredFinalRacer_Cps.Add(racer_Cp_tp);
        if (enteredFinalRacer_Cps.Count == racer_Cps.Count)
        {
            Finish();
        }
    }

    public void Finish()
    {
        RemoveGameStates(GameState_En.IsRankingAble);
        mainGameState = GameState_En.Finished;
        controller_Cp.OnRacingFinished();
    }

    #endregion

}
