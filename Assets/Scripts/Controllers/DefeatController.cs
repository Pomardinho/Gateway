using UnityEngine;

public class DefeatController : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;

    void Update() {
        if (Input.GetButtonDown("RestartLevel")) { // "RestartLevel" is the button assigned for r key
            sceneManager.LoadScene("Home");
        }
    }
}
