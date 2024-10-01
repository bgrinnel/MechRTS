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

    [SerializeField]
    private int ammo
    {
        get { return weaponStats.ammo; }
        set { weaponStats.ammo = value; }
    }

    [SerializeField]
    private bool homing
    {
        get { return weaponStats.homing; }
        set { weaponStats.homing = value; }
    }

    public void Fire()
    {
        switch (weaponType)
        {
            case WeaponScriptable.WeaponType.DirectEffect:
                break;
            case WeaponScriptable.WeaponType.Kenetic:
                break;
        }
    }

    private void KeneticSalvo(GameObject target)
    {
        var projectile = Instantiate(weaponStats.projectilePrefab);
        projectile.GetComponent<Projectile>().SetMode(0);
        projectile.GetComponent<Projectile>().InitializeProjectile(weaponDamage, weaponStats.projectileSpeed, target);
    }

    private void MissileSalvo(GameObject target)
    {
        var missile = Instantiate(weaponStats.projectilePrefab);
        missile.GetComponent<Projectile>().SetMode(1, homing);
        missile.GetComponent<Projectile>().InitializeProjectile(weaponDamage, weaponStats.projectileSpeed, target);

    }
}
