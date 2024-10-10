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
    public float SalvoReloadSec
    {
        get { return weaponStats.weaponSalvoReload; }
        private set { weaponStats.weaponSalvoReload = value; }
    }

    [SerializeField]
    public float SalvoLength
    {
        get { return weaponStats.weaponSalvoLength; }
        private set { weaponStats.weaponSalvoLength = (int)value; }
    }

    [SerializeField]
    public float ShotReloadSec
    {
        get { return weaponStats.weaponShotReload; }
        private set { weaponStats.weaponShotReload = value; }
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
    private bool _isReloading;
    private int _salvoIdx;
    private Coroutine _reloadTimer;

    public void Fire(GameObject target)
    {
        // Debug.Log($"{weaponStats.name} firing");
        if (_isReloading) return;
        switch (weaponType)
        {
            case WeaponScriptable.WeaponType.DirectEffect:
                DirectEffect(target);
                break;
            case WeaponScriptable.WeaponType.Kenetic:
                KeneticSalvo(target);
                break;
            case WeaponScriptable.WeaponType.Missile:
                if (homing) { MissileSalvo(target); }
                else { MissileSalvo(); }
                break;
        }
        if (++_salvoIdx < SalvoLength)
        {
            _reloadTimer = StartCoroutine(PauseWeaponFor(ShotReloadSec));
        }
        else
        {
            _salvoIdx = 0;
            _reloadTimer = StartCoroutine(PauseWeaponFor(SalvoReloadSec));
        }
    }

    private IEnumerator PauseWeaponFor(float time)
    {
        _isReloading = true;
        yield return new WaitForSeconds(time);
        _isReloading = false;
    }

    private void KeneticSalvo(GameObject target)
    {
        var projectile = Instantiate(weaponStats.projectilePrefab, transform.position,transform.rotation);
        projectile.transform.LookAt(target.transform);
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

    public void Instantiate(WeaponScriptable scriptable, MechBehavior mech)
    {
        owningMech = mech;
        weaponStats = scriptable;
        weaponRange = scriptable.weaponRange;
        weaponDamage = scriptable.weaponDamage;
        SalvoReloadSec = scriptable.weaponSalvoReload;
        SalvoLength = scriptable.weaponSalvoLength;
        ShotReloadSec = scriptable.weaponShotReload;
        weaponType = scriptable.weaponType;
        ammo = scriptable.ammo;
        homing = scriptable.homing;
        _isReloading = false;
        _salvoIdx = 0;
    }
}
