using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CameraController : MonoBehaviour
{
    [SerializeField] float scrollZoomSpeed;
    [SerializeField] float cameraSpeed;

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
        if((pos.y - (Input.mouseScrollDelta.y * scrollZoomSpeed)) > 10 && (pos.y - (Input.mouseScrollDelta.y * scrollZoomSpeed)) < 80){
            pos.y -= Input.mouseScrollDelta.y * scrollZoomSpeed;
            pos.z += Input.mouseScrollDelta.y * scrollZoomSpeed;
        }
        transform.position = pos;
    }
}
