using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.Storage;
using CodeStage.AntiCheat.ObscuredTypes;
using Unity.VisualScripting;

public struct GameData_St
{
    public bool hasRacingNewspaper;
    public int money;
    public int age, month;
    public int totalBettingCount, successBettingCount;
    public bool bettingDone, deliveringDone;
}

public struct RacerBaseData_St
{
    public int id;
    public string name;
    public Color color;
    public Texture jockeyTexture, horseTexture, saddleTexture;
    public Sprite racerSprite;
}

public class DataManager : MonoBehaviour
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

    public enum InitMode_En
    {
        Nothing, Start, Saved, Test,
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    [SerializeField] public int startMoney = 1000, startAge = 20, endAge = 80, startMonth = 1;
    [SerializeField] public int minBettingMoney = 100;
    [SerializeField] public int totalRacerCount = 12, lapsCount = 1;
    [SerializeField] List<Color> racerColors = new List<Color>();
    [SerializeField] string jockeyTextureResourcePath = "Textures/Jockey",
        horseTextureResourcePath = "Textures/Horse",
        saddleTextureResourcePath = "Textures/Saddle",
        racerSpriteResourcePath = "Sprites/Racer";
    [SerializeField] public float minRacerPerformance = 0.03f, maxRacerPerformance = 3f;
    [SerializeField] public float minOdds = 1.1f, maxOdds = 100f;
    [SerializeField] public List<int> deliverSuccBonus = new List<int>(new int[3] { 5000, 10000, 20000 });
    [SerializeField] public int coinPrice = 1000;

    //-------------------------------------------------- public fields
    [ReadOnly] public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public AssetsLoader assetsLoader_Cp;
    [ReadOnly] public GameData_St gameData;
    [ReadOnly] public List<RacerBaseData_St> racerBDatas = new List<RacerBaseData_St>();
    [ReadOnly] public int playerRacerId, bettingMoney;
    [ReadOnly] public bool betResult;
    [ReadOnly] public List<int> raceWinnerCandidateIds = new List<int>();
    [ReadOnly] public List<float> racersOdds = new List<float>();
    [ReadOnly] public List<float> racersPerformances = new List<float>();
    [ReadOnly] public int deliverSkyId;
    [ReadOnly] public bool deliverResult;
    [ReadOnly] public int gotBonus, gotCoin;
    [ReadOnly] public int gLevelId;

    //-------------------------------------------------- private fields
    string hasRacingNewspaper = "HasRacingNewspaper", money = "Money", age = "Age", month = "Month",
        totalBettingCount = "TotalBettingCount", successBettingCount = "SuccessBettingCount",
        bettingDone = "BettingDone", deliveringDone = "DeliveringDone";

    Texture[] jockeyTextures, horseTextures, saddleTextures;
    Sprite[] racerSprites;

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

    public void Init(InitMode_En mode = InitMode_En.Test)
    {
        if (gameStates.Count > 0)
        {
            return;
        }
        AddGameStates(GameState_En.Nothing);

        if (mode == InitMode_En.Start)
        {
            InitGameData_StartGame();
        }
        else if (mode == InitMode_En.Saved)
        {
            InitGameData_SavedGame();
        }
        else if (mode == InitMode_En.Test)
        {
            InitGameData_Test();
        }
        GenerateRacerColors();
        InitRacersBaseData();

        mainGameState = GameState_En.Inited;
    }

    public void InitGameData_StartGame()
    {
        ResetAndLoadData();
    }

    public void InitGameData_SavedGame()
    {
        LoadGameData();
    }

    public void InitGameData_Test()
    {
        if (!IsExistGameData())
        {
            ResetAndLoadData();
        }
        else
        {
            LoadGameData();
        }
    }

    void GenerateRacerColors()
    {
        racerColors.Add(DataManager.HexToColor("5d99c2"));
        racerColors.Add(DataManager.HexToColor("1b2149"));
        racerColors.Add(DataManager.HexToColor("83121e"));
        racerColors.Add(DataManager.HexToColor("349a63"));
        racerColors.Add(DataManager.HexToColor("7b1459"));
        racerColors.Add(DataManager.HexToColor("dfd232"));
        racerColors.Add(DataManager.HexToColor("319421"));
        racerColors.Add(DataManager.HexToColor("2079cb"));
        racerColors.Add(DataManager.HexToColor("d6c610"));
        racerColors.Add(DataManager.HexToColor("4d9ca8"));
        racerColors.Add(DataManager.HexToColor("25dc51"));
        racerColors.Add(DataManager.HexToColor("9a68c1"));
    }

    public void InitRacersPerformancesAndOdds_Test()
    {
        racersOdds.Clear();
        float oddsInterval = (maxOdds - minOdds) / totalRacerCount;
        List<int> randomIds = new List<int>();
        for (int i = 0; i < totalRacerCount; i++)
        {
            int randomId = 0;
            do
            {
                randomId = Random.Range(0, totalRacerCount);
            } while (randomIds.Contains(randomId));

            randomIds.Add(randomId);
            float odds = Random.Range(minOdds + oddsInterval * randomId,
                minOdds + (oddsInterval) * (randomId + 1));
            odds = Mathf.Round(odds * 10f) / 10f;
            racersOdds.Add(odds);
        }

        racersPerformances.Clear();
        for (int i = 0; i < totalRacerCount; i++)
        {
            racersPerformances.Add(racersOdds[i] / 100f
                * maxRacerPerformance);
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Handle GameData
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Handle GameData

    public void ResetAndLoadData()
    {
        ResetGameData();
        LoadGameData();
    }

    void LoadGameData()
    {
        GameData_St gameData_tp = new GameData_St();
        gameData_tp.hasRacingNewspaper = ObscuredPrefs.Get(hasRacingNewspaper, false);
        gameData_tp.money = ObscuredPrefs.Get(money, 0);
        gameData_tp.age = ObscuredPrefs.Get(age, 0);
        gameData_tp.month = ObscuredPrefs.Get(month, 0);
        gameData_tp.totalBettingCount = ObscuredPrefs.Get(totalBettingCount, 0);
        gameData_tp.successBettingCount = ObscuredPrefs.Get(successBettingCount, 0);
        gameData_tp.bettingDone = ObscuredPrefs.Get(bettingDone, false);
        gameData_tp.deliveringDone = ObscuredPrefs.Get(deliveringDone, false);
        gameData = gameData_tp;
    }

    void ResetGameData()
    {
        ObscuredPrefs.DeleteAll();

        ObscuredPrefs.Set(hasRacingNewspaper, false);
        ObscuredPrefs.Set(money, startMoney);
        ObscuredPrefs.Set(age, startAge);
        ObscuredPrefs.Set(month, startMonth);
        ObscuredPrefs.Set(totalBettingCount, 0);
        ObscuredPrefs.Set(successBettingCount, 0);
        ObscuredPrefs.Set(bettingDone, false);
        ObscuredPrefs.Set(deliveringDone, false);

        ObscuredPrefs.Save();
    }

    public void SaveGameData()
    {
        ObscuredPrefs.Set(hasRacingNewspaper, gameData.hasRacingNewspaper);
        ObscuredPrefs.Set(money, gameData.money);
        ObscuredPrefs.Set(age, gameData.age);
        ObscuredPrefs.Set(month, gameData.month);
        ObscuredPrefs.Set(totalBettingCount, gameData.totalBettingCount);
        ObscuredPrefs.Set(successBettingCount, gameData.successBettingCount);
        ObscuredPrefs.Set(bettingDone, gameData.bettingDone);
        ObscuredPrefs.Set(deliveringDone, gameData.deliveringDone);

        ObscuredPrefs.Save();
    }

    /// <summary>Add purchased in-game currency to the saved wallet.</summary>
    public void AddMoneyFromPurchase(int amountYen)
    {
        if (amountYen <= 0)
        {
            return;
        }
        GameData_St gd = gameData;
        gd.money += amountYen;
        gameData = gd;
        SaveGameData();
    }

    public void DeleteGameData()
    {
        ObscuredPrefs.DeleteAll();
        ObscuredPrefs.Save();
    }

    public bool IsExistGameData()
    {
        bool result = true;

        GameData_St gameData_tp = new GameData_St();
        gameData_tp.hasRacingNewspaper = ObscuredPrefs.Get(hasRacingNewspaper, false);
        gameData_tp.money = ObscuredPrefs.Get(money, 0);
        gameData_tp.age = ObscuredPrefs.Get(age, 0);
        gameData_tp.month = ObscuredPrefs.Get(month, 0);
        gameData_tp.totalBettingCount = ObscuredPrefs.Get(totalBettingCount, 0);
        gameData_tp.successBettingCount = ObscuredPrefs.Get(successBettingCount, 0);
        gameData_tp.bettingDone = ObscuredPrefs.Get(bettingDone, false);
        gameData_tp.deliveringDone = ObscuredPrefs.Get(deliveringDone, false);

        if (gameData_tp.hasRacingNewspaper == false &&
            gameData_tp.money == 0 &&
            gameData_tp.age == 0 &&
            gameData_tp.month == 0 &&
            gameData_tp.totalBettingCount == 0 &&
            gameData_tp.successBettingCount == 0 &&
            gameData_tp.bettingDone == false &&
            gameData_tp.deliveringDone == false)
        {
            result = false;
        }

        return result;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Handle RacerData
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Handle RacerData

    public void InitRacersBaseData()
    {
        LoadResources();
        GenerateRacersBaseData();
    }

    void LoadResources()
    {
        jockeyTextures = Resources.LoadAll<Texture>(jockeyTextureResourcePath);
        horseTextures = Resources.LoadAll<Texture>(horseTextureResourcePath);
        saddleTextures = Resources.LoadAll<Texture>(saddleTextureResourcePath);
        racerSprites = Resources.LoadAll<Sprite>(racerSpriteResourcePath);
    }

    void GenerateRacersBaseData()
    {
        for (int i = 0; i < totalRacerCount; i++)
        {
            RacerBaseData_St racerBData_tp = new RacerBaseData_St();

            racerBData_tp.id = i;
            racerBData_tp.name = racerSprites[i].name;
            racerBData_tp.color = racerColors[i];
            racerBData_tp.jockeyTexture = jockeyTextures[i];
            racerBData_tp.horseTexture = horseTextures[i];
            racerBData_tp.saddleTexture = saddleTextures[i];
            racerBData_tp.racerSprite = racerSprites[i];

            racerBDatas.Add(racerBData_tp);
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// API
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region API

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", ""); // In case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");  // In case the string is formatted #FFFFFF

        byte r = 0;
        byte g = 0;
        byte b = 0;
        byte a = 255; // Default to fully opaque

        if (hex.Length == 6)
        {
            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 8)
        {
            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return new Color32(r, g, b, a);
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion
}
