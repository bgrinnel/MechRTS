using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// TODO: To: Jeff, From: Jeff - See if Unity's State Machines would be a better place for this FSM logic
/// <summary>
/// The Mech's "Behaviour" Effectively a Finite State Machine for Mech AI
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(CapsuleCollider))]
[RequireComponent(typeof(CombatBehaviour))]
public class MechBehavior : MonoBehaviour
{
    public delegate void MechStateChange(EState @new, EState old, float timeInOld);

    // saved component references
    private NavMeshAgent _navMeshAgent;
    [HideInInspector] public CombatBehaviour combatBehaviour;
    private Collider _collider;

    // children
    private MeshRenderer selectionPulse;
    
    // actually class properties
    [SerializeField] private MechType _type;
    [SerializeField] private bool _isPlayer;
    [SerializeField] private Vector3[] _patrol = System.Array.Empty<Vector3>();
    private int _patrolIdx;

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
    private EState _state;
    public MechStateChange stateChange;
    private float _stateDuration;
    private EState _statePrev; 
    private int _aggroLayer;

    /// <summary>
    /// Define public getters and delegates for handling aggro events
    /// </summary>
    private bool _bGainingAggro;
    private float _timeTillAggro; 
    private float _timeGainingAggro; 
    private MechBehavior _target;
    private bool _bTargetSet;

    private Weapon[] _weapons;

    void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _type.agentType.speed;
        _navMeshAgent.angularSpeed = _type.agentType.angularSpeed;
        _navMeshAgent.acceleration = _type.agentType.acceleration;
        _navMeshAgent.stoppingDistance = _type.agentType.stoppingDistance;
        _navMeshAgent.autoBraking = _type.agentType.autoBraking;
        _navMeshAgent.radius = _type.agentType.radius;
        _navMeshAgent.height = _type.agentType.height;
        _navMeshAgent.height = _type.agentType.height / 2.2f;
        _navMeshAgent.autoTraverseOffMeshLink = true;
        _navMeshAgent.agentTypeID = -1372625422;                // I initially printed the mech id to find its value

        var collider = GetComponent<CapsuleCollider>();
        // Debug.Log($"NavMeshAgent typeId = {nav_type_id}");
        collider.radius = _type.agentType.radius;
        collider.height = _type.agentType.height;
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.mass = _type.mass;
        rigidbody.drag = _type.drag;
        rigidbody.angularDrag = _type.angularDrag;

        combatBehaviour = GetComponent<CombatBehaviour>();
        combatBehaviour.Initialize(_type.maxHealth);
        combatBehaviour.defeatedAgent += OnDefeatedAgent;
        combatBehaviour.death += OnDeath;
        _aggroLayer = LayerMask.NameToLayer(! _isPlayer ? "Player" : "Enemy");
        gameObject.layer = LayerMask.NameToLayer(_isPlayer ? "Player" : "Enemy");
        gameObject.tag = "Mech";
        _weapons = System.Array.Empty<Weapon>();
        SetState( HasPatrol() ? EState.Patroling : EState.Idle);
        selectionPulse = transform.GetChild(0).GetComponent<MeshRenderer>();
        SetIsSelected(false);
    }

    void Start()
    {
        _timeTillAggro = 5f;
        // Debug.Log($"{name} has the aggro mask \"{LayerMask.LayerToName(_aggroLayer)}\"");
        // Debug.Log($"{name}'s obj is in mask \"{LayerMask.LayerToName(gameObject.layer)}\"");
    }

    void Update()
    {
        var state = _state;
        _stateDuration += Time.deltaTime;
        if (state is EState.Dead) return;
        
        if (_bGainingAggro && _target == null) _timeGainingAggro += Time.deltaTime;
        else if (!_bGainingAggro && _target == null) _timeGainingAggro -= Time.deltaTime;


        var tmp_target = FindBestVisibleTarget();
        // if (tmp_target != null) Debug.Log($"{name} can see enemy mech named {tmp_target.name}");
        var heard_enemy = FindBestHearableTarget();
        // if (heard_enemy != null) Debug.Log($"{name} can hear enemy mech named {heard_enemy.name}");
        
        if (tmp_target == null)
        {
            if (_timeGainingAggro >= _timeTillAggro) tmp_target = heard_enemy;
            else if (heard_enemy != null) {
                // Debug.Log($"{name} is gaining Aggro");
                _bGainingAggro = true;
            }
            else if (_bGainingAggro) {
                // Debug.Log($"{name} is losing aggro");
                _bGainingAggro = false;
            }
        }
        

        switch (state)
        {
            case EState.Idle:
                if (tmp_target != null) SetTarget(tmp_target);
                else if (_stateDuration > 5f && HasPatrol()) 
                {
                    // goto closest patrol point
                    var dest = _patrol.OrderBy((pos) => (transform.position - pos).sqrMagnitude).First();
                    var patrol_list = _patrol.ToList();
                    _patrolIdx = patrol_list.FindIndex((pos) => pos == dest);
                    SetNavDestination(dest);
                    SetState(EState.Patroling);
                }
                break;
            case EState.Patroling:
                if (tmp_target != null) SetTarget(tmp_target);
                if (HasReachedDestination()) SetNavDestination(_patrol[++_patrolIdx % _patrol.Length]);
                break;
            case EState.Chasing:
                if (tmp_target != null && !_bTargetSet && tmp_target != _target) SetTarget(tmp_target);
                else if (_target != null) SetNavDestination(_target.transform.position);
                else if (_target == null && tmp_target == null) SetState(EState.Idle);

                bool b_close_as_needed = true;
                if (_weapons.Length == 0) b_close_as_needed = false;
                foreach (var weapon in _weapons) 
                {
                    if (weapon.weaponRange < (_target.transform.position - transform.position).magnitude) 
                    {   
                        b_close_as_needed = false;
                        // TODO: maybe set weapon states to auto-fire if within range (and then remove the break)
                        break;
                    }
                }
                if (b_close_as_needed) SetState(EState.Fighting);
                break;
            case EState.Fighting:
                // TODO: some logic for dodging shots or circling enemies or something?
                if (tmp_target == null && _target == null) SetState(EState.Idle);
                break;
            case EState.Retreating:
                // TODO: pathfind to a safe place or retreat to a base?
                break;
            case EState.FollowingWaypoint:
                // TODO: rotate through weapons and fire if there are enemies in range
                if (HasReachedDestination()) SetState(EState.AwaitingWaypoint);
                break;
            case EState.AwaitingWaypoint:
                // TODO: rotate through weapons and fire if there are enemies in range
                // TODO: wait for another waitpoint or for the player to unselect this unit? (Rimworld-like unit drafting)
                break;
        }
    }

    /// <summary>
    /// Returns a MechBehaviour that best satisfies MechType.selectionAlg within MechType.sightRange, null if none
    /// </summary>
    public MechBehavior FindBestVisibleTarget()
    {
        var aggro_mechs = FindAllVisibleEnemies();
        if (aggro_mechs.Length == 1) return aggro_mechs[0];
        else if (aggro_mechs.Length > 0) return _type.selectionAlg(this, aggro_mechs);
        else return null;
    }

    /// <summary>
    /// Returns a MechBehaviour that best satisfies MechType.selectionAlg within MechType.hearingRange, null if none
    /// </summary>
    public MechBehavior FindBestHearableTarget()
    {
        var aggro_mechs = FindAllHearableEnemies();
        if (aggro_mechs.Length == 1) return aggro_mechs[0];
        else if (aggro_mechs.Length > 0) return _type.selectionAlg(this, aggro_mechs);
        else return null;
    }

    private void SetTarget(MechBehavior target)
    {
        if (_state is EState.Dead) return;
        if (_state is not EState.Chasing && _state is not EState.Fighting) SetState(EState.Chasing);
        // Debug.Log($"{name} is targeting {target.name}");
        _target = target;
        SetNavDestination(_target.transform.position);
    }

    /// <summary>
    /// Internal setter for FSM current state
    /// </summary>
    private void SetState(EState newState)
    {
        Debug.Log($"\"{gameObject.name}\" is now \"{newState}\"");
        if (newState == _state) return;
        _statePrev = _state;
        _state = newState;
        stateChange?.Invoke(_state, _statePrev, _stateDuration);
        _stateDuration = 0f;
    }

    /// <summary>
    /// for internal setting only, if you're the player use a "Command" method
    /// </summary>
    private void SetNavDestination(Vector3 newDest)
    {
        _navMeshAgent.destination = newDest;
    }

    /// <summary>
    /// Getter for FSM current state, refer to the public enum MechBehaviour.EState for State descriptions
    /// </summary>
    public EState GetState() { return _state; }

    /// <summary>
    /// returns the MechType this MechBehaviour is instantiated from (read-only)
    /// </summary>
    public MechType GetMechType() { return _type; }

    /// <summary>
    /// returns the current patrol path this mech will follow while EState::Patroling (read-only)
    /// </summary>
    public Vector3[] GetPatrol() { return _patrol.ToList().ToArray(); } // a very sloppy way of making this not pass a direct reference to the _patrol path

    /// <summary>
    /// Whether this mech has a patrol path (read-only)
    /// </summary>
    public bool HasPatrol() { return _patrol.Length > 0; }

    /// <summary>
    /// Whether this mech has a patrol path (read-only)
    /// </summary>
    public bool IsPlayerMech() { return _isPlayer; }

    /// <summary>
    /// A player command for setting a waypoint
    /// </summary>
    public void CommandSetWaypoint(Vector3 waypoint){
        SetState(EState.FollowingWaypoint);
        SetNavDestination(waypoint);
    }

    /// <summary>
    /// A command for setting the current target (acts as an override, the mech will not chase other mechs even if they are a better target)
    /// </summary>
    public void CommandSetTarget(MechBehavior target)
    {
        if (_state is EState.Dead) return;
        SetTarget(target);
        _bTargetSet = true;
    }

    /// <summary>
    /// Returns an array of all aggro mechs within MechType.sightRange
    /// </summary>
    /// <returns></returns>
    public MechBehavior[] FindAllVisibleEnemies()
    {
        var collisions = Physics.OverlapSphere(transform.position, _type.sightRange);
        var aggro_mechs = new List<MechBehavior>();
        foreach (var collision in collisions)
        {
            if (collision.gameObject.layer == _aggroLayer) aggro_mechs.Add(collision.GetComponent<MechBehavior>());
        }
        return aggro_mechs.ToArray();
    }

    /// <summary>
    /// Returns an array of all aggro mechs within MechType.sightRange
    /// </summary>
    /// <returns></returns>
    public MechBehavior[] FindAllHearableEnemies()
    {
        var collisions = Physics.OverlapSphere(transform.position, _type.hearingRange);
        var aggro_mechs = new List<MechBehavior>();
        foreach (var collision in collisions)
        {
            if (collision.gameObject.layer == _aggroLayer) aggro_mechs.Add(collision.GetComponent<MechBehavior>());
        }
        return aggro_mechs.ToArray();
    }

    /// <summary>
    /// Whether this mech is at its nav destination
    /// </summary>
    public bool HasReachedDestination(){
        return _navMeshAgent.remainingDistance <= _type.agentType.radius + .8f;
    }

    /// <summary>
    /// A delegate for the CombatBehaviour.defeatedAgent event
    /// Already called internally, you don't need to call it if damage is done through CombatBehaviour
    /// </summary>
    public void OnDefeatedAgent(CombatBehaviour behaviour)
    {
        var mech = behaviour.GetComponent<MechBehavior>();
        if (mech != null && mech == _target)
        {
            _bTargetSet = false;
            _target = null;
        }
    }

    /// <summary>
    /// A delegate for the CombatBehaviour.death event
    /// Already called internally, you don't need to call it if damage is done through CombatBehaviour
    /// </summary>
    public void OnDeath()
    {
        // TODO: some death animation
        SetState(EState.Dead);
    }

    /// <summary>
    /// How to deal damage if you don't want to have to look at CombatBehaviour
    /// </summary>
    public void HitMech(MechBehavior mech, float damage)
    {
        CombatBehaviour.CombatEvent(combatBehaviour, mech.combatBehaviour, TCombatContext.BasicAttack(damage));
    }

    public bool IsSelected() { return selectionPulse.enabled; } 
    public void SetIsSelected(bool isSelected)
    {
        selectionPulse.enabled = isSelected;
    }
}
