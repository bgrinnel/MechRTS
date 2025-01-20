using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponStats")]
public class WeaponScriptable : ScriptableObject
{
    public enum WeaponType { Kenetic, Missile, Area, DirectEffect }
    public WeaponType weaponType = WeaponType.Kenetic;

    public AnimationCurve accuracyOverRange;
    public AnimationCurve damagePercentageOverRange;
    public AnimationCurve damageMitigationOverTargetSpeed;
    public AnimationCurve hitChanceOverSpeed;

    [Range(0f,360f)]
    public float weaponArcDegrees;

    
    public float weaponRange;
    public float weaponDamage;
    public float weaponSalvoReload;
    public int weaponSalvoLength;
    public float weaponShotReload;
    public int ammo;
    public bool homing;
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public float projectileRotationSpeed;
    public float maxDistancePredict = 100;
    public float minDistancePredict = 5;
    public float maxTimePrediction = 5;
    public float baseAccuracy;
    public float AccuracyIncrement;
    public float AccuracyFactor;
}

