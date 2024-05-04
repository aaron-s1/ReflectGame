using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyThisOnLoad : MonoBehaviour
{
    [SerializeField] bool dontDestroy = true;
    [SerializeField] float destroyAfter;

    void Awake()
    {
        if (dontDestroy)
        {            
            DontDestroyOnLoad(this.gameObject);
            
            if (destroyAfter > 0)
                Invoke("DeleteObject", destroyAfter);
        }
    }

    void Start()
    {
        if (GetComponent<ParticleSystem>() != null)
            GetComponent<ParticleSystem>().Play();
    }

    void DeleteObject() 
        => Destroy (this.gameObject);
}
