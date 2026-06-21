using UnityEngine;
using System.Collections;

/// <summary>
/// Spawnea "ositos" minion en horda durante la pelea del boss, SOLO en dificultad
/// Pesadilla. La idea es agobiar y entorpecer la pelea contra el oso grande: los
/// minions son enemigos normales (EnemyAI/EnemyHealth) que caminan hacia Emilio y
/// se meten en su cono de ataque/parry.
///
/// Arranca con <see cref="BossController.OnBossReady"/> (cuando empieza la pelea) y
/// se detiene + limpia con <see cref="BossBearHealth.OnBossDied"/>.
/// </summary>
public class BossMinionSpawner : MonoBehaviour
{
    [Header("Minion (osito)")]
    public GameObject minionPrefab;
    public EnemyData minionData;
    public Transform player;

    [Header("Horda")]
    [SerializeField] private int maxAlive = 8;
    [SerializeField] private float spawnInterval = 2.5f;
    [SerializeField] private float minDistanceFromPlayer = 6f;

    [Header("Área de spawn (entre las 4 paredes)")]
    [SerializeField] private Vector3 mapBoundsCenter = new Vector3(-56.5f, 0f, -0.25f);
    [SerializeField] private Vector3 mapBoundsSize   = new Vector3(280f, 0f, 235f);

    private int alive = 0;
    private bool active = false;
    private Coroutine loop;

    void OnEnable()
    {
        BossController.OnBossReady += HandleBossReady;
        BossBearHealth.OnBossDied += HandleBossDied;
        EnemyHealth.OnAnyEnemyDied += HandleEnemyDied;
    }

    void OnDisable()
    {
        BossController.OnBossReady -= HandleBossReady;
        BossBearHealth.OnBossDied -= HandleBossDied;
        EnemyHealth.OnAnyEnemyDied -= HandleEnemyDied;
        Stop();
    }

    // Empieza la pelea del boss: si es Pesadilla, arranca la horda.
    private void HandleBossReady()
    {
        if (GameDifficulty.Current != Difficulty.Pesadilla) return;
        if (minionPrefab == null || minionData == null)
        {
            Debug.LogWarning("[BossMinionSpawner] Falta minionPrefab o minionData; no spawnean ositos.", this);
            return;
        }
        if (player == null)
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo != null) player = pgo.transform;
        }

        active = true;
        if (loop == null) loop = StartCoroutine(Loop());
    }

    private void HandleBossDied(BossBearHealth _)
    {
        Stop();
        ClearMinions();
    }

    private void Stop()
    {
        active = false;
        if (loop != null) { StopCoroutine(loop); loop = null; }
    }

    private IEnumerator Loop()
    {
        while (active)
        {
            if (alive < maxAlive) Spawn();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void Spawn()
    {
        Vector3 pos = RandomPointInMap();
        for (int i = 0; i < 12 && player != null &&
             Vector3.Distance(pos, player.position) < minDistanceFromPlayer; i++)
            pos = RandomPointInMap();

        GameObject go = Instantiate(minionPrefab, pos, Quaternion.identity);

        EnemyHealth eh = go.GetComponent<EnemyHealth>();
        if (eh != null) eh.data = minionData;

        EnemyAI ai = go.GetComponent<EnemyAI>();
        if (ai != null) { ai.data = minionData; ai.player = player; }

        alive++;
    }

    // Solo contamos NUESTROS minions (los que usan minionData).
    private void HandleEnemyDied(EnemyHealth eh)
    {
        if (eh != null && eh.data == minionData)
            alive = Mathf.Max(0, alive - 1);
    }

    private void ClearMinions()
    {
        var all = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var e in all)
            if (e != null && e.data == minionData) Destroy(e.gameObject);
        alive = 0;
    }

    private Vector3 RandomPointInMap()
    {
        Vector3 half = mapBoundsSize * 0.5f;
        return new Vector3(
            mapBoundsCenter.x + Random.Range(-half.x, half.x),
            mapBoundsCenter.y,
            mapBoundsCenter.z + Random.Range(-half.z, half.z));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.12f);
        Gizmos.DrawCube(mapBoundsCenter, mapBoundsSize + Vector3.up * 0.1f);
    }
}
