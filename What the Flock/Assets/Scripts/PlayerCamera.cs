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
        float mouseXDelta = Input.GetAxis ("Mouse X");
        float mouseYDelta = Input.GetAxis("Mouse Y");
        Vector3 rotation = new Vector3(-mouseYDelta, mouseXDelta, 0);
        // float new_x = transform.localEulerAngles.x + rotation.x*speed;
        // Debug.Log(transform.localEulerAngles);
        // float clamp_x = Mathf.Clamp(new_x, -89.0f, 89.0f);
        // if(new_x != clamp_x)
        //     Debug.Log(new_x);

        // Vector3 new_euler_angles = new Vector3(clamp_x, transform.eulerAngles.y + rotation.y*speed, transform.eulerAngles.z);
        transform.eulerAngles += rotation * speed;

         if (Input.GetKey ("escape")) {
                 Application.Quit();
        }
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
