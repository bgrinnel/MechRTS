using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponScriptable))]
public class WeaponEditor : Editor
{
    SerializedProperty weaponRange;
    SerializedProperty weaponDamage;
    SerializedProperty weaponRatesOfFire;
    SerializedProperty weaponType;
    SerializedProperty projectilePrefab;
    SerializedProperty projectileSpeed;
    public void OnEnable()
    {
        weaponRange = serializedObject.FindProperty("weaponRange");
        weaponDamage = serializedObject.FindProperty("weaponDamage");
        weaponDamage = serializedObject.FindProperty("weaponRatesOfFire");
        weaponDamage = serializedObject.FindProperty("weaponType");
        weaponDamage = serializedObject.FindProperty("projectilePrefab");
        weaponDamage = serializedObject.FindProperty("projectileSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(weaponType);

        EditorGUILayout.PropertyField(weaponRange);
        EditorGUILayout.PropertyField(weaponDamage);
        EditorGUILayout.PropertyField(weaponDamage);
    }
}
