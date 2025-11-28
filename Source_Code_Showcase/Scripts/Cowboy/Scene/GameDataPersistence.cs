using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameDataPersistence : MonoBehaviour
{
    public static GameDataPersistence Instance;

    // เก็บ Win/Lose ---
    public string sceneToReturnTo;      // return pos
    public Vector2 winSpawnPosition;    // win pos
    public Vector2 loseSpawnPosition;   // lose pos
    public bool justWonBattle = false;  // won

    // update remember enemy ID
    public string currentEncounterID;
    public List<string> defeatedEnemies = new List<string>();
    public List<string> correctlyAnsweredQuestions = new List<string>();

    // boss
    public string sceneToLoadOnWin;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}