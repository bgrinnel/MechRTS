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
    public WeaponMount(WeaponMountScriptable weaponMountStats, WeaponScriptable scriptable, MechBehavior _mechBehavior)
    {
        _leftRotationLimit = weaponMountStats.leftRorationLimit;
        _rightRotationLimit = weaponMountStats.rightRorationLimit;
        _rotationSpeed = weaponMountStats.rorationSpeed;
        _mountedWeapon = new Weapon(scriptable, _mechBehavior);
    }
}
