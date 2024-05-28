using UnityEngine;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private AudioManager audioManager;

    void Awake() {
        audioManager = AudioManager.Instance;
    }

    void Start() {
        LoadMusicVolume();
        LoadSFXVolume();
    }

    public void OnMusicChange() {
        float localValue = musicSlider.value;
        audioManager.GetMusicSource().volume = localValue;
        float currentTime = audioManager.GetMusicSource().time;
        audioManager.GetMusicSource().Play();
        audioManager.GetMusicSource().time = currentTime;
        SaveMusicVolume();
    }

    public void OnSFXChange() {
        float localValue = sfxSlider.value;
        audioManager.GetSFXSource().volume = localValue;
        float currentTime = audioManager.GetMusicSource().time;
        audioManager.GetSFXSource().Play();
        audioManager.GetSFXSource().time = currentTime;
        SaveSFXVolume();
    }

    public void NavigateToHome() {
        audioManager.PlaySFX(audioManager.buttonClick);
        sceneManager.LoadScene("Home");
    }

    private void LoadMusicVolume() {
        musicSlider.value = PlayerPrefs.HasKey("musicVolume") ? PlayerPrefs.GetFloat("musicVolume") : .5f;
    }

    private void LoadSFXVolume() {
        sfxSlider.value = PlayerPrefs.HasKey("sfxVolume") ? PlayerPrefs.GetFloat("sfxVolume") : .5f;
    }

    private void SaveMusicVolume() {
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
    }

    private void SaveSFXVolume() {
        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
    }

}
