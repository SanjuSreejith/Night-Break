using UnityEngine;
using System.Collections;

public class FakeShadowProjector : MonoBehaviour
{
    public GameObject shadowProjectorPrefab; // Assign a projector prefab
    public Transform player;
    public float spawnRadius = 5f;
    public float minSpawnTime = 5f;
    public float maxSpawnTime = 15f;

    private GameObject currentShadow;
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(SpawnShadow());
    }

    IEnumerator SpawnShadow()
    {
        while (true)
        {
            if (!isSpawning)
            {
                isSpawning = true;
                yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

                // Random position near player
                Vector3 spawnPos = player.position + (Random.insideUnitSphere * spawnRadius);
                spawnPos.y = player.position.y + 0.1f; // Slightly above ground for better projection

                currentShadow = Instantiate(shadowProjectorPrefab, spawnPos, Quaternion.Euler(90, 0, 0)); // Project on floor

                yield return new WaitForSeconds(Random.Range(3f, 7f)); // Shadow stays for some time

                Destroy(currentShadow);
                isSpawning = false;
            }
            yield return null;
        }
    }
}
