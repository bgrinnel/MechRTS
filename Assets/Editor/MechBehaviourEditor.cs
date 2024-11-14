using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.AI;

[CustomEditor(typeof(BaseMech))]
public class MechBehaviourEditor : Editor
{  
    private SerializedProperty _serializedPatrol;
    private SerializedProperty _serializedIsPlayer;
    private SerializedProperty _serializedType;
    [SerializeField] public List<Vector3> _unaddedPoints;
    private bool _bModifyingPatrol;
    private List<KeyValuePair<Vector3, Vector3>> _raycasts;
    private MechVisualDebugger _visualizer;

    // Serialized GameObject parent and its properties
    private SerializedObject _serializedParent;
    private SerializedProperty _serializedParent_layer;
    private SerializedProperty _serializedParent_tag;

    // Serialized NavMeshAgent and its properties
    private SerializedObject _serializedAgent;
    private SerializedProperty _serializedAgent_speed;
    private SerializedProperty _serializedAgent_angularSpeed;
    private SerializedProperty _serializedAgent_acceleration;
    private SerializedProperty _serializedAgent_stoppingDistance;
    private SerializedProperty _serializedAgent_autoBraking;
    private SerializedProperty _serializedAgent_radius;
    private SerializedProperty _serializedAgent_height;  
    private SerializedProperty _serializedAgent_baseOffset;
    private SerializedProperty _serializedAgent_autoTraverseOffMeshLink;
    private SerializedProperty _serializedAgent_agentTypeID;

    // Serialized CapsuleCollider and its properties
    private SerializedObject _serializedCollider;  
    private SerializedProperty _serializedCollider_radius;
    private SerializedProperty _serializedCollider_height;  

    // Serialized RigidBody and its properties
    private SerializedObject _serializedRigibody;
    private SerializedProperty _serializedRigidBody_mass;
    private SerializedProperty _serializedRigidBody_drag;
    private SerializedProperty _serializedRigidBody_angularDrag;
    void OnEnable()
    {
        _serializedPatrol = serializedObject.FindProperty("_patrol");
        _unaddedPoints = new List<Vector3>();
        _serializedType = serializedObject.FindProperty("_type");
        _serializedIsPlayer = serializedObject.FindProperty("_isPlayer");


        // so much bullshit
        var mech = (BaseMech)target;
        _serializedParent = new SerializedObject(mech.gameObject);
        _serializedParent_layer = _serializedParent.FindProperty("m_Layer");
        _serializedParent_tag = _serializedParent.FindProperty("m_TagString"); 
        
        var agent = mech.GetComponent<NavMeshAgent>();
        _serializedAgent = new SerializedObject(agent);
        // var properties = _serializedAgent.GetIterator();                                     // If you need to mess with Unity classes use this to find their internal prop names
        // while (properties.NextVisible(true)) Debug.Log($"NavMeshAgent.{properties.name}");
        _serializedAgent_speed = _serializedAgent.FindProperty("m_Speed");
        _serializedAgent_angularSpeed = _serializedAgent.FindProperty("m_AngularSpeed");
        _serializedAgent_acceleration = _serializedAgent.FindProperty("m_Acceleration");
        _serializedAgent_stoppingDistance = _serializedAgent.FindProperty("m_StoppingDistance");
        _serializedAgent_autoBraking = _serializedAgent.FindProperty("m_AutoBraking");
        _serializedAgent_radius = _serializedAgent.FindProperty("m_Radius");
        _serializedAgent_height = _serializedAgent.FindProperty("m_Height");  
        _serializedAgent_baseOffset = _serializedAgent.FindProperty("m_BaseOffset");
        _serializedAgent_autoTraverseOffMeshLink = _serializedAgent.FindProperty("m_AutoTraverseOffMeshLink");
        _serializedAgent_agentTypeID = _serializedAgent.FindProperty("m_AgentTypeID");

        var collider = mech.GetComponent<CapsuleCollider>();
        _serializedCollider = new SerializedObject(collider);
        _serializedCollider_radius = _serializedCollider.FindProperty("m_Radius");
        _serializedCollider_height = _serializedCollider.FindProperty("m_Height");

        var rigidbody = mech.GetComponent<Rigidbody>();
        _serializedRigibody = new SerializedObject(rigidbody);
        _serializedRigidBody_mass = _serializedRigibody.FindProperty("m_Mass");
        _serializedRigidBody_drag = _serializedRigibody.FindProperty("m_Drag");
        _serializedRigidBody_angularDrag = _serializedRigibody.FindProperty("m_AngularDrag");

        _visualizer = mech.GetComponent<MechVisualDebugger>(); // if we have a debug comp installed then handle updates to it too
        if (_visualizer != null) 
        {
            _raycasts = new List<KeyValuePair<Vector3, Vector3>>();
            _visualizer.drawGizmosSelected += OnDrawGizmosSelected; // subscribe to the visualizers since Editor's don't have a OnDrawGizmos(Selected)
            _visualizer.EditorModifiedProperties(mech);   
        }

        _OnEnable = true;
    }

    void OnDrawGizmosSelected()
    {
        if (!_bModifyingPatrol) {
            if (_raycasts.Count > 0) _raycasts.Clear();
        }
        else
        {
            foreach (var raycast in _raycasts)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(raycast.Key, raycast.Value);
            }
        }
    }

    private bool _OnEnable = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var mech = (BaseMech)target;

        EditorGUILayout.PropertyField(_serializedIsPlayer);
        EditorGUILayout.PropertyField(_serializedType);
        if (EditorGUILayout.DropdownButton(new GUIContent("Modify Patrol"), FocusType.Passive))
        {
            _bModifyingPatrol = !_bModifyingPatrol;
        }

        if (_bModifyingPatrol)
        {
            int patrol_length = _serializedPatrol.arraySize;
            if (patrol_length < 2) EditorGUILayout.LabelField("WARNING: Won't save, needs >1 point");
            EditorGUILayout.LabelField("Click on a StaticWalkable object in the scene");
            if (patrol_length > 0)
            {
                if (EditorGUILayout.DropdownButton(new GUIContent("Remove Last Point"), FocusType.Passive)) //TODO: if this is confusing replace with normal button, I just couldn't find it
                {
                    _serializedPatrol.DeleteArrayElementAtIndex(patrol_length-1);
                }
            }
            foreach(var point in _unaddedPoints)
            {
                _serializedPatrol.InsertArrayElementAtIndex(patrol_length);
                _serializedPatrol.GetArrayElementAtIndex(patrol_length).vector3Value = point;
            }
            _unaddedPoints.Clear();
        }

        if (serializedObject.ApplyModifiedProperties() || _OnEnable)
        {   
            var mech_type = (MechType)_serializedType.objectReferenceValue;
            _serializedParent_layer.intValue = LayerMask.NameToLayer(mech.IsPlayerMech ? "Player" : "Enemy");
            _serializedParent_tag.stringValue = "Mech"; 

            _serializedAgent_speed.floatValue = mech_type.agentType.speed;
            _serializedAgent_angularSpeed.floatValue = mech_type.agentType.angularSpeed;
            _serializedAgent_acceleration.floatValue = mech_type.agentType.acceleration;
            _serializedAgent_stoppingDistance.floatValue = mech_type.agentType.stoppingDistance;
            _serializedAgent_autoBraking.boolValue = mech_type.agentType.autoBraking;
            _serializedAgent_radius.floatValue = mech_type.agentType.radius;
            _serializedAgent_height.floatValue = mech_type.agentType.height;
            _serializedAgent_baseOffset.floatValue = mech_type.agentType.height / 1.95f;
            _serializedAgent_autoTraverseOffMeshLink.boolValue = true;
            _serializedAgent_agentTypeID.intValue = -1372625422;                // I initially printed the mech id to find its value
            // Debug.Log($"NavMeshAgent typeId = {nav_type_id}");
            _serializedCollider_radius.floatValue = mech_type.agentType.radius;
            _serializedCollider_height.floatValue = mech_type.agentType.height;

            _serializedRigidBody_mass.floatValue = mech_type.mass;
            _serializedRigidBody_drag.floatValue = mech_type.drag;
            _serializedRigidBody_angularDrag.floatValue = mech_type.angularDrag;

            // mark dirty for scene serialization       
            if (_visualizer != null)
            {
                _visualizer.EditorModifiedProperties(mech);
                EditorUtility.SetDirty(_visualizer);
            }
            _serializedParent.ApplyModifiedProperties();
            _serializedAgent.ApplyModifiedProperties();
            _serializedCollider.ApplyModifiedProperties();
            _serializedRigibody.ApplyModifiedProperties();
            _OnEnable = false;
        }
    }

    void OnSceneGUI()
    {   
        Event e = Event.current;
        if (e.type == EventType.MouseUp && e.button == 0 && _bModifyingPatrol && e.type != EventType.DragExited)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (_visualizer != null) _raycasts.Add(new(ray.origin, ray.origin + ray.direction * 10000f)); // _visualizer stuff
            var hits = Physics.RaycastAll(ray);
            float closest_dist = Mathf.Infinity;
            Vector3 closest_hit = Vector3.zero;
            foreach (var hit in hits)
            {
                var layer = hit.collider.gameObject.layer;
                if ("Static".Equals(LayerMask.LayerToName(layer)) && hit.distance < closest_dist && hit.collider.tag == "Walkable")
                {
                    closest_hit = hit.point;
                    closest_dist = hit.distance;
                }
            }
            if (closest_dist < Mathf.Infinity) 
            {
                _unaddedPoints.Add(closest_hit);
            }
            e.Use();
            OnInspectorGUI();
        }
    }

    void OnDisable()
    {
        _unaddedPoints.Clear();
        if (_visualizer != null) _raycasts.Clear();
    }
}
