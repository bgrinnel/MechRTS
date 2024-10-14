using System;
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
    public delegate void MechStateChange(TState @new, TState old, float timeInOld);

    // saved component references
    private NavMeshAgent _navMeshAgent;
    [HideInInspector] public CombatBehaviour combatBehaviour;

    // children
    private MeshRenderer _selectionPulse;
    private MeshRenderer _targetPulse;
    
    // actually class properties
    [SerializeField] private MechType _type;
    [SerializeField] private bool _isPlayer;
    [SerializeField] private Vector3[] _patrol = System.Array.Empty<Vector3>();
    private int _patrolIdx;

    // public enum EState
    // {
    //     /// <summary>
    //     /// Mechs will stand around or, maybe in the future, wander in close proximity
    //     /// </summary>
    //     Idle = 1,

    //     /// <summary>
    //     /// For mechs that have a patrol list, will go from point to point
    //     /// </summary>
    //     Patroling = 2,

    //     /// <summary>
    //     /// When a mech catches aggro of an enemy out of firing range and approaches them
    //     /// </summary>
    //     Chasing = 4,

    //     /// <summary>
    //     /// When a mech is within range of enemies to attack
    //     /// </summary>
    //     Fighting = 8,

    //     /// <summary>
    //     /// Could be used used if we want mechs to run from aggro if they're in critical condition
    //     /// </summary>
    //     Retreating = 16, 

    //     /// <summary>
    //     /// For player controlled mechs, forces them to follow player waypoints
    //     /// </summary>
    //     FollowingWaypoint = 32,

    //     /// <summary>
    //     /// A state the machine will stay in for a few seconds after reaching a player specified waypoint before returning
    //     /// to other logic
    //     /// </summary>
    //     AwaitingWaypoint = 64,

    //     /// <summary>
    //     /// They're dead, duh
    //     /// </summary>
    //     Dead = 128,
    // }

    /// <summary>
    /// Never set this variable directly, always use SetState()
    /// </summary>
    private TState _state;
    public MechStateChange stateChange;
    private float _stateDuration;
    private TState _statePrev; 
    private int _aggroLayer;

    /// <summary>
    /// Define public getters and delegates for handling aggro events
    /// </summary>
    private bool _bGainingAggro;
    private float _timeTillAggro; 
    private float _timeGainingAggro; 
    private MechBehavior _target;
    private bool _playerSetTarget;

    public bool IsMoving { get; private set; }
    public Action stoppedMoving;
    public Action startedMoving;
    public Action weaponFired;
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
        _weapons = new Weapon[_type.weapons.Length];
        for (int i = 0; i < _weapons.Length; ++i)
        {
            _weapons[i] = gameObject.AddComponent<Weapon>();
            _weapons[i].Instantiate(_type.weapons[i], this);
        }

        SetState( HasPatrol() ? TState.Patroling : TState.Idle);
        _selectionPulse = transform.GetChild(0).GetComponent<MeshRenderer>();
        SetIsSelected(false);
        _targetPulse = transform.GetChild(1).GetComponent<MeshRenderer>();
        SetIsTargeted(false);
        _timeTillAggro = 5f;
    }

    void Update()
    {
        _stateDuration += Time.deltaTime;
        if (_state == TState.Dead) return;
        
        if (_bGainingAggro && _target == null) _timeGainingAggro += Time.deltaTime;
        else if (!_bGainingAggro && _target == null) _timeGainingAggro -= Time.deltaTime;


        var tmp_target = FindBestVisibleTarget();
        var heard_enemy = FindBestHearableTarget();
        
        if (tmp_target == null)
        {
            if (_timeGainingAggro >= _timeTillAggro) tmp_target = heard_enemy;
            else if (heard_enemy != null) {
                _bGainingAggro = true;
            }
            else if (_bGainingAggro) {
                _bGainingAggro = false;
            }
        }
        
        bool is_close_enough;
        switch (_state.Switch)
        {
            case TState.Idle:
                if (tmp_target != null) SetTarget(tmp_target);
                else if (HasPatrol()) 
                {
                    var dest = _patrol.OrderBy((pos) => (transform.position - pos).sqrMagnitude).First();
                    var patrol_list = _patrol.ToList();
                    _patrolIdx = patrol_list.FindIndex((pos) => pos == dest);
                    SetNavDestination(dest);
                    SetState(TState.Patroling);
                }
                break;
            case TState.Patroling:
                if (tmp_target != null) SetTarget(tmp_target);
                if (HasReachedDestination()) SetNavDestination(_patrol[++_patrolIdx % _patrol.Length]);
                break;
            case TState.Chasing:
                if (tmp_target != null && !_playerSetTarget && tmp_target != _target) SetTarget(tmp_target);
                else if (_target != null) SetNavDestination(_target.transform.position);
                else if (_target == null && tmp_target == null) 
                {
                    SetState(TState.Idle);
                    break;
                }

                is_close_enough = true;
                if (_weapons.Length == 0) is_close_enough = false;
                foreach (var weapon in _weapons) 
                {
                    if (_target == null) break; // we killed our target
                    if (!FireAt(weapon, _target))
                    {
                        is_close_enough = false;
                    }
                }
                if (is_close_enough) SetState(TState.Fighting);
                break;
            case TState.Fighting:
                // TODO: some logic for dodging shots or circling enemies or something?
                if (tmp_target == null && _target == null) 
                {
                    SetState(TState.Idle);
                    break;
                }

                is_close_enough = true;
                if (_weapons.Length == 0) is_close_enough = false;
                foreach (var weapon in _weapons) 
                {
                    if (_target == null) break; // we killed our target
                    if (!FireAt(weapon, _target))
                    {
                        is_close_enough = false;
                    }
                }
                if (!is_close_enough) SetState(TState.Chasing);
                break;
            case TState.Retreating:
                // TODO: pathfind to a safe place or retreat to a base?
                break;
            case TState.FollowingWaypoint:
                if (tmp_target != null && tmp_target._target != null)
                {
                    foreach (var weapon in _weapons) 
                    {
                        if (tmp_target.GetState() == TState.Dead) break;
                        FireAt(weapon, tmp_target);
                    }
                }
                if (HasReachedDestination()) 
                {
                    if (IsSelected()) SetState(TState.AwaitingWaypoint);
                    else SetState(TState.Idle);
                    break;
                }
                break;
            case TState.AwaitingWaypoint:
                if (tmp_target != null && tmp_target._target != null)
                {
                    foreach (var weapon in _weapons) 
                    {
                        if (tmp_target.GetState() == TState.Dead) break;
                        FireAt(weapon, tmp_target);
                    }
                }
                // have some more strict system for directly controlling mechs
                break;
        }

        bool is_moving = _navMeshAgent.velocity.sqrMagnitude > 0f; 
        if (is_moving != IsMoving)
        {
            if (is_moving) startedMoving?.Invoke();
            else stoppedMoving?.Invoke();
            IsMoving = is_moving;
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
        if (_state == TState.Dead) return;
        if (_state != (TState.Chasing | TState.Fighting)) SetState(TState.Chasing);
        if (IsSelected() && _target != null) _target.SetIsTargeted(false);
        _target = target;
        if (IsSelected()) _target.SetIsTargeted(true);
        SetNavDestination(_target.transform.position);
    }

    /// <summary>
    /// Internal setter for FSM current state
    /// </summary>
    private void SetState(TState newState)
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
    /// Attempts to fire at the given target, returns true if the shot was executed, false otherwise
    /// </summary>
    private bool FireAt(Weapon weapon, MechBehavior target)
    {
        if (weapon.weaponRange < (target.transform.position - transform.position).magnitude) 
        {   
            return false;
        }
        weapon.Fire(target.gameObject);
        weaponFired?.Invoke();
        return true;
    }

    /// <summary>
    /// Getter for FSM current state, refer to the public enum MechBehaviour.EState for State descriptions
    /// </summary>
    public TState GetState() { return _state; }

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
        SetState(TState.FollowingWaypoint);
        SetNavDestination(waypoint);
    }

    /// <summary>
    /// A command for setting the current target (acts as an override, the mech will not chase other mechs even if they are a better target)
    /// </summary>
    public void CommandSetTarget(MechBehavior target)
    {
        if (_state == TState.Dead) return;
        SetTarget(target);
        _playerSetTarget = true;
    }

    /// <summary>
    /// Returns an array of all aggro mechs within MechType.sightRange
    /// </summary>
    /// <returns></returns>
    public MechBehavior[] FindAllVisibleEnemies()
    {
        var collisions = Physics.OverlapSphere(transform.position, _type.sightRange, _aggroLayer);
        var aggro_mechs = new List<MechBehavior>();
        foreach (var collision in collisions)
        {
            if (collision.gameObject.CompareTag("Mech")) 
            {
                var mech = collision.GetComponent<MechBehavior>();
                if (mech._state != TState.Dead) aggro_mechs.Add(mech);
            }
        }
        return aggro_mechs.ToArray();
    }

    /// <summary>
    /// Returns an array of all aggro mechs within MechType.hearingRange
    /// </summary>
    /// <returns></returns>
    public MechBehavior[] FindAllHearableEnemies()
    {
        var collisions = Physics.OverlapSphere(transform.position, _type.hearingRange, _aggroLayer);
        var aggro_mechs = new List<MechBehavior>();
        foreach (var collision in collisions)
        {
            if (collision.gameObject.CompareTag("Mech")) 
            {
                var mech = collision.GetComponent<MechBehavior>();
                if (mech._state != TState.Dead) aggro_mechs.Add(mech);
            }
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
    /// A delegate for the CombatBehaviour.healthModified event
    /// Already called internally, you don't need to call it if damage is done through CombatBehaviour
    /// </summary>
    public void OnDamaged(TPropertyContainer health, TCombatContext context)
    {
        if (context.damage.current <= 0f) return;
        if (context.instigator == null) return;
        if (_target == null && _state != (TState.Chasing | TState.Fighting | TState.FollowingWaypoint | TState.AwaitingWaypoint))
        {
            SetTarget(context.instigator.GetComponent<MechBehavior>());
        }
    }

    /// <summary>
    /// A delegate for the CombatBehaviour.defeatedAgent event
    /// Already called internally, you don't need to call it if damage is done through CombatBehaviour
    /// </summary>
    public void OnDefeatedAgent(CombatBehaviour behaviour, TCombatContext context)
    {
        var mech = behaviour.GetComponent<MechBehavior>();
        if (mech != null && mech == _target)
        {
            _playerSetTarget = false;
            _target = null;
        }
    }

    /// <summary>
    /// A delegate for the CombatBehaviour.death event
    /// Already called internally, you don't need to call it if damage is done through CombatBehaviour
    /// </summary>
    public void OnDeath(TCombatContext context)
    {
        // TODO: some death animation
        SetState(TState.Dead);
        SetIsSelected(false);
        SetIsTargeted(false);
    }

    /// <summary>
    /// How to deal damage if you don't want to have to look at CombatBehaviour
    /// </summary>
    public void HitMech(MechBehavior mech, float damage)
    {
        CombatBehaviour.CombatEvent(combatBehaviour, mech.combatBehaviour, TCombatContext.BasicAttack(damage));
    }

    public bool IsSelected() { return _selectionPulse.enabled; } 
    public void SetIsSelected(bool isSelected)
    {
        _selectionPulse.enabled = isSelected;
        if (!isSelected) 
        {
            if (_state == TState.AwaitingWaypoint) SetState(TState.Idle);
            if (_target != null) _target.SetIsTargeted(false);
        }
        if (isSelected && _target != null) _target.SetIsTargeted(true);
    }

    public void SetIsTargeted(bool isTargeted)
    {
        _targetPulse.enabled = isTargeted;
    }
}
