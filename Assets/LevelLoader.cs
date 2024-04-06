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
    // [SerializeField] Vector3 transitionParticlePosition;
    [SerializeField] Animator fadeTransition;
    [Tooltip("How long transition particle plays for before next scene loads.")]
    [SerializeField] float preSceneLoadParticlePersistenceLength = 2f;
    [Tooltip("How long into new scene the transition particle keeps making new particles.")]
    [SerializeField] float postSceneLoadParticlePersistenceLength = 0.5f;



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
        if (activeSceneIndex == 0)
            StartCoroutine(AsyncLoadAllScenesOnGameStart());        
    }
    
    
    IEnumerator AsyncLoadAllScenesOnGameStart()
    {
        for (int i = 0; i < sceneCount; i++)
        {
            if (i == 0)
                continue;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetSceneAt(i).ToString());

            while (!asyncLoad.isDone)
                yield return null;
        }
    }



    public IEnumerator LoadNextScene() 
    {
        if (activeSceneIndex + 1 < sceneCount)
        {
            var newTransitionParticle = Instantiate(levelTransitionParticle, levelTransitionParticle.transform.position, Quaternion.identity);
            
            yield return new WaitForSeconds(preSceneLoadParticlePersistenceLength);
            newTransitionParticle.GetComponent<PlayStartingOnSceneEnd>().PlayIntoNextScene(postSceneLoadParticlePersistenceLength);

            // Debug.Log($"Waited for {newLevelTransitionTime} for transition particle");

            SceneManager.LoadScene(activeSceneIndex + 1);

            // fadeTransition.ResetTrigger("Start");
            // if (levelTransitionParticle == null)
                // levelTransitionParticle = GameObject.FindGameObjectWithTag("TransitionParticle");
                
                // var particleMain = newTransitionParticle.main;
                // particleMain.loop = false;            
            // levelTransitionParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        
        else
            StartCoroutine(EndGame.Instance.BeginEndingGame());
    }


    public IEnumerator ReloadCurrentScene()
    {
        Debug.Log("Level Loader is attempting to REload next scene.");
        yield return StartCoroutine(HandleFadeAnimation());
        
        SceneManager.LoadScene(activeSceneIndex);
        yield break;
    }


    // The end transition already occurs automatically on new scene load.
    IEnumerator HandleFadeAnimation()
    {        
        fadeTransition.SetTrigger("Start");
        fadeTransitionTime = fadeTransition.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        yield return new WaitForSeconds(fadeTransitionTime);
    }
}
