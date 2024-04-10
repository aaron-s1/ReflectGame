using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
// using System.Net;
// using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    
    [HideInInspector] public static GameManager Instance { get { return instance; } }
    [HideInInspector] public static GameManager playerInstance;
    static GameManager instance;

    public List<FireAttack> attackerList;

    [SerializeField] GameObject enemyHeroes;
    [SerializeField] float delayBeforeNewAttackerFires;

    [HideInInspector] public FireAttack lastHeroToAttack;
    [HideInInspector] public List<FireAttack> heroList;
    [HideInInspector] public bool heroesCanAttack;

    [SerializeField] float attackDelayOnNewScene;


    PlayerController player;
    FireAttack currentAttacker;

    List<Vector3> originalHeroPositions;
    // Vector3 originalPositions;


    int attackerIndex = -1;



    void Awake()
    {
        if (instance == null)
            instance = this;

        attackerList = new List<FireAttack>();
        originalHeroPositions = new List<Vector3>();        
    }


    void Start() =>
        StartCoroutine(PopulateAttackerLists());
        // finalSceneIndex = SceneManager.sceneCountInBuildSettings - 1;
        

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

        // Delay for a moment to insure other scripts' Instances get updated.
        yield return null;

        StartCoroutine(FindNextAttacker());
    }



    #region HANDLE ROTATIONS OF HEROES

    public IEnumerator FindNextAttacker()
    {
        yield return new WaitUntil(() => heroesCanAttack);

        // Player died...
        if (player.gameObject.GetComponent<FireAttack>().IsDead())
        {
            yield return StartCoroutine(LevelLoader.Instance.ReloadCurrentScene());
            yield break;
        }

        // .. or was the only character left (level is over)
        if (heroList.Count == 0)
        {
            if (attackerList[0] == player.gameObject.GetComponent<FireAttack>())
            {
                yield return StartCoroutine(LevelLoader.Instance.LoadNextScene());
                yield break;
            }
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
        yield return new WaitUntil(() => heroesCanAttack);

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
                // Debug.Log("RotateToNextAttacker");
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
    #endregion


    public IEnumerator RemoveHeroFromAttackerLists(FireAttack character)    
    {
        heroList.Remove(character);
        attackerList.Remove(character);

        attackerIndex--;

        yield break;
    }



    #region LEVEL TRANSITIONS
    
    public void OnSceneLoad()
    {
        heroesCanAttack = false;
        StartCoroutine(DelayHeroAttacksOnSceneLoad());        
    }

    // First level: Adds buffer time.
    // Later levels: Buffer time, allows sand transition to smoothly finish.
    // Remember that "delayBeforeNewAttackerFires" also applies.
    public IEnumerator DelayHeroAttacksOnSceneLoad()
    {
        yield return new WaitForSeconds(attackDelayOnNewScene);
        heroesCanAttack = true;
    }

    # endregion





    // 1 for start of method. 2 to add some info. 3 to signal end of method.
    public void LogCurrentMethod(int locationInMethod, string addendum = null, [CallerMemberName] string caller = null) 
    {
        if (locationInMethod == 1)
            UnityEngine.Debug.Log($"Called {caller}().");
        else if (locationInMethod == 2 && addendum != null)
            UnityEngine.Debug.Log($"{caller}() -- {addendum}.");
        else if (locationInMethod == 3)
            UnityEngine.Debug.Log($"Reached end of {caller}().");
    }

    public FireAttack FindRandomAliveHero() =>
        heroList[UnityEngine.Random.Range(0, heroList.Count)];    
}
