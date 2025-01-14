using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Linq;

[ExecuteInEditMode]
public class RTSController : MonoBehaviour
{
    public static RTSController Singleton;
    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private Vector3 screenPos;
    [SerializeField] private Vector3 worldPos;
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask backgroundLayerMask;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private float clickThreshold = 20f; // To distinguish between click and drag

    private Vector2 leftMBStartPos;
    private Vector2 rightMBStartPos;
    [SerializeField] CameraController camState;

    private bool isLeftMBDragging = false;
    private bool isRightMBDragging = false;

    [SerializeField] private GameObject moveTargetEffect;


    [SerializeField] private List<GameObject> _playerUnits;

    [SerializeField] private List<GameObject> _selectedUnits;
    public bool _shiftPressed;
    public bool _holdingCtrl;
    public List<GameObject>[] unitGroups = new List<GameObject>[10];

    [SerializeField]
    private GameObject playUI;
    [SerializeField]
    private GameObject settingsUI;
    private bool SettingsMenu = false;

    private void OnEnable()
    {
        if (_playerUnits == null) _playerUnits = new();
        if (Singleton == null) Singleton = this;
    }

    private void OnDisable()
    {
        if (Singleton != null) Singleton = null;
        // if (_playerUnits != null) _playerUnits.Clear();
    }

    private void Awake()
    {
        if (_playerUnits == null) _playerUnits = new();
        _selectedUnits = new();
        for (int i = 1; i < unitGroups.Length; i++) 
        {
            unitGroups[i] = new List<GameObject>();
        }
    }

    private void Start()
    {
        selectionBox.gameObject.SetActive(false);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            MenuChange();
        }
        if(!SettingsMenu){
            MouseInput();
        }
        
    }
    void MouseInput()
    {
        
        //Unit selection code
        if (Input.GetMouseButtonDown(0))
        {
            // CheckButtonPressed
            leftMBStartPos = Input.mousePosition;
            isLeftMBDragging = false;
            //selectionBox.gameObject.SetActive(false); // Hide Selection box initially
        }
        if (Input.GetMouseButtonDown(1))
        {
            rightMBStartPos = Input.mousePosition;
            isRightMBDragging = false;
            Debug.Log("draggingMB1 set false1");
        }

        if (Input.GetMouseButton(0))
        {
            // Check if dragging or clicking
            if (Vector2.Distance(leftMBStartPos, Input.mousePosition) > clickThreshold)
            {
                isLeftMBDragging = true;
                UpdateSelectionBox(leftMBStartPos, Input.mousePosition);
            }
        }
        if (Input.GetMouseButton(1))
        {
            // Check if dragging or clicking
            if (Vector2.Distance(rightMBStartPos, Input.mousePosition) > clickThreshold)
            {
                isRightMBDragging = true;
                Debug.Log("draggingMB1 set true");
            }
        }

        //left mouse click
        if (Input.GetMouseButtonUp(0) )
        {
            if (isLeftMBDragging)
            {
                // Finish drag selection
                //selectionBox.gameObject.SetActive(false);
                SelectUnitsWithinBox();
				selectionBox.gameObject.SetActive(false);
            }
            else
            {
                // Perform single click selection
                SelectSingleUnit();
            }
            isLeftMBDragging = false;
        }
        
        //right mouse click
        if (Input.GetMouseButtonDown(1) && _selectedUnits.Count > 0)
        {
            screenPos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if(Physics.Raycast(ray,  out RaycastHit hitData2, 1000, enemyLayerMask)){
                var mech = hitData2.collider.gameObject.GetComponent<BaseMech>();
                foreach (var unit in _selectedUnits){
                    unit.GetComponent<PlayerMech>().CommandSetTarget(mech);
                }
            }
            else RightMouseClick();
        }
        else if (Input.GetMouseButton(1) && _selectedUnits.Count == 1)
        {
            if (isRightMBDragging && GetWorldPosition(out Vector3 world_pos))
            {
                foreach (var unit in _selectedUnits)
                {
                    var player_mech = unit.GetComponent<PlayerMech>();
                    var waypoints = player_mech.Waypoints;
                    if (Vector3.Distance(world_pos, waypoints[waypoints.Length-1]) > 4f)
                    {
                        player_mech.CommandAppendWaypoint(world_pos);
                        GameObject splashEffect = Instantiate(moveTargetEffect, world_pos, transform.rotation);
                        Destroy(splashEffect, 0.5f);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRightMBDragging = false;
            Debug.Log("draggingMB1 set false2");
        }
    }

    void UpdateSelectionBox(Vector2 start, Vector2 end)
    {
        //Turn on the selectionBox if not yet active
		if(!selectionBox.gameObject.activeInHierarchy)
        selectionBox.gameObject.SetActive(true);
	
        //Vector2 center = (start + end) / 2;
		
		//Get new size of rectangle and update its center using anchored position
        Vector2 size = new Vector2(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
        
		selectionBox.sizeDelta = size;
		selectionBox.anchoredPosition = leftMBStartPos + new Vector2((end.x - start.x)/2, (end.y - start.y)/2);
    }
    
    private bool box_made = false;
    private Vector3 rect_prism_center;
    private Vector3 rect_prism_size;

    void SelectUnitsWithinBox()
    {
        const float rect_prism_height = 30f;

        Vector3? get_projection(Ray ray)
        {
            Vector3 origin = ray.origin;
            Vector3 direction = ray.direction;
            if (Mathf.Approximately(direction.y, 0f)) return null; // No intersection
            float t = -origin.y / direction.y;
            if (t < 0)return null; // Intersection is behind the ray origin
            return origin + t * direction;
        }

		Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
		Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);
        var camera = Camera.main;
        // Vector2 center = (max - min) / 2f;

        Vector3? result1 = get_projection(camera.ScreenPointToRay(min));
        Vector3? result2 = get_projection(camera.ScreenPointToRay(max));
        if (result1 == null || result2 == null) return;
        Vector3 min_world = (Vector3)result1;
        Vector3 max_world = (Vector3)result2;


        rect_prism_center = (min_world + max_world) / 2f;
        rect_prism_size = max_world - min_world;
        rect_prism_size.y = rect_prism_height;

        Debug.Log($"center:'{rect_prism_center}' | size:'{rect_prism_size}'");
        box_made = true;

		var player_colliders = Physics.OverlapBox(rect_prism_center, rect_prism_size/2f, Quaternion.identity, unitLayerMask);

		if (!_shiftPressed) { DeSelectAll(); }
        // foreach (var unit in _playerUnits)
        // {
        //     var mech = unit.GetComponent<PlayerMech>();
        //     var screen_space = Camera.main.WorldToScreenPoint(unit.transform.position);
        //     var forward_screen_space = Camera.main.WorldToScreenPoint(unit.transform.position + unit.transform.forward * mech.ScaledRadius);
        //     Vector2 position = new (screen_space.x, screen_space.y);
        //     var to_center = center - position;
        //     float distance = to_center.magnitude;
        //     Vector2 direction = to_center.normalized;
        //     Vector3 projected_pos = position + direction * Mathf.Min(distance, Vector3.Distance(screen_space, forward_screen_space));
        //     if(projected_pos.x > min.x && projected_pos.x < max.x && projected_pos.y > min.y && projected_pos.y < max.y)
		// 	{
		// 		Select(unit);
		// 	}
        // }
        foreach (var collider in player_colliders)
        {
            Debug.Log($"Collision with '{collider.name}'");
            if (_playerUnits.Contains(collider.gameObject))
            {
                Select(collider.gameObject);
            }
        }

    }

    void OnDrawGizmos()
    {
        if (!box_made) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(rect_prism_center, rect_prism_size);
    }

    void SelectSingleUnit()
    {


        screenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if(Physics.Raycast(ray,  out RaycastHit hitData, 1000, unitLayerMask)){
            GameObject clickedUnit = hitData.collider.gameObject;
            DeSelectAll();
            Select(clickedUnit);
            // Add visual feedback or other logic for selection
        }
        else
        {   
            DeSelectAll();
            // Handle deselection if needed
        }
        //transform.position = worldPos;
    
        /*
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = Camera.main.nearClipPlane + 1;
        //Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Debug.Log(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(screenPosition), Vector3.forward, Mathf.Infinity, unitLayerMask);
        transform.position = hit.point;
        */
    }

    /// <summary>
    /// Fetches a Vector3 world position from the Input.mousePosition, returns true if it found one false otherwise.
    /// </summary>
    private bool GetWorldPosition(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        screenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if(Physics.Raycast(ray,  out RaycastHit hitData, 1000, backgroundLayerMask))
        {
            if (!hitData.collider.gameObject.CompareTag("Walkable")) return false;
            worldPos = hitData.point;
            return true;
        }
        return false;
    }

    private void RightMouseClick()
    {
        if (GetWorldPosition(out worldPos))
        {
            GameObject splashEffect = Instantiate(moveTargetEffect, worldPos, transform.rotation);
            Destroy(splashEffect, 0.5f);

            int num_selected = _selectedUnits.Count;
            Vector3 center = Vector3.zero; 
            foreach (var unit in _selectedUnits) center += unit.transform.position;
            center /= num_selected;
            foreach (var unit in _selectedUnits)
            {
                var mech = unit.GetComponent<PlayerMech>();
                if (num_selected == 1) mech.CommandSetWaypoint(worldPos);
                else
                {
                    Vector3 dir_to_mech = (unit.transform.position - center).normalized;
                    mech.CommandSetWaypoint(worldPos + dir_to_mech * mech.ScaledRadius * 1.2f);
                }
            }
        }
    }

    
    public void Select(GameObject unit)
    {
        _selectedUnits.Add(unit);
        unit.GetComponent<PlayerMech>().SetIsSelected(true);
    }

    public List<GameObject> Selected()
    {
        return(_selectedUnits);
    }

    public void DeSelect(GameObject unit)
    {
        if (_selectedUnits.Contains(unit))
        {
            _selectedUnits.Remove(unit);
            unit.GetComponent<PlayerMech>().SetIsSelected(false);
        }
    }

    public void DeSelectAll()
    {
        foreach (var unit in _selectedUnits) unit.GetComponent<PlayerMech>().SetIsSelected(false);
        _selectedUnits.Clear();
    }

    

    public void AssignControlGroups(int groupnumber)
    {
        foreach (var unit in _selectedUnits)
        {
            unitGroups[groupnumber].Add(unit);
        }
        Debug.Log("unit assigned to group " + groupnumber);
    }

    public void CallControlGroups(int grounpnumber)
    {
        DeSelectAll();

        foreach (var unit in unitGroups[grounpnumber])
        {
            if (unit != null && !_selectedUnits.Contains(unit))
            {
                Select(unit);
            }
        }
    }
    public void MenuChange()
    {
        if(SettingsMenu){
            playUI.SetActive(true);
            settingsUI.SetActive(false);
            SettingsMenu = false;
        }
        else{
            playUI.SetActive(false);
            settingsUI.SetActive(true);
            SettingsMenu = true;
        }
        
    }
    
    public void RegisterPlayerMech(PlayerMech playerMech)
    {
        var playermech_gameobject = playerMech.gameObject;
        if (_playerUnits.Contains(playermech_gameobject)) return;
        _playerUnits.Add(playermech_gameobject);
    }
    
    public void UnregisterPlayerMech(PlayerMech playerMech)
    {
        var playermech_gameobject = playerMech.gameObject;
        if (!_playerUnits.Contains(playermech_gameobject)) return;
        _playerUnits.Remove(playermech_gameobject);
    }

    public void ExitButton()
    {
        Application.Quit();
    }
    
}
