using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HomeController : MonoBehaviour {
    [SerializeField] private SceneManager sceneManager;
    private AudioManager audioManager;
    
    private TextMeshProUGUI[] modifiers = new TextMeshProUGUI[3];
    private Vector2[] modifiersPositions = new Vector2[3];
    private int selectedModifierIndex;
    private TextMeshProUGUI modifierDescription;
    private Dictionary<string, bool> activeModifiers;
    private Dictionary<string, string> modifiersDesctriptions;

    private GameObject existingEventSystem;

    private Color activeModifierColor = new (0.55f, 1f, 0.28f, 1f); // Divide each value by 255 (RGBA) to get 0-1 values
    private Color inactiveModifierColor = new (0.39f, 0.28f, 0.46f, 1f); // Divide each value by 255 (RGBA) to get 0-1 values
    
    void Awake() {
        if (existingEventSystem == null) {
            existingEventSystem = FindObjectOfType<EventSystem>().gameObject;
        } else {
            Destroy(FindObjectOfType<EventSystem>());
        }

        /* Delete active modifiers from last time */
        if (PlayerPrefs.HasKey("GauntletModifier")) PlayerPrefs.DeleteKey("GauntletModifier");
        if (PlayerPrefs.HasKey("TimedModifier")) PlayerPrefs.DeleteKey("TimedModifier");
        if (PlayerPrefs.HasKey("BlindModifier")) PlayerPrefs.DeleteKey("BlindModifier");
        PlayerPrefs.Save();

        modifiers[0] = GameObject.FindWithTag("GauntletModifier").GetComponent<TextMeshProUGUI>();
        modifiers[1] = GameObject.FindWithTag("TimedModifier").GetComponent<TextMeshProUGUI>();
        modifiers[2] = GameObject.FindWithTag("BlindModifier").GetComponent<TextMeshProUGUI>();
        modifierDescription = GameObject.FindWithTag("ModifierDescription").GetComponent<TextMeshProUGUI>();
        
        modifiersPositions[0] = modifiers[0].GetComponent<RectTransform>().anchoredPosition; // Center
        modifiersPositions[1] = modifiers[1].GetComponent<RectTransform>().anchoredPosition; // Left
        modifiersPositions[2] = modifiers[2].GetComponent<RectTransform>().anchoredPosition; // Right

        activeModifiers = new Dictionary<string, bool>() {
            { "TimedModifier", false },
            { "GauntletModifier", false },
            { "BlindModifier", false },
        };

        modifiersDesctriptions = new Dictionary<string, string>() {
            { "TimedModifier", "Time limit" },
            { "GauntletModifier", "No checkpoints" },
            { "BlindModifier", "Limited visibility" },
        };
    }

    void Start() {
        audioManager = AudioManager.Instance; // Get audio manager
        /* Restart music */
        audioManager.GetMusicSource().time = 0;
    }

    void Update()
    {
        modifierDescription.color = modifierDescription.text == modifiersDesctriptions[modifiers[selectedModifierIndex].tag] 
        && activeModifiers[modifiers[selectedModifierIndex].tag] ? 
        activeModifierColor : inactiveModifierColor;

        if (Input.GetButtonDown("Next")) {
            UpdateModifiersDisplay(1); // Direction = right
        } else if (Input.GetButtonDown("Previous")) {
            UpdateModifiersDisplay(-1); // Direction = left
        } else if (Input.GetButtonDown("Dash")) { // "Dash" is the button assigned for up arrow key
            audioManager.PlaySFX(audioManager.pickUpObject);
            activeModifiers[modifiers[selectedModifierIndex].tag] = !activeModifiers[modifiers[selectedModifierIndex].tag];
        } else if (Input.GetButtonDown("Confirm")) { // "Confirm" is the button assigned for space key
            foreach (KeyValuePair<string, bool> activeModifier in activeModifiers) {
                if (activeModifier.Value) {
                   PlayerPrefs.SetInt(activeModifier.Key, 1);
                }
            }
            audioManager.PlaySFX(audioManager.play);
            sceneManager.LoadScene("Level1");
        }
    }

    public void Next() {
        UpdateModifiersDisplay(1);
    }

    public void Previous() {
        UpdateModifiersDisplay(-1);
    }

    public void NavigateToOptions() {
        audioManager.PlaySFX(audioManager.buttonClick);
        sceneManager.LoadScene("Options");
    }

    public void ExitGame() {
        audioManager.PlaySFX(audioManager.buttonClick);
        Application.Quit();
    }

    private void UpdateModifiersDisplay(int direction) {
        audioManager.PlaySFX(audioManager.buttonClick);
        selectedModifierIndex += direction;

        float selectedFontSize = 70f;
        float unselectedFontSize = 40f;
        Color selectedColor = new (0.64f, 0.12f, 1f, 1f);
        Color unselectedColor = new (0.4f, 0.09f, 0.58f, 1f);
        
        if (selectedModifierIndex < 0) { // If the index is lower than 0
            selectedModifierIndex = modifiers.Length - 1; // Set index to the last element on the array
        } else if (selectedModifierIndex >= modifiers.Length) { // If the index is higher or equal than the lenght of the array
            selectedModifierIndex = 0; // Set index to the first element on the array
        }

        modifierDescription.text = modifiersDesctriptions[modifiers[selectedModifierIndex].tag]; // Update modifier desctiption
        
        Vector2[] tempPositions = new Vector2[3];
        Array.Copy(modifiersPositions, tempPositions, modifiers.Length);
        for (int i = 0; i < modifiers.Length; i++) {
            int newIndex = (direction > 0) ? (i + 2) % modifiers.Length : (i + 1) % modifiers.Length;
            modifiers[i].GetComponent<RectTransform>().anchoredPosition = modifiersPositions[newIndex];
            modifiers[i].color = (i == selectedModifierIndex) ? selectedColor : unselectedColor;
            modifiers[i].fontSize = (i == selectedModifierIndex) ? selectedFontSize : unselectedFontSize;   
            tempPositions[i] = modifiersPositions[newIndex];
        }
        
        Array.Copy(tempPositions, modifiersPositions, modifiers.Length);
    }
}