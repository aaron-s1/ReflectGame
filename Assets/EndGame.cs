using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.HealthSystemCM;
using UnityEngine;
using UnityEngine.SceneManagement;



public class EndGame : MonoBehaviour
{
//////////  
// RENAME LATER.
//////////  

    static EndGame instance;
    [HideInInspector] public static EndGame Instance { get { return instance; } }

    [Space(10)]    
    [SerializeField] GameObject enemyHeroes;
    [SerializeField] GameObject reflectSequenceUIs;

    [SerializeField] GameObject youWinVictoryText;
    [SerializeField] GameObject thanksForPlayingText;    
    [SerializeField] ParticleSystem gameWonVictoryParticleObj;
    [Space(10)]
    [SerializeField] Animator entryWayGateAnim;

    [SerializeField] Transform entryGateHeroKillPoint;
    [SerializeField] float heroWalkSpeedToGate;
    [Space(10)]
    [SerializeField] float slimeSpriteSwapSpeed;

    [HideInInspector] public int heroesLeftToDestroy = 3;

    Vector3 victoryParticlePosOffset;

    Animator cameraAnim;

    int currentSceneIndex = -1;

    [SerializeField] Sprite[] slimeColorSprites;


    void Start()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;


        if (currentSceneIndex + 1 >= SceneManager.sceneCountInBuildSettings)
        {
            if (instance == null)
                instance = this;

            cameraAnim = Camera.main.GetComponent<Animator>();
        }

        // temp for testing.
        StartCoroutine(SwapSlimeSpritesOverTime());
    }



    public IEnumerator BeginEndingGame()
    {
        if (instance == null)
        {
            Debug.Log("EndGame.cs has no instance!");
            yield break;
        }

        Debug.Log("Level Loader tried to advance, but found no next scene.");

        Time.timeScale = 1f;

        DeleteReflectUI();
        DeleteHealthBars();


        Debug.Break();
        yield return StartCoroutine(FadeEnemiesBackIn());
        Debug.Break();
        yield return StartCoroutine(PanCameraToGate());
        Debug.Break();
        yield return StartCoroutine(OpenGate());
        Debug.Break();
        yield return StartCoroutine(EnemiesMoonwalkToGate());
        Debug.Break();
        yield return StartCoroutine(PanCameraToPlayer());


        Instantiate(gameWonVictoryParticleObj, PlayerController.Instance.transform.position + victoryParticlePosOffset, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        youWinVictoryText.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        thanksForPlayingText.SetActive(true);   

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SwapSlimeSpritesOverTime());
    }

    void DeleteHealthBars()
    {
        // foreach (Transform enemyHealth in enemyHeroes.transform)
            // enemyHealth.gameObject.GetComponentInChildren<HealthBarUI>().gameObject.SetActive(false);

        PlayerController.Instance.GetComponentInChildren<HealthBarUI>().gameObject.SetActive(false);
    }

    void DeleteReflectUI()
    {
        if (reflectSequenceUIs != null)
            reflectSequenceUIs.SetActive(false);
    }


    IEnumerator FadeEnemiesBackIn()
    {
        foreach (Transform enemy in enemyHeroes.transform)
        {
            enemy.position = enemy.gameObject.GetComponent<FireAttack>().originalPosition;            
            enemy.gameObject.SetActive(true);
            
            StartCoroutine(enemy.gameObject.GetComponent<FireAttack>().FadeSpriteOnDeath(5f, false));
        }

        
        for (int i = 0; i < enemyHeroes.transform.childCount; i++)
            yield return new WaitUntil(() => enemyHeroes.transform.GetChild(i).GetComponent<FireAttack>().renderer.color.a == 1);

        // yield return new WaitUntil(() => enemyHeroes.transform.GetChild(0).GetComponent<FireAttack>().renderer.color.a == 1);
        // yield return new WaitUntil(() => enemyHeroes.transform.GetChild(1).GetComponent<FireAttack>().renderer.color.a == 1);
        // yield return new WaitUntil(() => enemyHeroes.transform.GetChild(2).GetComponent<FireAttack>().renderer.color.a == 1);

        yield break;
    }

    IEnumerator PanCameraToGate()
    {
        cameraAnim.SetTrigger("PanCameraToGate");
        yield return null;
        yield return new WaitForSeconds(cameraAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        cameraAnim.ResetTrigger("PanCameraToGate");
    }

    IEnumerator OpenGate()
    {
        entryWayGateAnim.SetTrigger("OpenGate");
        yield return null;
        yield return new WaitForSeconds(entryWayGateAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        entryWayGateAnim.ResetTrigger("OpenGate");
    }

    

    IEnumerator EnemiesMoonwalkToGate()
    {
        foreach (Transform hero in enemyHeroes.transform)
        {
            Debug.Log($"{hero.gameObject} began walking");

            Animator heroAnim = hero.GetComponent<Animator>();
            heroAnim.ResetTrigger("idle");
            heroAnim.SetTrigger("walk");

            hero.GetComponent<MoveToExitOnGameOver>().MoveToGate(entryGateHeroKillPoint, heroWalkSpeedToGate);
        }

        yield return new WaitUntil(() => heroesLeftToDestroy == 0);
    }

    IEnumerator PanCameraToPlayer()
    {
        cameraAnim.SetTrigger("PanAndZoomToPlayer");
        yield return null;
        yield return new WaitForSeconds(cameraAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        cameraAnim.ResetTrigger("PanAndZoomToPlayer");
    }

    IEnumerator SwapSlimeSpritesOverTime()
    {
        PlayerController.Instance.GetComponent<Animator>().enabled = false;
        SpriteRenderer playerSpriteRenderer = PlayerController.Instance.GetComponent<SpriteRenderer>();

        int currentIndex = 0;
        int slimeSpritesLength = slimeColorSprites.Length;

        while (true)
        {
            yield return new WaitForSeconds(slimeSpriteSwapSpeed);
            playerSpriteRenderer.sprite = slimeColorSprites[currentIndex];
            currentIndex = (currentIndex + 1) % slimeSpritesLength;
        }
    }    
}
