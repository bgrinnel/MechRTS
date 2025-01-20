using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private BaseMech owningMech;

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
    [SerializeField]
    public float accBase
    {
        get { return weaponStats.baseAccuracy; }
        private set { weaponStats.baseAccuracy = value; }
    }
    [SerializeField]
    public float accIncrement
    {
        get { return weaponStats.AccuracyIncrement; }
        private set { weaponStats.AccuracyIncrement = value; }
    }
    [SerializeField]
    public float accFactor
    {
        get { return weaponStats.AccuracyFactor; }
        private set { weaponStats.AccuracyFactor = value; }
    }

    public GameObject target;
    private bool _isReloading;
    private int _salvoIdx;
    private Coroutine _reloadTimer;

    public bool Fire(GameObject target)
    {
        // Debug.Log($"{weaponStats.name} firing");
        if (_isReloading) return false;
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
        return true;
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
        // CoverType[] covers_in_fire;

        // TODO: raycast to see what cover we would hit with a shot, multiple weighted casts if needed
        // append those cover objects and weights to the 

        owningMech.HitMech(target.gameObject.GetComponent<BaseMech>(), weaponStats); // TODO: add the ability to process an array of coverTypes to the end of HitMech()
    }

    private void Area()
    {

    }

    public void SetTarget(GameObject designatedTarget)
    {
        target = designatedTarget;
    }

    public void Instantiate(WeaponScriptable scriptable, BaseMech mech)
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

    public float GetProjectileSpeed()
    {
        return weaponStats.projectileSpeed;
    }

    public bool InRange(GameObject target)
    {
        if (target != null)
        {
            float distanceToTarget = GetTargetDistance(target);
            if (distanceToTarget > weaponRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public float GetTargetDistance(GameObject target)
    {
        var horiTarget = new Vector3(target.transform.position.x, 0, target.transform.position.z);
        var horiWeapon = new Vector3(transform.position.x, 0, target.transform.position.z);
        var distanceToTarget = Vector3.Distance(horiWeapon, horiTarget);
        return distanceToTarget;
    } 

    public float AccCalculator(GameObject target)
    {
        if (InRange(target))
        {
            var distanceToTarget = GetTargetDistance(target);
            var disatnceFromRange = weaponRange - distanceToTarget;
            var accuracy = accBase + accIncrement * ((int)disatnceFromRange / accFactor);
            return accuracy;
        }
        else
        {
            return 0;
        }
    }

    private bool HitCheck(float weaponAccuracy)
    {
        var acc = Random.Range(0, 100);
        if (acc < weaponAccuracy)
        {
            return true ;
        }
        else
        {
            return false;
        }
    }
}
