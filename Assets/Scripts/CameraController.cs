using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float scrollZoomSpeed;
    [SerializeField] private float cameraSpeed;
	[SerializeField] private float cameraRotationSpeed;
    [SerializeField] private float maxHeight;
    [SerializeField] private float minHeight;
	private Vector3 currentEulerAngles;
	private Vector3 forward;
	
	void Start()
	{
		currentEulerAngles = transform.localEulerAngles;
	}

    void Update()
    {
        if(Input.GetKey(KeyCode.W)) {
            transform.position += new Vector3(transform.forward.x, 0f, transform.forward.z) * Time.deltaTime * cameraSpeed;
        }
        if(Input.GetKey(KeyCode.A)) {
			forward = new Vector3(-transform.forward.z, 0f, transform.forward.x);
            transform.position += new Vector3(forward.x, 0f, forward.z) * Time.deltaTime * cameraSpeed;
        }
        if(Input.GetKey(KeyCode.S)) {
            transform.position -= new Vector3(transform.forward.x, 0f, transform.forward.z) * Time.deltaTime * cameraSpeed;
        }
        if(Input.GetKey(KeyCode.D)) {
			forward = new Vector3(-transform.forward.z, 0f, transform.forward.x);
            transform.position -= new Vector3(forward.x, 0f, forward.z) * Time.deltaTime * cameraSpeed;
        }
		if(Input.GetKey(KeyCode.Q)) {
			currentEulerAngles -= new Vector3(0f, 1f, 0f) * Time.deltaTime * cameraRotationSpeed;
			transform.localEulerAngles = currentEulerAngles;
        }
		if(Input.GetKey(KeyCode.E)) {
			currentEulerAngles += new Vector3(0f, 1f, 0f) * Time.deltaTime * cameraRotationSpeed;
			transform.localEulerAngles = currentEulerAngles;
        }
        Vector3 pos = transform.position;
        if((pos.y - (Input.mouseScrollDelta.y * scrollZoomSpeed)) > minHeight && (pos.y - (Input.mouseScrollDelta.y * scrollZoomSpeed)) < maxHeight){
            pos.y -= Input.mouseScrollDelta.y * scrollZoomSpeed;
            // pos.z += Input.mouseScrollDelta.y * scrollZoomSpeed;
        }
        transform.position = pos;
    }
}
