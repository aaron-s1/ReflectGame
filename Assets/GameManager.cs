using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;

public class GameManager : MonoBehaviour
{
    
    [HideInInspector] public static GameManager Instance { get { return instance; } }
    [HideInInspector] public static GameManager playerInstance;
    static GameManager instance;

    // [HideInInspector]
    public List<FireAttack> attackerList;
    [HideInInspector] public List<FireAttack> heroList;

    [SerializeField] GameObject enemyHeroes;
    [SerializeField] float delayBeforeNewAttackerFires;

    PlayerController player;

    FireAttack currentAttacker;

    [HideInInspector] public FireAttack lastHeroToAttack;

    Vector3 originalPositions;

    int attackerIndex = -1;
    // int finalSceneIndex;
    
    // int currentSceneIndex;

    List<Vector3> originalHeroPositions;

    // bool gameJustStarted = true;


    void Awake()
    {
        if (instance == null)
            instance = this;

        attackerList = new List<FireAttack>();
        originalHeroPositions = new List<Vector3>();
    }


    void Start()
    {
        StartCoroutine(PopulateAttackerLists());
        // finalSceneIndex = SceneManager.sceneCountInBuildSettings - 1;
    }
        

    IEnumerator PopulateAttackerLists()
    {
        // player = PlayerController.Instance;
        // attackerList.Add(player.gameObject.GetComponent<FireAttack>());
        foreach (Transform hero in enemyHeroes.transform)
        {
            if (!hero.gameObject.activeInHierarchy)
                continue;

            heroList.Add(hero.gameObject.GetComponent<FireAttack>());
            originalHeroPositions.Add(hero.gameObject.transform.localPosition);

            attackerList.Add(hero.gameObject.GetComponent<FireAttack>());
        }

        player = PlayerController.Instance;
        attackerList.Add(player.gameObject.GetComponent<FireAttack>());

        // Delay for one frame to insure other scripts' Instances are updated.
        yield return null;
        StartCoroutine(FindNextAttacker());
    }




    #region HANDLE ROTATIONS OF HEROES

    public IEnumerator FindNextAttacker()
    {
        Debug.Log($"FindNextAttacker called. heroList count = {heroList.Count}");
        // Player died...
        if (player.gameObject.GetComponent<FireAttack>().IsDead())
        {
            levelHasEnded = true;
            yield return StartCoroutine(LevelLoader.Instance.ReloadCurrentScene());
            yield break;
        }

        // .. or is the only character left.
        if (heroList.Count == 0)
        {
            if (attackerList[0] == player.gameObject.GetComponent<FireAttack>())
            {
                levelHasEnded = true;                
                yield return StartCoroutine(LevelLoader.Instance.LoadNextScene());
                yield break;
                // yield break;
            }
        }
        Debug.Log($"hero list count = {heroList.Count}");
        Debug.Log("reached middle of FindNextAttacker()");


        if (levelHasEnded)
        {
        }

        attackerIndex++;

        if (attackerIndex >= attackerList.Count)
            attackerIndex = 0;

        currentAttacker = attackerList[attackerIndex];

        StartCoroutine(RotateToNextAttacker());
        yield break;
    }


    IEnumerator RotateToNextAttacker()
    {
        // currentAttacker = attackerList[3];
        if (currentAttacker.IsPlayer())
        {            
            yield return new WaitForSeconds(delayBeforeNewAttackerFires);
            StartCoroutine(currentAttacker.BeginAttackSequence());            
        }

        else
        {
            if (!currentAttacker.IsDead())
            {
                yield return StartCoroutine(RotateAllHeroPositions());
                                    
                yield return new WaitForSeconds(delayBeforeNewAttackerFires);
                
                lastHeroToAttack = currentAttacker;
                StartCoroutine(currentAttacker.BeginAttackSequence());
            }

            else
            {
                Debug.Log("RotateToNextAttacker");
                StartCoroutine(FindNextAttacker());
            }
        }
    }

   
    
    IEnumerator RotateAllHeroPositions()
    {
        var attackerIndex = heroList.IndexOf(currentAttacker);
        heroList.RemoveAt(attackerIndex);
        heroList.Insert(0, currentAttacker);

        foreach (FireAttack hero in heroList)
        {
            var newIndex = heroList.IndexOf(hero);
            hero.gameObject.transform.position = originalHeroPositions[newIndex];                
        }

        yield break;
    }



    public IEnumerator RemoveHeroFromAttackerLists(FireAttack character)    
    {
        // LogCurrentMethod(1);
        // character.gameObject.SetActive(false);
        heroList.Remove(character);
        attackerList.Remove(character);

        attackerIndex--;

        // StartCoroutine(FindNextAttacker());

        // LogCurrentMethod(3);
        yield break;
    }

    #endregion



    // public void DamageAllHeroes(int damage)
    // {
    //     foreach (FireAttack hero in heroList)
    //         StartCoroutine(hero.DealDamageTo(hero, damage));
    // }


    public FireAttack FindRandomAliveHero() =>
        heroList[UnityEngine.Random.Range(0, heroList.Count)];



    #region LEVEL TRANSITIONS

    bool levelHasEnded;

    // public void LevelWon()
    // {
    //     levelHasEnded = true;

    //     int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

    //     if (currentSceneIndex != finalSceneIndex)
    //     {

    //         // LoadNextLevel(currentSceneIndex + 1);
    //         LoadNextLevel();
    //     }

    //     // won final level.
    //     else
    //         EndGame();
    // }


    // void LoadNextLevel() 
    // {
    //     // int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    //     // SceneManager.LoadScene(SceneManager.GetSceneByBuildIndex(currentSceneIndex + 1).ToString());
    // }



    // void EndGame() =>
    //     LogCurrentMethod(1);




    // public void RestartCurrentLevel()
    // {
    //     // transition stuff
    //     levelHasEnded = true;
    //     // SceneManager.LoadScene(SceneManager.GetActiveScene().ToString());
    // }


    // 1 for start of method. 2 to add some info. 3 to signial end of method.
    public void LogCurrentMethod(int locationInMethod, string addendum = null, [CallerMemberName] string caller = null) 
    {
        if (locationInMethod == 1)
            UnityEngine.Debug.Log($"Called {caller}().");
        else if (locationInMethod == 2 && addendum != null)
            UnityEngine.Debug.Log($"{caller}() -- {addendum}.");
        else if (locationInMethod == 3)
            UnityEngine.Debug.Log($"Reached end of {caller}().");
    }

    #endregion
}
