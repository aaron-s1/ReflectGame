using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CodeMonkey.HealthSystemCM;
using System;

public class PlayerController : MonoBehaviour
{
    static PlayerController instance;
    public static PlayerController Instance { get { return instance; } }

    [SerializeField] GameObject reflectParticleObj;

    // Without this, multiple keys that require holding would only increment if pressed during the EXACT same frame.
    // This massively improves input fluidity.
    [Tooltip("If multiple keys need held simultaneously, this is the buffer length for how long the first key can be held down and still accept the second key.")]
    [SerializeField] float maxHoldLengthUntilNextHeldKeyIsDisallowed = 0.15f;

    [SerializeField] public List<KeySequenceItem> reflectSequence;


    [HideInInspector] public KeySequenceItem currentKey;
    [HideInInspector] public KeySequenceItem nextKey;    
    [HideInInspector] public float reflectWindow = -1f;

    [HideInInspector] public CurrentKeyData currentKeyData = new CurrentKeyData();
    [HideInInspector] public int currentKeyIndex;

    [System.Serializable]
    public class CurrentKeyData : UnityEvent<int, bool> { }


    HealthSystem healthSystem;

    // Vector3 locationOfHeroTarget;

    bool currentKeyWasSuccessful;

    bool allowReflectSequence;
    bool reflectedSuccessfully;

    float reflectStartTime;

    float simultaneousKeyHoldTimer; 



    void Awake()
    {        
        if (instance == null)
            instance = this;

        CanReflect(true);

        // make dynamic later.
        // locationOfHeroTarget = new Vector3(-0.75f, -0.6f, 0);
        // StartCoroutine(TestTransition());
    }

    // IEnumerator TestTransition()
    // {
    //     StartCoroutine(LevelLoader.Instance.LoadNextScene());
    //     Debug.Break();
    //     yield break;
    // }



    void Update()
    {
        if (reflectedSuccessfully || !allowReflectSequence)
            CompletelyResetAllKeys();

        IncrementKeyOnSuccessfulPressOrHold();
        CheckIfKeySequenceFullyCompleted();

        reflectStartTime = Time.time;   
    }

    void CompletelyResetAllKeys()
    {
        currentKeyIndex = 0;
        currentKeyData.Invoke(-1, false);
        ResetTimeHeldForAllKeys();        
    }

    
    

    void IncrementKeyOnSuccessfulPressOrHold()
    {
        if (currentKeyIndex > reflectSequence.Count)
            return;

        currentKey = reflectSequence[currentKeyIndex];
        var currentKeyCode = KeySequenceItem.KeyMap[currentKey.key];


        // Makes sure Key increments instantly if it doesn't need held down (improves input flow)
        if (currentKeyWasSuccessful)
        {
            if (!currentKey.KeyNeedsHeldDown() || Input.GetKeyUp(currentKeyCode))
            {
                currentKeyData.Invoke(currentKeyIndex, currentKey.alsoRequiresNextKey);
                currentKeyIndex++;

                if (currentKey.alsoRequiresNextKey)
                    currentKeyIndex++;

                currentKeyWasSuccessful = false;
            }
        }


        
        if (Input.GetKey(currentKeyCode))
        {
            if (!currentKey.KeyNeedsHeldDown())
            {
                if (Input.GetKeyDown(currentKeyCode))
                {
                    currentKeyWasSuccessful = true;
                    return;
                }
            }


            else if (currentKey.KeyNeedsHeldDown())
            {
                if (!currentKey.alsoRequiresNextKey)
                    currentKey.timeHeld += Time.deltaTime;

                // Requires next key to be held simultaneously.
                else
                {
                    // currentKey is last of sequence, so ignore needing to hold next key (as there isn't one).
                    if ((currentKeyIndex + 1 >= reflectSequence.Count) == true)
                        currentKey.timeHeld += Time.deltaTime;

                    else
                    {
                        simultaneousKeyHoldTimer += Time.deltaTime;
                        
                        nextKey = reflectSequence[currentKeyIndex + 1];

                        if (Input.GetKey(KeySequenceItem.KeyMap[nextKey.key]))
                        {
                            if (simultaneousKeyHoldTimer <= maxHoldLengthUntilNextHeldKeyIsDisallowed)
                            {
                                currentKey.timeHeld += Time.deltaTime;
                                simultaneousKeyHoldTimer = 0;
                            }
                        }
                    }

                }

                // Pass data to UI display arrows.
                currentKeyData.Invoke(currentKeyIndex, currentKey.alsoRequiresNextKey);

                // currentKey finished. Simply increase index, and currentKey changes on next frame.
                if (currentKey.timeHeld >= currentKey.holdDurationNeeded)
                {
                    currentKeyWasSuccessful = true;
                    return;
                }

            }                
        }

        else if (Input.GetKeyUp(currentKeyCode))
            simultaneousKeyHoldTimer = 0;

        // Reset if player inputs any incorrect key.
        else if (Input.anyKeyDown)
            CompletelyResetAllKeys();

    }


    void CheckIfKeySequenceFullyCompleted()
    {
        if (currentKeyIndex >= reflectSequence.Count)
        {
            // Debug.Log("Reflect input sequence accepted!");
            currentKeyIndex = 0;
                    // currentKeyData.Invoke(-1, false);
            ResetTimeHeldForAllKeys();

            allowReflectSequence = false;
            GetComponent<FireAttack>().SetActivityOfParticle(reflectParticleObj.GetComponent<ParticleSystem>(), true);
            // reflectParticleObj.SetActive(true);

            reflectedSuccessfully = true;
        }
    }



    [System.Serializable]
    public class KeySequenceItem
    {
        public AllowedKeys key;
        public enum AllowedKeys { Up, Down, Right, Left, None }
        [HideInInspector] public AllowedKeys nextKey;

        public float holdDurationNeeded;
        [Tooltip("If active, the next key requires the current key's hold duration.")]
        [SerializeField] public bool alsoRequiresNextKey;

        [HideInInspector] public float timeHeld;

        // Reference to the PlayerController's current key index
        [NonSerialized]
        [HideInInspector] public int currentKeyIndex;

        public static readonly Dictionary<AllowedKeys, KeyCode> KeyMap = new Dictionary<AllowedKeys, KeyCode>
        {
            { AllowedKeys.Up, KeyCode.UpArrow },
            { AllowedKeys.Down, KeyCode.DownArrow },
            { AllowedKeys.Right, KeyCode.RightArrow },
            { AllowedKeys.Left, KeyCode.LeftArrow }
        };

    

    public bool KeyNeedsHeldDown() =>
        holdDurationNeeded > 0;


    }


    void ResetTimeHeldForAllKeys()
    {
        foreach (KeySequenceItem item in reflectSequence)
        {
            item.timeHeld = 0;
            simultaneousKeyHoldTimer = 0;
        }
    }


    public IEnumerator ReflectedAttack()
    {
        // heroAttackParticle.gameObject.transform.eulerAngles = Vector3.zero;

        yield return new WaitForEndOfFrame();
        GetComponent<FireAttack>().SetActivityOfParticle(reflectParticleObj.GetComponent<ParticleSystem>(), false);
        // attacker.SetActivityOfParticle(reflectParticleObj.GetComponent<ParticleSystem>(), false);
        // attacker.SetActivityOfParticle(heroAttackParticle, true);

        // Debug.Break(); //

        reflectedSuccessfully = false;

        currentKeyData.Invoke(-1, false);

        yield break;
    }

  
    public bool CanReflect(bool allow) =>
        allowReflectSequence = allow;

    public bool PlayerReflected() =>
        reflectedSuccessfully;


    // public void TakeDamage(int value) =>
        // healthSystem.Damage(value);        

    // public void Heal(int value) =>
        // healthSystem.Heal(value);        


    // public HealthSystem GetHealthSystem() => 
        // healthSystem;
}
