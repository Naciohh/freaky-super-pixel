using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;
    public EnemyData data;
    [Range(1, 100)] public int weight = 33;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Types")]
    public List<EnemySpawnEntry> enemyTypes;

    [Header("Target")]
    public Transform player;

    [Header("Spawn Config")]
    [SerializeField] private int maxEnemiesSimultaneous = 15;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float minDistanceFromPlayer = 5f;

    [Header("Spawn Area (World Space)")]
    [SerializeField] private Vector3 spawnAreaCenter;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);

    private int activeEnemies = 0;
    private bool spawning = false;
    private Coroutine spawnCoroutine;

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= OnEnemyDied;
    }

    public void StartSpawning()
    {
        if (spawning) return;
        spawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        spawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void DestroyAllNormalEnemies()
    {
        EnemyHealth[] all = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var e in all)
        {
            if (e != null && e.data != null && !e.data.isBoss)
                Destroy(e.gameObject);
        }
        activeEnemies = 0;
    }

    private IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            if (activeEnemies < maxEnemiesSimultaneous)
                TrySpawn();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void TrySpawn()
    {
        if (enemyTypes == null || enemyTypes.Count == 0) return;

        EnemySpawnEntry entry = PickWeightedRandom();
        if (entry == null || entry.prefab == null) return;

        Vector3 pos = GetRandomSpawnPosition();

        GameObject go = Instantiate(entry.prefab, pos, Quaternion.identity);

        EnemyHealth eh = go.GetComponent<EnemyHealth>();
        if (eh != null) eh.data = entry.data;

        EnemyAI ai = go.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.data = entry.data;
            ai.player = player;
        }

        activeEnemies++;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 pos;
        int attempts = 0;
        do
        {
            pos = spawnAreaCenter + new Vector3(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                0f,
                Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            );
            attempts++;
        }
        while (player != null &&
               Vector3.Distance(pos, player.position) < minDistanceFromPlayer &&
               attempts < 10);

        return pos;
    }

    private EnemySpawnEntry PickWeightedRandom()
    {
        int total = 0;
        foreach (var e in enemyTypes) total += e.weight;
        if (total <= 0) return null;

        int roll = Random.Range(0, total);
        int cumulative = 0;
        foreach (var e in enemyTypes)
        {
            cumulative += e.weight;
            if (roll < cumulative) return e;
        }
        return enemyTypes[enemyTypes.Count - 1];
    }

    private void OnEnemyDied(EnemyHealth eh)
    {
        if (eh != null && eh.data != null && !eh.data.isBoss)
            activeEnemies = Mathf.Max(0, activeEnemies - 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize + Vector3.up * 0.1f);
    }
}
