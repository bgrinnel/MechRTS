using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponMount : MonoBehaviour
{
    private GameObject _target;
    private float _leftRotationLimit;
    private float _rightRotationLimit;
    private float _rotationSpeed;
    private Weapon _mountedWeapon;
    private MechBehavior _mechBehavior;

    private void Update()
    {
        
    }

    private void Aim(GameObject target)
    {
        Vector3 targetPosition = new Vector3(0,0,0);
        if((_mountedWeapon.weaponType == WeaponScriptable.WeaponType.Missile || _mountedWeapon.weaponType == WeaponScriptable.WeaponType.Kenetic) && !_mountedWeapon.homing)
        {
            targetPosition = _target.transform.position;
        }
        else
        {

        }
    }

    public void SetTarget(GameObject target)
    {
        _target = target;
    }
    public WeaponMount(WeaponMountScriptable weaponMountStats, WeaponScriptable scriptable)
    {
        _leftRotationLimit = weaponMountStats.leftRorationLimit;
        _rightRotationLimit = weaponMountStats.rightRorationLimit;
        _rotationSpeed = weaponMountStats.rorationSpeed;
        _mountedWeapon = new Weapon(scriptable, _mechBehavior);
    }
}
