using DG.Tweening;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(PathFollower_Redefine))]
public class WalkerAI : MonoBehaviour
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
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Fields
    /// </summary>
    //////////////////////////////////////////////////////////////////////
    #region Fields

    //-------------------------------------------------- serialize fields
    float desiredSpeeds = 1f, speedChangeDur = 0.5f,
        navOffset = 0.5f, colliderRadius = 0.3f, colliderOffset = 0.5f,
        scale = 1.6f, dieDur = 2f;
    Vector3 postureOffset = new Vector3(0f, 0f, 0f);

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    [ReadOnly] public GameObject dieEff_Pf;

    //-------------------------------------------------- private fields
    Controller_Delivering controller_Cp;
    CapsuleCollider collider_Cp;
    NavMeshObstacle nav_Cp;
    PathFollower_Redefine pathFollower_Cp;
    Animator anim_Cp;

    [SerializeField][ReadOnly] bool m_isAlive = true;

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
    public bool isAlive
    {
        get { return m_isAlive; }
        set { m_isAlive = value; }
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
        if (isPlaying)
        {
            
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

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Delivering>();
        collider_Cp = GetComponent<CapsuleCollider>();
        pathFollower_Cp = GetComponent<PathFollower_Redefine>();
        nav_Cp = GetComponent<NavMeshObstacle>();
        anim_Cp = GetComponentInChildren<Animator>();
    }

    void InitVariables()
    {
        transform.localScale = Vector3.one * scale;
        collider_Cp.radius = colliderRadius;
        collider_Cp.center = collider_Cp.center + new Vector3(0f, colliderOffset, 0f);
        nav_Cp.center = nav_Cp.center + new Vector3(0f, navOffset, 0f);
        pathFollower_Cp.pathCreator = GetComponentInParent<PathCreator>();
        pathFollower_Cp.postureOffset = postureOffset;
        pathFollower_Cp.speed = desiredSpeeds * scale;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    public void Play()
    {
        pathFollower_Cp.Play();

        mainGameState = GameState_En.Playing;
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    public void Die(TweenCallback cb)
    {
        StartCoroutine(Corou_Die(cb));
    }

    IEnumerator Corou_Die(TweenCallback cb)
    {
        pathFollower_Cp.Stop();
        anim_Cp.SetBool("die", true);
        GameObject dieEff_GO_tp = Instantiate(dieEff_Pf, transform.position, Quaternion.identity);
        Destroy(dieEff_GO_tp, dieDur);
        yield return new WaitForSeconds(dieDur);
        cb.Invoke();
    }

    public void Finish()
    {
        mainGameState = GameState_En.Finished;
    }

    #endregion
}
