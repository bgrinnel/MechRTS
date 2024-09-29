using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "WeaponStats")]
public class WeaponScriptable : MonoBehaviour
{
    public float weaponRange;
    public float weaponDamage;
    public float weaponRatesOfFire;
    public enum WeaponType { Kenetic, Missile, Area, DirectEffect }
    public WeaponType weaponType;
    public GameObject projectilePrefab;
    public float projectileSpeed; 
}

