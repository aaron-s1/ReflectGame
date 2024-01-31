using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelUpwardsOnEnable : MonoBehaviour
{
    [SerializeField] float moveDistance;
    [SerializeField] float moveDuration;

    Vector3 startPosition;
    Vector3 endPosition;

    float elapsedTime;

    void Awake() =>
        startPosition = transform.localPosition;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime <= moveDuration)
            transform.localPosition = Vector3.Lerp(transform.localPosition, endPosition, elapsedTime / moveDuration);
    }

    void OnEnable()
    {
        elapsedTime = 0;
        startPosition = transform.localPosition;
        endPosition = new Vector3(startPosition.x, startPosition.y + moveDistance, startPosition.z);
    }

    void OnDisable() 
    {
        elapsedTime = 0;
        transform.localPosition = startPosition;
    }
}
