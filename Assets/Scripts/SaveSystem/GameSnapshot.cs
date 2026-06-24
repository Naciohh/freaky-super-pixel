using UnityEngine;
using UnityEngine.SceneManagement;

// Arma un SaveData con TODO el estado actual de la partida (sin necesidad de referencias en escena).
public static class GameSnapshot
{
    public static SaveData Capture()
    {
        var data = new SaveData();
        data.sceneName = SceneManager.GetActiveScene().name;
        data.difficulty = (int)GameDifficulty.Current;

        // --- Jugador ---
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            Vector3 p = playerGO.transform.position;
            data.posX = p.x; data.posY = p.y; data.posZ = p.z;
            data.rotY = playerGO.transform.eulerAngles.y;

            var ph = playerGO.GetComponent<PlayerHealth>();
            data.currentHealth = ph != null ? ph.currentHealth : 100;

            var tm = playerGO.GetComponent<TransformationMode>();
            if (tm != null)
            {
                data.parryCount = tm.ParryCount;
                data.isTransformed = tm.IsTransformed;
            }
        }

        // --- Estadísticas ---
        if (GameStats.Instance != null)
        {
            data.enemiesKilled = GameStats.Instance.EnemiesKilled;
            data.parriesPerformed = GameStats.Instance.ParriesPerformed;
            data.timeElapsed = GameStats.Instance.TimeElapsed;
        }

        // --- Oleada ---
        var wave = Object.FindAnyObjectByType<WaveManager>();
        if (wave != null)
        {
            data.waveTimeRemaining = wave.TimeRemaining;
            data.waveActive = wave.WaveActive;
        }

        // --- Boss (solo si ya apareció) ---
        var boss = Object.FindAnyObjectByType<BossController>(FindObjectsInactive.Include);
        if (boss != null && boss.gameObject.activeSelf)
        {
            data.bossActive = true;
            data.bossHP = boss.CurrentHP;
        }

        // --- Enemigos comunes vivos ---
        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude);
        foreach (var e in enemies)
        {
            if (e == null || e.data == null || e.data.isBoss) continue;
            Vector3 ep = e.transform.position;
            data.enemies.Add(new EnemySaveData
            {
                dataName = e.data.name,
                posX = ep.x, posY = ep.y, posZ = ep.z,
                currentHP = e.currentHP
            });
        }

        return data;
    }
}
