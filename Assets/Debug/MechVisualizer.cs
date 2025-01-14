using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BaseMech), typeof(MeshRenderer))]
public class MechVisualDebugger : MonoBehaviour
{
    public Material playerMat;
    public Material enemyMat;

    [HideInInspector] 
    public BaseMech mech;
    [HideInInspector] 
    public NavMeshAgent navAgent;
    [HideInInspector] 
    public List<Vector3> navPath;
    [HideInInspector] 
    public System.Action drawGizmosSelected;
    private Camera _mainCamera;
    private GameObject _stateBanner;
    private TextMeshPro _stateText;

    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    void Awake()
    {
        mech = GetComponent<BaseMech>();
        navAgent = GetComponent<NavMeshAgent>();
        navPath ??= new List<Vector3>();
        var renderer = GetComponent<MeshRenderer>();
        renderer.material = mech.IsPlayerMech ? playerMat : enemyMat;
    }


    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    void Start()
    {
        GetComponent<MeshRenderer>().material = mech.IsPlayerMech ? playerMat : enemyMat;
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_stateBanner == null)
        {
            _stateBanner = transform.GetChild(transform.childCount-1).gameObject;
            if (_stateBanner.name != "StateBanner")
            {
                _stateBanner = new GameObject("StateBanner");
                _stateBanner.transform.SetParent(mech.transform);
                _stateText = _stateBanner.AddComponent<TextMeshPro>();
                _stateText.alignment = TextAlignmentOptions.Center;
                _stateText.fontSize = 20;
            }
            else
            {
                _stateText = _stateBanner.GetComponent<TextMeshPro>();
            }
        }
    }


    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    public void EditorModifiedProperties(BaseMech mechRef)
    {
        mech = mechRef;
        GetComponent<MeshRenderer>().material = mech.IsPlayerMech ? playerMat : enemyMat;
    }


    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    void Update()
    {
        if (_stateText == null || navAgent == null) return;
        var dest = navAgent.destination;
        if (!float.IsInfinity(dest.x) && !float.IsInfinity(dest.y) && !float.IsInfinity(dest.z))
        {
            navPath = navAgent.path.corners.ToList();
            navPath.Prepend(transform.position);
        }

        _stateText.text = mech.CurrentState.ToString();
        // if (mech.EvasionManuveur != "") _stateText.text += $"\n{mech.EvasionManuveur}";
    }    


    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    void LateUpdate()
    {
        if (_stateBanner == null || mech == null) return;
        _stateBanner.transform.position = mech.transform.position + new Vector3(0f, 4f, 0f);
        _stateBanner.transform.LookAt(_stateBanner.transform.position + _mainCamera.transform.rotation * Vector3.forward,
                             _mainCamera.transform.rotation * Vector3.up);
    }


    [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
    void OnDrawGizmosSelected()
    {
        if (!enabled) return;
        drawGizmosSelected?.Invoke();
        if (mech == null) return;
        Gizmos.color = new Color(0f,1f,0f,.3f);
        Gizmos.DrawWireSphere(transform.position, mech.MechType.sightRange);
        Gizmos.color = new Color(1f,.6f,.18f,.3f);
        Gizmos.DrawWireSphere(transform.position, mech.MechType.hearingRange);

        Gizmos.color = new Color(1f,.29f,.19f,1f);

        var patrol = mech.Patrol;
        List<Vector3> gizmo_path = new List<Vector3>();
        if (patrol.Count > 0)
        {
            for (int i = 0; i < patrol.Count; ++i)
            {
                gizmo_path.Add(patrol[i]);
                gizmo_path.Add(patrol[(i+1)%patrol.Count]);
            }
            Gizmos.DrawLineList(gizmo_path.ToArray());
            foreach (var point in patrol)
            {
                Gizmos.DrawSphere(point + new Vector3(0,2,0), 1);
            }
        }

        if (navAgent == null) return;
        Gizmos.color = new Color(1f, 0f, 0f, 1f);
        Gizmos.DrawSphere(navAgent.destination, .15f);
        Gizmos.color = new Color(1f, 0f, 0f, 1f);
        gizmo_path.Clear();
        for (int i = 0; i < navPath.Count-1; ++i)
        {
            gizmo_path.Add(navPath[i]);
            gizmo_path.Add(navPath[i+1]);
        }
        Gizmos.DrawLineList(gizmo_path.ToArray());
    }
}