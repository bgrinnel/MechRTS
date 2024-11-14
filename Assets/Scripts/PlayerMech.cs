using UnityEngine;
using System.Linq;

public class PlayerMech : BaseMech
{
    protected override void MechAwake(out bool isPlayer)
    {
        isPlayer = true;
    }

    protected override void MechUpdate(TState state, BaseMech tempTarget, BaseMech heardTarget)
    {
        
        switch (state.Switch)
        {
            case TState.Idle:
                if (HasPatrol) 
                {
                    var closest_patrol_point = Patrol.OrderBy((pos) => (transform.position - pos).sqrMagnitude).First();
                    patrolIdx = Patrol.FindIndex((patrol_point) => patrol_point == closest_patrol_point);
                    SetNavDestination(closest_patrol_point);
                    SetState(TState.Patroling);
                }
                if (tempTarget != null) EngageTarget(tempTarget);

                break;

            case TState.Patroling:
                if (HasReachedDestination()) SetNavDestination(Patrol[++patrolIdx % Patrol.Count]);
                break;

            case TState.Chasing:
                if (CurrentTarget == null) 
                {
                    if (IsSelected) SetState(TState.AwaitingWaypoint);
                    else SetState(TState.Idle);
                    SetNavDestination(transform.position);
                }
                else 
                {
                    var engagement = EngageTarget(CurrentTarget);
                    if (engagement == EEngagement.InRangeFull)
                    {
                        SetNavDestination(transform.position);
                    }
                    else if (CurrentTarget != null) // check if we killed the target after engagement
                    {
                        SetNavDestination(CurrentTarget);
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
                if (NavMeshAgent.remainingDistance < ScaledRadius * 6f)
                {
                    //  TODO: not a perfect solution to bumping - may need changed
                    if (OverlapCapsule(NavMeshAgent.destination, out Collider[] hits, "Enemy", "Player"))
                    {
                        var mech = hits[0].GetComponent<BaseMech>();
                        if (mech == null)
                        {
                            Debug.LogWarning($"PlayerMech, '{name}', had a OverlapCollision but wasn't able to retrieve a MechBehaviour");
                            break;
                        }
                        var direction_to_self = (transform.position - mech.transform.position).normalized;
                        SetNavDestination(NavMeshAgent.destination + direction_to_self * (ScaledRadius + mech.ScaledRadius));
                    }
                }
                if (state == TState.FollowingWaypoint && HasReachedDestination()) 
                {
                    SetState( IsSelected ? TState.AwaitingWaypoint : TState.Idle);
                }
                if (tempTarget != null) EngageTarget(tempTarget);
                break;
        }
    }
}