using UnityEditor.UIElements;
using UnityEngine;

/// <summary>
///  The combat class, defines all attributes of combat; damage, health, armor, etc. etc.
///  Statically handles combat events so that they're centralized and all invokations are handled internally
///  Subscribe to our combat delegates if you want to act on those events
/// </summary>
public class CombatBehaviour : MonoBehaviour
{
    /// <summary>
    /// Defines the method signature for an effect in the CriticalHitTable
    /// </summary>
    public delegate void CriticalHitEffect(BaseMech hitMech);
    
    public delegate void PostEvent(TCombatContext context);

    public delegate void AgentPostEvent(CombatBehaviour agent, TCombatContext context);

    /// <summary>
    /// The base delegate definition for all CombatProperty related events, allows for modification of the property
    /// </summary>
    /// <param name="property"> the property that's been modified </param>
    /// <param name="container"> the (reference-of) container for that property at Invokation </param>
    public delegate void PropertyChange(in TPropertyContainer container, TCombatContext context);

    /// <summary>
    /// The combat function, handles all combat events (eg anything where damage is (attempted to be) done)
    /// </summary>
   public static void CombatEvent(CombatBehaviour instigator, CombatBehaviour target, TCombatContext context)
   {
        // TODO: wire in all our new mech curves
        if (target == null) return;
        var target_health = target.GetHealth();
        if (target_health.current <= target_health.min) return;
        
        context.instigator = instigator;
        context.target = target;

        float fire_distance = Vector3.Distance(instigator.transform.position, target.transform.position);
        WeaponScriptable instigator_weapon = context.weaponType;
        MechType insitigator_type = instigator.owningMech.MechType;
        MechType target_type = target.owningMech.MechType;

        // calcuate hit chance
        float hit_chance = instigator_weapon.accuracyOverRange.Evaluate(fire_distance);
        float target_velocity = target.owningMech.Velocity;
        hit_chance += target_type.hitRateReductionOverSpeed.Evaluate(target.owningMech.Velocity); // this is expected to be negative
        
        AnimationCurve flankHitRateOverDistance;
        var hitDirection = (instigator.transform.position - target.transform.position).normalized;
        if (Vector3.Dot(hitDirection, target.transform.forward) > 0.5f)         // Front
        {
            flankHitRateOverDistance = target_type.frontFlankHitRateAddOverDistance;
        }
        else if (Vector3.Dot(hitDirection, -target.transform.forward) > 0.5f)   // Back
        {
            flankHitRateOverDistance = target_type.rearFlankHitRateAddOverDistance;
            // Debug.Log("Hit on Back Armor");
        }
        else if (Vector3.Dot(hitDirection, target.transform.right) > 0.5f)      // Right side
        {
            flankHitRateOverDistance = target_type.rightFlankHitRateAddOverDistance;
            // Debug.Log("Hit on Right Side Armor");
        }
        else// (Vector3.Dot(hitDirection, -target.transform.right) > 0.5f)     // Left side
        {
            flankHitRateOverDistance = target_type.leftFlankHitRateAddOverDistance;
            // Debug.Log("Hit on Left Side Armor");
        }
        if (Vector3.Dot(hitDirection, target.transform.up) > 0.5f)         // Top, non-exculsive to the other angles (for now)
        {
            // Debug.Log("Hit on Top Armor");
        }
        hit_chance += flankHitRateOverDistance.Evaluate(fire_distance);
        // hit_chance += context.weaponType.hitChanceOverSpeed() // TODO: continue

        // TODO: process coverType's in an array within 'context'

        // roll hit chance
        if (Random.Range(0f, 100f) < hit_chance) // SUCCESSFUL HIT
        {   
            float damage = context.weaponType.weaponDamage;

            // TODO: process a crit table of effects and crit chance

            target_health.current -= damage;
            target_health = target.SetHealth(target_health.current,  context);
            // Debug.Log($"{target.gameObject.name}'s Health fell to {target_health.current}");
            if (target_health.current <= target_health.min)
            {
                if (instigator != null) instigator.defeatedAgent?.Invoke(target, context);
                target.death?.Invoke(context);
            }
        }
        else                                    // MISSED
        {

        }
        
   }
   
    /// <summary>
    /// Invoked when this CombatBehaviour's Health changes
    /// </summary>
    public PropertyChange healthModified;
    
    /// <summary>
    /// Invoked when _healthContainer.current falls below _healthContainer.min, before anything is done to the operating agent
    /// </summary>
    public PropertyChange healthDepleted;

    /// <summary>
    /// Invoked by static CombatBehaviour.CombatEvent() when this CombatBehaviour's Health reaches min
    /// </summary>
    public PostEvent death;

    /// <summary>
    /// Invoked by static CombatBehaviour.CombatEvent() when this CombatBehaviour instigates a combat event that kills "agent"
    /// Invoked before "agent"'s Death invokation
    /// </summary>
    public AgentPostEvent defeatedAgent;

    private TPropertyContainer _healthContainer;

    // component references
    private BaseMech owningMech;

    public void Initialize(float maxHealth)
    {
        _healthContainer = new TPropertyContainer(maxHealth);
        owningMech = GetComponent<BaseMech>();
        if (owningMech == null) Debug.LogError("CombatBehavior doesn't have a supporting BaseMech component within its parent object");
    }
    
    public TPropertyContainer GetHealth() { return _healthContainer; }

    public TPropertyContainer SetHealth(float newHealth, TCombatContext context) 
    {
        if (newHealth == _healthContainer.current) return _healthContainer;
        _healthContainer.current = newHealth;
        healthModified?.Invoke(_healthContainer, context);
        if (_healthContainer.current <= _healthContainer.min)
        {
            healthDepleted?.Invoke(_healthContainer, context);
        }
        return _healthContainer;
    }
}