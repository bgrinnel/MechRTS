using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///  Defines Combat Behvaior, Must Be Nannied by another MonoBehavior (some kind of state machine)
/// </summary>
public partial class CombatBehaviour : MonoBehaviour
{
    public delegate void CombatMessage();

    public delegate void CombatAgentMessage(CombatBehaviour agent);

    // ===== delegates =====
    /// <summary>
    /// The base delegate definition for all CombatProperty related events
    /// </summary>
    /// <param name="property"> the property that's been modified </param>
    /// <param name="container"> the (reference-of) container for that property at Invokation </param>
    public delegate void CombatPropertyChanged(EPropertyType property, in TPropertyContainer container);

   public static void CombatEvent(CombatBehaviour instigator, CombatBehaviour target, TCombatContext context)
   {
        if (target == null) return;
        var target_health = target.GetHealth();
        if (target_health.Current <= target_health.Min) return;
        
        target_health.Current -= context.Damage.Current;
        target_health = target.SetHealth(target_health.Current);
        if (target_health.Current <= target_health.Min)
        {
            if (instigator != null) instigator.DefeatedAgent?.Invoke(target);
            target.Death?.Invoke();
        }
   }
}