using UnityEngine;

public class SceneManager : MonoBehaviour {
    // public static SceneController instance;

    // void Start()
    // {
    //     if (instance = null) {
    //         instance = this;
    //         DontDestroyOnLoad(gameObject); // If the next scene doesn't this gameObject, then don't destroy
    //     } else {
    //         Destroy(gameObject); // If the next scene has the same gameObject, then destroy
    //     }
    // }

    public void NextLevel() {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1); // Load next scene
    }

    public void PreviousLevel() {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex - 1); // Load previous scene
    }

    public void LoadScene(string sceneName) {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName); // Load scene with the specified name
    }
}
