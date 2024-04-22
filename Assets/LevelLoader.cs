using System.Xml.Schema;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelLoader : MonoBehaviour
{
    [HideInInspector] public static LevelLoader Instance { get { return instance; } }
    [HideInInspector] public static LevelLoader levelLoader;
    static LevelLoader instance;

    [SerializeField] ParticleSystem levelTransitionParticle;
    [SerializeField] Animator fadeTransition;

    [Tooltip("How long transition particle plays for before next scene loads.")]
    [SerializeField] float preSceneLoadParticlePersistenceLength = 2f;


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
        if (activeSceneIndex != (sceneCount - 1))
            StartCoroutine(AsyncLoadNextScene());
            
        StartCoroutine(GameManager.Instance.DelayHeroAttacksOnSceneLoad());
    }
    
    
    IEnumerator AsyncLoadNextScene()
    {
        var nextSceneName = SceneUtility.GetScenePathByBuildIndex(activeSceneIndex + 1);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
            yield return null;
    }



    public IEnumerator LoadNextScene() 
    {
        if (activeSceneIndex + 1 < sceneCount)
        {
            var newTransitionParticle = Instantiate(levelTransitionParticle, levelTransitionParticle.transform.position, Quaternion.identity);
            
            yield return new WaitForSeconds(preSceneLoadParticlePersistenceLength);
            SceneManager.LoadScene(activeSceneIndex + 1);
        }
        
        else
            StartCoroutine(EndGame.Instance.BeginEndingGame());
    }


    public IEnumerator ReloadCurrentScene()
    {
        yield return StartCoroutine(HandleCameraFadeAnimation());
        
        SceneManager.LoadScene(activeSceneIndex);
        yield break;
    }



    // The end transition already occurs automatically on new scene load.
    IEnumerator HandleCameraFadeAnimation()
    {        
        fadeTransition.SetTrigger("Start");

        if (fadeTransition?.GetCurrentAnimatorClipInfo(0)[0].clip != null)
            fadeTransitionTime = fadeTransition.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        yield return new WaitForSeconds(fadeTransitionTime);
    }
}
