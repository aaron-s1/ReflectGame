using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToExitOnGameOver : MonoBehaviour
{
    Vector3 target_X_Pos;
    bool canMove;

    float moveSpeed;

    // Vector3 originalPos;

    public void MoveToGate(Transform entrywayGate, float moveSpeed)
    {
        // originalPos = transform.position;
        target_X_Pos = new Vector3(entrywayGate.position.x, transform.position.y, transform.position.z);
        this.moveSpeed = moveSpeed;
        canMove = true; 
    }

    void FixedUpdate() 
    {
        if (canMove)
            transform.position = Vector3.MoveTowards(transform.position, target_X_Pos, this.moveSpeed * Time.deltaTime);
    }
}
