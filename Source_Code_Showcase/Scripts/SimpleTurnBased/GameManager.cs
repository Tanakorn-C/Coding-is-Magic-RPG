using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Data to save between scenes
    public Vector3 playerPosition;
    public string sceneToReturnTo;

    private void Awake()
    {
        // This is a Singleton pattern. It ensures only one GameManager ever exists.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Don't destroy this object when loading a new scene!
        }
        else
        {
            Destroy(gameObject);
        }
    }
}