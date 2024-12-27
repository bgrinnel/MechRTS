using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponScriptable))]
public class WeaponEditor : Editor
{
    private SerializedProperty _weaponType;
    private SerializedProperty _weaponMaxRange;
    private SerializedProperty _weaponMinRange;
    private SerializedProperty _weaponEffctiveRange;
    private SerializedProperty _weaponMaxDamage;
    private SerializedProperty _weaponMinDamage;
    private SerializedProperty _weaponDamageCurve;
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
    private SerializedProperty _accMax;
    private SerializedProperty _accMin;
    private SerializedProperty _accCurve;

    public void OnEnable()
    {
        _weaponType = serializedObject.FindProperty("weaponType");
        _weaponMaxRange = serializedObject.FindProperty("maximumRange");
        _weaponMinRange = serializedObject.FindProperty("minimumRange");
        _weaponEffctiveRange = serializedObject.FindProperty("effectiveRange");
        _weaponMaxDamage = serializedObject.FindProperty("weaponMaxDamage");
        _weaponMinDamage = serializedObject.FindProperty("weaponMinDamage");
        _weaponDamageCurve = serializedObject.FindProperty("weaponDamageCurve");
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
        _accMax = serializedObject.FindProperty("weaponMaxAccuracy");
        _accMin = serializedObject.FindProperty("weaponMinAccuracy");
        _accCurve = serializedObject.FindProperty("weaponAccuracyCurve");
    }

    public override void OnInspectorGUI()
    {
        WeaponScriptable _weaponScriptable = (WeaponScriptable)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(_weaponType);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon Damage", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_weaponMaxDamage);
        EditorGUILayout.PropertyField(_weaponMinDamage);
        EditorGUILayout.PropertyField(_weaponDamageCurve);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon Range", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_weaponMaxRange);
        EditorGUILayout.PropertyField(_weaponMinRange);
        EditorGUILayout.PropertyField(_weaponEffctiveRange);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon Reload", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_weaponSalvoReload);
        EditorGUILayout.PropertyField(_weaponSalvoLength);
        EditorGUILayout.PropertyField(_weaponShotReload);
        EditorGUILayout.PropertyField(_ammo);

        if (_weaponScriptable.weaponType == WeaponScriptable.WeaponType.Missile)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Projectile Projectiles", EditorStyles.boldLabel);
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
            EditorGUILayout.LabelField("Projectile Projectiles", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_projectilePrefab);
            EditorGUILayout.PropertyField(_projectileSpeed);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Weapon Accuracy", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_accMax);
        EditorGUILayout.PropertyField(_accMin);
        EditorGUILayout.PropertyField(_accCurve);


        serializedObject.ApplyModifiedProperties();
    }
}
