using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float scrollZoomSpeed;
    [SerializeField] private float cameraSpeed;
    [SerializeField] private float maxHeight;
    [SerializeField] private float minHeight;

    void Update()
    {
        if(Input.GetKey(KeyCode.W)) {
            transform.position += Vector3.forward * Time.deltaTime * cameraSpeed;
        }
        if(Input.GetKey(KeyCode.A)) {
            transform.position += Vector3.left * Time.deltaTime * cameraSpeed;
        }
        if(Input.GetKey(KeyCode.S)) {
            transform.position += Vector3.back * Time.deltaTime * cameraSpeed;
        }
        if(Input.GetKey(KeyCode.D)) {
            transform.position += Vector3.right * Time.deltaTime * cameraSpeed;
        }
        Vector3 pos = transform.position;
        if((pos.y - (Input.mouseScrollDelta.y * scrollZoomSpeed)) > minHeight && (pos.y - (Input.mouseScrollDelta.y * scrollZoomSpeed)) < maxHeight){
            pos.y -= Input.mouseScrollDelta.y * scrollZoomSpeed;
            // pos.z += Input.mouseScrollDelta.y * scrollZoomSpeed;
        }
        transform.position = pos;
    }
}
