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

    private void Missile()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.forward * _speed;
    }

    private void HomingMissile()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.forward * _speed;

        var leadTimePercentage = Mathf.InverseLerp(_minDistancePredict, _maxDistancePredict, Vector3.Distance(transform.position, _target.transform.position));

        PredictMovement(leadTimePercentage);

        AddDeviation(leadTimePercentage);

        RotateRocket();
    }
    private void KeneticWeapons()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.forward * _speed;
    }
    public void SetMode(int mode, bool homing = false)
    {
        _weaponModes = mode;
        _homing = homing;
    }

    public void InitializeProjectile(float damage, float speed, GameObject target = null)
    {
        _damage = damage;
        _speed = speed;
        _target = target;
    }

    public void InitializeHoming(float minPredictionDistance, float maxPredictionDistance, float maxTimePrediction)
    {
        _minDistancePredict = minPredictionDistance;
        _maxDistancePredict = maxPredictionDistance;
        _maxTimePrediction = maxTimePrediction;
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

}
