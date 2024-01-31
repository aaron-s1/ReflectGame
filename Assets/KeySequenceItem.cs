// using UnityEngine;

// [System.Serializable]
// public class KeySequenceItem : MonoBehaviour
// {
//     public KeyCode key;
//     public bool needsHeldDown;
//     public float holdDuration;
//     public bool requiresNextKey;

//     [HideInInspector]
//     public bool isHeldDown;
//     [HideInInspector]
//     public float timeHeld;

//     public bool IsKeyCompleted()
//     {
//         if (!needsHeldDown)
//             return Input.GetKeyDown(key);
//         else if (requiresNextKey)
//             return Input.GetKey(key) && timeHeld >= holdDuration;
//         else
//             return timeHeld >= holdDuration;
//     }
// }
