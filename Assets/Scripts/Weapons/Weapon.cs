using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private WeaponScriptable weaponStats;
    public float weaponRange
    {
        get { return weaponStats.weaponRange; }
        set { weaponStats.weaponRange = value; }
    }
    public float weaponDamage
    {
        get { return weaponStats.weaponDamage; }
        set { weaponStats.weaponDamage = value; }
    }
    public float weaponRof
    {
        get { return weaponStats.weaponRatesOfFire; }
        set { weaponStats.weaponDamage = value; }
    }

    public WeaponScriptable.WeaponType weaponType
    {
        get { return weaponStats.weaponType; }
        set { weaponStats.weaponType = value; }
    }

    public void Fire()
    {

    }
}
