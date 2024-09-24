using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class RTSController : MonoBehaviour
{
    public RectTransform selectionBox;
    public Vector3 screenPos;
    public Vector3 worldPos;
    public LayerMask unitLayerMask;
    public LayerMask background;
    public LayerMask enemy;
    public float clickThreshold = 0.5f; // To distinguish between click and drag

    private Vector2 startPos;
    [SerializeField] CameraController camState;
    private bool isDragging = false;
    [SerializeField] private GameObject moveTargetEffect;

    private void Start()
    {
        selectionBox.gameObject.SetActive(false);
    }
    void Update()
    {
        MouseInput();
        ShiftAndCtrlInput();
        ControlGroupsInput();
    }
    void MouseInput()
    {
        /*
        //Unit selection code
        if (Input.GetMouseButtonDown(0))
        {
            // CheckButtonPressed
            startPos = Input.mousePosition;
            isDragging = false;
            selectionBox.gameObject.SetActive(false); // Hide Selection box initially
        }

        if (Input.GetMouseButton(0)&& camState.getSettingsMenu() == false)
        {
            // Check if dragging or clicking
            if (Vector2.Distance(startPos, Input.mousePosition) > clickThreshold)
            {
                isDragging = true;
                selectionBox.gameObject.SetActive(true);
                UpdateSelectionBox(startPos, Input.mousePosition);
            }
        }
        //left mouse click
        if (Input.GetMouseButtonUp(0) && camState.getSettingsMenu() == false)
        {
            if (isDragging)
            {
                // Finish drag selection
                selectionBox.gameObject.SetActive(false);
                SelectUnitsWithinBox();
                isDragging = false;
            }
            else
            {
                // Perform single click selection
                SelectSingleUnit();
            }
        }
        */
        //right mouse click
        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if(Physics.Raycast(ray,  out RaycastHit hitData2, 1000, enemy)){
                Transform clickedEnemy = hitData2.collider.transform;
                foreach (var unit in UnitController.instance.Selected()){
                    Ship_Follow_Script pass_Script = unit.GetComponent<Ship_Follow_Script>();
                    pass_Script.PassTarget(clickedEnemy);
                }
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            RightMouseClick();
        }
    }
    void UpdateSelectionBox(Vector2 start, Vector2 end)
    {
        Vector2 center = (start + end) / 2;
        selectionBox.position = center;

        Vector2 size = new Vector2(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
        selectionBox.sizeDelta = size;
    }

    void SelectUnitsWithinBox()
    {

        Vector3 start = Camera.main.ScreenToWorldPoint(new Vector3(startPos.x, startPos.y, Camera.main.transform.position.z)) * -1;
        Vector3 end = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z)) * -1;
       
       

        Vector3 center = end -start;


        Vector3 size = new Vector3(end.x - start.x, end.y - start.y,  Camera.main.nearClipPlane + 1);
       //Debug.Log("size " + size);
        var unitsBoxed = Physics.OverlapBox(center,size , Quaternion.identity, unitLayerMask);
        //var unitsBoxed = Physics2D.OverlapAreaAll(start, end);
        UnitController.instance.DragSelect(unitsBoxed);
    }

    void SelectSingleUnit()
    {


        screenPos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if(Physics.Raycast(ray,  out RaycastHit hitData, 100, unitLayerMask)){
            GameObject clickedUnit = hitData.collider.gameObject;
            UnitController.instance.DeSelectAll();
            UnitController.instance.Select(clickedUnit);
            // Add visual feedback or other logic for selection
        }
        else
        {   
            UnitController.instance.DeSelectAll();
            // Handle deselection if needed
        }
        transform.position = worldPos;
    
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
        if(Physics.Raycast(ray,  out RaycastHit hitData, 1000, background)){
            worldPos = hitData.point;
            GameObject splashEffect = Instantiate(moveTargetEffect, worldPos, transform.rotation);
            Destroy(splashEffect, 0.5f);
            //GameObject[] selectedUnits = UnitController.instance.Selected();
            foreach (var unit in UnitController.instance.Selected()){
                Ship_Follow_Script pass_Script = unit.GetComponent<Ship_Follow_Script>();
                pass_Script.PassDestination(worldPos,UnitController.instance._shiftPressed);
            }
        }
        
    }

    void ShiftAndCtrlInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            UnitController.instance._shiftPressed = true;
        }
        else
        {
            UnitController.instance._shiftPressed = false;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            UnitController.instance._holdingCtrl = true;
        }
        else
        {
            UnitController.instance._holdingCtrl = false;
        }
    }

    void ControlGroupsInput()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Alpha0)) { UnitController.instance.AssignControlGroups(0); }
            else if (Input.GetKeyDown(KeyCode.Alpha1)) { UnitController.instance.AssignControlGroups(1); }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) { UnitController.instance.AssignControlGroups(2); }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) { UnitController.instance.AssignControlGroups(3); }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) { UnitController.instance.AssignControlGroups(4); }
            else if (Input.GetKeyDown(KeyCode.Alpha5)) { UnitController.instance.AssignControlGroups(5); }
            else if (Input.GetKeyDown(KeyCode.Alpha6)) { UnitController.instance.AssignControlGroups(6); }
            else if (Input.GetKeyDown(KeyCode.Alpha7)) { UnitController.instance.AssignControlGroups(7); }
            else if (Input.GetKeyDown(KeyCode.Alpha8)) { UnitController.instance.AssignControlGroups(8); }
            else if (Input.GetKeyDown(KeyCode.Alpha9)) { UnitController.instance.AssignControlGroups(9); }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha0)) { UnitController.instance.CallControlGroups(0); }
            else if (Input.GetKeyDown(KeyCode.Alpha1)) { UnitController.instance.CallControlGroups(1); }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) { UnitController.instance.CallControlGroups(2); }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) { UnitController.instance.CallControlGroups(3); }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) { UnitController.instance.CallControlGroups(4); }
            else if (Input.GetKeyDown(KeyCode.Alpha5)) { UnitController.instance.CallControlGroups(5); }
            else if (Input.GetKeyDown(KeyCode.Alpha6)) { UnitController.instance.CallControlGroups(6); }
            else if (Input.GetKeyDown(KeyCode.Alpha7)) { UnitController.instance.CallControlGroups(7); }
            else if (Input.GetKeyDown(KeyCode.Alpha8)) { UnitController.instance.CallControlGroups(8); }
            else if (Input.GetKeyDown(KeyCode.Alpha9)) { UnitController.instance.CallControlGroups(9); }
        }
    }
}
