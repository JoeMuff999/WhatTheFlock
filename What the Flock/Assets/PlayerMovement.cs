using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MovementSpeed = 1.0f;
    public float Gravity = 9.8f;
    public CharacterController CharacterController;

    // Update is called once per frame
    void Update()
    {
        Vector3 movementVec = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            movementVec += getNoYNormalized(transform.forward);
        }
        if (Input.GetKey(KeyCode.S))
        {
            movementVec -= getNoYNormalized(transform.forward);
        }
        if (Input.GetKey(KeyCode.D))
        {
            movementVec += transform.right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movementVec -= transform.right;
        }


        movementVec = movementVec.normalized * MovementSpeed * Time.deltaTime;
        Vector3 gravity = new Vector3(0, -Gravity, 0) * Time.deltaTime;
        CharacterController.Move(movementVec + gravity);
        
    }

    //this is pass by value
    private Vector3 getNoYNormalized(Vector3 vecToConvert)
    {
        vecToConvert.y = 0;
        vecToConvert = vecToConvert.normalized;
        return vecToConvert;
    }
}
