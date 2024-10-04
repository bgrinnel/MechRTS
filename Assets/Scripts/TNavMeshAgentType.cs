
using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A struct that allows easy exposure of a properties in a ScriptableObject to fields of a NavMeshAgent
/// </summary>
[Serializable]
public struct TNavMeshAgentType
{
    /// <summary>
    /// Maximum movement speed when following a path.
    /// </summary>
    public float speed;

    /// <summary>
    /// Maxmium turning speed in (deg/s) while following a path.
    /// </summary>
    public float angularSpeed;

    /// <summary>
    ///  The maxmium accerlation of an agent as it follows a path, given in units / sec^2.
    /// </summary>
    public float acceleration;
    
    /// <summary>
    /// Stop within this distance of the target position.
    /// </summary>
    public float stoppingDistance;

    /// <summary>
    /// Should the agent break automatically to avoid overshooting the destination point?
    /// </summary>
    public bool autoBraking;

    /// <summary>
    /// The minimum distance to keep clear between the center of this agent and any other agents or obstacles nearby.
    /// </summary>
    public float radius;

    /// <summary>
    /// The height of the agent for purposes of passing under obstacles.
    /// </summary>
    public float height;
}