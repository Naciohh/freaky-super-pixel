using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float snapRotationRange = 6f;
    [SerializeField] private int playerDamage = 34;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float attackWindup = 0.35f;   // demora hasta que el brazo baja y conecta el golpe
    [SerializeField] private LayerMask enemyLayer;

    [Header("Parry")]
    [SerializeField] private float parryWindow = 0.4f;
    [SerializeField] private float parryRange = 2.5f;          // radio para detectar el ataque a parar
    [SerializeField] private float parryStunRadius = 5f;       // radio (360°) de enemigos afectados por el stun
    [SerializeField] private float stunDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip noHitSound;

    [Header("FX")]
    [SerializeField] private GameObject parrySparksPrefab;
    [SerializeField] private float parrySparksScale = 0.3f;   // achica el flash (el prefab sale enorme)

    public static event Action OnParrySuccess;

    private float nextAttackTime = 0f;
    private bool isParrying = false;

    private PlayerMovement playerMovement;
    private AudioSource audioSource;

    public bool IsTransforming { get; set; } = false;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (IsTransforming)
            return;

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(ParryWindow());
        }

#if UNITY_EDITOR
        // DEBUG: tecla K -> daña al boss directo (forzando vulnerable) para verificar
        // que la barra de vida baja. Borrar este bloque cuando se confirme el flujo.
        if (Input.GetKeyDown(KeyCode.K))
        {
            var boss = FindAnyObjectByType<BossBearHealth>();
            if (boss != null)
            {
                boss.isVulnerable = true;
                boss.TakeDamage(100);
                Debug.Log($"[DEBUG K] Daño directo al boss -> HP={boss.currentHP}");
            }
            else
            {
                Debug.Log("[DEBUG K] No encontré ningún BossBearHealth activo en la escena.");
            }
        }
#endif
    }

    private void Attack()
    {
        SnapToNearestEnemy();

        playerMovement?.TriggerAnimation("Slash");

        // El golpe (detección + daño + sonido) se resuelve recién cuando el brazo baja.
        StartCoroutine(ResolveAttack());
    }

    private IEnumerator ResolveAttack()
    {
        yield return new WaitForSeconds(attackWindup);

        Vector3 origin = transform.position + transform.forward * (attackRange * 0.5f);

        Collider[] hits = Physics.OverlapSphere(origin, attackRange, enemyLayer);

        bool hitEnemy = false;

        foreach (var hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(playerDamage);
                hitEnemy = true;
            }

            BossBearHealth bossHealth = hit.GetComponentInParent<BossBearHealth>();

            if (bossHealth != null)
            {
                Debug.Log($"[PlayerCombat] Golpe al boss -> HP={bossHealth.currentHP}, vulnerable={bossHealth.isVulnerable}");
                bossHealth.TakeDamage(playerDamage);
                hitEnemy = true;
            }
        }

        if (audioSource != null)
        {
            if (hitEnemy)
            {
                if (hitSound != null)
                    audioSource.PlayOneShot(hitSound);
            }
            else
            {
                if (noHitSound != null)
                    audioSource.PlayOneShot(noHitSound);
            }
        }
    }

    private void SnapToNearestEnemy()
    {
        Collider[] nearby = Physics.OverlapSphere(
            transform.position,
            snapRotationRange,
            enemyLayer
        );

        if (nearby.Length == 0)
            return;

        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var col in nearby)
        {
            float d = Vector3.Distance(transform.position, col.transform.position);

            if (d < minDist)
            {
                minDist = d;
                closest = col.transform;
            }
        }

        if (closest != null)
        {
            Vector3 dir = closest.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
    }

    private IEnumerator ParryWindow()
    {
        if (isParrying)
            yield break;

        isParrying = true;

        float elapsed = 0f;

        while (elapsed < parryWindow)
        {
            elapsed += Time.deltaTime;

            Collider[] nearby = Physics.OverlapSphere(
                transform.position,
                parryRange,
                enemyLayer
            );

            bool attackerInRange = false;

            foreach (var col in nearby)
            {
                EnemyAI ai = col.GetComponentInParent<EnemyAI>();
                if (ai != null && ai.HasPendingStrike) { attackerInRange = true; break; }

                BossBearAI boss = col.GetComponentInParent<BossBearAI>();
                if (boss != null && boss.HasPendingStrike) { attackerInRange = true; break; }
            }

            if (attackerInRange)
            {
                // Afecta a TODOS los enemigos alrededor (360°), no solo a los de adelante.
                Collider[] around = Physics.OverlapSphere(transform.position, parryStunRadius, enemyLayer);

                HashSet<EnemyAI> stunned = new HashSet<EnemyAI>();
                HashSet<BossBearHealth> vulnerables = new HashSet<BossBearHealth>();

                foreach (var c in around)
                {
                    EnemyAI other = c.GetComponentInParent<EnemyAI>();
                    if (other != null && stunned.Add(other))
                    {
                        other.Stun(stunDuration);
                        SpawnFlashAt(other.transform.position + Vector3.up * 1.2f);
                    }

                    BossBearHealth bossHealth = c.GetComponentInParent<BossBearHealth>();
                    if (bossHealth != null && vulnerables.Add(bossHealth))
                    {
                        bossHealth.BecomeVulnerable();
                        SpawnFlashAt(bossHealth.transform.position + Vector3.up * 3f);
                    }
                }

                Debug.Log("PARRY OK");

                playerMovement?.TriggerAnimation("Parry");

                isParrying = false;

                OnParrySuccess?.Invoke();

                yield break;
            }

            yield return null;
        }

        isParrying = false;
    }

    private void SpawnFlashAt(Vector3 pos)
    {
        if (parrySparksPrefab == null)
            return;

        GameObject fx = Instantiate(parrySparksPrefab, pos, Quaternion.identity);
        fx.transform.localScale = Vector3.one * parrySparksScale;

        // El prefab tiene Play On Awake desactivado: hay que arrancarlo a mano.
        ParticleSystem ps = fx.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startLifetime = 0.15f;   // destello bien corto, no un sprite fijo
            main.startSpeed = 8f;          // las chispas salen disparadas (sensación de flash)
            ps.Play(true);
        }

        Destroy(fx, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position + transform.forward * (attackRange * 0.5f),
            attackRange
        );

        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(
            transform.position,
            parryRange
        );
    }
}