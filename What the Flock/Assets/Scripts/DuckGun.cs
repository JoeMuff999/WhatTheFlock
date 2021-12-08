using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DuckGun : MonoBehaviour
{

    AudioSource quack;
    public float PitchMin;
    public float PitchMax;

    public float ReloadTime;

    private float reloadTimer;



    private void Start() {
        quack = GetComponent<AudioSource>();
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
    private void fire(){
        Debug.Log("quack");
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Boid");
        quack.pitch = Random.Range(PitchMin, PitchMax);
        quack.Play();
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask.value))
        {
            hit.collider.gameObject.GetComponentInParent<Agent>().destroy();
        }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.yellow, 5);

    }
}