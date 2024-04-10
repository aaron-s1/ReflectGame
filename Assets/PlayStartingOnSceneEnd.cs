// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayStartingOnSceneEnd : MonoBehaviour
// {
//     ////////////////
//     public IEnumerator PlayIntoNextScene(float timeBeforeStopping)
//     {
//         yield break;
//         Debug.Log("PlayIntoNextScene triggered");
//         GameManager.Instance.heroesCanAttack = false;
//         Debug.Log("current canContinueAttacks value: " + GameManager.Instance.heroesCanAttack);
//         var particleMain = GetComponent<ParticleSystem>().main;

//         Debug.Log("PlayIntoNextScene began waiting");
//         yield return new WaitForSeconds(10f);
//         Debug.Log("PlayIntoNextScene stopped waiting");
//         particleMain.loop = false;

        
//         // GameManager.Instance.canContinueAttacks = true;
//         Debug.Log("PlayStartingOnSceneEnd.cs turned on attacks DESPITE NOT WAITING FIRST???");
//         Debug.Log("Allowed GameManager to continue attacks.");

//         Destroy(gameObject);
//     }
// }
