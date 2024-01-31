﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayActive : MonoBehaviour
{
    public GameObject[] m_activeObj;
    public float m_delayTime;
    float m_time;

    void Start() =>
        m_time = Time.time;

    void Update()
    {
        if (Time.time > m_time + m_delayTime)
            ToggleChildren(true);
    }

       
    void OnDisable() {
        m_time = Time.time;
        ToggleChildren(false);
    }

    void OnEnable() =>
        m_time = Time.time;


    void ToggleChildren(bool toggleValue)
    {
        for(int i = 0; i < m_activeObj.Length; i++)
            if(m_activeObj[i] != null)
                m_activeObj[i].SetActive(toggleValue);
    }
}
