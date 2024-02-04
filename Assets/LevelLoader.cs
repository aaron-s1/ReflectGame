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
    [SerializeField] Vector3 transitionParticlePosition;
    [SerializeField] Animator fadeTransition;
    [SerializeField] float newLevelTransitionTime = 2f;

    int activeSceneIndex;
    int sceneCount;



    void Awake() 
    {
        if (instance == null)
            instance = this;

        activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        sceneCount = SceneManager.sceneCountInBuildSettings;
        transitionParticlePosition = new Vector3 (0,0,0); // just for now, for testing
    }

    // void Start() => StartCoroutine(LoadScene(true));


    public IEnumerator LoadNextScene() 
    {
        GameManager.Instance.LogCurrentMethod(2, "about to load next scene");

        var newTransitionParticle = Instantiate(transitionParticle, transitionParticlePosition, Quaternion.identity);
        newTransitionParticle.Play();
        yield return new WaitForSeconds(3f);

        fadeTransition.SetTrigger("Start");


        if (activeSceneIndex + 1 < sceneCount)
            SceneManager.LoadScene(activeSceneIndex + 1);
        else
            GameOver();

        
        yield return new WaitForSeconds(newLevelTransitionTime);

        fadeTransition.ResetTrigger("Start");
        transitionParticle.Stop();
        GameManager.Instance.LogCurrentMethod(2, "particles / animation stopped");
    }


    public IEnumerator ReloadCurrentScene()
    {
        GameManager.Instance.LogCurrentMethod(2, "about to reload current scene");
        fadeTransition.SetTrigger("Start");
        yield return new WaitForSeconds(fadeTransition.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        
        SceneManager.LoadScene(activeSceneIndex);
        yield break;
    }


    void GameOver()
    {
        Debug.Log("Level Loader tried to advance, but found no next scene.");
    }
}
