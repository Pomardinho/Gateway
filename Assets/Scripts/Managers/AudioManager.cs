using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    public static AudioManager Instance { get; private set; }

    public AudioClip game;

    public AudioClip buttonClick;
    public AudioClip dash;
    public AudioClip jump;
    public AudioClip nextLevel;
    public AudioClip pickUpObject;
    public AudioClip play;
    public AudioClip stomp;
    public AudioClip death;

    void Awake () {
        if (Instance == null) {
            Instance = this;
            // Prevent destroying this object when a new scene is loaded so the music can keep playing
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    void Start () {
        if (PlayerPrefs.HasKey("musicVolume")){
            musicSource.volume = PlayerPrefs.GetFloat("musicVolume");
        } else {
            musicSource.volume = 1f;
        }

        if (PlayerPrefs.HasKey("sfxVolume")) {
            SFXSource.volume = PlayerPrefs.GetFloat("sfxVolume");
        } else {
            SFXSource.volume = 1f;
        }

        musicSource.clip = game;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip source) {
        SFXSource.PlayOneShot(source);
    }

    public AudioSource GetMusicSource() {
        return musicSource;
    }

    public AudioSource GetSFXSource() {
        return SFXSource;
    }
}
