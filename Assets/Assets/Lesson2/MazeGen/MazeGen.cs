using Unity.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MazeGen : MonoBehaviour
{
    [SerializeField] private GameObject Tile;
    [SerializeField] private GameObject Wall;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int coinCount = 10;

    [SerializeField] private Vector2Int GridSize;

    private int[,] matrix;
    private Vector2 offsets;
    int nx, ny;
    Vector2Int dir;

    private List<Vector3> tilePositions = new List<Vector3>();
    private Vector3 startPosition;

    Vector2Int[] directions = {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };
    Quaternion[] rotations = {
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 90, 0),
        Quaternion.Euler(0, 180, 0),
        Quaternion.Euler(0, 270, 0)
    };
    int[] index = {
        0, 1, 2, 3
    };
    int[] powers = {
        1, 2, 4, 8
    };

    bool borderBool;
    
    void Awake()
    {
        GenerateMaze();
        SpawnPlayer();
        SpawnCoins();
    }

    private void GenerateMaze()
    {
        matrix = new int[GridSize.x, GridSize.y];
        offsets = new Vector2(Tile.transform.localScale.x, Tile.transform.localScale.z);

        startPosition = transform.position + new Vector3(0, 0, 0);

        GenerateCell(0, 0);
    }

    private void GenerateCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= GridSize.x || y >= GridSize.y || matrix[x, y] > 0)
        {
            return;
        }

        matrix[x, y] = 16;
        Vector3 tilePosition = transform.position + new Vector3(x * offsets.x, 0, y * offsets.y);
        
        GameObject newTile = Instantiate(Tile, tilePosition, transform.rotation);
        newTile.transform.parent = transform;
        tilePositions.Add(tilePosition);

        int[] shuffledIndex = index.OrderBy(x => Random.value).ToArray();
        foreach (int ind in shuffledIndex)
        {
            dir = directions[ind];
            nx = x + dir.x;
            ny = y + dir.y;

            borderBool = nx < 0 || ny < 0 || nx >= GridSize.x || ny >= GridSize.y;
            if (borderBool || (matrix[nx, ny] > 0 && (((matrix[nx, ny] - 1) >> ((ind + 2) % 4)) % 2 == 1)))
            {
                Instantiate(Wall, transform.position + new Vector3(x * offsets.x + (offsets.x / 2f) * dir.x * 0.95f, Wall.transform.lossyScale.y / 2, y * offsets.y + (offsets.y / 2f) * dir.y * 0.95f), transform.rotation * rotations[ind]).transform.parent = transform;
            }
            else
            {
                if (matrix[nx, ny] == 0)
                {
                    matrix[x, y] -= powers[ind];
                    GenerateCell(nx, ny);
                }
            }
        }
    }

    private void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("Coin prefab is not assigned in MazeGen!");
            return;
        }

        if (tilePositions.Count == 0)
        {
            Debug.LogWarning("No tile positions found for coin spawning!");
            return;
        }

        List<Vector3> availablePositions = new List<Vector3>(tilePositions);
        int coinsToSpawn = Mathf.Min(coinCount, availablePositions.Count);
        
        for (int i = 0; i < coinsToSpawn; i++)
        {
            if (availablePositions.Count == 0) break;

            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector3 spawnPos = availablePositions[randomIndex];
            spawnPos.y += 0.5f;
            
            Instantiate(coinPrefab, spawnPos, Quaternion.identity, transform);
            availablePositions.RemoveAt(randomIndex);
        }

        Debug.Log($"Spawned {coinsToSpawn} coins in the maze");
    }

    private void SetupCamera(GameObject player)
    {
        // Находим или создаём камеру
        Camera mainCamera = Camera.main;
        GameObject cameraObject;
        
        if (mainCamera == null)
        {
            // Создаём новую камеру
            cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
        }
        else
        {
            // Используем существующую камеру
            cameraObject = mainCamera.gameObject;
        }

        // Добавляем или находим скрипт камеры на GameObject камеры
        PlayerCamera playerCamera = cameraObject.GetComponent<PlayerCamera>();
        if (playerCamera == null)
        {
            playerCamera = cameraObject.AddComponent<PlayerCamera>(); // Вызываем на GameObject!
        }

        // Настраиваем камеру
        playerCamera.playerTarget = player.transform;
        playerCamera.followSpeed = 5f;
        playerCamera.cameraOffset = new Vector3(0f, 5f, 0f);
        
        Debug.Log("Camera setup complete for player");
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("Player prefab is not assigned in MazeGen!");
            return;
        }

        Vector3 spawnPosition = FindSafeSpawnPosition();
        spawnPosition.y += 1f;

        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        SetupCamera(player);
        
        Debug.Log($"Player spawned at safe position: {spawnPosition}");
    }

    private Vector3 FindSafeSpawnPosition()
    {
        if (tilePositions.Count > 0)
        {
            return tilePositions[0];
        }
        
        return transform.position + new Vector3(2f, 0, 2f);
    }
}