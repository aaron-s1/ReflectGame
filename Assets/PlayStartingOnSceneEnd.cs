using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayStartingOnSceneEnd : MonoBehaviour
{
    public IEnumerator PlayIntoNextScene(float timeBeforeStopping)
    {
        var particleMain = GetComponent<ParticleSystem>().main;

        yield return new WaitForSeconds(timeBeforeStopping);
        particleMain.loop = false;

        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
