using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///  Defines "comabt behaviour", must be nannied by another MonoBehavior (some kind of state machine)
/// </summary>
public partial class CombatBehaviour : MonoBehaviour
{
    /// <summary>
    /// Invoked when this CombatBehaviour's Health changes
    /// </summary>
    public CombatPropertyChanged HealthModified;
    
    /// <summary>
    /// Invoked when this CombatBehaviour's Health reaches 0, before anything is done to the operating agent
    /// </summary>
    public CombatPropertyChanged HealthDepleted;

    /// <summary>
    /// Invoked by static CombatBehaviour.CombatEvent() when this CombatBehaviour's Health reaches min
    /// </summary>
    public CombatMessage Death;

    /// <summary>
    /// Invoked by static CombatBehaviour.CombatEvent() when this CombatBehaviour instigates a combat event that kills "agent"
    /// Invoked before agent's Death delegate
    /// </summary>
    public CombatAgentMessage DefeatedAgent;

    private TPropertyContainer HealthContainer;

    public void Initialize(float MaxHealth)
    {
        HealthContainer = new TPropertyContainer(MaxHealth);
    }
    
    public TPropertyContainer GetHealth() { return HealthContainer; }

    public TPropertyContainer SetHealth(float newHealth) 
    {
        if (newHealth == HealthContainer.Current) return HealthContainer;
        HealthContainer.Current = newHealth;
        if (HealthContainer.Current > HealthContainer.Min)
        {
            HealthModified?.Invoke(EPropertyType.Health, HealthContainer);
        }
        else
        {
            HealthDepleted?.Invoke(EPropertyType.Health, HealthContainer);
        }
        return HealthContainer;
    }
}