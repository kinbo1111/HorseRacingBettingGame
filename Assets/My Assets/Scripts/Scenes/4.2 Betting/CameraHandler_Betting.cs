using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler_Betting : MonoBehaviour
{

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Types
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Types

    public enum GameState_En
    {
        Nothing, Inited, Playing, LookArounded
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    [SerializeField] Camera mainCam_Cp, lookAroundCam_Cp;
    [SerializeField] CinemachineVirtualCamera cvCam_Cp;
    [SerializeField] float lookAroundTime = 20f;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

    //-------------------------------------------------- private fields
    Controller_Betting controller_Cp;
    Data_Betting data_Cp;
    BettingHandler betHandler_Cp;

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
    RacerHandler playerRacer_Cp { get { return betHandler_Cp.playerRacer_Cp; } }

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

        lookAroundCam_Cp.gameObject.SetActive(true);
        InitComponents();
        SetFollowTarget();

        mainGameState = GameState_En.Inited;
    }

    void InitComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Betting>();
        data_Cp = controller_Cp.data_Cp;
        betHandler_Cp = controller_Cp.betHandler_Cp;
    }

    void SetFollowTarget()
    {
        cvCam_Cp.Follow = playerRacer_Cp.camFollowPoint_Tf;
        cvCam_Cp.LookAt = playerRacer_Cp.camLookPoint_Tf;
    }

    #endregion

    public void LookAroundRaceTrack()
    {
        corouLookAroundRaceTrack = StartCoroutine(Corou_LookAroundRaceTrack());
    }

    Coroutine corouLookAroundRaceTrack;
    IEnumerator Corou_LookAroundRaceTrack()
    {
        lookAroundCam_Cp.GetComponent<Animator>().SetBool("LookAround", true);
        yield return new WaitForSeconds(lookAroundTime);
        lookAroundCam_Cp.gameObject.SetActive(false);

        betHandler_Cp.OpenStartGateDoors();
        yield return new WaitForSeconds(2f);

        AddGameStates(GameState_En.LookArounded);
    }

    public void StopLookAroundRaceTrack()
    {
        StartCoroutine(Corou_StopLookAroundRaceTrack());
    }

    IEnumerator Corou_StopLookAroundRaceTrack()
    {
        if (ExistGameStates(GameState_En.LookArounded))
        {
            yield break;
        }
        if (corouLookAroundRaceTrack != null)
        {
            StopCoroutine(corouLookAroundRaceTrack);
        }
        lookAroundCam_Cp.gameObject.SetActive(false);

        betHandler_Cp.OpenStartGateDoors();
        yield return new WaitForSeconds(2f);

        AddGameStates(GameState_En.LookArounded);
    }

    public void StopFollowingPlayer()
    {
        cvCam_Cp.Follow = null;
        cvCam_Cp.LookAt = null;
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion
}
