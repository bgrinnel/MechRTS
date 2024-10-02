using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// The Mech's "Behaviour" Effectively a Finite State Machine for Mech AI
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(Collider))]
[RequireComponent(typeof(CombatBehaviour))]
public class MechBehavior : MonoBehaviour
{
    public delegate void MechStateChange(EState @new, EState old, float timeInOld);

    // saved component references
    private NavMeshAgent _NavMeshAgent;
    [HideInInspector] public CombatBehaviour combateBehaviour;
    
    // actually class properties
    [SerializeField] private MechType _Type;
    [SerializeField] private bool bPlayerMech;
    [SerializeField][HideInInspector] private List<Vector3> _Patrol = null;
    private int _PatrolIndex;

    public enum EState
    {
        /// <summary>
        /// Mechs will stand around or, maybe in the future, wander in close proximity
        /// </summary>
        Idle = 1,

        /// <summary>
        /// For mechs that have a patrol list, will go from point to point
        /// </summary>
        Patroling = 2,

        /// <summary>
        /// When a mech catches aggro of an enemy out of firing range and approaches them
        /// </summary>
        Chasing = 4,

        /// <summary>
        /// When a mech is within range of enemies to attack
        /// </summary>
        Fighting = 8,

        /// <summary>
        /// Could be used used if we want mechs to run from aggro if they're in critical condition
        /// </summary>
        Retreating = 16, 

        /// <summary>
        /// For player controlled mechs, forces them to follow player waypoints
        /// </summary>
        FollowingWaypoint = 32,

        /// <summary>
        /// A state the machine will stay in for a few seconds after reaching a player specified waypoint before returning
        /// to other logic
        /// </summary>
        AwaitingWaypoint = 64,

        /// <summary>
        /// They're dead, duh
        /// </summary>
        Dead = 128,
    }

    /// <summary>
    /// Never set this variable directly, always use SetState()
    /// </summary>
    private EState _State;
    private float _SecInState;
    private EState _StatePrev; 
    private int _AggroMask;
    private MechBehavior _Target;
    private bool _bTargetSet;
    private Weapon[] _Weapons;
    public MechStateChange stateChange;

    // Start is called before the first frame update
    void Start()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();
        combateBehaviour = GetComponent<CombatBehaviour>();
        combateBehaviour.Initialize(_Type.maxHealth);
        combateBehaviour.DefeatedAgent += OnDefeatedAgent;
        combateBehaviour.Death += OnDeath;
        // _CombatBehaviour.HealthModified += some_function()
        // _CombatBehaviour.HealthDepleted += some_function()
        _Patrol ??= new List<Vector3>();
        _AggroMask = LayerMask.GetMask(bPlayerMech ? "EnemyMechs" : "PlayerMechs");
        
        // TODO: replace with proper intialization from _Type once Weapon is functional
        _Weapons = System.Array.Empty<Weapon>();
    }

    // TODO: check to see if we need FixedUpdate since we're using NavMeshAgent
    // void FixedUpdate()
    // {
    //     if (_State is EState.Dead) return;
    // }

    void Update()
    {
        _SecInState += Time.deltaTime;

        var state = _State;
        if (state == EState.Dead) return;
        var tmp_target = CanSeeEnemy();
        switch (state)
        {
            case EState.Idle:
                if (tmp_target is not null) SetTarget(tmp_target);
                if (_SecInState > 3.5f && HasPatrol()) 
                {
                    // goto closest patrol point
                    var dest = _Patrol.OrderBy((pos) => (transform.position - pos).sqrMagnitude).First();
                    _PatrolIndex = _Patrol.FindIndex((pos) => pos == dest);
                    updateDestination(dest);
                    SetState(EState.Patroling);
                }
                break;
            case EState.Patroling:
                if (tmp_target is not null) SetTarget(tmp_target);
                if (HasReachedDestination()) updateDestination(_Patrol[++_PatrolIndex % _Patrol.Count]);
                break;
            // TODO: finish state tree
            case EState.Chasing:
                break;
            case EState.Fighting:
                break;
            case EState.Retreating:
                break;
            case EState.FollowingWaypoint:
                break;
            case EState.AwaitingWaypoint:
                break;
        }
    }

    public EState GetState() { return _State; }
    private void SetState(EState newState)
    {
        _StatePrev = _State;
        _State = newState;
        stateChange?.Invoke(_State, _StatePrev, _SecInState);
        _SecInState = 0f;
    }

    public bool HasPatrol() { return _Patrol.Count > 0; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="waypoint"></param>
    public void SetWaypoint(Vector3 waypoint){
        SetState(EState.FollowingWaypoint);
        updateDestination(waypoint);
    }

    /// <summary>
    /// An internal method for making
    /// </summary>
    private void updateDestination(Vector3 newDest)
    {
        _NavMeshAgent.destination = newDest;
    }

    public bool HasReachedDestination(){
        return _NavMeshAgent.remainingDistance <= _Type.stopDistance;
    }

    public MechBehavior CanSeeEnemy()
    {
        var collisions = Physics.OverlapSphere(transform.position, _Type.sightRange, _AggroMask);
        if (collisions.Length > 0)
        {
            return _Type.targetingAlgorithm(
                this, 
                collisions.Select((collider) => collider.gameObject.GetComponent<MechBehavior>()).ToArray()
            );
        }
        return null;
    }

    private void SetTarget(MechBehavior target)
    {
        SetState(EState.Chasing);
        _Target = target;
    }

    /// <summary>
    /// For User input, modify with SetTarget otherwise
    /// </summary>
    /// <param name="target"></param>
    public void PlayerCommandSetTarget(MechBehavior target)
    {
        if (_State is EState.Dead) return;
        SetTarget(target);
        _bTargetSet = true;
    }

    // ===== event handlers =====
    void OnDefeatedAgent(CombatBehaviour behaviour)
    {
        var mech = behaviour.GetComponent<MechBehavior>();
        if (mech != null && mech == _Target)
        {
            _bTargetSet = false;
            _Target = null;
        }
    }

    void OnDeath()
    {
        // TODO: some death animation
        _State = EState.Dead;
    }
}
