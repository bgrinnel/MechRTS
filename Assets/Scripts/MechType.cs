using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MechType : ScriptableObject
{
    /// <summary>
    /// A generic delegate to be used as a function type declaration for c
    /// </summary>
    /// <param name="mechs"></param>
    /// <returns></returns>
    public delegate MechBehavior SelectTargetAlgorithm(MechBehavior selector, params MechBehavior[] mechs);

    /// <summary>
    /// The max health of this mech
    /// </summary>
    public float maxHealth;

    /// <summary>
    /// The distance form a waypoint this mech will stop (without aggro)
    /// </summary>
    public float stopDistance;

    /// <summary>
    /// The speed this mech can turn in place (in degrees)
    /// </summary>
    public float turnSpeed;

    /// <summary>
    /// The initial magnitude of the mech's velocity (from stand-still)
    /// </summary>
    public float moveSpeedInitial;

    /// <summary>
    /// The acceleration of this mech per second
    /// </summary>
    public float moveSpeedAcc;

    /// <summary>
    /// The maximum magnitude of this mech's velocity
    /// </summary>
    public float moveSpeedMax;

    /// <summary>
    /// How far enemies can be seen by this mech (draws immediate aggro)
    /// </summary>
    public float sightRange;

    /// <summary>
    /// How far enemies can be heard by this mech (makes the mech divert from patrol, no immediate aggro)
    /// </summary>
    public float hearingRange;

    /// <summary>
    /// How
    /// </summary>
    public SelectTargetAlgorithm targetingAlgorithm;

    /// <summary>
    /// A list of all the weapons this mech has
    /// </summary>
    public List<WeaponScriptable> WeaponTypes;
}

public static class SelectTargetAlgorithms
{
    public static MechBehavior SelectedLowestHealth(MechBehavior selector, params MechBehavior[] mechs)
    {
        return mechs.OrderBy((mech) => mech.combateBehaviour.GetHealth().Current).First();
    }
    public static MechBehavior SelectClosest(MechBehavior selector, params MechBehavior[] mechs)
    {
        return mechs.OrderBy((mech) => (selector.transform.position - mech.transform.position).sqrMagnitude).First();
    }
    public static MechBehavior SelectHighestHealth(MechBehavior selector, params MechBehavior[] mechs)
    {
        return mechs.OrderByDescending((mech) => (selector.transform.position - mech.transform.position).sqrMagnitude).First();
    }
}
