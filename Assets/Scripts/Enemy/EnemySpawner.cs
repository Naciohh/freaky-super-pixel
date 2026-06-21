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

    [Header("Área de spawn (todo el mapa, entre las 4 paredes)")]
    [Tooltip("Los enemigos aparecen en cualquier punto dentro de esta caja. Default = mapa de Nivel01 (entre las paredes con fotos).")]
    [SerializeField] private Vector3 mapBoundsCenter = new Vector3(-56.5f, 0f, -0.25f);
    [SerializeField] private Vector3 mapBoundsSize   = new Vector3(280f, 0f, 235f);

    private int activeEnemies = 0;
    private bool spawning = false;
    private Coroutine spawnCoroutine;

    void Awake()
    {
        // La dificultad escala cuántos enemigos hay a la vez y cada cuánto salen.
        maxEnemiesSimultaneous = Mathf.Max(1, Mathf.RoundToInt(maxEnemiesSimultaneous * GameDifficulty.SpawnCountMult));
        spawnInterval = Mathf.Max(0.2f, spawnInterval * GameDifficulty.SpawnIntervalMult);
    }

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

    // Re-instancia los enemigos guardados en su posición y con su HP.
    public void RestoreEnemies(List<EnemySaveData> saved)
    {
        if (saved == null) return;

        foreach (var s in saved)
        {
            EnemySpawnEntry entry = FindEntry(s.dataName);
            if (entry == null || entry.prefab == null) continue;

            Vector3 pos = new Vector3(s.posX, s.posY, s.posZ);
            GameObject go = Instantiate(entry.prefab, pos, Quaternion.identity);

            EnemyHealth eh = go.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.data = entry.data;
                eh.restoreHP = s.currentHP;
            }

            EnemyAI ai = go.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.data = entry.data;
                ai.player = player;
            }

            activeEnemies++;
        }
    }

    private EnemySpawnEntry FindEntry(string dataName)
    {
        if (enemyTypes == null) return null;
        foreach (var e in enemyTypes)
            if (e.data != null && e.data.name == dataName) return e;
        return null;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Cualquier punto dentro del mapa (entre las 4 paredes), evitando caer
        // demasiado cerca de Emilio para que no aparezcan encima de él.
        Vector3 pos = RandomPointInMap();
        for (int i = 0; i < 12 && player != null &&
             Vector3.Distance(pos, player.position) < minDistanceFromPlayer; i++)
            pos = RandomPointInMap();
        return pos;
    }

    private Vector3 RandomPointInMap()
    {
        Vector3 half = mapBoundsSize * 0.5f;
        return new Vector3(
            mapBoundsCenter.x + Random.Range(-half.x, half.x),
            mapBoundsCenter.y,
            mapBoundsCenter.z + Random.Range(-half.z, half.z));
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
        // Área de spawn = todo el mapa entre las 4 paredes.
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(mapBoundsCenter, mapBoundsSize + Vector3.up * 0.1f);
    }
}
