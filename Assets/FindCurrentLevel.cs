using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FindCurrentLevel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI levelText;
    string currentLevel;

    void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode) =>
        levelText.text = SceneManager.GetActiveScene().name.Last().ToString();


    void OnDisable() =>
        SceneManager.sceneLoaded -= OnSceneLoaded;
}
