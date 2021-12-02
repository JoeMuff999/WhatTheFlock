using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DuckGun : MonoBehaviour
{

    AudioSource quack;
    public float pitch_min;
    public float pitch_max;



    private void Start() {
        quack = GetComponent<AudioSource>();
    }
    private void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            fire();
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
        quack.pitch = Random.Range(pitch_min, pitch_max);
        quack.Play();
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask.value))
        {
            hit.collider.gameObject.GetComponentInParent<Agent>().destroy();
        }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.yellow, 5);

    }
}