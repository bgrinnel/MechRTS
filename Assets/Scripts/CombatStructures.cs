
using System;
using UnityEngine;
using TMPro;
using UnityEditor;

/// Note from Jeff:
/// This is a work-in-progress, only making necessary elements of this right now.
/// hoping it will allow us a lot of flexibilty in the future (also, I can steal this code of mine for elsewhere) <summary>
public enum EPropertyType : byte
{
    Damage = 1,
    Health = 2,
}

public enum EPropertyEffect : byte
{
    Add = 1,
    Min = 2,
    Max = 4,
    Mod = 8,
}

public struct TPropertyModifier
{

}

public struct TPropertyContainer
{
    public float max;
    public float min;
    public float current;

    public TPropertyContainer(float max)
    {
        this.max = max;
        current = max;
        min = 0f;
    }
}

public struct TCombatContext
{
    public TPropertyContainer damage;
    public CombatBehaviour instigator;
    public CombatBehaviour target;
    static public TCombatContext BasicAttack(float Damage)
    {
        return new TCombatContext{
            damage = new TPropertyContainer{max=float.MaxValue, current=Damage, min=float.MinValue}
        };
    }
}

public struct TState
{
    private uint _val;
    public readonly uint Switch { get { return _val; } }
    private TState(uint val) { _val = val; }
    public static TState operator |(TState a, TState b)
    {
        return new TState(a._val | b._val);
    }

    public static implicit operator TState(uint val)
    {
        return new TState{_val = val};
    }

    public static bool operator ==(TState a, TState b)
    {
        return (a._val & b._val) > 0;
    }

    public static bool operator !=(TState a, TState b)
    {
        return (a._val & b._val) == 0;
    }

    public override readonly int GetHashCode()
    {
        return _val.GetHashCode();
    }
    
    public override readonly string ToString()
    {
        return _val switch
        {
            Idle => "Idle",
            Patroling => "Patroling",
            Chasing => "Chasing",
            Fighting => "Fighting",
            Retreating => "Retreating",
            FollowingWaypoint => "FollowingWaypoint",
            AwaitingWaypoint => "AwaitingWaypoint",
            Dead => "Dead",
            _ => "_INVALID_STATE_",
        };
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is uint unsigned) return _val == unsigned;
        if (obj is TState state) return this == state;
        return false;
    }

    /// <summary>
    /// Mechs will stand around or, maybe in the future, wander in close proximity
    /// </summary>
    public const uint Idle = 1;

    /// <summary>
    /// For mechs that have a patrol list, will go from point to point
    /// </summary>
    public const uint Patroling = 2;

    /// <summary>
    /// When a mech catches aggro of an enemy out of firing range and approaches them
    /// </summary>
    public const uint Chasing = 4;

    /// <summary>
    /// When a mech is within range of enemies to attack
    /// </summary>
    public const uint Fighting = 8;

    /// <summary>
    /// Could be used used if we want mechs to run from aggro if they're in critical condition
    /// </summary>
    public const uint Retreating = 16;

    /// <summary>
    /// For player controlled mechs, forces them to follow player waypoints
    /// </summary>
    public const uint FollowingWaypoint = 32;

    /// <summary>
    /// A state the machine will stay in for a few seconds after reaching a player specified waypoint before returning
    /// to other logic
    /// </summary>
    public const uint AwaitingWaypoint = 64;

    /// <summary>
    /// They're dead, duh
    /// </summary>
    public const uint Dead = 128;
}