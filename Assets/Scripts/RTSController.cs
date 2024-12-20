using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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
        if (Input.GetMouseButtonDown(1) && _selectedUnits.Count > 0)
        {
            screenPos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if(Physics.Raycast(ray,  out RaycastHit hitData2, 1000, enemyLayerMask)){
                var mech = hitData2.collider.gameObject.GetComponent<BaseMech>();
                foreach (var unit in _selectedUnits){
                    unit.GetComponent<BaseMech>().CommandSetTarget(mech);
                }
            }
            else RightMouseClick();
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

    void RightMouseClick()
    {
        screenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if(Physics.Raycast(ray,  out RaycastHit hitData, 1000, backgroundLayerMask)){
            worldPos = hitData.point;
            GameObject splashEffect = Instantiate(moveTargetEffect, worldPos, transform.rotation);
            Destroy(splashEffect, 0.5f);

            int num_selected = _selectedUnits.Count;
            Vector3 center = Vector3.zero; 
            foreach (var unit in _selectedUnits) center += unit.transform.position;
            center /= num_selected;
            foreach (var unit in _selectedUnits){
                BaseMech mech = unit.GetComponent<BaseMech>();
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
        unit.GetComponent<BaseMech>().SetIsSelected(true);
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
            unit.GetComponent<BaseMech>().SetIsSelected(false);
        }
    }

    public void DeSelectAll()
    {
        foreach (var unit in _selectedUnits) unit.GetComponent<BaseMech>().SetIsSelected(false);
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
    
    public void ExitButton()
    {
        Application.Quit();
    }
    
}
