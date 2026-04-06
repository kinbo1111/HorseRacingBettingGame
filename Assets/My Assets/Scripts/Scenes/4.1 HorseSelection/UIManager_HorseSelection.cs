using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager_HorseSelection : MonoBehaviour
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

    public struct RacerUIPanel_St
    {
        public int id;
        public Text nameText_Cp;
        public Image racerImage_Cp;
        public Button racerImageBtn_Cp;
        public Image racerColor_Cp;
        public Toggle selectToggle_Cp;
        public Text oddsText_Cp;
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
    [SerializeField] GameObject newspaperGlitter_GO;
    [SerializeField] Button newspaperBtn_Cp;
    [SerializeField] Sprite hasRacingSprite, notHasRacngSprite;
    [SerializeField] Text moneyText_Cp;
    [SerializeField] Text ageText_Cp, monthText_Cp;
    [SerializeField] Text totalBettingCountText_Cp, successBettingCountText_Cp;

    [SerializeField] RectTransform racersContent_RT;
    [SerializeField] InputField bettingMoneyInput_Cp;
    [SerializeField] Text minBettingMoneyText_Cp;
    [SerializeField] GameObject racerPanel_Pf;
    [SerializeField] ToggleGroup racerToggleGroup_Cp;
    [SerializeField] Animator modalAnim_Cp;
    [SerializeField] Text newspaperRacerIdText_Cp;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();

    //-------------------------------------------------- private fields
    Controller_HorseSelection controller_Cp;
    DataManager dataManager_Cp;

    List<RacerUIPanel_St> racerPanels = new List<RacerUIPanel_St>();

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
        set
        {
            newspaperBtn_Cp.enabled = value;
            newspaperGlitter_GO.SetActive(value);
            hasRacingNewspaperImage_Cp.sprite = value ? hasRacingSprite : notHasRacngSprite;
        }
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
        set { minBettingMoneyText_Cp.text = "最小" + value.ToString() + "円"; }
    }

    //-------------------------------------------------- private properties
    GameData_St gameData { get { return dataManager_Cp.gameData; } }
    List<RacerBaseData_St> racerBDatas { get { return dataManager_Cp.racerBDatas; } }

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
        modalAnim_Cp.gameObject.SetActive(true);

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_HorseSelection>();
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

    public void InitRacersPanel()
    {
        for (int i = racersContent_RT.childCount - 1; i >= 0; i--)
        {
            Destroy(racersContent_RT.GetChild(i).gameObject);
        }
        for (int i = 0; i < racerBDatas.Count; i++)
        {
            int index = i;
            Transform racer_Tf_tp = Instantiate(racerPanel_Pf, racersContent_RT).transform;
            RacerUIPanel_St racerUIPanel_tp = new RacerUIPanel_St();
            racerUIPanel_tp.nameText_Cp = racer_Tf_tp.Find("Name Text").GetComponent<Text>();
            racerUIPanel_tp.racerImage_Cp = racer_Tf_tp.Find("RacerColor Image").Find("Racer Image").GetComponent<Image>();
            racerUIPanel_tp.racerImageBtn_Cp = racerUIPanel_tp.racerImage_Cp.GetComponent<Button>();
            racerUIPanel_tp.racerColor_Cp = racer_Tf_tp.Find("RacerColor Image").GetComponent<Image>();
            racerUIPanel_tp.selectToggle_Cp = racer_Tf_tp.Find("Toggle").GetComponent<Toggle>();
            racerUIPanel_tp.oddsText_Cp = racer_Tf_tp.Find("OddsTitle Text").Find("Odds Text").GetComponent<Text>();

            racerUIPanel_tp.id = i;
            racerUIPanel_tp.nameText_Cp.text = (racerBDatas[i].id + 1).ToString();
            racerUIPanel_tp.racerImage_Cp.sprite = racerBDatas[i].racerSprite;
            racerUIPanel_tp.racerImageBtn_Cp.onClick.AddListener(() => {
                racerUIPanel_tp.selectToggle_Cp.isOn = !racerUIPanel_tp.selectToggle_Cp.isOn;
            });
            //racerUIPanel_tp.racerColor_Cp.color = racerBDatas[i].color;
            racerUIPanel_tp.selectToggle_Cp.isOn = false;
            racerUIPanel_tp.selectToggle_Cp.onValueChanged.AddListener((bool isOn) =>
            {
                OnClick_RacerPanel(index, isOn);
            });
            racerUIPanel_tp.selectToggle_Cp.group = racerToggleGroup_Cp;
            racerUIPanel_tp.oddsText_Cp.text = dataManager_Cp.racersOdds[i].ToString();
            
            racerPanels.Add(racerUIPanel_tp);
        }
    }

    public void InitBettingMoneyInputPanel()
    {
        bettingMoneyInput_Cp.onEndEdit.AddListener((string amount) => { OnInput_BettingMoney(amount); });
    }

    public void InitNewspaperModal()
    {
        if (dataManager_Cp.gameData.hasRacingNewspaper)
        {
            newspaperRacerIdText_Cp.text = "◎最もお勧めの馬番号 " +
                (dataManager_Cp.raceWinnerCandidateIds[0] + 1).ToString() + '\n';
            newspaperRacerIdText_Cp.text += "〇２番目にお勧めの馬番号 " +
                (dataManager_Cp.raceWinnerCandidateIds[1] + 1).ToString() + '\n';
            newspaperRacerIdText_Cp.text += "▲３番目にお勧めの馬番号 " +
                (dataManager_Cp.raceWinnerCandidateIds[2] + 1).ToString() + '\n';
        }
    }

    void InitVariables()
    {
        minBettingMoney = dataManager_Cp.minBettingMoney;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Callback from UI
    //////////////////////////////////////////////////////////////////////
    #region Callback from UI

    public void OnClick_NewspaperBtn()
    {
        if (dataManager_Cp.gameData.hasRacingNewspaper)
        {
            modalAnim_Cp.SetInteger("flag", 1);
        }
    }

    public void OnClick_ModalWindow()
    {
        modalAnim_Cp.SetInteger("flag", -1);
    }

    public void OnClick_RacerPanel(int index, bool flag)
    {
        if (flag)
        {
            controller_Cp.selectedRacerIndex = index;
        }
    }

    public void OnInput_BettingMoney(string amountText)
    {
        if (string.IsNullOrEmpty(amountText))
        {
            return;
        }
        int amount = int.Parse(amountText);
        controller_Cp.inputBettingMoney = amount;
    }

    public void OnClick_BettingBtn()
    {
        controller_Cp.OnClick_BettingBtn();
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
