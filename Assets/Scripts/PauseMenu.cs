using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private SceneManager sceneManager;

    private static GameObject existingPauseMenu;    
    private PlayerController playerController;
    private TimerController timerController;
    private bool isPaused = false;

    void Awake() {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        timerController = FindObjectOfType<TimerController>();

        if (existingPauseMenu == null) {
            existingPauseMenu = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode) {
        // Update the player controller whenever a new scene is loaded
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Update() {
        if (Input.GetButtonDown("Cancel") // Show pause menu if playing
        && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Home"
        && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Options"
        && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Victory"
        && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Defeat") {
            isPaused = true;
        }

        if (isPaused) {
            pauseCanvas.SetActive(true);
            AudioListener.pause = true;
            Time.timeScale = 0;
        } else {
            pauseCanvas.SetActive(false);
            AudioListener.pause = false;
            Time.timeScale = 1;
        }
    }
    
    void FixedUpdate() {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void Pause() {
        isPaused = true;
    }

    public void Exit() {
        StartCoroutine(ExitCoroutine());
    }

    private IEnumerator ExitCoroutine() {
        sceneManager.LoadScene("Home");
        isPaused = false;
        yield return new WaitForSeconds(0.5f);
        timerController.StopTimer();
    }

    public void Resume() {
        isPaused = false;
    }

    public void Restart() {
        isPaused = false;
        StartCoroutine(playerController.Respawn(0.5f));
    }
}
