using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillHeroAsItExitsGate : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            EndGameTest.Instance.heroesLeftToDestroy--;
            Destroy(other.gameObject);
        }
    }
}
