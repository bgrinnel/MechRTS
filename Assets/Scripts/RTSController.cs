using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RTSController : MonoBehaviour
{
    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private Vector3 screenPos;
    [SerializeField] private Vector3 worldPos;
    [SerializeField] private LayerMask unitLayerMask;
    [SerializeField] private LayerMask backgroundLayerMask;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private float clickThreshold = 20f; // To distinguish between click and drag

    private Vector2 startPos;
    [SerializeField] CameraController camState;

    private bool isDragging = false;
    [SerializeField] private GameObject moveTargetEffect;


    [SerializeField] private List<GameObject> _playerUnits = new List<GameObject>();

    [SerializeField] private List<GameObject> _selectedUnits = new List<GameObject>();
	
	[SerializeField] private List<Vector3> _waypointQueue = new List<Vector3>();
	
    public bool _shiftPressed;
    public bool _holdingCtrl;
    public List<GameObject>[] unitGroups = new List<GameObject>[10];

    private void Awake()
    {
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
        MouseInput();
    }
    void MouseInput()
    {
        
        //Unit selection code
        if (Input.GetMouseButtonDown(0))
        {
            // CheckButtonPressed
            startPos = Input.mousePosition;
            isDragging = false;
            //selectionBox.gameObject.SetActive(false); // Hide Selection box initially
        }

        if (Input.GetMouseButton(0))
        {
            // Check if dragging or clicking
            if (Vector2.Distance(startPos, Input.mousePosition) > clickThreshold)
            {
                isDragging = true;
                //selectionBox.gameObject.SetActive(true);
                UpdateSelectionBox(startPos, Input.mousePosition);
            }
        }
        //left mouse click
        if (Input.GetMouseButtonUp(0) )
        {
            if (isDragging)
            {
                // Finish drag selection
                //selectionBox.gameObject.SetActive(false);
                SelectUnitsWithinBox();
				selectionBox.gameObject.SetActive(false);
                isDragging = false;
            }
            else
            {
                // Perform single click selection
                SelectSingleUnit();
            }
        }
        
        //right mouse click
        if (Input.GetMouseButtonDown(1))
        {
            screenPos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if(Physics.Raycast(ray,  out RaycastHit hitData2, 1000, enemyLayerMask)){
                var mech = hitData2.collider.gameObject.GetComponent<MechBehavior>();
                foreach (var unit in _selectedUnits){
                    unit.GetComponent<MechBehavior>().CommandSetTarget(mech);
                }
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
			if (_shiftPressed)
			{
				//select waypoints for chain movement while shift is down
				ShiftRightMouseClick();
			}
			else
			{
				//Release shift after waypoint chain and right click to execute path
				if (_waypointQueue.Count > 0)
				{
					StartCoroutine(ChainMovementCoroutine());
				}
				else
				{
					//Single waypoint
					RightMouseClick();
				}
			}
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
		selectionBox.anchoredPosition = startPos + new Vector2((end.x - start.x)/2, (end.y - start.y)/2);
    }

    void SelectUnitsWithinBox()
    {
		/*
        Vector3 start = Camera.main.ScreenToWorldPoint(new Vector3(startPos.x, startPos.y, Camera.main.transform.position.z)) * -1;
        Vector3 end = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z)) * -1;
       
       

        Vector3 center = end -start;


        Vector3 size = new Vector3(end.x - start.x, end.y - start.y,  Camera.main.nearClipPlane + 1);
       //Debug.Log("size " + size);
        var unitsBoxed = Physics.OverlapBox(center,size , Quaternion.identity, unitLayerMask);
        //var unitsBoxed = Physics2D.OverlapAreaAll(start, end);
        if (!_shiftPressed) { DeSelectAll(); }
        foreach (var unit in _playerUnits)
        {
            if (!_selectedUnits.Contains(unit.gameObject))
            {
                _selectedUnits.Add(unit.gameObject);
                unit.GetComponent<MechBehavior>().SetIsSelected(true);
            }
        }
		*/
		Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
		Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);
		
		if (!_shiftPressed) { DeSelectAll(); }
		
		foreach(var unit in _playerUnits)
		{
			Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
        
			if(screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
			{
				Select(unit);
			}
		}
    }

    void SelectSingleUnit()
    {


        screenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if(Physics.Raycast(ray,  out RaycastHit hitData, 100, unitLayerMask)){
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

    void RightMouseClick()
    {
        screenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if(Physics.Raycast(ray,  out RaycastHit hitData, 1000, backgroundLayerMask)){
            worldPos = hitData.point;
            GameObject splashEffect = Instantiate(moveTargetEffect, worldPos, transform.rotation);
            Destroy(splashEffect, 0.5f);

            foreach (var unit in _selectedUnits){
                MechBehavior mech = unit.GetComponent<MechBehavior>();
                mech.CommandSetWaypoint(worldPos);
            }
        }
        
    }
	
	//Same as above except we add each point to a waypoint queue to execute later
	void ShiftRightMouseClick()
	{
		screenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
		if(Physics.Raycast(ray,  out RaycastHit hitData, 1000, backgroundLayerMask)){
            worldPos = hitData.point;
			_waypointQueue.Add(worldPos);
            GameObject splashEffect = Instantiate(moveTargetEffect, worldPos, transform.rotation);
            Destroy(splashEffect, 0.5f);
		}
	}
	
	//Use a coroutine to iterate to a waypoint after the previous one has been reached
	IEnumerator<MechBehavior> ChainMovementCoroutine()
	{
    foreach(var unit in _selectedUnits) {
        MechBehavior mech = unit.GetComponent<MechBehavior>();
		foreach (var waypoint in _waypointQueue) {
			mech.CommandSetWaypoint(waypoint);
			while (!mech.HasReachedDestination()) {yield return null;}
			}
		}
		//Make sure queue is clear after chain movement is done
		_waypointQueue.Clear();
	}

    
    public void Select(GameObject unit)
    {
        _selectedUnits.Add(unit);
        unit.GetComponent<MechBehavior>().SetIsSelected(true);
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
            unit.GetComponent<MechBehavior>().SetIsSelected(false);
        }
    }

    public void DeSelectAll()
    {
        foreach (var unit in _selectedUnits) unit.GetComponent<MechBehavior>().SetIsSelected(false);
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
    
}
