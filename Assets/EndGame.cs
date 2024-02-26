using System.Collections;
using System.Collections.Generic;
using CodeMonkey.HealthSystemCM;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    [HideInInspector] public static EndGame Instance { get { return instance; } }
    static EndGame instance;

    [Space(10)]    
    [SerializeField] GameObject enemyHeroes;
    [SerializeField] GameObject youWinVictoryText;
    [SerializeField] GameObject reflectSequenceUIs;
    [SerializeField] ParticleSystem gameWonVictoryParticleObj;
    [SerializeField] Animator entryWayGateAnim;

    [SerializeField] Transform killPoint;
    [SerializeField] float heroWalkSpeedToGate;

    [HideInInspector] public int heroesLeftToDestroy = 3;

    Vector3 victoryParticlePosOffset;

    Animator cameraAnim;

    int currentSceneIndex = -1;

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


/*

    order:
            fade out / remove health bars
    fade in enemies
    pan camera
    lift gate
    activate enemy walk anim
    move enemies through gate
    they disappear as they go through
    pan back to player

    activate 'you win' text, play particle, play some slime anim
 */
    public IEnumerator GameOver()
    {
        if (instance == null)
        {
            Debug.Log("EndGame.cs has no instance");
            yield break;
        }

        Debug.Log("Level Loader tried to advance, but found no next scene.");

        Time.timeScale = 1f;

        DeleteReflectUI();
        DeleteHealthBars();


        yield return StartCoroutine(FadeEnemiesBackIn());
        yield return StartCoroutine(PanCameraToGate());
        yield return StartCoroutine(OpenGate());
        yield return StartCoroutine(EnemiesWalkToGate());
        yield return StartCoroutine(PanCameraToPlayer());

                
        Instantiate(gameWonVictoryParticleObj, PlayerController.Instance.transform.position + victoryParticlePosOffset, Quaternion.identity);

        // add some animation? rainbow text? just something.
        youWinVictoryText.SetActive(true);

        // play slime victory animation
    }

    void DeleteHealthBars()
    {
        foreach (Transform enemyHealth in enemyHeroes.transform)
            enemyHealth.gameObject.GetComponentInChildren<HealthBarUI>().gameObject.SetActive(false);

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
            enemy.position = enemy.GetComponent<FireAttack>().originalPosition;
            enemy.gameObject.SetActive(true);
        }

        yield break;
        // GameObject healthBar = GetComponentInChildren<HealthBarUI>().transform.parent.gameObject;
        // healthBar.SetActive(false);

        // float targetAlpha = 0;
        // Color startColor = renderer.color;
        // Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        // float elapsedTime = 0f;

        // while (elapsedTime < timeToFade)
        // {
        //     renderer.color = Color.Lerp(startColor, targetColor, elapsedTime / timeToFade);
        //     yield return null;
        //     elapsedTime += Time.deltaTime;
        // }

        // renderer.color = targetColor;        

        // yield break;
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

    

    IEnumerator EnemiesWalkToGate()
    {
        foreach (Transform hero in enemyHeroes.transform)
        {
            Debug.Log($"{hero.gameObject} began walking");

            Animator heroAnim = hero.GetComponent<Animator>();
            heroAnim.ResetTrigger("idle");
            heroAnim.SetTrigger("walk");

            hero.GetComponent<MoveToExitOnGameOver>().MoveToGate(killPoint, heroWalkSpeedToGate);
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
}
