using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float attackRange = 2.5f;     // alcance del golpe; debe cubrir el attackDistance de los enemigos (arañas atacan desde 2)
    [SerializeField] private float attackAngle = 270f;     // apertura del cono de daño (grados); deja afuera solo la espalda
    [SerializeField] private int playerDamage = 34;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float attackWindup = 0.35f;   // demora hasta que el slash conecta el golpe
    [SerializeField] private LayerMask enemyLayer;

    [Header("Slash FX")]
    [SerializeField] private GameObject slashVfxPrefab;        // FX_Orange_Slash_1
    [SerializeField] private float slashVfxScale = 1f;
    [SerializeField] private float slashVfxYOffset = 1f;       // altura a la que sale el slash
    [SerializeField] private float slashVfxForwardOffset = 0.5f;
    [SerializeField] private Vector3 slashRotationOffset = new Vector3(90f, 0f, 0f);  // deja el slash horizontal (plano); ajustar si sale girado
    [SerializeField] private float slashVfxLifetime = 1f;

    [Header("Parry")]
    [SerializeField] private float parryWindow = 0.4f;
    [Tooltip("Tiempo (seg) que hay que esperar entre parry y parry. Ajustable a gusto desde el inspector.")]
    [SerializeField] private float parryCooldown = 2f;
    [SerializeField] private float parryRange = 2.5f;          // radio para detectar el ataque a parar
    [SerializeField] private float stunDuration = 2f;
    [Tooltip("Altura del cartel 'PARRY' sobre el oso, como fracción de su altura (0 = pies, 1 = coronilla). Bajalo si queda muy arriba / tapando la barra de vida.")]
    [Range(0f, 1f)]
    [SerializeField] private float bossParryPopupHeightFactor = 0.55f;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip noHitSound;

    [Header("FX")]
    [SerializeField] private GameObject parrySparksPrefab;
    [SerializeField] private float parrySparksScale = 0.3f;   // achica el flash (el prefab sale enorme)

    // Se dispara UNA vez por parry exitoso (cuenta para la transformación + sonido).
    public static event Action<Vector3> OnParrySuccess;

    // Se dispara por CADA enemigo parryado (muestra el cartelito de parry en cada uno).
    public static event Action<Vector3> OnParryHitEnemy;

    private float nextAttackTime = 0f;
    private float nextParryTime = 0f;
    private bool isParrying = false;

    private PlayerMovement playerMovement;
    private AudioSource audioSource;

    public bool IsTransforming { get; set; } = false;

    // --- Estado del cooldown de parry (para el HUD) ---
    public float ParryCooldown => parryCooldown;
    // Segundos que faltan para poder volver a parryar (0 = listo).
    public float ParryCooldownRemaining => Mathf.Max(0f, nextParryTime - Time.time);
    public bool IsParryReady => Time.time >= nextParryTime;
    // Progreso de recarga 0..1 (0 = recién usado, 1 = listo).
    public float ParryCooldownProgress =>
        parryCooldown <= 0f ? 1f : Mathf.Clamp01(1f - ParryCooldownRemaining / parryCooldown);

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();

        // La dificultad escala el cooldown del parry (sobre el valor del Inspector).
        parryCooldown *= GameDifficulty.ParryCooldownMult;

        // En dificultades altas Emilio pega más fuerte y desde más lejos, para poder
        // lidiar con la horda (en Fácil/Normal estos multiplicadores son x1).
        playerDamage = Mathf.Max(1, Mathf.RoundToInt(playerDamage * GameDifficulty.PlayerDamageMult));
        attackRange *= GameDifficulty.PlayerAttackRangeMult;
    }

    void Update()
    {
        if (IsTransforming)
            return;

        // Ataque automático: dispara solo cada attackCooldown, haya o no enemigos cerca.
        if (Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }

        // Parry: clic derecho del mouse o R1 (right shoulder) del joystick.
        bool parryPressed = Input.GetMouseButtonDown(1);
        Gamepad gp = Gamepad.current;
        if (gp != null && gp.rightShoulder.wasPressedThisFrame)
            parryPressed = true;

        if (parryPressed && Time.time >= nextParryTime)
        {
            nextParryTime = Time.time + parryCooldown;
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

    // Dirección hacia la que apunta el ataque: el mouse (exacto, sin el retraso del
    // giro del cuerpo). Si no hay PlayerMovement, cae al forward del transform.
    private Vector3 AimDir()
    {
        Vector3 dir = playerMovement != null ? playerMovement.AimDirection : transform.forward;
        dir.y = 0f;
        return dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward;
    }

    private void Attack()
    {
        // El ataque apunta a donde mira el mouse; el WASD solo mueve.
        SpawnSlashVfx();

        // El golpe (detección + daño + sonido) se resuelve recién cuando el slash conecta.
        StartCoroutine(ResolveAttack());
    }

    private void SpawnSlashVfx()
    {
        if (slashVfxPrefab == null)
            return;

        // Lo parentamos a Emilio: el slash queda SIEMPRE pegado a él, sale desde el
        // personaje y gira con su orientación (uniforme y "hacia donde mira").
        GameObject fx = Instantiate(slashVfxPrefab, transform);
        fx.transform.localPosition = Vector3.up * slashVfxYOffset + Vector3.forward * slashVfxForwardOffset;
        fx.transform.localRotation = Quaternion.Euler(slashRotationOffset);
        fx.transform.localScale = Vector3.one * slashVfxScale;

        ParticleSystem ps = fx.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
            ps.Play(true);

        Destroy(fx, slashVfxLifetime);
    }

    private IEnumerator ResolveAttack()
    {
        yield return new WaitForSeconds(attackWindup);

        // Daño en área alrededor de Emilio, recortado a un cono de attackAngle grados
        // centrado en transform.forward (deja afuera solo lo que está a su espalda).
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        Vector3 aim = AimDir();
        float halfAngle = attackAngle * 0.5f;
        bool hitEnemy = false;

        foreach (var hit in hits)
        {
            Vector3 dir = hit.transform.position - transform.position;
            dir.y = 0f;

            // Fuera del cono (en la espalda respecto al mouse) -> no recibe daño.
            if (dir.sqrMagnitude > 0.001f && Vector3.Angle(aim, dir) > halfAngle)
                continue;

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
            Vector3 parriedPos = transform.position;

            foreach (var col in nearby)
            {
                EnemyAI ai = col.GetComponentInParent<EnemyAI>();
                if (ai != null && ai.HasPendingStrike) { attackerInRange = true; parriedPos = ai.transform.position; break; }

                SpiderAI spider = col.GetComponentInParent<SpiderAI>();
                if (spider != null && spider.HasPendingStrike) { attackerInRange = true; parriedPos = spider.transform.position; break; }

                BossBearAI boss = col.GetComponentInParent<BossBearAI>();
                if (boss != null && boss.HasPendingStrike) { attackerInRange = true; parriedPos = BossPhotoAnchor(boss.transform); break; }
            }

            if (attackerInRange)
            {
                // El parry afecta el MISMO área que el slash (cono de attackAngle / attackRange)
                // y, ADEMÁS, SIEMPRE neutraliza a cualquier enemigo con un golpe en camino
                // (HasPendingStrike) dentro del rango aunque quede fuera del cono. Si no,
                // un atacante de costado/espalda dispara el parry pero te pega igual.
                float reach = Mathf.Max(attackRange, parryRange);
                Collider[] around = Physics.OverlapSphere(transform.position, reach, enemyLayer);

                Vector3 aim = AimDir();
                float half = attackAngle * 0.5f;

                HashSet<EnemyAI> stunned = new HashSet<EnemyAI>();
                HashSet<SpiderAI> spidersStunned = new HashSet<SpiderAI>();
                HashSet<BossBearHealth> vulnerables = new HashSet<BossBearHealth>();

                foreach (var c in around)
                {
                    Vector3 toEnemy = c.transform.position - transform.position;
                    toEnemy.y = 0f;

                    // Dentro del cono del slash (alcance + ángulo).
                    bool inCone = toEnemy.magnitude <= attackRange &&
                        (toEnemy.sqrMagnitude <= 0.001f || Vector3.Angle(aim, toEnemy) <= half);

                    EnemyAI other = c.GetComponentInParent<EnemyAI>();
                    if (other != null && (inCone || other.HasPendingStrike) && stunned.Add(other))
                    {
                        other.Stun(stunDuration);
                        SpawnFlashAt(other.transform.position + Vector3.up * 1.2f);
                        OnParryHitEnemy?.Invoke(other.transform.position);
                    }

                    SpiderAI spider = c.GetComponentInParent<SpiderAI>();
                    if (spider != null && (inCone || spider.HasPendingStrike) && spidersStunned.Add(spider))
                    {
                        spider.Stun(stunDuration);
                        SpawnFlashAt(spider.transform.position + Vector3.up * 1f);
                        OnParryHitEnemy?.Invoke(spider.transform.position);
                    }

                    BossBearAI bossAI = c.GetComponentInParent<BossBearAI>();
                    BossBearHealth bossHealth = c.GetComponentInParent<BossBearHealth>();
                    bool bossPending = bossAI != null && bossAI.HasPendingStrike;
                    if (bossHealth != null && (inCone || bossPending) && vulnerables.Add(bossHealth))
                    {
                        bossHealth.BecomeVulnerable();
                        SpawnFlashAt(bossHealth.transform.position + Vector3.up * 3f);
                        OnParryHitEnemy?.Invoke(BossPhotoAnchor(bossHealth.transform));
                    }
                }

                Debug.Log("PARRY OK");

                playerMovement?.TriggerAnimation("Parry");

                isParrying = false;

                // Una sola vez: cuenta para la transformación + sonido de parry.
                OnParrySuccess?.Invoke(parriedPos);

                yield break;
            }

            yield return null;
        }

        isParrying = false;
    }

    // El boss es enorme: si el cartel de parry sale a la altura de su base queda
    // DENTRO de su malla (el ZTest lo tapa) y no se ve, pero anclarlo al tope lo manda
    // demasiado arriba (tapa la barra de vida). Lo ubicamos a una fracción ajustable de
    // su altura (bossParryPopupHeightFactor) para dejarlo sobre el cuerpo.
    private Vector3 BossPhotoAnchor(Transform boss)
    {
        var rends = boss.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
            return boss.position + Vector3.up * 3f;

        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);

        Vector3 p = boss.position;
        p.y = Mathf.Lerp(b.min.y, b.max.y, bossParryPopupHeightFactor);   // ParryFeedback le suma su yOffset por encima
        return p;
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
        // Cono de daño del ataque (attackAngle centrado en forward).
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        float half = attackAngle * 0.5f;
        Vector3 left = Quaternion.Euler(0f, -half, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, half, 0f) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + left * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + right * attackRange);

        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(
            transform.position,
            parryRange
        );
    }
}