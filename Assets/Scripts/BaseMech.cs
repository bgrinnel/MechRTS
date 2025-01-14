using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// TODO: To: Jeff, From: Jeff - See if Unity's State Machines would be a better place for this FSM logic
/// <summary>
/// The Mech's "Behaviour" Effectively a Finite State Machine for Mech AI
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(CapsuleCollider))]
[RequireComponent(typeof(CombatBehaviour))]
public abstract class BaseMech : MonoBehaviour
{

    /// <summary>
    /// Defines several states that a target can be from this mech
    /// </summary>
    protected enum EEngagement 
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
        /// </summary>s
        InRangeFull,
        /// <summary>
        /// The target is closer than MechBehaviour.engageDistanceRetreat
        /// </summary>
        TooClose
    }


    /********************
    *     FIELDS       *
    ********************/
    // Private and protected fields
    [SerializeField] private MechType _type;
    [SerializeField] private Vector3[] _patrol = System.Array.Empty<Vector3>();
    [SerializeField] protected List<Vector3> waypoints;

    private float _scaledRadius;
    private float _scaledHalfHeight;

    private Weapon[] _weapons;

    /// <summary>
    /// the distance egagement starts (i.e. max(all_weapon_ranges))
    /// </summary>
    private float _enageDistanceStart;

    /// <summary>
    /// the distance egagement is full (i.e. min(all_weapon_ranges))
    /// </summary>
    private float _engageDistanceFull;

    /// <summary>
    /// the distance a mech will move back to create space (i.e. engageDistanceFull * 0.5f)
    /// </summary>
    private float _engageDistanceRetreat;

    private bool _isPlayer;
    private int _aggroMask; 
    private TState _currentState;
    private float _currentStateDurationSeconds;
    private TState _previousState;
    private bool _isMoving;
    private bool _isGainingAggro;
    private float _timeTillAggro; 
    private float _timeGainingAggro;

    // saved component references
    private CombatBehaviour _combatBehaviour;
    private NavMeshAgent _navMeshAgent;
    protected CapsuleCollider capsuleCollider;
    private ParticleSystem _onDeathSmoke;


    /// <summary>
    /// the current index in _patrol the mech should move towards
    /// </summary>
    protected int patrolIdx;
    protected BaseMech currentTarget;
    
        
    /********************
    *    PROPERTIES    *
    ********************/
    // Public and private properties

    /// <summary>
    /// A list of the Vector3 values in this Mech's Patrol path (read-only)
    /// </summary>
    public List<Vector3> Patrol => _patrol.ToList();

    /// <summary>
    /// Whether the mech has a non-zero velocity
    /// </summary>
    public bool IsMoving => _isMoving;

    /// <summary>
    /// The current MechBehavior enemy this Mech is chasing 
    /// </summary>
    public BaseMech CurrentTarget => currentTarget;

    /// <summary>
    /// The current waypoints set to this Mech
    /// </summary>
    public Vector3[] Waypoints => waypoints.ToArray();

    /// <summary>
    /// Whether the Mech has a viable Target (doesn't mean that Target is non-null)
    /// </summary>
    public bool HasViableTarget => currentTarget != null || _timeGainingAggro >= _timeTillAggro;

    /// <summary>
    /// The NavMeshAgent component of this mech
    /// </summary>
    public NavMeshAgent NavMeshAgent => _navMeshAgent;

    /// <summary>
    /// The CombatBehaviour component of this mech
    /// </summary>
    public CombatBehaviour CombatBehviour => _combatBehaviour;

    /// <summary>
    /// Getter for FSM current state, refer to the public enum MechBehaviour.EState for State descriptions
    /// </summary>
    public TState CurrentState => _currentState;

    /// <summary>
    /// returns the MechType this MechBehaviour is instantiated from (read-only)
    /// </summary>
    public MechType MechType => _type;

    /// Whether this mech has a patrol path (read-only)
    /// </summary>
    public bool HasPatrol => _patrol.Length > 0;

    /// <summary>
    /// Whether this mech has a patrol path (read-only)
    /// </summary>
    public bool IsPlayerMech => _isPlayer;

    /********************
    *     EVENTS       *
    ********************/
    // Events declarations go here
    public delegate void MechStateChange(TState @new, TState old, float secondsInOld);

    /// <summary>
    /// Called when Mech velocity falls to zero
    /// </summary>
    public event System.Action StoppedMoving;

    /// <summary>
    /// Called when Mech velocity become non-zero
    /// </summary>
    public event System.Action StartedMoving;

    /// <summary>
    /// Called when a weapon is fired by the mech
    /// </summary>
    public event System.Action WeaponFired;

    /// <summary>
    /// Called when SetState() is called with a TState argument different than MechBehaviour._state
    /// </summary>
    public event MechStateChange StateChanged;
    
    /// <summary>
    /// The radius of this mech scaled based on its Transform.LocalScale
    /// </summary>
    public float ScaledRadius => _scaledRadius;

    /********************
    *     METHODS      *
    ********************/
    // Methods and functions
    private void Awake()
    {   
        waypoints = new();
        MechAwake(out bool is_player);
        _isPlayer = is_player;
        _onDeathSmoke = GetComponentInChildren<ParticleSystem>();

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _type.speed;
        _navMeshAgent.angularSpeed = _type.angularSpeed;
        _navMeshAgent.acceleration = _type.acceleration;
        _navMeshAgent.stoppingDistance = _type.stoppingDistance;
        _navMeshAgent.autoBraking = _type.autoBraking;
        _navMeshAgent.radius = _type.radius * .95f;
        _navMeshAgent.height = _type.height;
        _navMeshAgent.height = _type.height / 2f;
        _navMeshAgent.autoTraverseOffMeshLink = true;
        _navMeshAgent.agentTypeID = -1372625422;    // can be found in the baked nav mesh surface at the bottom of the editor window
        _navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        _navMeshAgent.autoRepath = true;
        
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.radius = _type.radius;
        capsuleCollider.height = _type.height;
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.mass = _type.mass;
        rigidbody.drag = _type.drag;
        rigidbody.angularDrag = _type.angularDrag;

        _combatBehaviour = GetComponent<CombatBehaviour>();
        _combatBehaviour.Initialize(_type.maxHealth);
        _combatBehaviour.defeatedAgent += OnDefeatedAgent;
        _combatBehaviour.death += OnDeath;
        _aggroMask = LayerMask.GetMask(!_isPlayer ? "Player" : "Enemy");
        gameObject.layer = LayerMask.NameToLayer(_isPlayer ? "Player" : "Enemy");
        gameObject.tag = "Mech";

        _weapons = new Weapon[_type.weapons.Length];
        _enageDistanceStart = 0;
        _engageDistanceFull = float.PositiveInfinity;
        for (int i = 0; i < _weapons.Length; ++i)
        {
            _weapons[i] = gameObject.AddComponent<Weapon>();
            _weapons[i].Instantiate(_type.weapons[i], this);
            float weapon_range = _weapons[i].weaponRange;
            if (weapon_range > _enageDistanceStart) _enageDistanceStart = weapon_range;
            if (weapon_range < _engageDistanceFull) _engageDistanceFull =  weapon_range;
        }
        _engageDistanceRetreat = _engageDistanceFull / 2f;
        SetState( HasPatrol ? TState.Patroling : TState.Idle);
        _timeTillAggro = 5f;
        _scaledRadius = _type.radius * Mathf.Max(transform.localScale.x, transform.localScale.z); // take the largest to be safe
        _scaledHalfHeight = _type.height * transform.localScale.y / 2f - _scaledRadius;
    }
    
    /// <summary>
    /// Called at the start of Awake(), used to define derived MechBehaviour and set _isPlayer
    /// </summary>
    protected abstract void MechAwake(out bool isPlayer);

    private void Update()
    {
        _currentStateDurationSeconds += Time.deltaTime;
        if (_currentState == TState.Dead) return;
        
        if (_isGainingAggro && currentTarget == null) _timeGainingAggro += Time.deltaTime;
        else if (!_isGainingAggro && currentTarget == null) _timeGainingAggro -= Time.deltaTime;


        var temp_target = FindBestVisibleTarget();
        var heard_enemy = FindBestHearableTarget();
        
        if (temp_target == null)
        {
            if (_timeGainingAggro >= _timeTillAggro) temp_target = heard_enemy;
            else if (heard_enemy != null) {
                _isGainingAggro = true;
            }
            else if (_isGainingAggro) {
                _isGainingAggro = false;
            }
        }

        MechUpdate(_currentState, temp_target, heard_enemy);

        bool is_moving = _navMeshAgent.velocity.sqrMagnitude > 0f; 
        if (is_moving != _isMoving)
        {
            if (is_moving) StartedMoving?.Invoke();
            else StoppedMoving?.Invoke();
            _isMoving = is_moving;
        }
    }
    
    /// <summary>
    /// Called withing MechBehaviour.Upate(), allows the definition of derived Mech state behaviour
    /// </summary>
    /// <param name="state"> Current Mech TState </param>
    /// <param name="tempTarget"> the closest enemy Mech within visible range </param>
    /// <param name="heardTarget"> the closest enemy Mech within hearable range (always equal to tempTarget when tempTarget != null) </param>
    protected abstract void MechUpdate(TState state, BaseMech tempTarget, BaseMech heardTarget);

    /// <summary>
    /// Handles firing logic for weapons to reduce wet code
    /// </summary>
    /// <returns>EEngagement enum to describe our current distance from the target</returns>
    protected EEngagement EngageTarget(BaseMech target)
    {
        float distance_to_target = Vector3.Distance(transform.position, target.transform.position);
        if (distance_to_target > _enageDistanceStart) return EEngagement.OutOfRange;
        else if (distance_to_target < _engageDistanceFull)
        {
            foreach (var weapon in _weapons)
            {
                if (target == null) break; // we killed our target
                if (target._currentState == TState.Dead) break; // target is dead
                FireAt(weapon, target);
            }
            if (_currentState != TState.Retreating)
                return distance_to_target < _engageDistanceRetreat ? EEngagement.TooClose : EEngagement.InRangeFull;
            else 
                // Add a little buffer to the retreat distance while retreating so that it doesn't flicker between the two states
                return distance_to_target < (_engageDistanceRetreat * 1.5f) ? EEngagement.TooClose : EEngagement.InRangeFull;
        }
        else
        {
            foreach (var weapon in _weapons)
            {
                if (weapon.weaponRange >= distance_to_target)
                {
                    if (target == null) break; // we killed our target
                    if (target._currentState == TState.Dead) break; // target is dead
                    FireAt(weapon, target);
                }
            }
            return EEngagement.InRangePartial;
        }
    }

    /// <summary>
    /// Wraps Physics.OverlapMech() for the exact dimensions of this Mech
    /// </summary>
    public bool OverlapCapsule(Vector3 position, out Collider[] hits, params string[] layerNames)
    {
        if (layerNames == null) 
            throw new NoNullAllowedException("layerNames argument of MechBehaviour.IsDirectionClear() is not allowed to be null, please specify collision layers names");

        Vector3 start = position + _scaledRadius * Vector3.up;
        Vector3 end = start + _scaledHalfHeight * Vector3.up * 2f;

        hits = Physics.OverlapCapsule(
            start, end, _scaledRadius,
            LayerMask.GetMask(layerNames)
        );
        return hits.Length != 0;
    }

    /// <summary>
    /// Wraps Physics.CapsuleCast() for the exact dimensions of this Mech
    /// </summary>
    public bool CapsuleCast(Vector3 position, Vector3 direction, out RaycastHit hitInfo, float maxDistance, params string[] layerNames)
    {
        if (layerNames == null) 
            throw new NoNullAllowedException("layerNames argument of MechBehaviour.IsDirectionClear() is not allowed to be null, please specify collision layers names");

        Vector3 start = position + _scaledRadius * Vector3.up;
        Vector3 end = start + _scaledHalfHeight * Vector3.up * 2f;

        return Physics.CapsuleCast(
            start, end, _scaledRadius, 
            direction,
            out hitInfo,
            maxDistance,
            LayerMask.GetMask(layerNames)
        );
    }

    /// <summary>
    /// Raycast's the body of the mech in the given direction for the given distance, returns whether there were any collisions (checks against the given layers)
    /// </summary>
    public bool IsDirectionClear(Vector3 direction, float maxDistance, params string[] layerNames)
    {
        return CapsuleCast(transform.position, direction, out RaycastHit hit, maxDistance, layerNames);
    }
    
    /// <summary>
    /// Raycast's the body of the mech in the given direction for the given distance, returns whether there were any collisions (checks against "Static" and "Enemy" Layers)
    /// </summary>
    public bool IsDirectionClear(Vector3 direction, float distance)
    {
        return IsDirectionClear(direction, distance, "Static", "Enemy");
    }

    /// <summary>
    /// Returns a MechBehaviour that best satisfies MechType.selectionAlg within MechType.sightRange, null if none
    /// </summary>
    public BaseMech FindBestVisibleTarget()
    {
        var aggro_mechs = FindAllVisibleEnemies();
        if (aggro_mechs.Length == 1) return aggro_mechs[0];
        else if (aggro_mechs.Length > 0) return _type.selectionAlg(this, aggro_mechs);
        else return null;
    }

    /// <summary>
    /// Returns a MechBehaviour that best satisfies MechType.selectionAlg within MechType.hearingRange, null if none
    /// </summary>
    public BaseMech FindBestHearableTarget()
    {
        var aggro_mechs = FindAllHearableEnemies();
        if (aggro_mechs.Length == 1) return aggro_mechs[0];
        else if (aggro_mechs.Length > 0) return _type.selectionAlg(this, aggro_mechs);
        else return null;
    }

    protected abstract void SetTarget(BaseMech target);


    /// <summary>
    /// Internal setter for FSM current state
    /// </summary>
    protected void SetState(TState newState)
    {
        if (newState == _currentState) return;
        // Debug.Log($"\"{gameObject.name}\" is now \"{newState}\"");
        _previousState = _currentState;
        _currentState = newState;
        StateChanged?.Invoke(_currentState, _previousState, _currentStateDurationSeconds);
        _currentStateDurationSeconds = 0f;
    }

    protected void SetIsUsingNavAgent(bool isUsingNavAgent)
    {
        if (isUsingNavAgent == _navMeshAgent.enabled) return;
        if (!isUsingNavAgent) SetDestination(transform.position, true);
        _navMeshAgent.enabled = isUsingNavAgent;
    }
    
    /// <summary>
    /// for internal setting only, if you're the player use a "Command" method
    /// </summary>
    protected void SetDestination(Vector3 newDest, bool overrideWaypoints)
    {
        if (_currentState == TState.Dead) return;
        if (!_navMeshAgent.enabled) SetIsUsingNavAgent(true);
        if (overrideWaypoints) 
        {   
            waypoints.Clear();
            waypoints.Add(newDest);
        }
        _navMeshAgent.destination = newDest;
    }

    protected void AppendWaypoint(Vector3 newWaypoint)
    {
        if (_currentState == TState.Dead) return;
        if (!_navMeshAgent.enabled) SetIsUsingNavAgent(true);
        waypoints.Add(newWaypoint);
    }

    /// <summary>
    /// Set's the destination as the position of the other mech while avoiding collision
    /// </summary>
    protected void SetDestination(BaseMech targetDestination)
    {
        var direction_to_self = (transform.position - targetDestination.transform.position).normalized;
        SetDestination(targetDestination.transform.position + direction_to_self * (_scaledRadius + targetDestination._scaledRadius), true);
    }

    /// <summary>
    /// Fires At target (do your own distance check first)
    /// </summary>
    protected void FireAt(Weapon weapon, BaseMech target)
    {
        if (weapon.Fire(target.gameObject)) WeaponFired?.Invoke();
    }

    /// <summary>
    /// Returns an array of all aggro mechs within MechType.sightRange
    /// </summary>
    /// <returns></returns>
    public BaseMech[] FindAllVisibleEnemies()
    {
        var collisions = Physics.OverlapSphere(transform.position, _type.sightRange, _aggroMask);
        var aggro_mechs = new List<BaseMech>();
        foreach (var collision in collisions)
        {
            if (collision.gameObject.CompareTag("Mech")) 
            {
                var mech = collision.GetComponent<BaseMech>();
                if (mech._currentState != TState.Dead) aggro_mechs.Add(mech);
            }
        }
        return aggro_mechs.ToArray();
    }

    /// <summary>
    /// Returns an array of all aggro mechs within MechType.hearingRange
    /// </summary>
    /// <returns></returns>
    public BaseMech[] FindAllHearableEnemies()
    {
        var collisions = Physics.OverlapSphere(transform.position, _type.hearingRange, _aggroMask);
        var aggro_mechs = new List<BaseMech>();
        foreach (var collision in collisions)
        {
            if (collision.gameObject.CompareTag("Mech")) 
            {
                var mech = collision.GetComponent<BaseMech>();
                if (mech._currentState != TState.Dead) aggro_mechs.Add(mech);
            }
        }
        return aggro_mechs.ToArray();
    }

    /// <summary>
    /// Whether this mech is at its nav destination
    /// </summary>
    public bool HasReachedDestination(){
        if (_navMeshAgent.isActiveAndEnabled) 
        {
            return _navMeshAgent.remainingDistance <= _type.radius + .8f;
        }
        return false;
    }

    /// <summary>
    /// A delegate for the CombatBehaviour.healthModified event
    /// Already called internally, you don't need to call it if damage is done through CombatBehaviour
    /// </summary>
    public virtual void OnDamaged(TPropertyContainer health, TCombatContext context)
    {
    }

    /// <summary>
    /// A delegate for the CombatBehaviour.defeatedAgent event
    /// Already called internally, you don't need to call it if damage is done through CombatBehaviour
    /// </summary>
    public virtual void OnDefeatedAgent(CombatBehaviour behaviour, TCombatContext context)
    {
    }

    /// <summary>
    /// A delegate for the CombatBehaviour.death event
    /// Already called internally, you don't need to call it if damage is done through CombatBehaviour
    /// </summary>
    public virtual void OnDeath(TCombatContext context)
    {
        _onDeathSmoke.Play(true); // TODO: I want this out of here but that may just be my brain - Jeff
        SetState(TState.Dead);
    }

    public virtual void OnTargetDeath(TCombatContext context)
    {
        SetTarget(null);
    }

    /// <summary>
    /// How to deal damage if you don't want to have to look at CombatBehaviour
    /// </summary>
    public void HitMech(BaseMech mech, float damage)
    {
        CombatBehaviour.CombatEvent(_combatBehaviour, mech._combatBehaviour, TCombatContext.BasicAttack(damage));
    }
}

