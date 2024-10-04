using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private WeaponScriptable weaponStats;
    [SerializeField]
    public float weaponRange
    {
        get { return weaponStats.weaponRange; }
        private set { weaponStats.weaponRange = value; }
    }
    [SerializeField]
    public float weaponDamage
    {
        get { return weaponStats.weaponDamage; }
        private set { weaponStats.weaponDamage = value; }
    }
    [SerializeField]
    public float weaponSalvoReload
    {
        get { return weaponStats.weaponSalvoReload; }
        private set { weaponStats.weaponDamage = value; }
    }

    [SerializeField]
    public float weaponSalvoLength
    {
        get { return weaponStats.weaponSalvoLength; }
        private set { weaponStats.weaponDamage = value; }
    }

    [SerializeField]
    public float weaponShotReload
    {
        get { return weaponStats.weaponShotReload; }
        private set { weaponStats.weaponDamage = value; }
    }

    [SerializeField]
    public WeaponScriptable.WeaponType weaponType
    {
        get { return weaponStats.weaponType; }
        private set { weaponStats.weaponType = value; }
    }

    [SerializeField]
    public int ammo
    {
        get { return weaponStats.ammo; }
        private set { weaponStats.ammo = value; }
    }

    [SerializeField]
    public bool homing
    {
        get { return weaponStats.homing; }
        private set { weaponStats.homing = value; }
    }

    public void Fire(GameObject target = null)
    {
        switch (weaponType)
        {
            case WeaponScriptable.WeaponType.DirectEffect:
                break;
            case WeaponScriptable.WeaponType.Kenetic:
                KeneticSalvo();
                break;
            case WeaponScriptable.WeaponType.Missile:
                if (homing) {MissileSalvo(target); }
                else { MissileSalvo(); }
                break;
        }
    }

    private void KeneticSalvo()
    {
        var projectile = Instantiate(weaponStats.projectilePrefab);
        projectile.GetComponent<Projectile>().SetMode(0);
        projectile.GetComponent<Projectile>().InitializeProjectile(weaponDamage, weaponStats.projectileSpeed);
    }

    private void MissileSalvo(GameObject target = null)
    {
        var missile = Instantiate(weaponStats.projectilePrefab);
        missile.GetComponent<Projectile>().SetMode(1, homing);
        missile.GetComponent<Projectile>().InitializeProjectile(weaponDamage, weaponStats.projectileSpeed, target);
    }

    private void DirectEffect(GameObject target = null)
    {

    }

    private void Area()
    {

    }
}
