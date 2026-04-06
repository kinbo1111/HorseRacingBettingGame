using KartGame.KartSystems;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Player : MonoBehaviour
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
    [SerializeField] public ArcadeKart kart_Cp;
    [SerializeField] public int maxHp = 100;
    [SerializeField] GameObject getCoinEff_Pf, getNewspaperEff_Pf;
    [SerializeField] Transform directorPoint_Tf, effPoint_Tf;
    [SerializeField] GameObject crashToWalkerEff_Pf;
    [SerializeField] float directorUpDistance = 6f;
    [SerializeField] AudioSource crashAudioS_Cp, getItemAudioS_Cp;
    [SerializeField] AudioClip crashAudioClip, getItemAudioClip;
    [SerializeField] float crashAudioVolume = 0.4f, getItemAudioVolume = 0.4f;

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public int hp;
    [ReadOnly] public int coin;
    [ReadOnly] public bool gotNewspaper;

    //-------------------------------------------------- private fields
    Controller_Delivering controller_Cp;
    DataManager dataManager_Cp;
    EnvHandler env_Cp;
    BikersHandler bikersHandler_Cp;
    UIManager_Delivering ui_Cp;
    TrafficHandler trafficHandler_Cp;

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
    public bool isPlaying
    {
        get { return gameStates.Count > 0 && mainGameState == GameState_En.Playing; }
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

    private void LateUpdate()
    {
        if (isPlaying)
        {
            UpdateDirection();
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
        InitUI();
        InitKart();
        InitAudio();

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Delivering>();
        dataManager_Cp = controller_Cp.dataManager_Cp;
        env_Cp = controller_Cp.envHandler_Cp;
        ui_Cp = controller_Cp.ui_Cp;
        bikersHandler_Cp = controller_Cp.bikersHandler_Cp;
        trafficHandler_Cp = controller_Cp.trafficHandler_Cp;
    }

    void InitVariables()
    {
        SetHp(maxHp);
        SetCoin(0);
    }

    void InitUI()
    {
        
    }

    void InitKart()
    {
        
    }

    void InitAudio()
    {
        crashAudioS_Cp.clip = crashAudioClip;
        crashAudioS_Cp.loop = false;
        crashAudioS_Cp.volume = crashAudioVolume;

        getItemAudioS_Cp.clip = getItemAudioClip;
        getItemAudioS_Cp.loop = false;
        getItemAudioS_Cp.volume = getItemAudioVolume;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    public void Play()
    {
        mainGameState = GameState_En.Playing;
    }

    void UpdateDirection()
    {
        directorPoint_Tf.rotation = Quaternion.LookRotation(env_Cp.endPoint_Tf.position - transform.position,
            Vector3.up);
        directorPoint_Tf.position = transform.position + new Vector3(0f, directorUpDistance, 0f);
    }

    void OnCrashToBiker(GameObject biker_GO_tp)
    {
        BikerAI biker_Cp_tp = biker_GO_tp.GetComponent<BikerAI>();
        if (biker_Cp_tp.ExistGameStates(BikerAI.GameState_En.CrashedToPlayer))
        {
            return;
        }
        biker_Cp_tp.AddGameStates(BikerAI.GameState_En.CrashedToPlayer);
        bikersHandler_Cp.OnCollideBiker(biker_Cp_tp);

        crashAudioS_Cp.Play();

        int changedHp = bikersHandler_Cp.damage;
        SetChangeHp(changedHp);
    }

    void SetChangeHp(int hp_tp)
    {
        hp = (int)Mathf.Clamp(((float)hp - (float)hp_tp), 0f, (float)maxHp);
        SetHp(hp);

        if (hp == 0)
        {
            Die();
        }
    }

    void SetHp(int hp_tp)
    {
        hp = hp_tp;
        float hpPercent = (float)hp / (float)maxHp;
        ui_Cp.SetHp(hpPercent);
    }

    void Die()
    {
        controller_Cp.OnFailed();
    }

    void OnArriveAtEnd()
    {
        controller_Cp.OnSuccess();
    }

    void OnGetCoin(GameObject coin_GO)
    {
        SetCoin(coin + dataManager_Cp.coinPrice);
        GameObject getCoinEff_GO = Instantiate(getCoinEff_Pf, effPoint_Tf);
        Destroy(getCoinEff_GO, 2f);

        env_Cp.OnPlayerGetCoin(coin_GO);

        getItemAudioS_Cp.Play();
    }

    void SetCoin(int coin_tp)
    {
        coin = coin_tp;
        ui_Cp.SetCoin(coin);
    }

    void OnCrashToWalker(WalkerAI walker_Cp_tp, Vector3 contactPoint)
    {
        if (!walker_Cp_tp.isAlive)
        {
            return;
        }

        trafficHandler_Cp.OnCrashPlayerWithWalker(walker_Cp_tp);
        GameObject crashEff_GO_tp = Instantiate(crashToWalkerEff_Pf, contactPoint, Quaternion.identity);
        Destroy(crashEff_GO_tp, 2f);

        crashAudioS_Cp.Play();

        int changedHp = env_Cp.walkerDamages[controller_Cp.gLevelHandler_Cp.gLevelId];
        SetChangeHp(changedHp);
    }

    void OnGetNewspaper(GameObject newspaper_GO_tp)
    {
        gotNewspaper = true;
        GameObject getNewspaperEff_GO = Instantiate(getNewspaperEff_Pf, effPoint_Tf);
        Destroy(getNewspaperEff_GO, 2f);

        env_Cp.OnPlayerGetNewspaper(newspaper_GO_tp);

        getItemAudioS_Cp.Play();
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// On Collide
    //////////////////////////////////////////////////////////////////////
    #region On Collide

    private void OnCollisionEnter(Collision other)
    {
        if (!isPlaying)
        {
            return;
        }

        Transform other_Tf = other.transform;
        Transform root_Tf = other_Tf.root;

        if (root_Tf.tag == "AIPlayer")
        {
            GameObject biker_GO_tp = root_Tf.gameObject;
            OnCrashToBiker(biker_GO_tp);
        }
        if (other_Tf.gameObject.layer == LayerMask.NameToLayer("Walker"))
        {
            OnCrashToWalker(other_Tf.GetComponentInParent<WalkerAI>(), other.contacts[0].point);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPlaying)
        {
            return;
        }

        Transform other_Tf = other.transform;
        Transform root_Tf = other_Tf.root;

        if (other_Tf == env_Cp.goal_Tf)
        {
            OnArriveAtEnd();
        }
        else if (other_Tf.tag == "Coin")
        {
            OnGetCoin(other_Tf.gameObject);
        }
        else if (other_Tf.tag == "Newspaper")
        {
            OnGetNewspaper(other_Tf.gameObject);
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void FinishPlay()
    {
        mainGameState = GameState_En.Playing;

        kart_Cp.enabled = false;
    }

    #endregion

}
