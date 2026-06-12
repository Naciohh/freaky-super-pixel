using UnityEngine;

// Va en el Player. Si hay un guardado pendiente, restaura TODO el estado de la partida.
public class GameLoader : MonoBehaviour
{
    void Start()
    {
        var save = GameSession.PendingSave;
        if (save == null) return;

        GameSession.PendingSave = null;

        // --- Jugador ---
        // Ignoramos posY: que la gravedad lo apoye en el piso.
        transform.position = new Vector3(save.posX, transform.position.y, save.posZ);
        transform.rotation = Quaternion.Euler(0f, save.rotY, 0f);

        var ph = GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.currentHealth = save.currentHealth;
            if (ph.healthSlider != null)
                ph.healthSlider.value = save.currentHealth;
        }

        var tm = GetComponent<TransformationMode>();
        if (tm != null)
            tm.RestoreState(save.parryCount, save.isTransformed);

        // --- Estadísticas ---
        if (GameStats.Instance != null)
            GameStats.Instance.RestoreState(save.enemiesKilled, save.parriesPerformed, save.timeElapsed);

        // --- Mundo: oleada / boss / enemigos ---
        var wave = FindAnyObjectByType<WaveManager>();
        var spawner = FindAnyObjectByType<EnemySpawner>();

        if (save.bossActive)
        {
            // Estaba en la pelea del boss.
            wave?.RestoreBossPhase();
            var boss = FindAnyObjectByType<BossController>(FindObjectsInactive.Include);
            boss?.ActivateRestored(save.bossHP);
        }
        else
        {
            // Estaba en la oleada: re-spawnear enemigos y retomar el contador.
            if (spawner != null)
                spawner.RestoreEnemies(save.enemies);
            wave?.RestoreWave(save.waveTimeRemaining);
        }
    }
}
