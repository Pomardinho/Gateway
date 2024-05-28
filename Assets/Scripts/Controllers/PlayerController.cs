using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour {
    // Possible improvements: buffer input (prior on jump, maybe in power-ups), coyote time

    private readonly float Speed = 8f;
    private readonly float JumpForce = 13f;
    private readonly float DashVelocity = 24f;
    private readonly float DashingTime = 0.2f;
    private readonly float StompVelocity = 24f;
    private readonly float CastDistance = 0.45f;
    
    [SerializeField] public Light2D spotLight;
    [SerializeField] public ParticleSystem deathParticles;
    private Light2D globalLight;
    private bool gauntletModifierEnabled = false;

    private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;
    private float xAxis;
    private float startDirection;
    private Vector2 startPosition;
    private bool isTouchingBreakable;
    private bool isTouchingJumpableWall;
    private bool isTouchingOneWayPlatform;
    private bool isTouchingPortal;
    private float wallJumpTime;
    private Vector2 wallJumpDirection;
    private Vector2 wallJumpForce = new (8f, 13f);

    private Vector2 boxSize = new (0.60f, 0.1f);
    private Vector3 boxOffset = new (0.03f, 0f);
    private List<Vector3Int> tilesToBreakPositions;

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private PowerUpsManager powerUpsManager;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private Tilemap breakableTilemap;
    [SerializeField] private Tile breakableTile;
    private AudioManager audioManager;
    private float gravity;
    private List<GameObject> powerUpsToRestore = new ();
    private List<Vector3Int> tilesToRestorePositions = new ();

    void Awake() {
        audioManager = AudioManager.Instance;
        globalLight = GameObject.FindGameObjectWithTag("GlobalLight").GetComponent<Light2D>();

        if (PlayerPrefs.HasKey("GauntletModifier")) {
            gauntletModifierEnabled = true;
        }
        
        if (PlayerPrefs.HasKey("BlindModifier")) {
            globalLight.intensity = 0;
        }
    }

    void Start () {
        startDirection = transform.localScale.x;
        startPosition = transform.position;
        gravity = rigidBody.gravityScale;
    }

    void Update() {
        GetInputs();
        if (wallJumpTime <= 0) { // Don't let the player move or flip while the wall jumping
            Move();
            Flip();
        } else {
            if (IsGrounded()) {
                wallJumpTime = 0;
            } else {
                wallJumpTime -= Time.deltaTime;
            }
        }

        Jump();
        Dash();
        Stomp();
        NextLevel();
        RestartLevel();
    }

    void GetInputs() {
        xAxis = Input.GetAxis("Horizontal");
    }

    private void Move() {
        rigidBody.velocity = new Vector2(Speed * xAxis, rigidBody.velocity.y);
        if (Mathf.Abs(rigidBody.velocity.x) > 0.1f) {
            animator.SetBool("IsRunning", true);
        } else {
            animator.SetBool("IsRunning", false);
        }
    }

    private bool IsGrounded() {
        return Physics2D.BoxCast(transform.position + boxOffset, boxSize, 0, -transform.up, CastDistance, groundLayer);
    }

    private void Jump() {
        coyoteTimeCounter = IsGrounded() ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        if (Input.GetButtonDown("Jump")) {
            if (coyoteTimeCounter > 0f || isTouchingOneWayPlatform) {
                animator.SetBool("IsJumping", true);
                audioManager.PlaySFX(audioManager.jump);
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, JumpForce);
            } else if (isTouchingJumpableWall) {
                animator.SetBool("IsJumping", true);
                audioManager.PlaySFX(audioManager.jump);
                rigidBody.velocity = new Vector2(wallJumpDirection.x * wallJumpForce.x, wallJumpForce.y);
                wallJumpTime = 0.5f;
                
                // Force flip
                if (transform.localScale.x != wallJumpDirection.x) {
                    transform.localScale = new Vector2(wallJumpDirection.x, transform.localScale.y);
                }
            } else if (powerUpsManager.GetPowerUpCounts()["DoubleJump"] > 0) {
                if (powerUpsManager.UpdatePowerUpCount("DoubleJump", true)) {
                    animator.SetBool("IsJumping", true);
                    audioManager.PlaySFX(audioManager.jump);
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, JumpForce);
                }
            }
        } else if (!IsGrounded()) {
            if (isTouchingOneWayPlatform) {
                animator.SetBool("IsJumping", false);
            } else {
                animator.SetBool("IsJumping", true);
            }
        } else {
            animator.SetBool("IsJumping", false);
        }
    }

    private void Dash() {
        if (Input.GetButtonDown("Dash") && powerUpsManager.GetPowerUpCounts()["Dash"] > 0) {
            StartCoroutine(DashRoutine());
        }
    }

    private void Stomp() {
        if (Input.GetButtonDown("Stomp") && powerUpsManager.GetPowerUpCounts()["Stomp"] > 0) {
            if (powerUpsManager.UpdatePowerUpCount("Stomp", true)) {
                if (isTouchingBreakable) {
                    foreach (Vector3Int tileToBreakPosition in tilesToBreakPositions) {
                        tilesToRestorePositions.Add(tileToBreakPosition);
                        breakableTilemap.SetTile(tileToBreakPosition, null);
                    }

                    rigidBody.velocity = new (0f, -StompVelocity); // Accelerate the player to the ground
                    audioManager.PlaySFX(audioManager.stomp);
                    StartCoroutine(RestoreBreakableTiles(3));
                }
            }
        }
    }

    private void NextLevel() {
        if (Input.GetButtonDown("Confirm") && isTouchingPortal) {
            audioManager.PlaySFX(audioManager.nextLevel);
            sceneManager.NextLevel();
        }
    }

    private void RestartLevel() {
        if (Input.GetButtonDown("RestartLevel")) {
            StartCoroutine(Respawn(0.5f));
        }
    }

    private IEnumerator DashRoutine() {
        if (powerUpsManager.UpdatePowerUpCount("Dash", true)) {
            float elapsedTime = 0f;
            animator.SetBool("IsDashing", true);
            audioManager.PlaySFX(audioManager.dash);
            rigidBody.gravityScale = 0; // Make the player don't be affected by gravity
            while (elapsedTime < DashingTime) {
                elapsedTime += Time.deltaTime;
                rigidBody.velocity = new (transform.localScale.x * DashVelocity, 0f);
                yield return null;
            }

            animator.SetBool("IsDashing", false);
            rigidBody.gravityScale = gravity; // Restore normal gravity
        }
    }

    private IEnumerator RestoreBreakableTiles(float delay) {
        yield return new WaitForSeconds(delay);
        foreach (Vector3Int tileToRestorePosition in tilesToRestorePositions) {
            if (breakableTilemap.GetTile(tileToRestorePosition) as Tile != breakableTile) { // Check if tile has to be restored
                breakableTilemap.SetTile(tileToRestorePosition, breakableTile);
            }
        }
    }

    public IEnumerator Respawn(float delay) {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level1" || !gauntletModifierEnabled) {
            // Restore level power-ups
            foreach (GameObject powerUpToRestore in powerUpsToRestore) {
                powerUpsManager.UpdatePowerUpCount(powerUpToRestore.tag, true);
                powerUpToRestore.SetActive(true);
            }

            // Restore level breakable tiles
            foreach (Vector3Int tileToRestorePosition in tilesToRestorePositions) {
                if (breakableTilemap.GetTile(tileToRestorePosition) as Tile != breakableTile) { // Check if tile has to be restored
                    breakableTilemap.SetTile(tileToRestorePosition, breakableTile);
                }
            }
            audioManager.PlaySFX(audioManager.death); // Play death sound
            deathParticles.Play(); // Play death particle system
            rigidBody.simulated = false; // Stop physics
            rigidBody.velocity = new (0, 0); // Reset player velocity
            transform.localScale = new (0, 0, 0); // Make the player invisible by removing its scale
            spotLight.gameObject.SetActive(false); // Disable the player's light
            yield return new WaitForSeconds(delay); // Wait for x seconds to continue
            transform.position = startPosition; // Restart the position of the player
            rigidBody.simulated = true; // Re-enable physics
            transform.localScale = new (startDirection, 1, 1); // Make the player visible again
            spotLight.gameObject.SetActive(true); // Re-enable the player's light
        } else {
            sceneManager.LoadScene("Level1");
        }
    }

    private void Flip() {
        if (xAxis < 0) {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        } else if (xAxis > 0) {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Portal")) {
            isTouchingPortal = true;
        } else if (powerUpsManager.UpdatePowerUpCount(other.gameObject.tag, false)) {
            powerUpsToRestore.Add(other.gameObject);
            other.gameObject.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Portal")) {
            isTouchingPortal = false;
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Breakable")) {
            List<ContactPoint2D> contacts = new ();
            other.GetContacts(contacts); // Get collision contact points

            // Get the first contact (normally the closest to the player)
            Vector2 tilePosition = IsGrounded() ? contacts[0].point + new Vector2(0, -1) : contacts[0].point + new Vector2(0, 1);

            // Breakable tiles will always be in a set of 3 so we want to find the other 2 tiles adjacent to the tile we collided with
            List<Vector3Int> adjacentTiles = new ();
            Grid grid = breakableTilemap.layoutGrid;
            BoundsInt bounds = breakableTilemap.cellBounds;
            for (int x = bounds.min.x; x <= bounds.max.x; x++) {
                for (int y = bounds.min.y; y <= bounds.max.y; y++) {
                    Vector3Int position = new (x, y, 0);
                    if (breakableTilemap.HasTile(position)) {
                        // Find the tiles in range (2 because is the maximum gap distance between a set of 3 tiles)
                        if (Mathf.Abs(position.x - grid.WorldToCell(tilePosition).x) <= 2 && Mathf.Abs(position.y - grid.WorldToCell(tilePosition).y) <= 1) {
                            adjacentTiles.Add(position);
                        }
                    }
                }
            }

            tilesToBreakPositions = adjacentTiles;
            isTouchingBreakable = true;
        } else if (other.gameObject.CompareTag("JumpableWall")) {
            ContactPoint2D contact = other.contacts[0];
            wallJumpDirection = contact.normal.x < 0 ? Vector2.left : Vector2.right; // (-1, 0) or (1, 0) depending on contact
            isTouchingJumpableWall = true;
        } else if (other.gameObject.CompareTag("OneWayPlatform")) {
            isTouchingOneWayPlatform = true;
        } else if (other.gameObject.CompareTag("Spike")) {
            StartCoroutine(Respawn(0.5f));
        }
    }

    void OnCollisionExit2D(Collision2D other) {
        if (other.gameObject.CompareTag("Breakable")) {
            isTouchingBreakable = false;
        } else if (other.gameObject.CompareTag("JumpableWall")) {
            isTouchingJumpableWall = false;
        } else if (other.gameObject.CompareTag("OneWayPlatform")) {
            isTouchingOneWayPlatform = false;
        }
    }

    // private void OnDrawGizmos() {
    //     Gizmos.DrawWireCube(transform.position - transform.up * CastDistance + boxOffset, boxSize);
    // }
}
