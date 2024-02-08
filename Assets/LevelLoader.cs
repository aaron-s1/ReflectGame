using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

    // Rename later.
    // public class SceneLoader : MonoBehaviour
public class LevelLoader : MonoBehaviour
{
    [HideInInspector] public static LevelLoader Instance { get { return instance; } }
    [HideInInspector] public static LevelLoader levelLoader;
    static LevelLoader instance;

    [SerializeField] ParticleSystem transitionParticle;
    // [SerializeField] Vector3 transitionParticlePosition;
    [SerializeField] Animator fadeTransition;
    [SerializeField] float newLevelTransitionTime = 2f;

    [Space(10)]    
    [SerializeField] [Tooltip("Only used on Level 1 (final level.)")] GameObject reflectSequenceUIs;
    [SerializeField] [Tooltip("Only used on Level 1 (final level.)")] GameObject gameWonVictoryParticleObj;

    int activeSceneIndex;
    int sceneCount;

    float fadeTransitionTime;



    void Awake() 
    {
        if (instance == null)
            instance = this;

        activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        sceneCount = SceneManager.sceneCountInBuildSettings;
    }






    public IEnumerator LoadNextScene() 
    {
        Debug.Log("Level Loader is attempting to load next scene.");


        if (activeSceneIndex + 1 < sceneCount)
        {
            Instantiate(transitionParticle, transitionParticle.transform.position, Quaternion.identity);
            
            yield return new WaitForSeconds(newLevelTransitionTime);

            SceneManager.LoadScene(activeSceneIndex + 1);

            fadeTransition.ResetTrigger("Start");
            transitionParticle.Stop();
        }
        
        else
        {
            GameOver();
        }
    }


    public IEnumerator ReloadCurrentScene()
    {
        Debug.Log("Level Loader is attempting to REload next scene.");
        yield return StartCoroutine(HandleFadeAnimation());
        
        SceneManager.LoadScene(activeSceneIndex);
        yield break;
    }


    // End transition occurs automatically on new scene load.
    IEnumerator HandleFadeAnimation()
    {
        fadeTransition.SetTrigger("Start");
        fadeTransitionTime = fadeTransition.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(fadeTransitionTime);
    }



    void GameOver()
    {
        if (reflectSequenceUIs != null)
            reflectSequenceUIs.SetActive(false);

        if (gameWonVictoryParticleObj)
            Instantiate(gameWonVictoryParticleObj, PlayerController.Instance.transform.position, Quaternion.identity);
        


        Debug.Log("Level Loader tried to advance, but found no next scene.");
    }
}
