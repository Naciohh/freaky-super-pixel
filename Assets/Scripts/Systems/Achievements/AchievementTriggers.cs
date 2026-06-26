using UnityEngine;

// Glue por nivel: traduce eventos del juego a desbloqueos. Colocar UN GameObject con
// este componente en cada escena de nivel (Nivel01). No requiere referencias en el Inspector.
public class AchievementTriggers : MonoBehaviour
{
    private bool _tookDamage;

    void Start()
    {
        var mgr = AchievementManager.Instance;
        if (mgr == null) return;

        mgr.ResetRunCounters();   // contadores arañas/esqueletos son POR PARTIDA
        _tookDamage = false;

        // "De compras": bienvenida al entrar al Mercadito.
        mgr.Unlock(AchievementId.DeCompras);
    }

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied       += HandleEnemyDied;
        PlayerHealth.OnPlayerDamaged     += HandlePlayerDamaged;
        BossIntroDirector.OnIntroStarted += HandleBossIntro;
        BossBearHealth.OnBossDied        += HandleBossDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied       -= HandleEnemyDied;
        PlayerHealth.OnPlayerDamaged     -= HandlePlayerDamaged;
        BossIntroDirector.OnIntroStarted -= HandleBossIntro;
        BossBearHealth.OnBossDied        -= HandleBossDied;
    }

    // Arañas vs esqueletos: la araña tiene SpiderAI; el resto de enemigos comunes (no boss)
    // se cuentan como esqueletos.
    private void HandleEnemyDied(EnemyHealth eh)
    {
        if (eh == null) return;
        var mgr = AchievementManager.Instance;
        if (mgr == null) return;

        if (eh.GetComponent<SpiderAI>() != null)
            mgr.AddSpiderKill();
        else if (eh.data != null && !eh.data.isBoss)
            mgr.AddSkeletonKill();
    }

    private void HandlePlayerDamaged() => _tookDamage = true;

    private void HandleBossIntro()
        => AchievementManager.Instance?.Unlock(AchievementId.UnOsoWacho);

    // El stage se completa al morir el oso. Si no recibiste daño en toda la corrida -> Axel el capo.
    private void HandleBossDied(BossBearHealth boss)
    {
        if (!_tookDamage)
            AchievementManager.Instance?.Unlock(AchievementId.AxelElCapo);
    }
}
