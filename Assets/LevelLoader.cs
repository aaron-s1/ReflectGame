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
    [SerializeField] Animator transition;
    [SerializeField] float newLevelTransitionTime = 2f;

    int activeSceneIndex;


    void Awake() 
    {
        if (instance == null)
            instance = this;

        activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        transitionParticlePosition = new Vector3 (0,0,0); // just for now, for testing
    }

    // void Start() => StartCoroutine(LoadScene(true));


    public IEnumerator LoadNextScene() 
    {
        GameManager.Instance.LogCurrentMethod(2, "about to load next scene");
        yield return LoadScene(true);

    }


    public IEnumerator ReloadCurrentScene()
    {
        GameManager.Instance.LogCurrentMethod(2, "about to reload current scene");
        yield return LoadScene(false);
    }


    IEnumerator LoadScene(bool advanceToNextScene = false)
    {
        var newParticle = Instantiate(transitionParticle, transitionParticlePosition, Quaternion.identity);
        newParticle.Play();
        yield return new WaitForSeconds(3f);

        transition.SetTrigger("Start");

        if (advanceToNextScene)
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;

            if (activeSceneIndex + 1 < sceneCount)
                SceneManager.LoadScene(activeSceneIndex + 1);
            else
                GameOver();
        }

        else
            SceneManager.LoadScene(activeSceneIndex);
        
        yield return new WaitForSeconds(newLevelTransitionTime);

        transition.ResetTrigger("Start");
        transitionParticle.Stop();
        GameManager.Instance.LogCurrentMethod(2, "particles / animation stopped");
    }

    void GameOver()
    {
        Debug.Log("Level Loader tried to advance, but found no next scene.");
        // GameManager.Instance.LogCurrentMethod(1);
    }
}
