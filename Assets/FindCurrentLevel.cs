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

    void Start()
    {        
        currentLevel = SceneManager.GetActiveScene().name.Last().ToString();
        levelText.text = currentLevel;
    }
}
