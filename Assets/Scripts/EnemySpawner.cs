using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour {
    public World world;
    public GameObject enemyPrefab;
    public int enemiesToSpawn = 5;
    private float spawnRadius = VoxelData.WorldSizeInVoxels / 3f;

    private bool hasSpawned = false;

    private void Update() {
        if (!hasSpawned) {
            hasSpawned = true;
            StartCoroutine(SpawnAfterDelay(15f));
        }
    }

    private IEnumerator SpawnAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < enemiesToSpawn; i++) {
            SpawnEnemy();
        }
    }

    void SpawnEnemy() {
        Vector3 randomXZ = world.player.position + new Vector3(
            Random.Range(-spawnRadius, spawnRadius), 0, Random.Range(-spawnRadius, spawnRadius)
        );

        randomXZ.x = Mathf.Clamp(randomXZ.x, 0, VoxelData.WorldSizeInVoxels - 1);
        randomXZ.z = Mathf.Clamp(randomXZ.z, 0, VoxelData.WorldSizeInVoxels - 1);

        if (world.GetSurfacePosition(randomXZ, out Vector3 spawnPos)) {
            spawnPos += Vector3.up;
            Instantiate(enemyPrefab, spawnPos, enemyPrefab.transform.rotation);
        } else {
            Debug.LogWarning("No surface found at " + randomXZ);
        }
    }
}