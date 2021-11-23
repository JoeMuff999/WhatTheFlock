using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DuckGun : MonoBehaviour
{

    private void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            fire();
        }
    }
    private void fire(){
        Debug.Log("quack");
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Boid");

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, mask.value))
        {
            hit.collider.gameObject.GetComponentInParent<Agent>().destroy();
        }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.yellow, 5);

    }
}