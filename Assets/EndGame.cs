using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.HealthSystemCM;
using UnityEngine;
using UnityEngine.SceneManagement;



public class EndGame : MonoBehaviour
{
    static EndGame instance;
    [HideInInspector] public static EndGame Instance { get { return instance; } }

    [Space(10)]    
    [SerializeField] GameObject enemyHeroes;
    [SerializeField] GameObject reflectSequenceUIs;

    [SerializeField] GameObject heroesExpelledText;
    [SerializeField] GameObject youWinVictoryText;
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
    }



    public IEnumerator BeginEndingGame()
    {
        if (instance == null)
        {
            Debug.Log("EndGame.cs has no instance!");
            yield break;
        }


        Destroy(GameManager.Instance.GetComponent<PauseGame>());
        DeleteAllUIs();

        
        yield return StartCoroutine(FadeEnemiesBackIn());
        yield return StartCoroutine(PanCameraToGate());
        yield return StartCoroutine(OpenGate());
        yield return StartCoroutine(EnemiesMoonwalkToGate());
        yield return StartCoroutine(PanCameraToPlayer());


        Instantiate(gameWonVictoryParticleObj, PlayerController.Instance.transform.position + victoryParticlePosOffset, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        youWinVictoryText.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        heroesExpelledText.SetActive(true);
        // thanksForPlayingText.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SwapSlimeSprite());
    }


    // Player health UIs will be active, but hidden already, as their health is obviously 0.
    void DeleteAllUIs()
    {
        PlayerController.Instance.GetComponentInChildren<HealthBarUI>().gameObject.SetActive(false);

        if (reflectSequenceUIs != null)
            reflectSequenceUIs.SetActive(false);
    }


    IEnumerator FadeEnemiesBackIn()
    {
        foreach (Transform enemy in enemyHeroes.transform)
        {
            enemy.position = enemy.gameObject.GetComponent<FireAttack>().originalPosition;            
            enemy.gameObject.SetActive(true);
            
            StartCoroutine(enemy.gameObject.GetComponent<FireAttack>().FadeSpriteOnDeath(1f, false));
        }

        
        for (int i = 0; i < enemyHeroes.transform.childCount; i++)
            yield return new WaitUntil(() => enemyHeroes.transform.GetChild(i).GetComponent<FireAttack>().renderer.color.a == 1);

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
            Animator heroAnim = hero.GetComponent<Animator>();
            heroAnim.ResetTrigger("idle");
            heroAnim.SetTrigger("walk");

            hero.GetComponent<MoveToExitOnGameOver>().MoveToGate(entryGateHeroKillPoint, heroWalkSpeedToGate);
        }

        // Adjust so Player doesn't look weirdly positioned when zoomed in on.
        Vector3 playerPos = PlayerController.Instance.gameObject.transform.position;
        PlayerController.Instance.gameObject.GetComponent<Animator>().enabled = false;
        PlayerController.Instance.gameObject.transform.position = new Vector3(playerPos.x, playerPos.y - .011f, playerPos.z);


        // Heroes call this via KillHeroAsItExitsGate.cs
        yield return new WaitUntil(() => heroesLeftToDestroy == 0);
    }

    IEnumerator PanCameraToPlayer()
    {
        cameraAnim.SetTrigger("PanAndZoomToPlayer");
        yield return null;
        yield return new WaitForSeconds(cameraAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        cameraAnim.ResetTrigger("PanAndZoomToPlayer");
    }


    // Swap color over time to create a [rainbow] effect.
    // Currently just have slimeColorSprites set to a length of 1, which swaps in a fabulous-looking Slime.
    IEnumerator SwapSlimeSprite()
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
