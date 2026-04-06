using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data_Betting : MonoBehaviour
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

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

    //-------------------------------------------------- private fields
    Controller_Betting controller_Cp;
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
    public GameData_St gameData { get { return dataManager_Cp.gameData; } }
    public int totalMoney { get { return gameData.money; } }
    public List<RacerBaseData_St> racerBDatas { get { return dataManager_Cp.racerBDatas; } }
    public int totalRacersCount { get { return dataManager_Cp.totalRacerCount; } }
    public int lapsCount { get { return dataManager_Cp.lapsCount; } }
    public int bettingMoney { get { return dataManager_Cp.bettingMoney; } }
    public int playerRacerId { get { return dataManager_Cp.playerRacerId; } }

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

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Betting>();
        dataManager_Cp = controller_Cp.dataManager_Cp;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion

}
