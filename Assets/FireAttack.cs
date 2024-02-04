using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.HealthSystemCM;
using TMPro;

public class FireAttack : MonoBehaviour, IEnemyFire, IGetHealthSystem
{
    [SerializeField] int health;
    [SerializeField] int damage;
    [SerializeField] float numberOfDamageHits = 1;
    [SerializeField] float timeBeforeAttack;
    [Space(5)]

    [Tooltip("Character enters its damage step at this percentage into its attack particle's duration.")]
    [SerializeField] [Range(0, 100f)] float firstDamageStepAttackPercent = 50f;
    [Tooltip("The percentage into the attack animation's length to wait before firing attack.")]
    [SerializeField] [Range(0, 1f)] float partialOfAnimationToPlayBeforeAttacking;

    [Space(10)]
    [Tooltip("Tweak for each enemy. Allows enemy to accurately position its attack particles.")]
    // [SerializeField] Vector3 baseGlobalPlayerPosOffset;
    [SerializeField] ParticleSystem attackParticle;
    [SerializeField] ParticleSystem auraParticle;
    float countdownToAttackTimer;
    

    [Space(10)]
    [Tooltip("Currently, Player should not have one.")]
    [SerializeField] GameObject countdownToAttackObject;
    TextMeshProUGUI countdownToAttackTMPro;


    [Space(5)]
    [Tooltip("If not AoE, attack frontmost hero target.")]
    [SerializeField] bool attackIsAoE;

    [SerializeField] bool playerIsMage;

    [SerializeField] float x_ReflectOffset;

    PlayerController player;
    HealthSystem healthSystem;
    Animator anim;
    new SpriteRenderer renderer;
    Vector3 newPlayerSpritePositionOffset;

    bool countingDownToAttack;
    // FireAttack playerFire;

    GameManager gameManager;

    // This is also used for finding damage steps.
    float attackParticleLifetime;
    float remainderOfAttackDuration;
    float remainderOfAnimDuration;

    bool isFiring;
    bool isDead;


    float timeUntilFirstDamageStep;
    // bool isTargetable;
    // public GameObject targetingArrow;

    Vector3 originalAttackParticlePosition;
    Vector3 originalRotationOfAttackParticle;



    void Awake() 
    {
        healthSystem = new HealthSystem(health);
        // healthSystem.SetHealth(health);
        attackParticleLifetime = attackParticle.main.duration;
        anim = GetComponent<Animator>();    
        renderer = GetComponent<SpriteRenderer>();    
    }
    
    void Start() {
        player =  PlayerController.Instance;
        gameManager = GameManager.Instance;        
        
        if (countdownToAttackObject != null)
            countdownToAttackTMPro = countdownToAttackObject.GetComponentInChildren<TextMeshProUGUI>();

        // FindNewAttackParticlePosition(false);
        originalRotationOfAttackParticle = attackParticle.gameObject.transform.eulerAngles;
        originalAttackParticlePosition = attackParticle.gameObject.transform.position;

        // Find diff between original spawned attack pos and current scene's player pos.
        newPlayerSpritePositionOffset = player.gameObject.transform.position - originalAttackParticlePosition;        
    }


    void Update() =>
        UpdateTimeBeforeAttackText();


    void UpdateTimeBeforeAttackText()
    {
        if (countdownToAttackObject == null)
            return;

        if (countdownToAttackObject.activeInHierarchy)
            countdownToAttackTimer = Mathf.Max(countdownToAttackTimer - Time.deltaTime, 0f);
        else
            countdownToAttackTimer = timeBeforeAttack;
            
        countdownToAttackTMPro.text = countdownToAttackTimer.ToString("F1");
    }



    public IEnumerator BeginAttackSequence()
    {
        if (IsDead())
        {
            Debug.Log($"{gameObject} is dead and failed to fire attack. Moved to next attacker.");
            StartCoroutine(gameManager.FindNextAttacker());
            yield break;
        }

            
        // Open up Player's reflect window.
        SetActivityOfParticle(auraParticle, true);


        if (!IsPlayer())
        {
            player = PlayerController.Instance;
            player.reflectWindow = timeBeforeAttack;
            player.CanReflect(true);
        }

                    
        // Display remaining time until attack completion.
        countdownToAttackObject.SetActive(true);
        yield return new WaitForSeconds(timeBeforeAttack);
        countdownToAttackObject.SetActive(false);

        // Wait for more appropriate sprite before attacks fires. A bit finnicky, obviously.
        if (anim)
        {
            anim.SetTrigger("attack");
            var animLength = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            yield return new WaitForSeconds(animLength * partialOfAnimationToPlayBeforeAttacking);
        }


        // Fire attack/particle, then complete animation.
        player.CanReflect(false);
        FindNewAttackParticlePositionAndRotation(false);
        SetActivityOfParticle(auraParticle, false);
        SetActivityOfParticle(attackParticle, true);

        
        StartCoroutine(EnterDamageStep());

    }


    IEnumerator EnterDamageStep()
    {
        // Create point where first damage step occurs.        
        timeUntilFirstDamageStep = attackParticleLifetime * firstDamageStepAttackPercent * 0.01f;
        remainderOfAttackDuration = attackParticleLifetime - timeUntilFirstDamageStep;        

        // Wait until heroes would damage player.
        yield return new WaitForSeconds(timeUntilFirstDamageStep);

        if (IsPlayer())
        {
            yield return new WaitForSeconds(timeUntilFirstDamageStep);

            if (attackIsAoE)
                yield return StartCoroutine(DealDamageTo(damage, null));

            else
                yield return StartCoroutine(DealDamageTo(damage, GetHeroToDamage()));
        }

        else if (!IsPlayer())
        {
            if (!player.PlayerReflected())
            {
                FindNewAttackParticlePositionAndRotation(false);
                yield return StartCoroutine(DealDamageTo(damage, player.GetComponent<FireAttack>()));
            }

            else
            {
                SetActivityOfParticle(attackParticle, false);
                FindNewAttackParticlePositionAndRotation(true);

                yield return StartCoroutine(player.ReflectedAttack());

                SetActivityOfParticle(attackParticle, true);

                // SetActivityOfParticle(player.reflectParticleObj.GetComponent<ParticleSystem>(), true);

                // Add another damage step for when the reflected damage applies.
                // For now, for simplicity purposes, it uses the original attacker's time until its first damage step

                yield return new WaitForSeconds(timeUntilFirstDamageStep);

                if (attackIsAoE)
                    yield return StartCoroutine(DealDamageTo(damage, null));
                else
                    yield return StartCoroutine(DealDamageTo(damage, this));
                
                yield return new WaitUntil(() => !attackParticle.isPlaying);
                
                FindNewAttackParticlePositionAndRotation(false);
            }
        }

        yield break;
    }


    // Always target hero that's in front. Except with Mage, whom targets furthest hero.
    FireAttack GetHeroToDamage()
    {
        if (playerIsMage)
        {
            if (gameManager.heroList.Count > 1)
                return gameManager.heroList[1];
            else
                return gameManager.heroList[0];            
        }
        else
            return gameManager.heroList[0];        
    }


    public IEnumerator DealDamageTo(float damageValue, FireAttack target = null)
    {                
        if (target != null)
            yield return StartCoroutine(ApplyDamage(damageValue, target));

        else if (attackIsAoE)
        {
            foreach (var hero in gameManager.heroList)
            {
                // Damage other heroes first, in case attacker would die mid-loop.
                if (hero != this)
                    StartCoroutine(ApplyDamage(damageValue, hero));
            }
            
            StartCoroutine(ApplyDamage(damageValue, this));
        }

        if (anim)
            yield return new WaitUntil(() => !anim.IsInTransition(0));

        yield return new WaitUntil(() => !attackParticle.isPlaying);

        SetActivityOfParticle(attackParticle, false);        

        // Attack is now finally fully completed.
        StartCoroutine(gameManager.FindNextAttacker());
        yield break;
    }


    // Multi-hit example: Attack lasts 1 seconds, with a firstDamageStepAttackPercent of 25% and 10 hits.
    // First hit occurs 25% into 1.0f, at 0.25f. Remaining 9 attacks occur over 0.75 seconds, 
    // with each occuring every 0.083e seconds, starting at 0.33333.
    IEnumerator ApplyDamage(float damageValue, FireAttack target)
    {
        if (!target.healthSystem.IsDead() && target != null)
        {
            if (numberOfDamageHits > 1)
            {                
                // (first hit already occurred at damage step)
                for (int hits = 0; hits < numberOfDamageHits; hits++)
                {
                    yield return new WaitForSeconds(remainderOfAttackDuration / numberOfDamageHits);
                    target.healthSystem.Damage(damageValue / numberOfDamageHits);
                }

            }

            else
                target.healthSystem.Damage(damageValue);


            if (target.healthSystem.IsDead())
            {
                var targetAnim = target.gameObject.GetComponent<Animator>();

                yield return new WaitUntil(() => !attackParticle.isPlaying);
                

                // if (target != this)
                // {
                    // yield return StartCoroutine(target.FadeOutSpriteOnDeath(1f));
                    yield return StartCoroutine(target.KillTarget(target));
                // }
            }
        }
    }


    public IEnumerator KillTarget(FireAttack target) 
    {
        yield return StartCoroutine(gameManager.RemoveHeroFromAttackerLists(target));
        target.gameObject.SetActive(false);
    }


    IEnumerator FadeOutSpriteOnDeath(float timeToFade)
    {
        anim.enabled = false;

        GameObject healthBar = GetComponentInChildren<HealthBarUI>().transform.parent.gameObject;
        healthBar.SetActive(false);

        float targetAlpha = 0;
        Color startColor = renderer.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        float elapsedTime = 0f;

        while (elapsedTime < timeToFade)
        {
            renderer.color = Color.Lerp(startColor, targetColor, elapsedTime / timeToFade);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        renderer.color = targetColor;        

        yield break;
    }




    public void SetActivityOfParticle(ParticleSystem parentParticle, bool activity)
    {
        if (parentParticle.gameObject.activeSelf == activity)
            return;

        if (attackParticle.transform.childCount >= 1)
        {
            foreach (Transform childParticle in parentParticle.gameObject.transform)
                childParticle.gameObject.SetActive(activity);
        }

        parentParticle.gameObject.SetActive(activity);

        if (activity == true && !parentParticle.isPlaying)
            parentParticle.Play();
    }

Vector3 positionDiffBetweenPlayerAndEnemy;
Vector3 reflectedPosition;
    
    // Currently does not account for direction, or non-AoE attacks.
    void FindNewAttackParticlePositionAndRotation(bool playerReflected)
    {
        if (playerIsMage)
        {
            // Adjust static particle effect to land on only remaining hero.
            if (gameManager.heroList.Count == 1)
            {
                attackParticle.gameObject.transform.position = new Vector3(
                                                                gameManager.heroList[0].transform.position.x,
                                                                originalAttackParticlePosition.y, 
                                                                originalAttackParticlePosition.z);
            }
        }
        else
            attackParticle.gameObject.transform.position = originalAttackParticlePosition;


        if (playerReflected)
        {
            // Offset aiming towards middle hero.
            var middleHeroPosition = gameManager.heroList[1].gameObject.transform.position;
            if (middleHeroPosition == null)
                middleHeroPosition = gameManager.heroList[0].gameObject.transform.position;

            var positionDiffBetweenMiddleHeroAndPlayer =  middleHeroPosition - newPlayerSpritePositionOffset;

            reflectedPosition = new Vector3(
                                attackParticle.transform.position.x + positionDiffBetweenMiddleHeroAndPlayer.x + x_ReflectOffset,
                                originalAttackParticlePosition.y,
                                originalAttackParticlePosition.z);                        


            attackParticle.gameObject.transform.position = reflectedPosition;
            attackParticle.transform.eulerAngles = Vector3.zero;
        }

        else
            attackParticle.transform.eulerAngles = originalRotationOfAttackParticle;
    }

    // IEnumerator TriggerAnimationOf(FireAttack target, string triggerName, bool waitForAnimationToEnd = false)
    // {
    //     Animator animator = target.animator;

    //     // if (animator)
    //         // animator.SetTrigger("getHit");

    //     if (waitForAnimationToEnd)
    //     {
    //         while (animator.GetCurrentAnimatorStateInfo(0).IsName("getHit"))
    //            yield return null;
    //     }

    //     yield break;
    // }


    public void Heal(int value) =>
        healthSystem.Heal(value);


    public bool IsPlayer() => gameObject == PlayerController.Instance.gameObject;

    public HealthSystem GetHealthSystem() => healthSystem;
    public bool IsFiring() => isFiring;
    public bool IsDead() => healthSystem.IsDead();
    // public bool IsTargetable() => isTargetable;
}
