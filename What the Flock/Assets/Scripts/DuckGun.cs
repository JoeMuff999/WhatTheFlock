using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DuckGun : MonoBehaviour
{

    public AudioSource shoot;

    public GameObject Duck;
    public float PitchMin;
    public float PitchMax;

    public float ReloadTime;

    private float reloadTimer;



    private void Start() {
        reloadTimer = 0;
    }
    private void Update() {
        reloadTimer -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            if(reloadTimer < 0)
            {
                fire();
                reloadTimer = ReloadTime;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            PlayerCamera.ZoomCamera();
        }
    }

    private void fire()
    {
        // Debug.Log("quack");
        shoot.Play();
        GameObject duck = Instantiate(Duck, transform.position , Camera.main.transform.rotation);
        

        
            // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.yellow, 5);

    }
}