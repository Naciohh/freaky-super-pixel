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

    private bool _restored;

    void Start()
    {
        // Si venimos de un guardado, el restore (GameLoader) arranca la oleada/boss.
        if (GameSession.PendingSave != null || _restored) return;
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

    // --- Restauración desde guardado ---

    // Retoma la oleada con el tiempo restante guardado.
    public void RestoreWave(float remaining)
    {
        _restored = true;
        if (spawner == null) spawner = FindAnyObjectByType<EnemySpawner>();
        if (bossController == null) bossController = FindAnyObjectByType<BossController>(FindObjectsInactive.Include);

        TimeRemaining = Mathf.Max(remaining, 0f);
        WaveActive = true;
        spawner?.StartSpawning();
        OnWaveStart?.Invoke();
        StartCoroutine(WaveCountdown());
    }

    // Restaura la partida ya en fase de boss (la oleada terminó).
    public void RestoreBossPhase()
    {
        _restored = true;
        if (spawner == null) spawner = FindAnyObjectByType<EnemySpawner>();

        WaveActive = false;
        spawner?.StopSpawning();
        OnWaveEnd?.Invoke();   // el HUD muestra la barra del boss; el boss lo activa el restore
    }
}
