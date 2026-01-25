using System.Collections.Generic;
using UnityEngine;

public class Instance_Placer : MonoBehaviour
{
    [Header("GPU Forest Settings")]
    [SerializeField] Mesh treeMesh;
    [SerializeField] List<Material> treeMaterials = new List<Material>();
    [SerializeField] int treeCount = 5000;
    [SerializeField] float spawnHeight = 10f;
    
    [Header("Optional Colliders")]
    [SerializeField] bool generateColliders = false;
    [SerializeField] LayerMask terrainLayers = 1;

    private List<Matrix4x4[]> matrices;
    private List<GameObject> colliderObjects;

    void Awake()
    {
        GenerateForest();
    }

    void GenerateForest()
    {
        matrices = new List<Matrix4x4[]>();
        colliderObjects = new List<GameObject>();

        if (treeMesh == null || treeMaterials.Count == 0)
        {
            return;
        }

        int treesPerMaterial = Mathf.Max(1, treeCount / treeMaterials.Count);

        for (int i = 0; i < treeMaterials.Count; i++)
        {
            matrices.Add(new Matrix4x4[treesPerMaterial]);
            
            for (int j = 0; j < treesPerMaterial; j++)
            {
                if (GenerateTreePosition(i, j, out Matrix4x4 matrix))
                {
                    matrices[i][j] = matrix;
                    
                    if (generateColliders)
                        SpawnTreeCollider(matrix);
                }
            }
        }
    }

    bool GenerateTreePosition(int materialIndex, int treeIndex, out Matrix4x4 matrix)
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-80f, 80f),
            spawnHeight,
            Random.Range(-80f, 80f)
        );

        if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, spawnHeight))
        {
            Vector3 position = hit.point + Vector3.up * 0.5f;
            
            float uniformScale = Random.Range(1.5f, 3.5f);
            float heightMultiplier = Random.Range(1.2f, 2.8f);
            
            Quaternion rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), Random.Range(-5f, 5f));

            Vector3 scale = new Vector3(
                uniformScale, 
                uniformScale * heightMultiplier,
                uniformScale
            );
            
            matrix = Matrix4x4.TRS(position, rotation, scale);
            return true;
        }
        
        matrix = Matrix4x4.identity;
        return false;
    }

    void SpawnTreeCollider(Matrix4x4 treeMatrix)
    {
        GameObject colliderObj = new GameObject("TreeCollider");
        colliderObj.transform.position = treeMatrix.GetPosition();
        colliderObj.transform.rotation = treeMatrix.rotation;
        colliderObj.transform.localScale = treeMatrix.lossyScale;
        colliderObj.layer = LayerMask.NameToLayer("Wall");
        
        MeshCollider mc = colliderObj.AddComponent<MeshCollider>();
        mc.sharedMesh = treeMesh;
        mc.convex = true;
        
        colliderObjects.Add(colliderObj);
    }

    void Update()
    {
        for (int i = 0; i < treeMaterials.Count; i++)
        {
            if (matrices[i] != null)
            {
                Graphics.DrawMeshInstanced(
                    treeMesh,
                    0,
                    treeMaterials[i],
                    matrices[i]
                );
            }
        }
    }

    void OnDestroy()
    {
        if (colliderObjects != null)
        {
            for (int i = colliderObjects.Count - 1; i >= 0; i--)
            {
                if (colliderObjects[i] != null)
                    Destroy(colliderObjects[i]);
            }
        }
    }
}
