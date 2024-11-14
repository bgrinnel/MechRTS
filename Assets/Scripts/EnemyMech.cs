using UnityEngine;
using System.Linq;

public class EnemyMech : BaseMech
{
    protected override void MechAwake(out bool isPlayer)
    {
        isPlayer = false;
    }

    protected override void MechUpdate(TState state, BaseMech tempTarget, BaseMech heardTarget)
    {
        EEngagement engagement;
        switch (state.Switch)
        {
            case TState.Idle:
            case TState.Patroling:
                if (tempTarget != null)
                {
                    SetTarget(tempTarget);
                    break;
                }
                else if (heardTarget != null && HasViableTarget)
                {
                    SetTarget(heardTarget);
                    break;
                }
                else if (HasPatrol) 
                {
                    if (state == TState.Idle)
                    {
                        var closest_patrol_point = Patrol.OrderBy((pos) => (transform.position - pos).sqrMagnitude).First();
                        patrolIdx = Patrol.FindIndex((patrol_point) => patrol_point == closest_patrol_point);
                        SetNavDestination(closest_patrol_point);
                        SetState(TState.Patroling);
                    }
                    else if (HasReachedDestination())
                    { 
                        SetNavDestination(Patrol[++patrolIdx % Patrol.Count]);
                    }
                    break;
                }
                break;

            case TState.Chasing:
                if (CurrentTarget == null && tempTarget == null) 
                {
                    SetState(TState.Idle);
                    SetNavDestination(transform.position);
                    break;
                }

                if (tempTarget != null && tempTarget != CurrentTarget)
                {
                    SetTarget(tempTarget);
                }
                engagement = EngageTarget(CurrentTarget);
                if (engagement == EEngagement.InRangeFull || engagement == EEngagement.TooClose)
                {
                    SetNavDestination(transform.position);
                }
                else if (CurrentTarget != null) // check if we killed the target after engagement
                {
                    SetNavDestination(CurrentTarget);
                }
                break;

            case TState.Fighting:
                // if (CurrentTarget == null)
                // {
                //     SetState(TState.Idle);
                //     break;
                // } 
                // engagement = EngageTarget(CurrentTarget);
                // if (engagement == EEngagement.InRangePartial || engagement == EEngagement.OutOfRange)
                // {
                //     if (CurrentTarget == null) break;
                //     SetState(TState.Chasing);
                //     SetNavDestination(CurrentTarget);
                // }
                break;

            case TState.Retreating:
                break; // currently unused
            
            // Enemy Mechs won't use these states
            case TState.FollowingWaypoint:
            case TState.AwaitingWaypoint:
                break; 
        }
    }
}