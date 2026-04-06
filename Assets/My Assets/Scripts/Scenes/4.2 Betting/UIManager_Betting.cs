using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Betting : MonoBehaviour
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
    [SerializeField] GameObject racerRankingPanel_Pf;
    [SerializeField] Transform rankingPanel_Tf;
    [SerializeField] Text bettingMoneyText_Cp;

    //-------------------------------------------------- public fields
    [ReadOnly] public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public List<RectTransform> racerRankingPanel_RTs = new List<RectTransform>();
    [ReadOnly] public List<GameObject> myRacerImage_GOs = new List<GameObject>();
    [ReadOnly] public List<Image> racerImageHolder_Cps = new List<Image>();
    [ReadOnly] public List<Image> racerImage_Cps = new List<Image>();
    [ReadOnly] public List<Text> racerOddsText_Cps = new List<Text>();
    [ReadOnly] public List<Text> racerRankingText_Cps = new List<Text>();

    //-------------------------------------------------- private fields
    Controller_Betting controller_Cp;
    DataManager dataManager_Cp;
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
    public int bettingMoney
    {
        set { bettingMoneyText_Cp.text = value.ToString() + "円"; }
    }

    //-------------------------------------------------- private properties
    List<RacerRealTimeData_St> racerRTDatas
    {
        get { return betHandler_Cp.racerRTDatas; }
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
        InstantRankingPanel();
        bettingMoney = dataManager_Cp.bettingMoney;

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Betting>();
        dataManager_Cp = controller_Cp.dataManager_Cp;
        data_Cp = controller_Cp.data_Cp;
        betHandler_Cp = controller_Cp.betHandler_Cp;
    }

    void InstantRankingPanel()
    {
        for (int i = 0; i < data_Cp.totalRacersCount; i++)
        {
            RectTransform rankingPanel_RT = Instantiate(racerRankingPanel_Pf, rankingPanel_Tf)
                .GetComponent<RectTransform>();
            racerRankingPanel_RTs.Add(rankingPanel_RT);
            myRacerImage_GOs.Add(rankingPanel_RT.Find("MyRacer Image").gameObject);
            racerImageHolder_Cps.Add(rankingPanel_RT.Find("RacerImage Holder").GetComponent<Image>());
            racerImage_Cps.Add(rankingPanel_RT.Find("RacerImage Holder")
                .Find("RacerImage Mask").Find("Racer Image").GetComponent<Image>());
            racerOddsText_Cps.Add(rankingPanel_RT.Find("OddsText Holder").Find("Odds Text")
                .GetComponent<Text>());
            racerRankingText_Cps.Add(rankingPanel_RT.Find("RankingText Holder").Find("Ranking Text")
                .GetComponent<Text>());
        }
        racerRankingPanel_Pf.SetActive(false);
    }

    public void InitRacerRankingPanel(int id, Color color, Sprite sprite, bool isPlayerRacer = false)
    {
        racerImageHolder_Cps[id].color = color;
        racerImage_Cps[id].sprite = sprite;
        racerOddsText_Cps[id].text = dataManager_Cp.racersOdds[id].ToString();
        myRacerImage_GOs[id].SetActive(isPlayerRacer);
    }

    #endregion

    public void RefreshRacerRankingData()
    {
        for (int i = 0; i < racerRTDatas.Count; i++)
        {
            racerRankingText_Cps[i].text = racerRTDatas[i].rank.ToString();
        }
    }

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
