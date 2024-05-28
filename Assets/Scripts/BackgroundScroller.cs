using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private float xSpeed, ySpeed;

    private static GameObject existingBackgroundScroller;

    void Awake () {
        if (existingBackgroundScroller == null) {
            // Prevent destroying the canvas when a new scene is loaded so the timer keeps running
            GameObject canvas = this.transform.parent.gameObject;
            existingBackgroundScroller = canvas;
            DontDestroyOnLoad(canvas);
        } else {
            // Destroy if there's already a background scroller canvas
            Destroy(gameObject);
        }
    }

    void Update()
    {
        rawImage.uvRect = new Rect(rawImage.uvRect.position + new Vector2(xSpeed, ySpeed) * Time.deltaTime, rawImage.uvRect.size);        
    }
}
