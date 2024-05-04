using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyThisOnLoad : MonoBehaviour
{
    [SerializeField] Material testSelfMat;
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

        // GetComponent<ParticleSystem>().Play();
    }
    void Start() 
    {
        // GetComponent<Material>().mainTexture = testSelfMat.mainTexture;        

        // Debug.Log(GetComponent<Material>().mainTexture);
        // return;
        // // Debug.Log(GetComponent<Material>().get);
        // Debug.Log(GetComponent<Material>().color);
        // Debug.Log(GetComponent<Material>().shader);
        GetComponent<ParticleSystem>().Play();
    }

    void DeleteObject() 
        => Destroy (this.gameObject);
}
