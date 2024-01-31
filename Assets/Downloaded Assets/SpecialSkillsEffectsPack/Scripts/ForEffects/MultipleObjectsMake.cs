using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleObjectsMake : _ObjectsMakeBase
{
    public float m_startDelay;
    public int m_makeCount;
    public float m_makeDelay;
    public Vector3 m_randomPos;
    public Vector3 m_randomRot;
    public Vector3 m_randomScale;
    public bool isObjectAttachToParent = true;

    float m_Time;
    float m_Time2;
    float m_delayTime;
    float m_count;
    float m_scalefactor;

    Transform parentTransform;


    void OnEnable()
    // void Start()
    {
        // parentTransform = transform.parent;
        m_Time = m_Time2 = Time.time;
        m_scalefactor = VariousEffectsScene.m_gaph_scenesizefactor; //transform.parent.localScale.x; 
    }

    void OnDisable()
    {
        m_Time = m_Time2 = 0;
        m_count = 0;

        foreach (Transform grandchild in gameObject.transform)
            // grandchild.gameObject.SetActive(false);
            Destroy(grandchild.gameObject);

    //     if (parentTransform != null && parentTransform.gameObject.activeSelf)
    //         gameObject.SetActive(false);
    }

    void Update()
    {
        if (Time.time > m_Time + m_startDelay)
        {            
            if (Time.time > m_Time2 + m_makeDelay && m_count < m_makeCount)
            {
                Vector3 m_pos = transform.position + GetRandomVector(m_randomPos)* m_scalefactor; 
                Quaternion m_rot = transform.rotation * Quaternion.Euler(GetRandomVector(m_randomRot));
                

                for (int i = 0; i < m_makeObjs.Length; i++)
                {
                    GameObject m_obj = Instantiate(m_makeObjs[i], m_pos, m_rot);
                    Vector3 m_scale = (m_makeObjs[i].transform.localScale + GetRandomVector2(m_randomScale));
                    if(isObjectAttachToParent)
                        m_obj.transform.parent = this.transform;
                    m_obj.transform.localScale = m_scale;
                }

                m_Time2 = Time.time;
                m_count++;
            }
        }

        // Debug.Log("parent = " + parentTransform);
        // if (parentTransform != null)
        // {
        //     Debug.Log("has a parent");

        //     if (gameObject.activeSelf)
        //         Debug.Log("self is not active");
        //     if (parentTransform.gameObject.activeSelf)
        //         Debug.Log("parent is active");
        // }

        // && gameObject.activeSelf && !parentTransform.gameObject.activeSelf)
        // {
        //     Debug.Log("????");
        //     m_Time = m_Time2 = 0;
        //     gameObject.SetActive(false);
        // }
    }
}
