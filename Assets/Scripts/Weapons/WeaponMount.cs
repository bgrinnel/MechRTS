using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponMount : MonoBehaviour
{
    [SerializeField]
    private float _leftRotationLimit;
    [SerializeField] 
    private float _rightRotationLimit;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private Weapon _mountedWeapon;

    public GameObject _target;
   // public float
    public void Aim()
    {
        Vector3 targetPosition = new Vector3(0,0,0);
        if((_mountedWeapon.weaponType == WeaponScriptable.WeaponType.Missile || _mountedWeapon.weaponType == WeaponScriptable.WeaponType.Kenetic) && !_mountedWeapon.homing)
        {
            
        }
        else
        {
            targetPosition = _target.transform.position;
        }
    }

    public void SetTarget(GameObject target)
    {
        _target = target;
    }
    public WeaponMount(WeaponMountScriptable weaponMountStats, WeaponScriptable scriptable, MechBehavior _mechBehavior)
    {
        _leftRotationLimit = weaponMountStats.leftRorationLimit;
        _rightRotationLimit = weaponMountStats.rightRorationLimit;
        _rotationSpeed = weaponMountStats.rorationSpeed;
        // _mountedWeapon = new Weapon(scriptable, _mechBehavior);
    }

    private Vector3 LeadPosition(Rigidbody owningMech, Rigidbody targetMech)
    {
        Vector3 ownPosition = owningMech.position;
        Vector3 ownVelocity = owningMech.velocity;
        Vector3 targetPosition = targetMech.position;
        Vector3 targetVelocity = targetMech.velocity;

        Vector3 displacement = targetPosition - ownPosition;
        Vector3 relativeVelocity = targetVelocity - ownVelocity;

        float distance = displacement.magnitude;
        float relativeSpeed = relativeVelocity.magnitude;

        float timeToImpact = distance / _mountedWeapon.weaponStats.projectileSpeed;

        Vector3 predictionPosition = targetPosition + targetVelocity * timeToImpact;

        return predictionPosition;
    }
}
