using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Projectile : MonoBehaviour
{
    private GameObject _target;
    private int _weaponModes;
    private bool _homing;
    private float _damage, _speed, _maxDistancePredict, _minDistancePredict, _maxTimePrediction, _projectileRotationSpeed;
    private Vector3 _standardPrediction, _deviatedPrediction;
    private MechBehavior _owningMech;
    [SerializeField] private float _deviationAmount = 50;
    [SerializeField] private float _deviationSpeed = 2;

    private void Update()
    {
        switch (_weaponModes)
        {
            case 0:
                KeneticWeapons();
                break;
            case 1:
                if (_homing)
                {
                    HomingMissile();
                }
                else
                {
                    Missile();
                }
                break;
        }
    }

    // To: VK, From: Jeff - for damaging a mech you'll need a reference to this projectilies owning MechBehaviour and then you just say owning_mech.HitMech(MechBehaviour target, float damage)
    // also, their are two collision layers for characters; "Player" and "Enemy". I have already made a tag for "Mech" that all mechs have. When spawning Projectiles it would be nice if you assigned them
    // the same collision layer as their spawner (you can get it directly from the GameObject.layer or assign it based on bool MechBehaviour.IsPlayer()). It would also be nice if you gave all projectiles.gameObject a tag (probably called projectile)
    // currently I don't initialize Weapons[] MechBehaviour._weapons because you don't have a constructor. The CustomEditor is a beast so if you just want to hand that off to me once you finish here that's completely fine.
    private void Missile()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * _speed;
    }

    private void HomingMissile()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * _speed;

        var leadTimePercentage = Mathf.InverseLerp(_minDistancePredict, _maxDistancePredict, Vector3.Distance(transform.position, _target.transform.position));

        PredictMovement(leadTimePercentage);

        AddDeviation(leadTimePercentage);

        RotateRocket();
    }
    private void KeneticWeapons()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * _speed;
    }
    public void SetMode(int mode, bool homing = false)
    {
        _weaponModes = mode;
        _homing = homing;
    }

    public void InitializeProjectile(float damage, float speed, MechBehavior owningMech,GameObject target = null)
    {
        _damage = damage;
        _speed = speed;
        _target = target;
        _owningMech = owningMech;
        gameObject.layer = owningMech.gameObject.layer;
    }

    public void InitializeHoming(float minPredictionDistance, float maxPredictionDistance, float maxTimePrediction, float rotationSpeed)
    {
        _minDistancePredict = minPredictionDistance;
        _maxDistancePredict = maxPredictionDistance;
        _maxTimePrediction = maxTimePrediction;
        _projectileRotationSpeed = rotationSpeed;
    }
    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, _maxTimePrediction, leadTimePercentage);

        _standardPrediction = _target.GetComponent<Rigidbody>().position + _target.GetComponent<Rigidbody>().velocity * predictionTime;
    }

    private void AddDeviation(float leadTimePercentage)
    {
        var deviation = new Vector3(Mathf.Cos(Time.time * _deviationSpeed), 0, 0);

        var predictionOffset = transform.TransformDirection(deviation) * _deviationAmount * leadTimePercentage;

        _deviatedPrediction = _standardPrediction + predictionOffset;
    }

    private void RotateRocket()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        var heading = _deviatedPrediction - transform.position;

        var rotation = Quaternion.LookRotation(heading);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, _projectileRotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        _owningMech.HitMech(other.gameObject.GetComponent<MechBehavior>(), _damage);
        Destroy(this.gameObject);
    }
}
