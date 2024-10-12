using UnityEngine;

/// <summary>
///  The combat class, defines all attributes of combat; damage, health, armor, etc. etc.
///  Statically handles combat events so that they're centralized and all invokations are handled internally
///  Subscribe to our combat delegates if you want to act on those events
/// </summary>
public class CombatBehaviour : MonoBehaviour
{
    public delegate void Message(TCombatContext context);

    public delegate void AgentMessage(CombatBehaviour agent, TCombatContext context);

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
        if (target == null) return;
        var target_health = target.GetHealth();
        if (target_health.current <= target_health.min) return;
        
        context.instigator = instigator;
        context.target = target;
        target_health.current -= context.damage.current;
        target_health = target.SetHealth(target_health.current,  context);
        Debug.Log($"{target.gameObject.name}'s Health fell to {target_health.current}");
        if (target_health.current <= target_health.min)
        {
            if (instigator != null) instigator.defeatedAgent?.Invoke(target, context);
            target.death?.Invoke(context);
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
    public Message death;

    /// <summary>
    /// Invoked by static CombatBehaviour.CombatEvent() when this CombatBehaviour instigates a combat event that kills "agent"
    /// Invoked before "agent"'s Death invokation
    /// </summary>
    public AgentMessage defeatedAgent;

    private TPropertyContainer _healthContainer;

    public void Initialize(float maxHealth)
    {
        _healthContainer = new TPropertyContainer(maxHealth);
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