using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DuckGun : MonoBehaviour
{

    public AudioSource shoot;

    public GameObject Duck;
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
        shoot.Play();
        GameObject duck = Instantiate(Duck, transform.position , Camera.main.transform.rotation);
    }
}