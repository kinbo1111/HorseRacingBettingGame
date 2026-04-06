using DG.Tweening;
using PathCreation;
using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(PathFollower_Redefine))]
public class VehicleAI : MonoBehaviour
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
    [SerializeField] float desiredSpeed = 5f, rayDist = 10f, speedChangeDur = 0.5f, wheelRotCoef = 30f;
    [SerializeField] Vector3 vehicleExtent = new Vector3(2f, 1f, 4f),
        postureOffset = new Vector3(0f, 180f, 0f);

    //-------------------------------------------------- public fields
    [ReadOnly]
    public List<GameState_En> gameStates = new List<GameState_En>();
    public PathCreator vehiclePath;

    //-------------------------------------------------- private fields
    Controller_Delivering controller_Cp;
    TrafficHandler trafficHandler_Cp;
    MeshCollider collider_Cp;
    PathFollower_Redefine pathFollower_Cp;
    List<Transform> wheel_Tfs = new List<Transform>();

    Vector3 rayStartPos;
    bool isObstacleDetected;

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
    float speed
    {
        get { return pathFollower_Cp.speed; }
        set { pathFollower_Cp.speed = value; }
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
        if (isPlaying)
        {
            UpdateRaystartPos();
            Raycasts();
            HandleWheels();
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
        InitVehicleExtent();
        InitComponents();

        mainGameState = GameState_En.Inited;
    }

    void SetComponents()
    {
        controller_Cp = FindObjectOfType<Controller_Delivering>();
        trafficHandler_Cp = controller_Cp.trafficHandler_Cp;
        collider_Cp = GetComponent<MeshCollider>();
        pathFollower_Cp = GetComponent<PathFollower_Redefine>();
        for (int i = 0; i < transform.childCount; i++)
        {
            wheel_Tfs.Add(transform.GetChild(i));
        }
    }

    void InitComponents()
    {
        collider_Cp.convex = true;
        pathFollower_Cp.pathCreator = transform.parent.GetComponent<PathCreator>();
        pathFollower_Cp.endOfPathInstruction = EndOfPathInstruction.Loop;
        pathFollower_Cp.speed = desiredSpeed;
        pathFollower_Cp.travelFromStart = false;
        pathFollower_Cp.postureOffset = postureOffset;
        vehiclePath = pathFollower_Cp.pathCreator;
    }

    void InitVehicleExtent()
    {
        // I am gonna get the length of z-axis direction of the square 3d object
        //collider_Cp.bounds.extents
        //transform.rotation
        //
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Play
    //////////////////////////////////////////////////////////////////////
    #region Play

    public void Play()
    {
        mainGameState = GameState_En.Playing;

        pathFollower_Cp.Play();
    }

    void Raycasts()
    {
        //RaycastHit hit;
        //if (Physics.Raycast(rayStartPos, -transform.forward, out hit, rayDist, LayerMask.GetMask("Obstacle")))
        //{
        //    PathCreator otherPath = hit.transform.GetComponentInParent<VehicleAI>().vehiclePath;
        //    if (trafficHandler_Cp.IsEqualPaths(vehiclePath, otherPath))
        //    {
        //        if (!isObstacleDetected)
        //        {
        //            isObstacleDetected = true;
        //            SetSpeed(0f);

        //        }
        //    }
        //}
        //else
        //{
        //    if (isObstacleDetected)
        //    {
        //        isObstacleDetected = false;
        //        SetSpeed(desiredSpeed);
        //    }
        //}
    }

    Tween speedTween;
    void SetSpeed(float targetSpeed)
    {
        if (speedTween != null && speedTween.IsPlaying())
        {
            speedTween.Kill();
        }
        speedTween = DOTween.To(() => speed, x => speed = x, targetSpeed, speedChangeDur);
    }

    void UpdateRaystartPos()
    {
        rayStartPos = transform.position + -transform.forward * vehicleExtent.z + transform.up * vehicleExtent.y;
    }

    void HandleWheels()
    {
        for (int i = 0; i < wheel_Tfs.Count; i++)
        {
            wheel_Tfs[i].Rotate(-Vector3.right * speed * Time.deltaTime * wheelRotCoef);
        }
    }

    #endregion

    //////////////////////////////////////////////////////////////////////
    /// Finish
    //////////////////////////////////////////////////////////////////////
    #region Finish

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rayStartPos, 0.5f);
        Gizmos.DrawLine(rayStartPos, rayStartPos + -transform.forward * rayDist);
    }

}
