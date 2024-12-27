using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponScriptable))]
public class WeaponEditor : Editor
{
    private SerializedProperty _weaponType;
    private SerializedProperty _weaponRange;
    private SerializedProperty _weaponDamage;
    private SerializedProperty _weaponSalvoReload;
    private SerializedProperty _projectilePrefab;
    private SerializedProperty _projectileSpeed;
    private SerializedProperty _weaponSalvoLength;
    private SerializedProperty _weaponShotReload;
    private SerializedProperty _ammo;
    private SerializedProperty _homing;
    private SerializedProperty _projectileRotationSpeed;
    private SerializedProperty _maxDistancePredict;
    private SerializedProperty _minDistancePredict;
    private SerializedProperty _maxTimePrediction;
    private SerializedProperty _accBase;
    private SerializedProperty _accIncrement;
    private SerializedProperty _accFatcor;

    public void OnEnable()
    {
        _weaponType = serializedObject.FindProperty("weaponType");
        _weaponRange = serializedObject.FindProperty("weaponRange");
        _weaponDamage = serializedObject.FindProperty("weaponDamage");
        _weaponSalvoReload = serializedObject.FindProperty("weaponSalvoReload");
        _projectilePrefab = serializedObject.FindProperty("projectilePrefab");
        _projectileSpeed = serializedObject.FindProperty("projectileSpeed");
        _weaponSalvoLength = serializedObject.FindProperty("weaponSalvoLength");
        _weaponShotReload = serializedObject.FindProperty("weaponShotReload");
        _ammo = serializedObject.FindProperty("ammo");
        _homing = serializedObject.FindProperty("homing");
        _projectileRotationSpeed = serializedObject.FindProperty("projectileRotationSpeed");
        _maxDistancePredict = serializedObject.FindProperty("maxDistancePredict");
        _minDistancePredict = serializedObject.FindProperty("minDistancePredict");
        _maxTimePrediction = serializedObject.FindProperty("maxTimePrediction");
        _accBase = serializedObject.FindProperty("baseAccuracy");
        _accIncrement = serializedObject.FindProperty("AccuracyIncrement");
        _accFatcor = serializedObject.FindProperty("AccuracyFactor");
    }

    public override void OnInspectorGUI()
    {
        WeaponScriptable _weaponScriptable = (WeaponScriptable)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(_weaponType);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon Base Statistics", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_weaponRange);
        EditorGUILayout.PropertyField(_weaponDamage);
        EditorGUILayout.PropertyField(_weaponSalvoReload);
        EditorGUILayout.PropertyField(_weaponSalvoLength);
        EditorGUILayout.PropertyField(_weaponShotReload);
        EditorGUILayout.PropertyField(_ammo);

        if (_weaponScriptable.weaponType == WeaponScriptable.WeaponType.Missile)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Projectiles", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_projectilePrefab);
            EditorGUILayout.PropertyField(_projectileSpeed);
            EditorGUILayout.PropertyField(_homing);
            if (_weaponScriptable.homing)
            {
                EditorGUILayout.PropertyField(_projectileRotationSpeed);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Projectile Properties", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_maxDistancePredict);
                EditorGUILayout.PropertyField(_minDistancePredict);
                EditorGUILayout.PropertyField(_maxTimePrediction);
            }
        }
        else if(_weaponScriptable.weaponType == WeaponScriptable.WeaponType.Kenetic)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Projectiles", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_projectilePrefab);
            EditorGUILayout.PropertyField(_projectileSpeed);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon accuracy", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_accBase);
        EditorGUILayout.PropertyField(_accIncrement);
        EditorGUILayout.PropertyField(_accFatcor);


        serializedObject.ApplyModifiedProperties();
    }
}
