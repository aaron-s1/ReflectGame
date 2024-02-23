using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToExitOnGameOver : MonoBehaviour
{
    Vector3 goal;
    bool canMove;

    float moveSpeed;
    


    public void MoveToGate(Transform goalTransform, float moveSpeed)
    {        
        goal = goalTransform.position;
        this.moveSpeed = moveSpeed;
        canMove = true;
    }

    void FixedUpdate() 
    {
        if (canMove)
            transform.position = Vector3.MoveTowards(transform.position, goal, this.moveSpeed * Time.deltaTime);
    }
}
