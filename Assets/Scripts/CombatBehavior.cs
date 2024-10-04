using UnityEngine;

/// <summary>
///  The combat class, defines all attributes of combat; damage, health, armor, etc. etc.
///  Statically handles combat events so that they're centralized and all invokations are handled internally
///  Subscribe to our combat delegates if you want to act on those events
/// </summary>
public class CombatBehaviour : MonoBehaviour
{
    public delegate void Message();

    public delegate void AgentMessage(CombatBehaviour agent);

    /// <summary>
    /// The base delegate definition for all CombatProperty related events, allows for modification of the property
    /// </summary>
    /// <param name="property"> the property that's been modified </param>
    /// <param name="container"> the (reference-of) container for that property at Invokation </param>
    public delegate void PropertyChange(EPropertyType property, in TPropertyContainer container);

    /// <summary>
    /// The combat function, handles al
    /// </summary>
    /// <param name="instigator"></param>
    /// <param name="target"></param>
    /// <param name="context"></param>
   public static void CombatEvent(CombatBehaviour instigator, CombatBehaviour target, TCombatContext context)
   {
        if (target == null) return;
        var target_health = target.GetHealth();
        if (target_health.Current <= target_health.Min) return;
        
        target_health.Current -= context.Damage.Current;
        target_health = target.SetHealth(target_health.Current);
        if (target_health.Current <= target_health.Min)
        {
            if (instigator != null) instigator.defeatedAgent?.Invoke(target);
            target.death?.Invoke();
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

    public TPropertyContainer SetHealth(float newHealth) 
    {
        if (newHealth == _healthContainer.Current) return _healthContainer;
        _healthContainer.Current = newHealth;
        healthModified?.Invoke(EPropertyType.Health, _healthContainer);
        if (_healthContainer.Current <= _healthContainer.Min)
        {
            healthDepleted?.Invoke(EPropertyType.Health, _healthContainer);
        }
        return _healthContainer;
    }
}