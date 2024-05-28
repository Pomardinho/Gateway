using TMPro;
using UnityEngine;

public class TimerController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private SceneManager sceneManager;
    private bool timeIsRunning;
    private bool timedModifierEnabled = false;

    private static GameObject existingTimerController;

    public float timeSpent = 0;
    private float timeLimit = 5 * 60; // 5 minutes

    void Awake() {
        if (existingTimerController == null) {
            // Prevent destroying the canvas when a new scene is loaded so the timer keeps running
            GameObject canvas = this.transform.parent.gameObject;
            existingTimerController = canvas;
            DontDestroyOnLoad(canvas);
        } else {
            // Destroy if there's already a timer contoller canvas
            Destroy(gameObject);
        }

        if (PlayerPrefs.HasKey("TimedModifier")) {
            timedModifierEnabled = true;
        }
    }
    
    void Start() {
        StartTimer();
    }

    void Update() {
        if (timeIsRunning) {
            if (timedModifierEnabled) {
                if (timeSpent > 0) {
                    timeSpent -= Time.deltaTime;
                } else if (timeSpent < 0) {
                    timeSpent = 0;
                    sceneManager.LoadScene("Defeat");
                }
            } else {
                timeSpent += Time.deltaTime;
            }

            float minutes = Mathf.FloorToInt(timeSpent / 60);
            float seconds = Mathf.FloorToInt(timeSpent % 60);
            float miliseconds = Mathf.FloorToInt(timeSpent % 1 * 1000);
            timeText.text = string.Format("{0:00}.{1:00}.{2:000}", minutes, seconds, miliseconds);
        }
    }

    void FixedUpdate() {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level1" && !timeIsRunning) StartTimer();
        timedModifierEnabled = PlayerPrefs.HasKey("TimedModifier");
    }

    public void StopTimer() {
        timeIsRunning = false;
    }

    public void StartTimer() {
        timeSpent = 0;
        if (timedModifierEnabled) timeSpent = timeLimit;
        timeIsRunning = true;
    }
}
