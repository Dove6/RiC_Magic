using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    private Transform TransformReference;
    private Camera CameraReference;
    private Vector3 MousePosition;

    // Start is called before the first frame update
    void Start()
    {
        TransformReference = GetComponent<Transform>();
        CameraReference = Camera.main;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        MousePosition = CameraReference.ScreenToWorldPoint(Input.mousePosition);
        TransformReference.position = new Vector3(MousePosition.x, MousePosition.y, -1f);
    }
}
