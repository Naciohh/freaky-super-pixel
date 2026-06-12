using UnityEngine;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    public int EnemiesKilled { get; private set; }
    public int ParriesPerformed { get; private set; }
    public float TimeElapsed { get; private set; }

    private bool tracking = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += OnEnemyDied;
        PlayerCombat.OnParrySuccess += OnParry;
        WaveManager.OnWaveStart += StartTracking;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= OnEnemyDied;
        PlayerCombat.OnParrySuccess -= OnParry;
        WaveManager.OnWaveStart -= StartTracking;
    }

    void Update()
    {
        if (tracking) TimeElapsed += Time.deltaTime;
    }

    private void StartTracking() => tracking = true;

    private void OnEnemyDied(EnemyHealth eh)
    {
        if (eh != null && eh.data != null && !eh.data.isBoss)
            EnemiesKilled++;
    }

    private void OnParry() => ParriesPerformed++;

    public void StopTracking() => tracking = false;

    // Restaura las stats desde un guardado y reanuda el conteo de tiempo.
    public void RestoreState(int kills, int parries, float time)
    {
        EnemiesKilled    = kills;
        ParriesPerformed = parries;
        TimeElapsed      = time;
        tracking         = true;
    }
}
