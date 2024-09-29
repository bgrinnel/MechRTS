using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponStats")]
public class WeaponScriptable : ScriptableObject
{
    public enum WeaponType { Kenetic, Missile, Area, DirectEffect }
    public WeaponType weaponType = WeaponType.Kenetic;
    public float weaponRange;
    public float weaponDamage;
    public float weaponSalvoReload;
    public float weaponSalvoLength;
    public float weaponShotReload;
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public int ammo;
    public bool homing;
}

