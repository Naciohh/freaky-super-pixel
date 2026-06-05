using UnityEngine;
using System;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float waveDuration = 90f;

    [Header("Referencias")]
    public EnemySpawner spawner;
    public BossController bossController;

    public static event Action OnWaveStart;
    public static event Action OnWaveEnd;

    public float TimeRemaining { get; private set; }
    public bool WaveActive { get; private set; }

    void Start()
    {
        StartWave();
    }

    public void StartWave()
    {
        if (WaveActive) return;

        // Auto-resolver referencias si no se asignaron en el Inspector.
        if (spawner == null) spawner = FindAnyObjectByType<EnemySpawner>();
        if (bossController == null) bossController = FindAnyObjectByType<BossController>();

        if (spawner == null)
            Debug.LogWarning("[WaveManager] EnemySpawner no encontrado; no se generarán enemigos en esta oleada.", this);
        if (bossController == null)
            Debug.LogWarning("[WaveManager] BossController no encontrado; el jefe no se activará.", this);

        // La oleada y la música arrancan igual aunque falte alguna referencia:
        // así la transición de música nunca queda bloqueada en silencio.
        TimeRemaining = waveDuration;
        WaveActive = true;
        spawner?.StartSpawning();
        OnWaveStart?.Invoke();
        StartCoroutine(WaveCountdown());
    }

    private IEnumerator WaveCountdown()
    {
        while (TimeRemaining > 0f)
        {
            TimeRemaining -= Time.deltaTime;
            yield return null;
        }

        TimeRemaining = 0f;
        WaveActive = false;
        EndWave();
    }

    private void EndWave()
    {
        spawner?.StopSpawning();
        spawner?.DestroyAllNormalEnemies();
        OnWaveEnd?.Invoke();
        bossController?.Activate();
    }
}
