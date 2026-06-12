using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyData bossData;
    public Transform player;
    public BossIntroDirector introDirector;

    // Soporta tanto enemigos genéricos (EnemyAI/EnemyHealth) como el oso (BossBearAI/BossBearHealth).
    private EnemyAI enemyAI;
    private EnemyHealth enemyHealth;
    private BossBearAI bearAI;
    private BossBearHealth bearHealth;

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        enemyHealth = GetComponent<EnemyHealth>();
        bearAI = GetComponent<BossBearAI>();
        bearHealth = GetComponent<BossBearHealth>();

        if (bossData != null)
        {
            if (enemyAI != null) enemyAI.data = bossData;
            if (enemyHealth != null) enemyHealth.data = bossData;
            if (bearAI != null) bearAI.data = bossData;
            if (bearHealth != null) bearHealth.data = bossData;
        }
        else
            Debug.LogError("[BossController] bossData not assigned on " + gameObject.name, this);

        // El boss queda inactivo hasta que termine la oleada (lo activa WaveManager).
        if (enemyAI != null) enemyAI.isAIActive = false;
        if (bearAI != null) bearAI.isAIActive = false;
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);

        if (player != null)
        {
            if (enemyAI != null) enemyAI.player = player;
            if (bearAI != null) bearAI.player = player;
        }

        if (introDirector != null)
            introDirector.PlayIntro(this);
        else
        {
            Debug.LogWarning("[BossController] No BossIntroDirector assigned — activating AI directly.", this);
            OnIntroComplete();
        }
    }

    public void OnIntroComplete()
    {
        if (enemyAI != null) enemyAI.isAIActive = true;
        if (bearAI != null) bearAI.isAIActive = true;
        MusicManager.Instance?.PlayBoss();
    }

    // Activa el boss directo (sin cinemática) con un HP dado, para restaurar un guardado.
    public void ActivateRestored(int hp)
    {
        if (bearHealth != null) bearHealth.restoreHP = hp;
        if (enemyHealth != null) enemyHealth.restoreHP = hp;

        gameObject.SetActive(true);

        if (player != null)
        {
            if (enemyAI != null) enemyAI.player = player;
            if (bearAI != null) bearAI.player = player;
        }

        OnIntroComplete();
    }

    // Vida actual del boss para la HUD, sin importar qué sistema use.
    public int CurrentHP => bearHealth != null ? bearHealth.currentHP
                          : enemyHealth != null ? enemyHealth.currentHP : 0;

    public int MaxHP => bossData != null ? bossData.maxHP : 1;
}
