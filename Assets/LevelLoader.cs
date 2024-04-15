using System.Xml.Schema;
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

    [SerializeField] ParticleSystem levelTransitionParticle;
    [SerializeField] Animator fadeTransition;

    [Tooltip("How long transition particle plays for before next scene loads.")]
    [SerializeField] float preSceneLoadParticlePersistenceLength = 2f;
    // [Tooltip("How long into new scene the transition particle keeps making new particles.")]
    // [SerializeField] float postSceneLoadParticlePersistenceLength = 0.5f;


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


    void Start() 
    {
        StartCoroutine(AsyncLoadNextScene());
        // if (activeSceneIndex == 0)
            
        StartCoroutine(GameManager.Instance.DelayHeroAttacksOnSceneLoad());
    }
    
    
    IEnumerator AsyncLoadNextScene()
    {
        // for (int i = 1; i < sceneCount; i++)
        // {
            // if (i == 0)
                // continue;
            // Debug.Log("async loading scene: index: " + SceneManager.LoadSceneAsync(SceneManager.GetSceneAt(i).name));

            // level 6, index 3.
            // 6 - 1. 
            // 
            
            var nextSceneName = SceneUtility.GetScenePathByBuildIndex(activeSceneIndex + 1);
            Debug.Log(nextSceneName);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName); //($SceneManager.GetSceneAt(i).ToString());
            asyncLoad.allowSceneActivation = false;

            // Debug.Log("Loading scene: " + SceneManager.GetSceneAt(i).name);

            while (!asyncLoad.isDone)
                yield return null;
        // }
    }



    public IEnumerator LoadNextScene() 
    {
        if (activeSceneIndex + 1 < sceneCount)
        {
            var newTransitionParticle = Instantiate(levelTransitionParticle, levelTransitionParticle.transform.position, Quaternion.identity);
            
            yield return new WaitForSeconds(preSceneLoadParticlePersistenceLength);
            SceneManager.LoadScene(activeSceneIndex + 1);
            // PlayerController.Instance.CanReflect(false);
        }
        
        else
            StartCoroutine(EndGame.Instance.BeginEndingGame());
    }


    public IEnumerator ReloadCurrentScene()
    {
        // Debug.Log("Level Loader is attempting to REload next scene.");
        yield return StartCoroutine(HandleCameraFadeAnimation());
        
        SceneManager.LoadScene(activeSceneIndex);
        yield break;
    }



    // The end transition already occurs automatically on new scene load.
    IEnumerator HandleCameraFadeAnimation()
    {        
        fadeTransition.SetTrigger("Start");

        var fadeInfoClip = fadeTransition.GetCurrentAnimatorClipInfo(0)[0].clip;

        if (fadeInfoClip != null)
            fadeTransitionTime = fadeInfoClip.length;

        yield return new WaitForSeconds(fadeTransitionTime);
    }
}
