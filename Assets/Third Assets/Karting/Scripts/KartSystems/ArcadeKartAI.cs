using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArcadeKartAI : MonoBehaviour
{

    [SerializeField]
    public NavMeshAgent navAgent_Cp;

    [SerializeField]
    Transform player_Tf;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        navAgent_Cp.destination = player_Tf.position;
    }
}
