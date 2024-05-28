using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PowerUpsManager : MonoBehaviour {
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tile emptyDoubleJump;
    [SerializeField] private Tile fullDoubleJump;
    [SerializeField] private Tile emptyDash;
    [SerializeField] private Tile fullDash;
    [SerializeField] private Tile emptyStomp;
    [SerializeField] private Tile fullStomp;
    
    private readonly int PowerUpCountLimit = 2;
    private Dictionary<string, int> powerUpCounts;
    private Dictionary<string, List<Vector3Int>> powerUpTilePositions;
    private Dictionary<string, (Tile emptyPowerUp, Tile fullPowerUp)> powerUpTiles;
    private AudioManager audioManager;

    void Awake() {
        audioManager = AudioManager.Instance;
    }

    void Start() {
        powerUpCounts = new Dictionary<string, int>() {
            { "DoubleJump", 0 },
            { "Dash", 0 },
            { "Stomp", 0 }
        };

        powerUpTiles = new Dictionary<string, (Tile emptyPowerUp, Tile fullPowerUp)>() {
            { "DoubleJump", (emptyDoubleJump, fullDoubleJump) },
            { "Dash", (emptyDash, fullDash) },
            { "Stomp", (emptyStomp, fullStomp) }
        };

        powerUpTilePositions = new Dictionary<string, List<Vector3Int>>() {
            { "DoubleJump", new List<Vector3Int>() },
            { "Dash", new List<Vector3Int>() },
            { "Stomp", new List<Vector3Int>() }
        };
    }

    // Returns a boolean that indicates if the specified power-up count has been updated
    public bool UpdatePowerUpCount(string tag, bool subtract) {
        bool success = false;
        if (powerUpCounts.ContainsKey(tag)) {
            // Check if the power-up count should be subtracted or added
            if (subtract && powerUpCounts[tag] > 0) {
                success = true;
                powerUpCounts[tag]--;
                UpdatePowerUpsDisplay(tag, subtract);
            } else if (!subtract && powerUpCounts[tag] < PowerUpCountLimit) {
                success = true;
                powerUpCounts[tag]++;
                audioManager.PlaySFX(audioManager.pickUpObject);
                UpdatePowerUpsDisplay(tag, subtract);
            }
        } else {
            throw new ArgumentException("Invalid power-up tag");
        }
        
        return success;
    }

    // Updates the visualization of the specified power-up
    public void UpdatePowerUpsDisplay(string tag, bool subtract) {
        if (powerUpTiles.TryGetValue(tag, out (Tile emptyTile, Tile fullTile) powerUpTile)) {
            // Get the tile positions of the specified power-up
            List<Vector3Int> tilePositions = powerUpTilePositions[tag];
            tilePositions.Clear();
            
            // Iterate over all positions of the grid
            BoundsInt bounds = tilemap.cellBounds;
            for (int x = bounds.min.x; x <= bounds.max.x; x++) {
                for (int y = bounds.min.y; y <= bounds.max.y; y++) {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    if (tilemap.HasTile(position)) {
                        Tile tile = tilemap.GetTile(position) as Tile;
                        if (tile == powerUpTile.emptyTile || tile == powerUpTile.fullTile) { // Check if the position contains a power-up tile
                            tilePositions.Add(position); // Add the power-up tile position
                        }
                    }
                }
            }

            // Update the visualization of the power-up based on whether the count should be subtracted or added
            if (subtract) tilemap.SetTile(tilePositions.FindLast(tile => tilemap.GetTile(tile) == powerUpTile.fullTile), powerUpTile.emptyTile);
            else tilemap.SetTile(tilePositions.Find(tile => tilemap.GetTile(tile) == powerUpTile.emptyTile), powerUpTile.fullTile);
        }
    }

    public void ClearPowerUps() {
        foreach (var key in powerUpCounts.Keys) {
            powerUpCounts[key] = 0;
        }
    }

    public Dictionary<string, int> GetPowerUpCounts() {
        return powerUpCounts;
    }
}
