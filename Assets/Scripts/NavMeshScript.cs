using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshScript : MonoBehaviour
{
    [SerializeField] private float stopDistance;
    void updateDestination(NavMeshAgent agent, Transform newDest){
        agent.destination = newDest.position;
    }
    bool reachedDestination(NavMeshAgent agent){
        if(agent.remainingDistance <= stopDistance){
            return true;
        }
        return false;
    }
}
