using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "MechType_new_mech_type_name", menuName = "MechType")]
public class MechType : ScriptableObject
{
    /// <summary>
    /// A generic delegate to be used as a function type declaration for c
    /// </summary>
    /// <param name="mechs"></param>
    /// <returns></returns>
    public delegate BaseMech SelectionAlgorithm(BaseMech selector, params BaseMech[] mechs);

    /// <summary>
    /// The max health of this mech
    /// </summary>
    public float maxHealth;

    /// <summary>
    /// A struct for wrapping
    /// </summary>
    public TNavMeshAgentType agentType = new TNavMeshAgentType{
        speed = 15f,
        angularSpeed = 120f,
        acceleration = .3f,
        stoppingDistance = 3f,
        autoBraking = true,
        radius = 2f,
        height = 5f
    };

    /// <summary>
    /// The RigidBody.mass of this mech
    /// </summary>
    public float mass;

    /// <summary>
    /// The RigidBody.drag of this mech
    /// </summary>
    public float drag;

    /// <summary>
    /// The RigidBody.angularDrag of this mech
    /// </summary>
    public float angularDrag;

    /// <summary>
    /// How far enemies can be seen by this mech (draws immediate aggro)
    /// </summary>
    public float sightRange;

    /// <summary>
    /// How far enemies can be heard by this mech (makes the mech divert from patrol, no immediate aggro)
    /// </summary>
    public float hearingRange;

    /// <summary>
    /// How the unit chooses an enemy target
    /// </summary>
    public SelectionAlgorithm selectionAlg = SelectionAlg.Closest;

    /// <summary>
    /// A list of all the weapons this mech has as Types
    /// </summary>
    public WeaponScriptable[] weapons;
}

public static class SelectionAlg
{
    public static BaseMech LowestHealth(BaseMech selector, params BaseMech[] mechs)
    {
        return mechs.OrderBy((mech) => mech.CombatBehviour.GetHealth().current).First();
    }
    public static BaseMech Closest(BaseMech selector, params BaseMech[] mechs)
    {
        return mechs.OrderBy((mech) => (selector.transform.position - mech.transform.position).sqrMagnitude).First();
    }
    public static BaseMech HighestHealth(BaseMech selector, params BaseMech[] mechs)
    {
        return mechs.OrderByDescending((mech) => (selector.transform.position - mech.transform.position).sqrMagnitude).First();
    }
}
