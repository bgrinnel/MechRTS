using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshScript : MonoBehaviour
{
    [SerializeField] private float stopDistance;
    public void updateDestination(NavMeshAgent agent, Vector3 newDest){
        agent.destination = newDest;
    }
    public bool reachedDestination(NavMeshAgent agent){
        if(agent.remainingDistance <= stopDistance){
            return true;
        }
        return false;
    }
}