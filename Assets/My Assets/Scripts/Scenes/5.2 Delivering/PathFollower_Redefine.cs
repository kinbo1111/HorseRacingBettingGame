using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower_Redefine : MonoBehaviour
{

    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 5f;
    public bool travelFromStart;
    public Vector3 postureOffset;

    [ReadOnly][SerializeField] float distanceTravelled;
    bool isPlaying;

    void Start()
    {
        
    }

    void Update()
    {
        if (isPlaying)
        {
            if (pathCreator != null)
            {
                distanceTravelled += speed * Time.deltaTime;
                SetPosAndRot();
            }
        }
    }

    public void Play()
    {
        if (!travelFromStart)
        {
            InitPosAndRot();
        }

        isPlaying = true;
        if (pathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            pathCreator.pathUpdated += OnPathChanged;
        }
    }

    public void Stop()
    {
        isPlaying = false;
    }

    void InitPosAndRot()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        SetPosAndRot();
    }

    void SetPosAndRot()
    {
        transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
        transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.y, 0f);
        AdjustPosture();
    }

    void AdjustPosture()
    {
        transform.Rotate(postureOffset);
    }

    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
}
