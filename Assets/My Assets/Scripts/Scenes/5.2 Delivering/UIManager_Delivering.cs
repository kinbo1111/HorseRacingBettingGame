using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Delivering : MonoBehaviour
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
    [SerializeField] Text coinText_Cp, timerText_Cp;
    [SerializeField] RectTransform hp_RT;
    [SerializeField] public VariableJoystick joystick_Cp;
    [SerializeField] RectTransform map_RT, player_RT, dest_RT;
    [SerializeField] Animator successPanelAnim_Cp, failedPanelAnim_Cp;
    [SerializeField] List<GameObject> gLevel_GOs = new List<GameObject>();

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    public static UIManager_Delivering instance;

    //-------------------------------------------------- private fields
    Controller_Delivering controller_Cp;

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
        instance = this;
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
        AddGameStates(GameState_En.Nothing);

        SetComponents();
        InitVariables();

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Delivering>();
    }

    void InitVariables()
    {
        Vector2 playerSizeDelta = player_RT.sizeDelta;
        player_RT.offsetMin = Vector2.zero;
        player_RT.offsetMax = Vector2.zero;
        player_RT.sizeDelta = playerSizeDelta;

        Vector2 destSizeDelta = dest_RT.sizeDelta;
        dest_RT.offsetMin = Vector2.zero;
        dest_RT.offsetMax = Vector2.zero;
        dest_RT.sizeDelta = destSizeDelta;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    public void SetCoin(int coin)
    {
        coinText_Cp.text = coin.ToString();
    }

    public void SetHp(float hpPercent)
    {
        hp_RT.anchorMax = new Vector2(hpPercent, hp_RT.anchorMax.y);
    }

    public void SetTime(int time)
    {
        int minute = time / 60;
        int second = time % 60;
        timerText_Cp.text = minute.ToString() + ":" + second.ToString();
    }

    public void SetPlayerMap(Vector2 percentPos, float rotZ)
    {
        player_RT.anchorMin = percentPos;
        player_RT.anchorMax = percentPos;
        player_RT.rotation = Quaternion.Euler(player_RT.rotation.eulerAngles.x, player_RT.rotation.eulerAngles.y,
            rotZ);
    }

    public void SetEndPointMap(Vector2 percentPos)
    {
        dest_RT.anchorMin = percentPos;
        dest_RT.anchorMax = percentPos;
    }

    public void SetActivePopupPanels(bool active)
    {
        successPanelAnim_Cp.gameObject.SetActive(active);
        failedPanelAnim_Cp.gameObject.SetActive(active);
    }

    public void SetActiveSuccessPanel(bool active)
    {
        successPanelAnim_Cp.gameObject.SetActive(active);
        successPanelAnim_Cp.SetInteger("show", active ? 1 : -1);
    }

    public void SetActiveFailedPanel(bool active)
    {
        failedPanelAnim_Cp.gameObject.SetActive(active);
        failedPanelAnim_Cp.SetInteger("show", active ? 1 : -1);
    }

    public void SetGameLevel(int gLevelId)
    {
        for (int i = 0; i < gLevel_GOs.Count; i++)
        {
            if (i <= gLevelId)
            {
                gLevel_GOs[i].gameObject.SetActive(true);
            }
            else
            {
                gLevel_GOs[i].gameObject.SetActive(false);
            }
        }
    }

    #endregion

    public void OnClick_Escape()
    {
        controller_Cp.Escape();
    }

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion
}
