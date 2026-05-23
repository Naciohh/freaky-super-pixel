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

        if (spawner == null)
        {
            Debug.LogError("[WaveManager] EnemySpawner not assigned.", this);
            return;
        }
        if (bossController == null)
        {
            Debug.LogError("[WaveManager] BossController not assigned.", this);
            return;
        }

        TimeRemaining = waveDuration;
        WaveActive = true;
        spawner.StartSpawning();
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
        spawner.StopSpawning();
        spawner.DestroyAllNormalEnemies();
        OnWaveEnd?.Invoke();
        bossController.Activate();
    }
}
