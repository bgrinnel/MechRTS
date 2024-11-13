using System;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

// TODO: To: Jeff, From: Jeff - See if Unity's State Machines would be a better place for this FSM logic
/// <summary>
/// The Mech's "Behaviour" Effectively a Finite State Machine for Mech AI
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(CapsuleCollider))]
[RequireComponent(typeof(CombatBehaviour))]
public class MechBehavior : MonoBehaviour
{
    [SerializeField] private ParticleSystem _smoke;
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

    /// <summary>
    /// Never set this variable directly, always use SetState()
    /// </summary>
    private TState _state;
    public MechStateChange stateChange;
    private float _stateDuration;
    private TState _statePrev; 
    private int _aggroMask;


    // Define public getters and delegates for handling aggro events
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

    /// <summary>
    /// the distance egagement starts (i.e. max(all_weapon_ranges))
    /// </summary>
    private float enageDistanceStart;

    /// <summary>
    /// the distance egagement is full (i.e. min(all_weapon_ranges))
    /// </summary>
    private float engageDistanceFull;

    /// <summary>
    /// the distance a mech will move back to create space (i.e. engageDistanceFull * 0.5f)
    /// </summary>
    private float engageDistanceRetreat;
    
    /// <summary>
    /// A string that used for visiual debugging, desrives the current manuever of this mech
    /// </summary>
    public string EvasionManuveur { get; private set; }

    private float scaledRadius;

    /// <summary>
    /// Defines several states that a target can be from this mech
    /// </summary>
    private enum EEngagment 
    {
        /// <summary>
        /// The target is farther away than MechBehaviour.engageDistanceStart
        /// </summary>
        OutOfRange,
        /// <summary>
        /// The target is farther away than MechBehaviour.engageDistanceFull
        /// </summary>
        InRangePartial,
        /// <summary>
        /// The target is closer than MechBehaviour.engageDistanceFull
        /// </summary>
        InRangeFull,
        /// <summary>
        /// The target is closer than MechBehaviour.engageDistanceRetreat
        /// </summary>
        TooClose
    }

    void Awake()
    {
        _smoke = GetComponentInChildren<ParticleSystem>();

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _type.agentType.speed;
        _navMeshAgent.angularSpeed = _type.agentType.angularSpeed;
        _navMeshAgent.acceleration = _type.agentType.acceleration;
        _navMeshAgent.stoppingDistance = _type.agentType.stoppingDistance;
        _navMeshAgent.autoBraking = _type.agentType.autoBraking;
        _navMeshAgent.radius = _type.agentType.radius;
        _navMeshAgent.height = _type.agentType.height;
        _navMeshAgent.height = _type.agentType.height / 1.95f;
        _navMeshAgent.autoTraverseOffMeshLink = true;
        _navMeshAgent.agentTypeID = -1372625422;                // I initially printed the mech id to find its value

        var collider = GetComponent<CapsuleCollider>();
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
        _aggroMask = LayerMask.GetMask(!_isPlayer ? "Player" : "Enemy");
        gameObject.layer = LayerMask.NameToLayer(_isPlayer ? "Player" : "Enemy");
        gameObject.tag = "Mech";

        _weapons = new Weapon[_type.weapons.Length];
        enageDistanceStart = 0;
        engageDistanceFull = float.PositiveInfinity;
        for (int i = 0; i < _weapons.Length; ++i)
        {
            _weapons[i] = gameObject.AddComponent<Weapon>();
            _weapons[i].Instantiate(_type.weapons[i], this);
            float weapon_range = _weapons[i].weaponRange;
            if (weapon_range > enageDistanceStart) enageDistanceStart = weapon_range;
            if (weapon_range < engageDistanceFull) engageDistanceFull =  weapon_range;
        }
        engageDistanceRetreat = engageDistanceFull / 2f;
        EvasionManuveur = "";
        SetState( HasPatrol() ? TState.Patroling : TState.Idle);
        _selectionPulse = transform.GetChild(0).GetComponent<MeshRenderer>();
        SetIsSelected(false);
        _targetPulse = transform.GetChild(1).GetComponent<MeshRenderer>();
        SetIsTargeted(false);
        _timeTillAggro = 5f;
        scaledRadius = _type.agentType.radius * ((transform.localScale.x + transform.localScale.z) / 2f); // take an average if it's not round
        // Debug.Log($"'Player' GetMask() = {LayerMask.GetMask("Player")} NameToLayer() = {LayerMask.NameToLayer("Player")}");
        // Debug.Log($"'Enemy' GetMask() = {LayerMask.GetMask("Enemy")} NameToLayer() = {LayerMask.NameToLayer("Enemy")}");
        // Debug.Log($"'{name}'.layer = {gameObject.layer} | LayerToName() = {LayerMask.LayerToName(gameObject.layer)}");
    }

    void Update()
    {
        _stateDuration += Time.deltaTime;
        EvasionManuveur = "";
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
            case TState.Fighting:
            case TState.Retreating:
                if (tmp_target != null && !_playerSetTarget && tmp_target != _target) SetTarget(tmp_target);
                if (_target == null && tmp_target == null) 
                {
                    SetState(TState.Idle);
                    break;
                }

                const int angle_checks = 10;
                var clear_distance = scaledRadius + 1f;
                int lateral_ratio; Vector3 lateral_dir_blend;
                var engagment = EngageTarget(_target);
                switch (engagment)
                {
                    case EEngagment.OutOfRange:
                    case EEngagment.InRangePartial:
                        SetState(TState.Chasing);
                        SetNavDestination(_target.transform.position);
                        break;

                    case EEngagment.InRangeFull:
                        SetIsUsingNavAgent(false);
                        SetState(TState.Fighting);
                        transform.LookAt(_target.transform, Vector3.up);
                        
                        int forward_ratio;
                        Vector3 forward = -transform.forward;
                        for (forward_ratio = 0; (forward_ratio-1) < angle_checks; ++forward_ratio)
                        {
                            lateral_ratio = angle_checks - forward_ratio;
                            lateral_dir_blend = (forward * forward_ratio + transform.right * lateral_ratio).normalized;
                            if (IsDirectionClear(lateral_dir_blend, clear_distance))
                            {
                                transform.position += _type.agentType.speed * .5f * Time.deltaTime * lateral_dir_blend;
                                EvasionManuveur = $"strafe {lateral_dir_blend}";
                                break;
                            }
                        }
                        if (EvasionManuveur == "")
                            for (forward_ratio = 0; (forward_ratio-1) < angle_checks; ++forward_ratio)
                            {
                                lateral_ratio = angle_checks - forward_ratio;
                                lateral_dir_blend = (forward * forward_ratio - transform.right * lateral_ratio).normalized;
                                if (IsDirectionClear(lateral_dir_blend, clear_distance))
                                {
                                    transform.position += _type.agentType.speed * .5f * Time.deltaTime * lateral_dir_blend;
                                    EvasionManuveur = $"strafe {lateral_dir_blend}";
                                    break;
                                }
                            }
                        // Debug.Log($"{name} - InRange - {EvasionManuveur}");
                        break;
                    case EEngagment.TooClose:
                        SetState(TState.Retreating);
                        SetIsUsingNavAgent(false);
                        transform.LookAt(_target.transform.position, Vector3.up);
                        int back_ratio;
                        Vector3 back = -transform.forward; 
                        for (back_ratio = 4; (back_ratio-1) < angle_checks; ++back_ratio)
                        {
                            lateral_ratio = angle_checks - back_ratio;
                            lateral_dir_blend = (back * back_ratio + transform.right * lateral_ratio).normalized;
                            if (IsDirectionClear(lateral_dir_blend, clear_distance))
                            {
                                transform.position += _type.agentType.speed * .65f * Time.deltaTime * lateral_dir_blend;
                                EvasionManuveur = $"strafe {lateral_dir_blend}";
                                break;
                            }
                        }
                        if (EvasionManuveur == "")
                            for (back_ratio = 4; (back_ratio-1) < angle_checks; ++back_ratio)
                            {
                                lateral_ratio = angle_checks - back_ratio;
                                lateral_dir_blend = (back * back_ratio - transform.right * lateral_ratio).normalized;
                                if (IsDirectionClear(lateral_dir_blend, clear_distance))
                                {
                                    transform.position += _type.agentType.speed * .65f * Time.deltaTime * lateral_dir_blend;
                                    EvasionManuveur = $"strafe {lateral_dir_blend}";
                                    break;
                                }
                            }
                        if (EvasionManuveur == "" && IsDirectionClear(back, 10f))
                        {
                            transform.position += _type.agentType.speed * .65f * Time.deltaTime * back;
                            EvasionManuveur = "Making Distance";
                        }
                        // Debug.Log($"{name} TooClose {EvasionManuveur}");
                        break;
                }
                break;

            case TState.FollowingWaypoint:
            case TState.AwaitingWaypoint:
                if (tmp_target != null && tmp_target._target != null)
                {
                    EngageTarget(tmp_target);
                }
                if (_state == TState.FollowingWaypoint && HasReachedDestination()) 
                {
                    SetState( IsSelected() ? TState.AwaitingWaypoint : TState.Idle);
                }
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
    /// Handles firing logic for weapons to reduce wet code
    /// </summary>
    /// <returns>EEngagement enum to describe our current distance from the target</returns>
    private EEngagment EngageTarget(MechBehavior target)
    {
        float distance_to_target = Vector3.Distance(transform.position, target.transform.position);
        if (distance_to_target > enageDistanceStart) return EEngagment.OutOfRange;
        else if (distance_to_target < engageDistanceFull)
        {
            foreach (var weapon in _weapons)
            {
                if (target == null) break; // we killed our target
                if (target._state == TState.Dead) break; // target is dead
                FireAt(weapon, target);
            }
            if (_state != TState.Retreating)
                return distance_to_target < engageDistanceRetreat ? EEngagment.TooClose : EEngagment.InRangeFull;
            else 
                // Add a little buffer to the retreat distance while retreating so that it doesn't flicker between the two states
                return distance_to_target < (engageDistanceRetreat * 1.5f) ? EEngagment.TooClose : EEngagment.InRangeFull;
        }
        else
        {
            foreach (var weapon in _weapons)
            {
                if (weapon.weaponRange >= distance_to_target)
                {
                    if (target == null) break; // we killed our target
                    if (target._state == TState.Dead) break; // target is dead
                    FireAt(weapon, target);
                }
            }
            return EEngagment.InRangePartial;
        }
    }

    /// <summary>
    /// Lets you know if a mech has an objestruction in the given distance and direction from its center
    /// </summary>
    public bool IsDirectionClear(Vector3 direction, float distance)
    {
        
        return !Physics.Raycast(
            new Ray(transform.position, direction),
            distance,
            LayerMask.GetMask("Static", "Enemy")
        );

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
        _target.GetComponent<CombatBehaviour>().death += OnTargetDeath;
    }

    public void OnTargetDeath(TCombatContext context)
    {
        _target = null;
        _playerSetTarget = false;
    }

    /// <summary>
    /// Internal setter for FSM current state
    /// </summary>
    private void SetState(TState newState)
    {
        if (newState == _state) return;
        Debug.Log($"\"{gameObject.name}\" is now \"{newState}\"");
        _statePrev = _state;
        _state = newState;
        stateChange?.Invoke(_state, _statePrev, _stateDuration);
        _stateDuration = 0f;
    }

    private void SetIsUsingNavAgent(bool isUsingNavAgent)
    {
        if (isUsingNavAgent == _navMeshAgent.enabled) return;
        if (!isUsingNavAgent) SetNavDestination(transform.position);
        _navMeshAgent.enabled = isUsingNavAgent;
    }
    
    /// <summary>
    /// for internal setting only, if you're the player use a "Command" method
    /// </summary>
    private void SetNavDestination(Vector3 newDest)
    {
        if (_state == TState.Dead) return;
        if (!_navMeshAgent.enabled) SetIsUsingNavAgent(true);
        _navMeshAgent.destination = newDest;
    }

    /// <summary>
    /// Fires At target (do your own distance check first)
    /// </summary>
    private void FireAt(Weapon weapon, MechBehavior target)
    {
        if (weapon.Fire(target.gameObject)) weaponFired?.Invoke();
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
        var collisions = Physics.OverlapSphere(transform.position, _type.sightRange, _aggroMask);
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
        var collisions = Physics.OverlapSphere(transform.position, _type.hearingRange, _aggroMask);
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
        _smoke.Play(true);
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
        if (_state == TState.Dead) return;
        Debug.Log($"Setting is Selected {isSelected}");
        _selectionPulse.enabled = isSelected;
        if (_target != null) _target.SetIsTargeted(isSelected);
        if (!isSelected) 
        {
            if (_state == TState.AwaitingWaypoint) SetState(TState.Idle);
        }
        if (isSelected) 
        {
            if (_state == TState.Idle) SetState(TState.AwaitingWaypoint);
        }
    }

    public void SetIsTargeted(bool isTargeted)
    {
        _targetPulse.enabled = isTargeted;
    }
}
