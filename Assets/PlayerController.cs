

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

    bool currentKeyWasSuccessful;

    bool allowReflectSequence;
    bool reflectedSuccessfully;

    float reflectStartTime;

    float simultaneousKeyHoldTimer;

    bool lockoutKeyHold;

    void Awake()
    {        
        if (instance == null)
            instance = this;

        CanReflect(false);
        // CompletelyResetAllKeys();
    }

    void Start() =>
        CompletelyResetKeySequence();


    void Update()
    {
        if (reflectedSuccessfully || !allowReflectSequence)
            CompletelyResetKeySequence();

        IncrementKeyOnSuccessfulPressOrHold();
        CheckIfKeySequenceFullyCompleted();

        reflectStartTime = Time.time;   
    }

    public void CompletelyResetKeySequence()
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


        // Makes sure Key increments instantly if it doesn't need held down (improves flow)
        if (currentKeyWasSuccessful)
        {
            if (!currentKey.KeyNeedsHeldDown() || Input.GetKeyUp(currentKeyCode))
                AdvanceToNextKey();
        }

        #region Hold key mechanics.

        // Improves INTENDED input flow by first demanding the preceding non-hold Key first be keyed up.
        // This is especially important if the previous key had the same Key requirement
        // (e.g. Left, non-hold --> Left, hold), as otherwise holdDuration on currentKey would automatically 
        // start accruing without direct player input, making that bit of the sequence easier to cheese.
        if (lockoutKeyHold && currentKeyIndex != 0)
        {
            KeySequenceItem lastKey = reflectSequence[currentKeyIndex - 1];
            
            if (Input.GetKeyUp(KeySequenceItem.KeyMap[lastKey.key]))
            {
                lockoutKeyHold = false;
                return;
            }
            else return;
        }
        //

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

                // Held key success. currentKey will increase next frame.
                if (currentKey.timeHeld >= currentKey.holdDurationNeeded)
                {
                    currentKeyWasSuccessful = true;
                    return;
                }

            }
        }
        #endregion
        

        else if (Input.GetKeyUp(currentKeyCode))
            simultaneousKeyHoldTimer = 0;

        // Reset if player inputs any incorrect key.
        else if (Input.anyKeyDown)
            CompletelyResetKeySequence();

    }


    void AdvanceToNextKey()
    {
        currentKeyData.Invoke(currentKeyIndex, currentKey.alsoRequiresNextKey);
        currentKeyIndex++;

        if (currentKey.alsoRequiresNextKey)
            currentKeyIndex++;

        currentKeyWasSuccessful = false;

        // Works with IncrementKeyOnSuccessfulPressOrHold() to prevent any accidental holdDuration increases.
        // (currentKey is local, and may not be updated for a frame)
        if (currentKeyIndex < reflectSequence.Count)
        {
            KeySequenceItem soonToBeCurrentKey = reflectSequence[currentKeyIndex];
            if (soonToBeCurrentKey.KeyNeedsHeldDown())
                lockoutKeyHold = true;
        }
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
        yield return new WaitForEndOfFrame();
        GetComponent<FireAttack>().SetActivityOfParticle(reflectParticleObj.GetComponent<ParticleSystem>(), false);

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
