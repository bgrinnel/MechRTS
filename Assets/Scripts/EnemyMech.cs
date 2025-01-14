using UnityEngine;
using System.Linq;

public class EnemyMech : BaseMech
{
    private MeshRenderer _targetPulse;
    protected override void MechAwake(out bool isPlayer)
    {
        isPlayer = false;
        _targetPulse = transform.GetChild(0).GetComponent<MeshRenderer>();
        SetIsTargeted(false);
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
                        SetDestination(closest_patrol_point, true);
                        SetState(TState.Patroling);
                    }
                    else if (HasReachedDestination())
                    { 
                        SetDestination(Patrol[++patrolIdx % Patrol.Count], true);
                    }
                    break;
                }
                break;

            case TState.Chasing:
                if (CurrentTarget == null && tempTarget == null) 
                {
                    SetState(TState.Idle);
                    SetDestination(transform.position, true);
                    break;
                }

                if (tempTarget != null && tempTarget != CurrentTarget)
                {
                    SetTarget(tempTarget);
                }
                engagement = EngageTarget(CurrentTarget);
                if (engagement == EEngagement.InRangeFull || engagement == EEngagement.TooClose)
                {
                    SetDestination(transform.position, true);
                }
                else if (CurrentTarget != null) // check if we killed the target after engagement
                {
                    SetDestination(CurrentTarget);
                }
                break;

            case TState.Fighting:
                break;

            case TState.Retreating:
                break; // currently unused
            
            // Enemy Mechs won't use these states
            case TState.FollowingWaypoint:
            case TState.AwaitingWaypoint:
                break; 
        }
    }

    protected override void SetTarget(BaseMech target)
    {
        if (CurrentState == TState.Dead) return;
        currentTarget = target;
        if (target != null) 
        {
            SetState(TState.Chasing);
            SetDestination(target);
            target.CombatBehviour.death += OnTargetDeath;
        }
    }

    public override void OnDamaged(TPropertyContainer health, TCombatContext context)
    {
        base.OnDamaged(health, context);
        if (context.damage.current <= 0f) return;
        if (context.instigator == null) return;
        if (CurrentTarget == null)
        {
            SetTarget(context.instigator.GetComponent<BaseMech>());
        }
    }

    public override void OnDeath(TCombatContext context)
    {
        base.OnDeath(context);
        SetIsTargeted(false);
    }

    /// <summary>
    /// Set whether this 
    /// </summary>
    public void SetIsTargeted(bool isTargeted)
    {
        if (_targetPulse == null) return;
        _targetPulse.enabled = isTargeted;
    }
}