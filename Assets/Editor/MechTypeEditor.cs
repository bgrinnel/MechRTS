using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CustomEditor(typeof(MechType))]
public class MechTypeEditor : Editor
{
    private string[] _selectionAlgorithmNames;
    private int _selectedAlgorithmIndex;

    private SerializedProperty _serialiedMaxHealth;
    private SerializedProperty _serializedSpeed;
    private SerializedProperty _serializedAngularSpeed;
    private SerializedProperty _serializedAcceleration;
    private SerializedProperty _serializedStoppingDistance;
    private SerializedProperty _serializedAutoBraking;
    private SerializedProperty _serializedRadius;
    private SerializedProperty _serializedHeight;    
    private SerializedProperty _serialiedMass;
    private SerializedProperty _serialiedDrag;
    private SerializedProperty _serialiedAngularDrag;
    private SerializedProperty _serialiedSightRange;
    private SerializedProperty _serialiedHearingRange;
    private SerializedProperty _serialiedSelectionAlgorithm;
    private SerializedProperty _serialiedWeaponArray;
    private SerializedProperty _serializedhitRateReductionOverSpeed;
    private SerializedProperty _serializeddamageReductionOverSpeed;
    private SerializedProperty _serializedcritRateReductionOverSpeed;
    private SerializedProperty _serializedfrontFlankHitRateAddOverDistance;
    private SerializedProperty _serializedfrontFlankCritRateAddOverDistance;
    private SerializedProperty _serializedleftFlankHitRateAddOverDistance;
    private SerializedProperty _serializedleftFlankCritRateAddOverDistance;
    private SerializedProperty _serializedrightFlankHitRateAddOverDistance;
    private SerializedProperty _serializedrightFlankCritRateAddOverDistance;
    private SerializedProperty _serializedrearFlankHitRateAddOverDistance;
    private SerializedProperty _serializedrearFlankCritRateAddOverDistance;
    private bool _isShowingAgentProperties;
    private bool _isShowingCurves;
    private bool _isShowingSpeedCurves;
    private bool _isShowingFrontFlankCurves;
    private bool _isShowingLeftFlankCurves;
    private bool _isShowingRightFlankCurves;
    private bool _isShowingRearFlankCurves;
    void OnEnable()
    {
        var mech_type = (MechType)target;
        _selectionAlgorithmNames = typeof(SelectionAlg).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.ReturnType == typeof(BaseMech)
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType == typeof(BaseMech)
                    && m.GetParameters()[1].ParameterType == typeof(BaseMech[]))
        .Select(m => m.Name)
        .ToArray();

        _selectedAlgorithmIndex = Array.IndexOf(_selectionAlgorithmNames, mech_type.selectionAlg?.Method.Name);
        if (_selectedAlgorithmIndex == -1) _selectedAlgorithmIndex = 0;

        _serialiedMaxHealth = serializedObject.FindProperty("maxHealth");
        
        _serializedSpeed = serializedObject.FindProperty("speed");
        _serializedAngularSpeed = serializedObject.FindProperty("angularSpeed");
        _serializedAcceleration = serializedObject.FindProperty("acceleration");
        _serializedStoppingDistance = serializedObject.FindProperty("stoppingDistance");
        _serializedAutoBraking = serializedObject.FindProperty("autoBraking");
        _serializedRadius = serializedObject.FindProperty("radius");
        _serializedHeight = serializedObject.FindProperty("height");   

        _serialiedMass = serializedObject.FindProperty("mass");
        _serialiedDrag = serializedObject.FindProperty("drag");
        _serialiedAngularDrag = serializedObject.FindProperty("angularDrag");
        _serialiedSightRange = serializedObject.FindProperty("sightRange");
        _serialiedHearingRange = serializedObject.FindProperty("hearingRange");
        _serialiedSelectionAlgorithm = serializedObject.FindProperty("selectionAlg");
        _serialiedWeaponArray = serializedObject.FindProperty("weapons");

        _serializedhitRateReductionOverSpeed = serializedObject.FindProperty("hitRateReductionOverSpeed");
        _serializeddamageReductionOverSpeed = serializedObject.FindProperty("damageReductionOverSpeed");
        _serializedcritRateReductionOverSpeed = serializedObject.FindProperty("critRateReductionOverSpeed");
        _serializedfrontFlankHitRateAddOverDistance = serializedObject.FindProperty("frontFlankHitRateAddOverDistance");
        _serializedfrontFlankCritRateAddOverDistance = serializedObject.FindProperty("frontFlankCritRateAddOverDistance");
        _serializedleftFlankHitRateAddOverDistance = serializedObject.FindProperty("leftFlankHitRateAddOverDistance");
        _serializedleftFlankCritRateAddOverDistance = serializedObject.FindProperty("leftFlankCritRateAddOverDistance");
        _serializedrightFlankHitRateAddOverDistance = serializedObject.FindProperty("rightFlankHitRateAddOverDistance");
        _serializedrightFlankCritRateAddOverDistance = serializedObject.FindProperty("rightFlankCritRateAddOverDistance");
        _serializedrearFlankHitRateAddOverDistance = serializedObject.FindProperty("rearFlankHitRateAddOverDistance");
        _serializedrearFlankCritRateAddOverDistance = serializedObject.FindProperty("rearFlankCritRateAddOverDistance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var mech_type = (MechType)target;

        EditorGUILayout.PropertyField(_serialiedMaxHealth);
        EditorGUILayout.PropertyField(_serialiedSightRange);
        EditorGUILayout.PropertyField(_serialiedHearingRange);
        EditorGUILayout.PropertyField(_serialiedWeaponArray);

        int new_selection_index = EditorGUILayout.Popup("Targeting Algorithm", _selectedAlgorithmIndex, _selectionAlgorithmNames);
        if (new_selection_index != _selectedAlgorithmIndex)
        {
            _selectedAlgorithmIndex = new_selection_index;
            var methodName = _selectionAlgorithmNames[_selectedAlgorithmIndex];
            var methodInfo = typeof(SelectionAlg).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

            if (methodInfo != null)
            {
                mech_type.selectionAlg = (MechType.SelectionAlgorithm)Delegate.CreateDelegate(
                    typeof(MechType.SelectionAlgorithm), methodInfo);
                EditorUtility.SetDirty(mech_type);
            }
        }

        if (_isShowingAgentProperties = EditorGUILayout.Foldout(_isShowingAgentProperties, "PHSYSICS & NAVIGATION"))
        {
            EditorGUILayout.PropertyField(_serializedSpeed);
            EditorGUILayout.PropertyField(_serializedAngularSpeed);
            EditorGUILayout.PropertyField(_serializedAcceleration);
            EditorGUILayout.PropertyField(_serializedStoppingDistance);
            EditorGUILayout.PropertyField(_serializedAutoBraking);
            EditorGUILayout.PropertyField(_serializedRadius);
            EditorGUILayout.PropertyField(_serializedHeight);
            EditorGUILayout.PropertyField(_serialiedMass);
            EditorGUILayout.PropertyField(_serialiedDrag);
            EditorGUILayout.PropertyField(_serialiedAngularDrag);
        }
        
        // TODO look up indentations in Unity Editory and Labels
        if (_isShowingCurves = EditorGUILayout.Foldout(_isShowingCurves, "CURVES"))
        {
            if (_isShowingSpeedCurves = EditorGUILayout.Foldout(_isShowingSpeedCurves, "SPEED CURVES"))
            {
                EditorGUILayout.PropertyField(_serializedhitRateReductionOverSpeed);
                EditorGUILayout.PropertyField(_serializeddamageReductionOverSpeed);
                EditorGUILayout.PropertyField(_serializedcritRateReductionOverSpeed);
            }
            if (_isShowingFrontFlankCurves = EditorGUILayout.Foldout(_isShowingFrontFlankCurves, "FRONT FLANK CURVES"))
            {
                EditorGUILayout.PropertyField(_serializedfrontFlankHitRateAddOverDistance);
                EditorGUILayout.PropertyField(_serializedfrontFlankCritRateAddOverDistance);
            }
            if (_isShowingLeftFlankCurves = EditorGUILayout.Foldout(_isShowingLeftFlankCurves, "LEFT FLANK CURVES"))
            {
                EditorGUILayout.PropertyField(_serializedleftFlankHitRateAddOverDistance);
                EditorGUILayout.PropertyField(_serializedleftFlankCritRateAddOverDistance);
            }
            if (_isShowingRightFlankCurves = EditorGUILayout.Foldout(_isShowingRightFlankCurves, "RIGHT FLANK CURVES"))
            {
                EditorGUILayout.PropertyField(_serializedrightFlankHitRateAddOverDistance);
                EditorGUILayout.PropertyField(_serializedrightFlankCritRateAddOverDistance);
            }
            if (_isShowingRearFlankCurves = EditorGUILayout.Foldout(_isShowingRearFlankCurves, "REAR FLANK CURVES"))
            {
                EditorGUILayout.PropertyField(_serializedrearFlankHitRateAddOverDistance);
                EditorGUILayout.PropertyField(_serializedrearFlankCritRateAddOverDistance);
            }
        }
        if (serializedObject.ApplyModifiedProperties())
        {
            // propogate changes as needed   
        }
    }
}