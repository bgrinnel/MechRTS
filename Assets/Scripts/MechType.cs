using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "MechType_new_mech_type_name", menuName = "MechType")]
public class MechType : ScriptableObject
{
    // ===================
    // ===== STATICS =====
    // ===================

    /// <summary>
    /// A generic delegate to be used as the function template for all mech slection algorithmns
    /// </summary>
    /// <param name="mechs"></param>
    /// <returns></returns>
    public delegate BaseMech SelectionAlgorithm(BaseMech selector, params BaseMech[] mechs);

    // ==================
    // ===== COMBAT =====
    // ==================

    /// <summary>
    /// The max health of this mech
    /// </summary>
    public float maxHealth = 100f;

    /// <summary>
    /// A list of all the weapons this mech has as Types
    /// </summary>
    public WeaponScriptable[] weapons = default;

    // ==================================
    // ===== PHYSICS AND NAVIGATION =====
    // ==================================

    /// <summary>
    /// meters/second speed. For navigation and physics
    /// </summary>
    public float speed = 15f;

    /// <summary>
    /// degrees/second. For navigation
    /// </summary>
    public float angularSpeed = 120f;

    /// <summary>
    /// meters/(second^2). For naviation and physics
    /// </summary>
    public float acceleration = .3f;

    /// <summary>
    /// meters. For Navigation
    /// </summary>
    public float stoppingDistance = 3f;

    /// <summary>
    /// Whether the mech will attempt to break in time to come to a stop at the destination
    /// </summary>
    public bool autoBraking = true;

    /// <summary>
    /// meters. The size of the mech's capsule collider. For naviation and physics
    /// </summary>
    public float radius = 2f;

    /// <summary>
    /// m/s speed. For navigation and physics
    /// </summary>
    public float height = 5f;

    /// <summary>
    /// The RigidBody.mass of this mech
    /// </summary>
    public float mass = 300f;

    /// <summary>
    /// The RigidBody.drag of this mech
    /// </summary>
    public float drag = 3f;

    /// <summary>
    /// The RigidBody.angularDrag of this mech
    /// </summary>
    public float angularDrag = 3f;

    // ===== SPEED CURVES =====

    /// <summary>
    /// Yields hit rate reduction (eg. dodge chance) of this mech at a given speed (meters/second)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve hitRateReductionOverSpeed = default;

    /// <summary>
    /// Yields the (%) damage mitagation of this mech at a given speed (meters/second).
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve damageReductionOverSpeed = default;

    /// <summary>
    /// Yields the critical hit rate reduction of this mech at a given speed (meters/second)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve critRateReductionOverSpeed = default;

    // ===== FLANK CURVES =====

    /// <summary>
    /// Yields hit rate addition of this mech at a given distance (meters)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve frontFlankHitRateAddOverDistance = default;

    /// <summary>
    /// Yields crit rate addition of this mech at a given distance (meters)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve frontFlankCritRateAddOverDistance = default;
    
    /// <summary>
    /// Yields hit rate addition of this mech at a given distance (meters)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve leftFlankHitRateAddOverDistance = default;

    /// <summary>
    /// Yields crit rate addition of this mech at a given distance (meters)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve leftFlankCritRateAddOverDistance = default;
    /// <summary>
    /// Yields hit rate addition of this mech at a given distance (meters)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve rightFlankHitRateAddOverDistance = default;

    /// <summary>
    /// Yields crit rate addition of this mech at a given distance (meters)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve rightFlankCritRateAddOverDistance = default;
    /// <summary>
    /// Yields hit rate addition of this mech at a given distance (meters)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve rearFlankHitRateAddOverDistance = default;

    /// <summary>
    /// Yields crit rate addition of this mech at a given distance (meters)
    /// Must yield a negative value to be a reduction
    /// </summary>
    public AnimationCurve rearFlankCritRateAddOverDistance = default;

    // ==============
    // ===== AI =====
    // ==============

    /// <summary>
    /// How the unit chooses an enemy target
    /// </summary>
    public SelectionAlgorithm selectionAlg = SelectionAlg.Closest;

    /// <summary>
    /// How far enemies can be seen by this mech (draws immediate aggro)
    /// </summary>
    public float sightRange = 50f;

    /// <summary>
    /// How far enemies can be heard by this mech (makes the mech divert from patrol, no immediate aggro)
    /// </summary>
    public float hearingRange = 100f;
}

/// <summary>
/// Where all methods that are to be used as <see cref="MechType.SelectionAlgorithm"/> delegates should be defined.
/// Make sure that all <see cref="SelectionAlgorithType"/> numeration names match all <see cref="SelectionAlg"/> method names
/// </summary>
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

/// <summary>
/// Enumerations for exposing <see cref="MechType.SelectionAlgorithm"/> delegates in the Editor.
/// Make sure that all <see cref="SelectionAlgorithType"/> numeration names match all <see cref="SelectionAlg"/> method names
/// </summary>
public enum SelectionAlgorithType
{
    LowestHealth,
    Closest,
    HighestHealth
}
