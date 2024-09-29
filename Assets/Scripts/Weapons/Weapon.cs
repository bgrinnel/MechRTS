using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private WeaponScriptable weaponStats;
    [SerializeField]
    private float weaponRange
    {
        get { return weaponStats.weaponRange; }
        set { weaponStats.weaponRange = value; }
    }
    [SerializeField]
    private float weaponDamage
    {
        get { return weaponStats.weaponDamage; }
        set { weaponStats.weaponDamage = value; }
    }
    [SerializeField]
    private float weaponSalvoReload
    {
        get { return weaponStats.weaponSalvoReload; }
        set { weaponStats.weaponDamage = value; }
    }

    [SerializeField]
    private float weaponSalvoLength
    {
        get { return weaponStats.weaponSalvoLength; }
        set { weaponStats.weaponDamage = value; }
    }

    [SerializeField]
    private float weaponShotReload
    {
        get { return weaponStats.weaponShotReload; }
        set { weaponStats.weaponDamage = value; }
    }

    [SerializeField]
    private WeaponScriptable.WeaponType weaponType
    {
        get { return weaponStats.weaponType; }
        set { weaponStats.weaponType = value; }
    }

    public void Fire()
    {

    }
}
