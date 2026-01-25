using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class MazeGen : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject rocketLauncherPrefab;

    [Header("Settings")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(15, 15);
    [SerializeField] private int coinCount = 10;
    [SerializeField] private int turretCount = 4;

    private GameObject playerInstance;
    private List<Vector3> tilePositions = new List<Vector3>();
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int[,] mazeMatrix;

    private readonly Vector2Int[] directions = {
        new Vector2Int(0, 1), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(-1, 0)
    };
    private readonly Quaternion[] rotations = {
        Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 90, 0),
        Quaternion.Euler(0, 180, 0), Quaternion.Euler(0, 270, 0)
    };
    private readonly int[] powers = { 1, 2, 4, 8 };

    void Awake()
    {
        Health.OnPlayerDeath += HandlePlayerDeath;
        InitializeMaze();
    }

    void OnDestroy()
    {
        Health.OnPlayerDeath -= HandlePlayerDeath;
    }

    private void InitializeMaze()
    {
        ClearAllObjects();
        GenerateMaze();
        SpawnPlayer();
        SpawnCoins();
        SpawnTurrets();
    }

    private void ClearAllObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
                DestroyImmediate(spawnedObjects[i]);
        }
        spawnedObjects.Clear();
        
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        mazeMatrix = null;
        tilePositions.Clear();
    }

    public void GenerateMaze()
    {
        if (tilePrefab == null)
        {
            return;
        }

        tilePositions.Clear();
        mazeMatrix = new int[gridSize.x, gridSize.y];
        
        Vector3 tileScale = tilePrefab.transform.localScale;
        Vector2 cellSize = new Vector2(tileScale.x, tileScale.z);
        
        GenerateCell(0, 0, cellSize);
    }

    private void GenerateCell(int x, int y, Vector2 cellSize)
    {
        if (x < 0 || y < 0 || x >= gridSize.x || y >= gridSize.y || mazeMatrix[x, y] > 0)
            return;

        mazeMatrix[x, y] = 16;
        Vector3 tilePos = transform.position + new Vector3(x * cellSize.x, 0, y * cellSize.y);
        
        GameObject tile = Instantiate(tilePrefab, tilePos, Quaternion.identity, transform);
        spawnedObjects.Add(tile);
        tilePositions.Add(tilePos);

        int[] shuffledDirs = new int[] { 0, 1, 2, 3 };
        ShuffleArray(shuffledDirs);
        
        for (int i = 0; i < 4; i++)
        {
            int dirIndex = shuffledDirs[i];
            Vector2Int dir = directions[dirIndex];
            int nx = x + dir.x;
            int ny = y + dir.y;

            bool isBorder = nx < 0 || ny < 0 || nx >= gridSize.x || ny >= gridSize.y;
            if (isBorder || (mazeMatrix[nx, ny] > 0 && HasWall(mazeMatrix[nx, ny], (dirIndex + 2) % 4)))
            {
                PlaceWall(x, y, dirIndex, cellSize);
            }
            else if (mazeMatrix[nx, ny] == 0)
            {
                mazeMatrix[x, y] -= powers[dirIndex];
                GenerateCell(nx, ny, cellSize);
            }
        }
    }

    private void ShuffleArray(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    private bool HasWall(int cellValue, int direction)
    {
        return ((cellValue - 1) >> direction) % 2 == 1;
    }

    private void PlaceWall(int x, int y, int dirIndex, Vector2 cellSize)
    {
        Vector2Int dir = directions[dirIndex];
        Vector3 wallPos = transform.position + new Vector3(
            x * cellSize.x + (cellSize.x / 2f) * dir.x * 0.95f,
            wallPrefab.transform.lossyScale.y / 2,
            y * cellSize.y + (cellSize.y / 2f) * dir.y * 0.95f
        );
        GameObject wall = Instantiate(wallPrefab, wallPos, rotations[dirIndex], transform);
        spawnedObjects.Add(wall);
    }

    public void SpawnCoins()
    {
        if (coinPrefab == null || tilePositions.Count == 0) return;

        List<Vector3> spawnPositions = new List<Vector3>();
        GetTileBounds(out float minX, out float maxX, out float minZ, out float maxZ);
        
        int gridSizeCoins = Mathf.CeilToInt(Mathf.Sqrt(coinCount));
        for (int x = 0; x < gridSizeCoins; x++)
        {
            for (int z = 0; z < gridSizeCoins; z++)
            {
                float targetX = minX + (maxX - minX) * (x + 0.5f) / gridSizeCoins;
                float targetZ = minZ + (maxZ - minZ) * (z + 0.5f) / gridSizeCoins;
                Vector3 nearestTile = FindNearestTile(new Vector3(targetX, 0, targetZ));
                if (nearestTile != Vector3.zero)
                    spawnPositions.Add(nearestTile);
            }
        }

        int coinsToSpawn = Mathf.Min(coinCount, spawnPositions.Count);
        for (int i = 0; i < coinsToSpawn; i++)
        {
            Vector3 pos = spawnPositions[i] + Vector3.up * 0.5f;
            GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity, transform);
            spawnedObjects.Add(coin);
        }
    }

    private void GetTileBounds(out float minX, out float maxX, out float minZ, out float maxZ)
    {
        minX = float.MaxValue; maxX = float.MinValue;
        minZ = float.MaxValue; maxZ = float.MinValue;
        
        for (int i = 0; i < tilePositions.Count; i++)
        {
            Vector3 pos = tilePositions[i];
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minZ = Mathf.Min(minZ, pos.z);
            maxZ = Mathf.Max(maxZ, pos.z);
        }
    }

    private Vector3 FindNearestTile(Vector3 target)
    {
        Vector3 nearest = Vector3.zero;
        float minDist = float.MaxValue;
        
        for (int i = 0; i < tilePositions.Count; i++)
        {
            float dist = Vector3.Distance(tilePositions[i], target);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = tilePositions[i];
            }
        }
        return nearest;
    }

    public void SpawnTurrets()
    {
        if (rocketLauncherPrefab == null || playerInstance == null || tilePositions.Count == 0) 
            return;

        List<Vector3> candidatePositions = new List<Vector3>();
        Vector3 playerPos = playerInstance.transform.position;
        
        foreach (Vector3 tilePos in tilePositions)
        {
            float distance = Vector3.Distance(tilePos, playerPos);
            
            if (distance > 6f)
                candidatePositions.Add(tilePos);
        }

        List<Vector3> validPositions = new List<Vector3>();
        int rejected = 0;
        
        foreach (Vector3 pos in candidatePositions)
        {
            bool rejectedReason = false;
            
            if (HasCoinNearby(pos)) { rejected++; rejectedReason = true; continue; }
            
            bool hasWall = HasWallBetween(playerPos, pos);
            if (!hasWall) { rejected++; rejectedReason = true; continue; }
            
            foreach (Vector3 validPos in validPositions)
            {
                if (Vector3.Distance(pos, validPos) < 4f)
                {
                    rejected++;
                    rejectedReason = true;
                    break;
                }
            }
            if (rejectedReason) continue;
            
            validPositions.Add(pos);
        }
        
        int spawnedCount = Mathf.Min(turretCount, validPositions.Count);
        for (int i = 0; i < spawnedCount; i++)
        {
            int index = UnityEngine.Random.Range(0, validPositions.Count);
            Vector3 pos = validPositions[index];
            
            GameObject turret = Instantiate(rocketLauncherPrefab, pos + Vector3.up * 0.1f, Quaternion.identity);
            turret.transform.localScale *= 0.2f;
            spawnedObjects.Add(turret);
            
            RLLook lookScript = turret.GetComponentInChildren<RLLook>();
            if (lookScript != null)
                lookScript.target = playerInstance.transform;
            
            validPositions.RemoveAt(index);
        }
    }

    private bool HasWallBetween(Vector3 pointA, Vector3 pointB)
    {
        Vector3 direction = (pointB - pointA).normalized;
        float distance = Vector3.Distance(pointA, pointB);
        
        return Physics.Raycast(pointA + Vector3.up * 0.5f, direction, distance * 0.95f, LayerMask.GetMask("Wall"));
    }

    private bool HasCoinNearby(Vector3 position)
    {
        Collider[] nearby = Physics.OverlapSphere(position + Vector3.up * 0.5f, 0.5f);
        for (int i = 0; i < nearby.Length; i++)
        {
            if (nearby[i].CompareTag("Coin"))
                return true;
        }
        return false;
    }

    public void SpawnPlayer()
    {
        if (playerPrefab == null || tilePositions.Count == 0) return;
        
        Vector3 spawnPos = tilePositions[0] + Vector3.up;
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        
        SetupCamera(playerInstance);
        
        Health playerHealth = playerInstance.GetComponent<Health>();
        if (playerHealth != null)
            playerHealth.Respawn();
    }

    private void SetupCamera(GameObject player)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
        }
        
        PlayerCamera playerCam = cam.GetComponent<PlayerCamera>();
        if (playerCam == null)
            playerCam = cam.gameObject.AddComponent<PlayerCamera>();
            
        playerCam.playerTarget = player.transform;
        playerCam.followSpeed = 5f;
        playerCam.cameraOffset = Vector3.up * 5f;
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(RespawnEverything());
    }

    private IEnumerator RespawnEverything()
    {
        if (playerInstance != null)
        {
            Destroy(playerInstance);
            playerInstance = null;
        }
        
        yield return new WaitForSeconds(1.5f);
        
        InitializeMaze();
    }
}
