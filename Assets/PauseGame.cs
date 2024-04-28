using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGame : MonoBehaviour
{
    [SerializeField] GameObject pauseOverlay;

    float originalTimeScale;

    void Awake() =>
        originalTimeScale = Time.timeScale;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale == originalTimeScale)
            {
                pauseOverlay.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                pauseOverlay.SetActive(false);
                Time.timeScale = originalTimeScale;
            }
        }
    }
}
