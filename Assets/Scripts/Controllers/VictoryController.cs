using TMPro;
using UnityEngine;

public class VictoryController : MonoBehaviour
{
    private TimerController timerController;
    private readonly int modifierBonus = 2500;
    private readonly int scoreLimit = 100000;
    private float timeSpent;
    private int totalScore = 0;

    [SerializeField] private SceneManager sceneManager;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI modifiersBonusText;
    [SerializeField] TextMeshProUGUI totalScoreText;

    void Awake() {
        timerController = FindObjectOfType<TimerController>();
    }

    void Start() {
        timerController.StopTimer();
        timeSpent = timerController.timeSpent;

        float minutes = Mathf.FloorToInt(timeSpent / 60);
        float seconds = Mathf.FloorToInt(timeSpent % 60);
        float miliseconds = Mathf.FloorToInt(timeSpent % 1 * 1000);
        timeText.text += "   " + string.Format("{0:00}.{1:00}.{2:000}", minutes, seconds, miliseconds);

        if (PlayerPrefs.HasKey("GauntletModifier")) totalScore += modifierBonus;
        if (PlayerPrefs.HasKey("TimedModifier")) totalScore += modifierBonus;
        if (PlayerPrefs.HasKey("BlindModifier")) totalScore += modifierBonus;
        modifiersBonusText.text += "   " + totalScore;

        int timeScore = Mathf.RoundToInt(timeSpent * 200);
        totalScore += Mathf.Max(0, scoreLimit - timeScore); // If result is less than 0, max will be 0

        totalScoreText.text += "   " + totalScore;
    }

    void Update() {
        if (Input.GetButtonDown("RestartLevel")) { // "RestartLevel" is the button assigned for r key
            sceneManager.LoadScene("Home");
        }
    }
}
