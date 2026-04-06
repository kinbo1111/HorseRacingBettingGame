using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Lobby : MonoBehaviour
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
    [SerializeField] Image hasRacingNewspaperImage_Cp;
    [SerializeField] Sprite hasRacingSprite, notHasRacngSprite;
    [SerializeField] Text moneyText_Cp;
    [SerializeField] Text ageText_Cp, monthText_Cp;
    [SerializeField] Text totalBettingCountText_Cp, successBettingCountText_Cp;
    [SerializeField] Button bettingButton_Cp, deliveringButton_Cp;
    [SerializeField] Text minBettingMoneyText_Cp;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

    //-------------------------------------------------- private fields
    Controller_Lobby controller_Cp;
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
    public bool hasRacingNewspaper
    {
        set { hasRacingNewspaperImage_Cp.sprite = value ? hasRacingSprite : notHasRacngSprite; }
    }
    public int money
    {
        set { moneyText_Cp.text = value.ToString() + "円"; }
    }
    public int age
    {
        set { ageText_Cp.text = value.ToString(); }
    }
    public int month
    {
        set { monthText_Cp.text = value.ToString(); }
    }
    public int totalBettingCount
    {
        set { totalBettingCountText_Cp.text = value.ToString(); }
    }
    public int successBettingCount
    {
        set { successBettingCountText_Cp.text = value.ToString(); }
    }
    public int minBettingMoney
    {
        set { minBettingMoneyText_Cp.text = value.ToString() + "円"; }
    }

    //-------------------------------------------------- private properties
    GameData_St gameData { get { return dataManager_Cp.gameData; } }

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
        InitVariables();

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Lobby>();
        dataManager_Cp = controller_Cp.dataManager_Cp;
    }

    public void InitGameDataUI()
    {
        hasRacingNewspaper = gameData.hasRacingNewspaper;
        money = gameData.money;
        age = gameData.age;
        month = gameData.month;
        totalBettingCount = gameData.totalBettingCount;
        successBettingCount = gameData.successBettingCount;
    }

    void InitVariables()
    {
        minBettingMoney = dataManager_Cp.minBettingMoney;
    }

    #endregion

    public void SetActiveBettingBtn(bool active)
    {
        bettingButton_Cp.interactable = active;
    }

    public void SetActiveDeliveringBtn(bool active)
    {
        deliveringButton_Cp.interactable = active;
    }

    //////////////////////////////////////////////////////////////////////
    /// Callback from UI
    //////////////////////////////////////////////////////////////////////
    #region Callback from UI

    public void OnClick_BettingBtn()
    {
        controller_Cp.OnClick_BettingBtn();
    }

    public void OnClick_DeliveringBtn()
    {
        controller_Cp.OnClick_DeliveringBtn();
    }

    public void OnClick_EscapeBtn()
    {
        controller_Cp.Escape();
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion

}
