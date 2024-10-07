using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private MechBehavior owningMech;
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

    public GameObject target;
    private bool reload = true;

    private void Update()
    {
        
    }

    public IEnumerator Fire(GameObject target = null)
    {
        Debug.Log("Fire");
        reload = false;
        for (int i = 0; i < weaponStats.weaponSalvoLength; i++) 
        {
            switch (weaponType)
            {
                case WeaponScriptable.WeaponType.DirectEffect:
                    DirectEffect(target);
                    break;
                case WeaponScriptable.WeaponType.Kenetic:
                    KeneticSalvo();
                    break;
                case WeaponScriptable.WeaponType.Missile:
                    if (homing) { MissileSalvo(target); }
                    else { MissileSalvo(); }
                    break;
            }
            if(i < weaponStats.weaponSalvoLength - 1)
            {
                yield return new WaitForSeconds(weaponStats.weaponShotReload);
            }
        }
        yield return new WaitForSeconds(weaponStats.weaponSalvoReload);
        reload = true;
    }

    private void KeneticSalvo()
    {
        var projectile = Instantiate(weaponStats.projectilePrefab, transform.position,transform.rotation);
        projectile.GetComponent<Projectile>().SetMode(0);
        projectile.GetComponent<Projectile>().InitializeProjectile(weaponDamage, weaponStats.projectileSpeed, owningMech);
    }

    private void MissileSalvo(GameObject target = null)
    {
        var missile = Instantiate(weaponStats.projectilePrefab, transform.position, transform.rotation);
        missile.GetComponent<Projectile>().SetMode(1, homing);
        missile.GetComponent<Projectile>().InitializeProjectile(weaponDamage, weaponStats.projectileSpeed, owningMech, target);
        if (homing)
        {
            missile.GetComponent<Projectile>().InitializeHoming(weaponStats.minDistancePredict, weaponStats.minDistancePredict, weaponStats.maxTimePrediction, weaponStats.projectileRotationSpeed);
        }
    }

    private void DirectEffect(GameObject target)
    {
        owningMech.HitMech(target.gameObject.GetComponent<MechBehavior>(), weaponDamage);
    }

    private void Area()
    {

    }

    public void SetTarget(GameObject designatedTarget)
    {
        target = designatedTarget;
    }

    public Weapon (WeaponScriptable scriptable, MechBehavior mech)
    {
        owningMech = mech;
        weaponStats = scriptable;
        weaponRange = scriptable.weaponRange;
        weaponDamage = scriptable.weaponDamage;
        weaponSalvoReload = scriptable.weaponSalvoReload;
        weaponSalvoLength = scriptable.weaponSalvoLength;
        weaponShotReload = scriptable.weaponShotReload;
        weaponType = scriptable.weaponType;
        ammo = scriptable.ammo;
        homing = scriptable.homing;
    }
}
