using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{

    Vector2 rotation = Vector2.zero;
    public float DefaultSpeed = 3;
    public float ZoomedSpeed = 0.5f;
    private static bool isZoomed = false;

    public static float ZoomedFov = 15.0f;
    public static float DefaultFov = 90.0f;
    private void Start() {
        Cursor.visible = false;
    }
    void Update () {
        float speed = isZoomed ? ZoomedSpeed : DefaultSpeed;
        Debug.Log(speed);
        float mouseXDelta = Input.GetAxis ("Mouse X");
        float mouseYDelta = Input.GetAxis("Mouse Y");
        Vector3 rotation = new Vector3(-mouseYDelta, mouseXDelta, 0);
        transform.eulerAngles += (rotation * speed);
    }

    public static void ZoomCamera()
    {
        if(isZoomed)
            Camera.main.fieldOfView = DefaultFov;
        else
            Camera.main.fieldOfView = ZoomedFov;
        isZoomed = !isZoomed;
    }
}
