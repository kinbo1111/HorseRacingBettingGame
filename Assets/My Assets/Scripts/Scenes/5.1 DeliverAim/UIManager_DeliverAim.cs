using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_DeliverAim : MonoBehaviour
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
    [SerializeField] List<Toggle> gLevelToggle_Cps = new List<Toggle>();

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

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
    public int gLevelId
    {
        get
        {
            int result = 0;
            for (int i = 0; i < gLevelToggle_Cps.Count; i++)
            {
                if (gLevelToggle_Cps[i].isOn)
                {
                    result = i; break;
                }
            }
            return result;
        }
    }

    //-------------------------------------------------- private properties
    Controller_DeliverAim controller_Cp;

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
        controller_Cp = FindObjectOfType<Controller_DeliverAim>();
    }

    #endregion

    public void OnToggle_GameLevelBtn0(bool value)
    {
        if (value)
        {
            controller_Cp.OnToggle_GameLevel(0);
        }
    }

    public void OnToggle_GameLevelBtn1(bool value)
    {
        if (value)
        {
            controller_Cp.OnToggle_GameLevel(1);
        }
    }

    public void OnToggle_GameLevelBtn2(bool value)
    {
        if (value)
        {
            controller_Cp.OnToggle_GameLevel(2);
        }
    }

    public void OnClick_StartBtn()
    {
        controller_Cp.OnClick_DeliverBtn();
    }

    public void OnClick_EscapeBtn()
    {
        controller_Cp.Escape();
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion
}
