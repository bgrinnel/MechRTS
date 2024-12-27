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
    public float weaponMaxDamage
    {
        get { return weaponStats.weaponMaxDamage; }
        private set { weaponStats.weaponMaxDamage = value; }
    }

    [SerializeField]
    public float weaponMinDamage
    {
        get { return weaponStats.weaponMinDamage; }
        private set { weaponStats.weaponMinDamage = value; }
    }

    [SerializeField]
    public float weaponMaxRange
    {
        get { return weaponStats.maximumRange; }
        private set { weaponStats.maximumRange = value; }
    }

    [SerializeField]
    public float weaponMinRange
    {
        get { return weaponStats.minimumRange; }
        private set { weaponStats.minimumRange = value; }
    }

    [SerializeField]
    public float weaponEffectiveRange
    {
        get { return weaponStats.effectiveRange; }
        private set { weaponStats.effectiveRange = value; }
    }
    [SerializeField]
    private float weaponMaxAcc
    {
        get { return weaponStats.weaponMaxAccuracy; }
        set { weaponStats.weaponMaxAccuracy = value; }
    }

    [SerializeField]
    private float weaponMinAcc
    {
        get { return weaponStats.weaponMinAccuracy; }
        set { weaponStats.weaponMinAccuracy = value; }
    }
    [SerializeField]
    private AnimationCurve accuracyCurve
    {
        get { return weaponStats.weaponAccuracyCurve; }
        set { weaponStats.weaponAccuracyCurve = value; }
    }
    [SerializeField]
    private AnimationCurve damageCurve
    {
        get { return weaponStats.weaponDamageCurve; }
        set { weaponStats.weaponDamageCurve = value; }
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
        projectile.GetComponent<Projectile>().InitializeProjectile(CalculateDamage(target), weaponStats.projectileSpeed, owningMech);
    }

    private void MissileSalvo(GameObject target = null)
    {
        var missile = Instantiate(weaponStats.projectilePrefab, transform.position, transform.rotation);
        missile.GetComponent<Projectile>().SetMode(1, homing);
        missile.GetComponent<Projectile>().InitializeProjectile(CalculateDamage(target), weaponStats.projectileSpeed, owningMech, target);
        if (homing)
        {
            missile.GetComponent<Projectile>().InitializeHoming(weaponStats.minDistancePredict, weaponStats.minDistancePredict, weaponStats.maxTimePrediction, weaponStats.projectileRotationSpeed);
        }
    }

    private void DirectEffect(GameObject target)
    {
        owningMech.HitMech(target.gameObject.GetComponent<BaseMech>(), CalculateDamage(target));
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
        weaponMaxDamage = scriptable.weaponMaxDamage;
        weaponMinDamage = scriptable.weaponMinDamage;
        weaponMaxRange = scriptable.maximumRange;
        weaponMinRange = scriptable.minimumRange;
        weaponEffectiveRange = scriptable.effectiveRange;
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

    public float CalculateDamage(GameObject target)
    {
        if (InRange(target))
        {
            float distanceToTarget = GetTargetDistance(target);
            if (distanceToTarget > weaponEffectiveRange)
            {
                float xIndex = (weaponMaxRange - distanceToTarget)/(weaponMaxRange - weaponEffectiveRange);
                float indexInverse = 1f - xIndex;
                var effctiveDamage = damageCurve.Evaluate(indexInverse) * (weaponMaxDamage - weaponMinDamage);
                return effctiveDamage;
            }
            else if (distanceToTarget < weaponEffectiveRange)
            {
                return weaponMaxDamage;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }

    public bool InRange(GameObject target)
    {
        if (target != null)
        {
            float distanceToTarget = GetTargetDistance(target);
            if (distanceToTarget > weaponMaxRange)
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
            var weaponAccuracy = 0f;
            if (distanceToTarget > weaponEffectiveRange)
            {
                float xIndex = (weaponMaxRange - distanceToTarget) / (weaponMaxRange - weaponEffectiveRange);
                float indexInverse = 1f - xIndex;
                var effctiveAccuracy = damageCurve.Evaluate(indexInverse) * (weaponMaxAcc - weaponMinAcc);
                weaponAccuracy = effctiveAccuracy;
            }
            else
            {
                weaponAccuracy = weaponMaxAcc;
            }
            var accuracy = weaponAccuracy;
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
