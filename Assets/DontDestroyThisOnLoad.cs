using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyThisOnLoad : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
