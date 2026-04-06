using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public class RacerHandler : MonoBehaviour
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
        InstantEffectAble,
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    [SerializeField] SkinnedMeshRenderer jockeyMeshR_Cp, jockeyLodMeshR_Cp,
        horseMeshR_Cp, horseLodMeshR_Cp, saddleMeshR_Cp, saddleLodMeshR_Cp;
    [SerializeField] Animator jockeyAnim_Cp, horseAnim_Cp;
    [SerializeField] AudioSource horseAudioS_Cp, rankingUpAudioS_Cp;
    [SerializeField] public Transform camFollowPoint_Tf, camLookPoint_Tf;
    [SerializeField] public NavMeshAgent agent_Cp;
    [SerializeField] float m_maxSpeed;
    [SerializeField] GameObject effectsHolder_GO;
    [SerializeField] GameObject myRacerEff_Pf, rankingUpEff_Pf;
    [SerializeField] float speedChangeDur = 3f;
    [SerializeField] float maxHorseVolume = 0.4f, rankingUpVolume = 0.4f;

    //-------------------------------------------------- public fields
    [ReadOnly] public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public RacerBaseData_St racerBData;
    [ReadOnly] public int racerId;

    //-------------------------------------------------- private fields
    Controller_Betting controller_Cp;
    Data_Betting data_Cp;
    UIManager_Betting ui_Cp;
    BettingHandler betHandler_Cp;

    [SerializeField][ReadOnly] int m_lapId, m_passPointId, m_ranking;
    bool isRunning;

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
    public int lapId
    {
        get { return m_lapId; }
        set { m_lapId = value; }
    }
    public int targetPassPointId
    {
        get { return m_passPointId; }
        set { m_passPointId = value; }
    }
    public int ranking
    {
        get { return m_ranking; }
        set { m_ranking = value; }
    }
    public float maxSpeed
    {
        get { return m_maxSpeed; }
        set
        {
            m_maxSpeed = value;
            SetTargetSpeed(maxSpeed);
        }
    }

    //-------------------------------------------------- private properties
    bool isPlayerRacer { get { return racerBData.id == data_Cp.playerRacerId; } }

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
        if (isRunning)
        {
            jockeyAnim_Cp.SetFloat("Speed", agent_Cp.speed / maxSpeed);
            horseAnim_Cp.SetFloat("Speed", agent_Cp.speed / maxSpeed);
            horseAudioS_Cp.volume = agent_Cp.speed / maxSpeed / maxHorseVolume;
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

        // note: racerBData should be setted first
        InitComponents();
        InitRacerMaterial();
        InitUI_RankingPanel();
        SetRacerAnim_Idle();
        InitEffects();
        InitVariables();
        InitAudio();

        mainGameState = GameState_En.Inited;
    }

    void InitComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Betting>();
        data_Cp = controller_Cp.data_Cp;
        ui_Cp = controller_Cp.ui_Cp;
        betHandler_Cp = controller_Cp.betHandler_Cp;
    }

    void InitRacerMaterial()
    {
        jockeyMeshR_Cp.material.mainTexture = racerBData.jockeyTexture;
        jockeyLodMeshR_Cp.material.mainTexture = racerBData.jockeyTexture;
        horseMeshR_Cp.material.mainTexture = racerBData.horseTexture;
        horseLodMeshR_Cp.material.mainTexture = racerBData.horseTexture;
        saddleMeshR_Cp.material.mainTexture = racerBData.saddleTexture;
        saddleLodMeshR_Cp.material.mainTexture = racerBData.saddleTexture;
    }

    void InitUI_RankingPanel()
    {
        ui_Cp.InitRacerRankingPanel(racerBData.id, racerBData.color, racerBData.racerSprite, isPlayerRacer);
    }

    void InitEffects()
    {
        if (isPlayerRacer)
        {
            Instantiate(myRacerEff_Pf, effectsHolder_GO.transform);
        }
    }

    void InitVariables()
    {
        racerId = racerBData.id;
        agent_Cp.speed = 0;
    }

    void InitAudio()
    {
        horseAudioS_Cp.volume = maxHorseVolume;
        rankingUpAudioS_Cp.volume = rankingUpVolume;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Handle Animator
    //////////////////////////////////////////////////////////////////////
    #region Handle Animator

    void SetRacerAnim_Idle()
    {
        StartCoroutine(Corou_SetRacerAnim_Idle());        
    }

    IEnumerator Corou_SetRacerAnim_Idle()
    {
        float randDelay = Random.Range(0f, 3f);
        yield return new WaitForSeconds(randDelay);

        int randIdleFlag = Random.Range(0, 2);
        jockeyAnim_Cp.SetInteger("IdleFlag", randIdleFlag);
        horseAnim_Cp.SetInteger("IdleFlag", randIdleFlag);
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    public void Play()
    {
        jockeyAnim_Cp.SetBool("Play", true);
        horseAnim_Cp.SetBool("Play", true);
        Invoke("Invoke_PlayHorseAudio", Random.value);

        SetNewDestination();
        SetTargetSpeed(maxSpeed);

        DOTween.To(() => agent_Cp.radius, x => agent_Cp.radius = x, 1f, 3f);
        isRunning = true;

        mainGameState = GameState_En.Playing;
    }

    void Invoke_PlayHorseAudio()
    {
        horseAudioS_Cp.Play();
    }

    void SetNewDestination()
    {
        agent_Cp.destination = betHandler_Cp.passPoint_Tfs[targetPassPointId].position;
    }

    Tween setSpeedTween;
    void SetTargetSpeed(float targetSpeed)
    {
        if (setSpeedTween != null && setSpeedTween.IsPlaying())
        {
            setSpeedTween.Kill();
        }
        setSpeedTween = DOTween.To(() => agent_Cp.speed, x => agent_Cp.speed = x, targetSpeed, speedChangeDur);
    }

    public void InstantRankingUpEffect()
    {
        if (ExistGameStates(GameState_En.InstantEffectAble))
        {
            GameObject rankingUpEff_GO_tp = Instantiate(rankingUpEff_Pf, effectsHolder_GO.transform);
            Destroy(rankingUpEff_GO_tp, 2f);

            rankingUpAudioS_Cp.Play();
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// OnCollide
    //////////////////////////////////////////////////////////////////////
    #region OnCollide

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PassPoint"))
        {
            GameObject other_GO = other.gameObject;
            int passPointColliderId_tp = betHandler_Cp.passPointCollider_GOs.IndexOf(other_GO);

            if (passPointColliderId_tp == 0)
            {
                lapId += 1;
                if (lapId == data_Cp.lapsCount + 1)
                {
                    Finish();
                }
            }
            if (passPointColliderId_tp == betHandler_Cp.passPointCollider_GOs.Count - 1)
            {
                targetPassPointId = 0;
            }
            else
            {
                targetPassPointId = passPointColliderId_tp + 1;
            }
            if (lapId == 1 && passPointColliderId_tp == 0)
            {
                AddGameStates(GameState_En.InstantEffectAble);
            }
            
            SetNewDestination();
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void Finish()
    {
        StartCoroutine(Corou_Finish());
    }

    IEnumerator Corou_Finish()
    {
        mainGameState = GameState_En.Finished;

        betHandler_Cp.RacerEnteredFinal(this);
        RemoveGameStates(GameState_En.InstantEffectAble);

        SetTargetSpeed(0f);
        yield return new WaitUntil(() => Mathf.Approximately(agent_Cp.speed, 0f));
        jockeyAnim_Cp.SetBool("Play", false);
        horseAnim_Cp.SetBool("Play", false);
        horseAudioS_Cp.Stop();
        isRunning = false;
    }

    #endregion

}
