using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Design;

// [ExecuteInEditMode]
public class PlayerMech : BaseMech
{
    private bool _playerSetTarget;
    private MeshRenderer _selectionPulse;

    /// <summary>
    /// Returns whether this mech is Selected by the Player
    /// </summary>
    public bool IsSelected => _selectionPulse == null ? false : _selectionPulse.enabled; 

    private void OnEnable()
    {
        if (RTSController.Singleton != null) RTSController.Singleton.RegisterPlayerMech(this);
    }

    private void OnDisable()
    {
        if (RTSController.Singleton != null) RTSController.Singleton.UnregisterPlayerMech(this);
    }

    protected override void MechAwake(out bool isPlayer)
    {
        isPlayer = true;
        _selectionPulse = transform.GetChild(0).GetComponent<MeshRenderer>();
        SetIsSelected(false);
    }

    protected override void MechUpdate(TState state, BaseMech tempTarget, BaseMech heardTarget)
    {
		/* INTENDED DESIGN?
		
		Idle 						-> attack any unit that comes into range but doesn't follow

		SetWaypoint, MB1 command	->	move to position, don't fire till in position	

		Chase,  MB1 on Enemy		-> follow enemy till killed, then stop moving

		SetWaypoint, MB1 + Hotkey 	-> move to position, fire on-way as needed 
		*/
        switch (state.Switch)
        {
            case TState.Idle:
                if (HasPatrol) 
                {
                    var closest_patrol_point = Patrol.OrderBy((pos) => (transform.position - pos).sqrMagnitude).First();
                    patrolIdx = Patrol.FindIndex((patrol_point) => patrol_point == closest_patrol_point);
                    SetDestination(closest_patrol_point, true);
                    SetState(TState.Patroling);
                }
                if (tempTarget != null) EngageTarget(tempTarget);

                break;

            case TState.Patroling:
                if (HasReachedDestination()) SetDestination(Patrol[++patrolIdx % Patrol.Count], true);
                break;

            case TState.Chasing:
                if (CurrentTarget == null) 
                {
                    if (IsSelected) SetState(TState.AwaitingWaypoint);
                    else SetState(TState.Idle);
                    SetDestination(transform.position, true);
                }
                else 
                {
                    var engagement = EngageTarget(CurrentTarget);
                    if (engagement == EEngagement.InRangeFull)
                    {
                        SetDestination(transform.position, true);
                    }
                    else if (CurrentTarget != null) // check if we killed the target after engagement
                    {
                        SetDestination(CurrentTarget);
                    }
                    else
                    {
                        SetState(IsSelected ? TState.AwaitingWaypoint : TState.Idle);
                    }
                }
                break;

            case TState.Fighting:
                break; // currently unused

            case TState.Retreating:
                break; // currently unused

            case TState.FollowingWaypoint:
            case TState.AwaitingWaypoint:
                // if (NavMeshAgent.remainingDistance < ScaledRadius * 6f)
                // {
                //     Vector3 destination = NavMeshAgent.destination;
                //     if (OverlapCapsule(destination, out Collider[] hits, "Enemy", "Player"))
                //     {
                //         var mech = hits[0].GetComponent<BaseMech>();
                //         if (mech == null)
                //         {
                //             Debug.LogWarning($"PlayerMech, '{name}', had a OverlapCollision but wasn't able to retrieve a MechBehaviour");
                //             break;
                //         }
                //         var direction_to_self = (transform.position - mech.transform.position).normalized;
                //         SetDestination(destination + direction_to_self * ScaledRadius * 1.5f);
                //     }
                // }
                if (state == TState.FollowingWaypoint)
                {
                    if (HasReachedDestination()) 
                    {
                        if (waypoints.Count > 0) waypoints.RemoveAt(0);
                        if (waypoints.Count > 0) SetDestination(waypoints[0], false);
                        else
                        {
                            SetState(IsSelected ? TState.AwaitingWaypoint : TState.Idle);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < waypoints.Count; ++i)
                        {
                            if (capsuleCollider.bounds.Contains(waypoints[i]))
                            {
                                // TODO: find out why this interrupts the path and stops early
                                waypoints.RemoveRange(0, i+1);
                                SetDestination(waypoints[0], false);
                            }
                        }
                    }
                } 
                else
                if (tempTarget != null) EngageTarget(tempTarget);
                break;
        }
    }

    protected override void SetTarget(BaseMech target)
    {
        var new_enemy_target = target as EnemyMech;
        var old_enemy_target = currentTarget as EnemyMech;

        if (CurrentState == TState.Dead) return;
        if (IsSelected && old_enemy_target != null) old_enemy_target.SetIsTargeted(false);
        currentTarget = new_enemy_target;
        if (new_enemy_target != null) 
        {
            SetState(TState.Chasing);
            if (IsSelected) new_enemy_target.SetIsTargeted(true);
            SetDestination(new_enemy_target);
            new_enemy_target.CombatBehviour.death += OnTargetDeath;
        }
    }

    /// <summary>
    /// Set whether this mech is Selected by the player
    /// </summary>
    public void SetIsSelected(bool isSelected)
    {
        if (CurrentState == TState.Dead) return;
        if (_selectionPulse == null) return;
        _selectionPulse.enabled = isSelected;
        if (currentTarget != null) (currentTarget as EnemyMech).SetIsTargeted(isSelected);
        if (!isSelected) 
        {
            if (CurrentState == TState.AwaitingWaypoint) SetState(TState.Idle);
        }
        if (isSelected) 
        {
            if (CurrentState == TState.Idle) SetState(TState.AwaitingWaypoint);
        }
    }

    public override void OnDefeatedAgent(CombatBehaviour behaviour, TCombatContext context)
    {
        base.OnDefeatedAgent(behaviour, context);
    }
    public override void OnDeath(TCombatContext context)
    {
        base.OnDeath(context);
        SetIsSelected(false);
    }

    public override void OnTargetDeath(TCombatContext context)
    {
        base.OnTargetDeath(context);
        _playerSetTarget = false;
    }

    /// <summary>
    /// A player command for setting a waypoint
    /// </summary>
    public void CommandSetWaypoint(Vector3 waypoint){
        SetState(TState.FollowingWaypoint);
        if (CurrentTarget != null) SetTarget(null);
        SetDestination(waypoint, true);
    }

    /// <summary>
    /// A player command for setting a waypoint
    /// </summary>
    public void CommandAppendWaypoint(Vector3 waypoint){
        SetState(TState.FollowingWaypoint);
        if (CurrentTarget != null) SetTarget(null);
        AppendWaypoint(waypoint);
    }


    /// <summary>
    /// A command for setting the current target (acts as an override, the mech will not chase other mechs even if they are a better target)
    /// </summary>
    public void CommandSetTarget(BaseMech target)
    {
        if (CurrentState == TState.Dead) return;
        SetTarget(target);
        _playerSetTarget = true;
    }
}