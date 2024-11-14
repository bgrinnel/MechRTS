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

    private void Update()
    {
        if (_target != null)
        {
            var targetPosition = AimPosition(_target);
            TargetPointing(targetPosition);
        }
    }

    public void TargetPointing(Vector3 targetPosition)
    {
        float angleDifference = AngleDifference(targetPosition);

        float currentAngle = transform.localEulerAngles.y;

        if (currentAngle > 180) currentAngle -= 360;


        float rotationStep = _rotationSpeed * Time.deltaTime;

        if (angleDifference == 0f)
        {
            _mountedWeapon.Fire(_target);
        }

        else
        {
            if (angleDifference > 0)
            {
                currentAngle += rotationStep;
            }
            else if (angleDifference < 0)
            {
                currentAngle -= rotationStep;
            }
        }
        
        currentAngle = Mathf.Clamp(currentAngle, -_leftRotationLimit, _rightRotationLimit);

        transform.localRotation = Quaternion.Euler(0, currentAngle, 0);

        
    }

    public float AngleDifference(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - gameObject.transform.position;

        Vector3 localDirection = gameObject.transform.InverseTransformDirection(direction);

        Vector3 localForward = gameObject.transform.InverseTransformDirection(gameObject.transform.transform.forward);

        float relativeAngle = Vector3.SignedAngle(localForward, localDirection, Vector3.up);

        return relativeAngle;
    }

    public Vector3 AimPosition(GameObject target)
    {
        Vector3 targetPosition = new Vector3(0,0,0);
        if((_mountedWeapon.weaponType == WeaponScriptable.WeaponType.Missile || _mountedWeapon.weaponType == WeaponScriptable.WeaponType.Kenetic) && !_mountedWeapon.homing)
        {
            targetPosition = LeadPosition(transform.parent.GetComponent<Rigidbody>(), target.GetComponent<Rigidbody>());
        }
        else
        {
            targetPosition = target.transform.position;
        }
        return targetPosition;
    }

    public void SetTarget(GameObject target)
    {
        _target = target;
    }
    public void InitalizeWeaponMount(WeaponMountScriptable weaponMountStats, WeaponScriptable scriptable, BaseMech _mechBehavior)
    {
        _leftRotationLimit = weaponMountStats.leftRorationLimit;
        _rightRotationLimit = weaponMountStats.rightRorationLimit;
        _rotationSpeed = weaponMountStats.rorationSpeed;

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

        float timeToImpact = distance / _mountedWeapon.GetProjectileSpeed();

        Vector3 predictionPosition = targetPosition + targetVelocity * timeToImpact;

        return predictionPosition;
    }
}
