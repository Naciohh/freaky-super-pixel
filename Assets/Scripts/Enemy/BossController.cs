using UnityEngine;
using System;

public class BossController : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyData bossData;
    public Transform player;
    public BossIntroDirector introDirector;

    // Se dispara cuando el boss queda listo para pelear (al terminar la cinemática
    // o al restaurar un guardado). La HUD lo usa para recién ahí mostrar la barra de vida.
    public static event Action OnBossReady;

    // Soporta tanto enemigos genéricos (EnemyAI/EnemyHealth) como el oso (BossBearAI/BossBearHealth).
    private EnemyAI enemyAI;
    private EnemyHealth enemyHealth;
    private BossBearAI bearAI;
    private BossBearHealth bearHealth;

    private Animator anim;
    private AnimatorUpdateMode origUpdateMode;

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        enemyHealth = GetComponent<EnemyHealth>();
        bearAI = GetComponent<BossBearAI>();
        bearHealth = GetComponent<BossBearHealth>();
        anim = GetComponent<Animator>();

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

        // Recién ahora la HUD muestra la barra de vida del boss.
        OnBossReady?.Invoke();
    }

    // --- Soporte de cinemática (lo usa BossIntroDirector) ---

    // Durante la intro corre Time.timeScale = 0, así que el Animator (que por defecto
    // usa tiempo escalado) quedaría congelado. Lo pasamos a tiempo no escalado para
    // que se vean el idle y el golpe, y restauramos al terminar.
    public void SetIntroAnimatorUnscaled(bool on)
    {
        if (anim == null) return;

        if (on)
        {
            origUpdateMode = anim.updateMode;
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        else
        {
            anim.updateMode = origUpdateMode;
        }
    }

    // Pega un golpe puramente visual durante la cinemática (sin dañar al player).
    public void TriggerIntroAttack()
    {
        if (anim != null) anim.SetTrigger("Attack");
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
