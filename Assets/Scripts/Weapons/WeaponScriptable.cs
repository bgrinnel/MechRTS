using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponStats")]
public class WeaponScriptable : ScriptableObject
{
    public enum WeaponType { Kenetic, Missile, Area, DirectEffect }
    public WeaponType weaponType = WeaponType.Kenetic;
    public float weaponMaxDamage;
    public float weaponMinDamage;
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
    public float effectiveRange;
    public float maximumRange;
    public float minimumRange;
    public float weaponMaxAccuracy;
    public float weaponMinAccuracy;
}

