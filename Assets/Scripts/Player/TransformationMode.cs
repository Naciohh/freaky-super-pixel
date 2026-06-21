using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TransformationMode : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int parriesToTransform = 5;
    [SerializeField] private float transformDuration = 8f;

    [Header("Aura (modo Freaky)")]
    [Tooltip("Prefab del aura visual (Aura Lightning). Se instancia pegado al jugador mientras dura la transformación.")]
    [SerializeField] private GameObject auraPrefab;
    [SerializeField] private float auraYOffset = 1f;
    [SerializeField] private float auraScale = 1f;
    [Tooltip("Radio del daño de aura alrededor de Freaky.")]
    [SerializeField] private float auraRadius = 3f;
    [Tooltip("Daño aplicado por tick a los enemigos dentro del aura.")]
    [SerializeField] private int auraDamagePerTick = 25;
    [Tooltip("Cada cuánto (seg) el aura aplica daño.")]
    [SerializeField] private float auraTickInterval = 0.5f;
    [Tooltip("Distancia mínima a la que se mantiene a los enemigos: no pueden acercarse más que esto (los repele para que no lo golpeen). Conviene que sea menor que auraRadius para que igual reciban daño.")]
    [SerializeField] private float repelRadius = 2.6f;

    [Header("Regeneración (modo Freaky)")]
    [Tooltip("Vida que se regenera por tick mientras está transformado.")]
    [SerializeField] private int healthRegenPerTick = 3;

    private int parryCount = 0;
    private bool isTransformed = false;
    private Coroutine transformCoroutine;
    private GameObject auraInstance;
    private float nextAuraTick = 0f;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerHealth playerHealth;

    public int ParryCount => parryCount;
    public bool IsTransformed => isTransformed;
    public float TransformProgress => Mathf.Clamp01((float)parryCount / parriesToTransform);

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();

        if (playerMovement == null)
            Debug.LogError("[TransformationMode] PlayerMovement not found on this GameObject.", this);
        if (playerCombat == null)
            Debug.LogError("[TransformationMode] PlayerCombat not found on this GameObject.", this);
    }

    void OnEnable()
    {
        PlayerCombat.OnParrySuccess += HandleParrySuccess;
    }

    void OnDisable()
    {
        PlayerCombat.OnParrySuccess -= HandleParrySuccess;
        if (transformCoroutine != null)
        {
            StopCoroutine(transformCoroutine);
            transformCoroutine = null;
        }
    }

    // Restaura el estado de transformación desde un guardado.
    public void RestoreState(int savedParryCount, bool wasTransformed)
    {
        parryCount = Mathf.Clamp(savedParryCount, 0, parriesToTransform);

        if (wasTransformed && transformCoroutine == null)
            transformCoroutine = StartCoroutine(TransformRoutine());
    }

    private void HandleParrySuccess(Vector3 _)
    {
        if (isTransformed) return;

        parryCount++;
        if (parryCount >= parriesToTransform)
        {
            parryCount = parriesToTransform;
            if (transformCoroutine == null)
                transformCoroutine = StartCoroutine(TransformRoutine());
        }
    }

    private IEnumerator TransformRoutine()
    {
        isTransformed = true;
        if (playerMovement != null) playerMovement.SetTransformed(true);
        if (playerCombat != null) playerCombat.IsTransforming = true;

        SpawnAura();
        nextAuraTick = Time.time;

        float elapsed = 0f;
        while (elapsed < transformDuration)
        {
            elapsed += Time.deltaTime;
            parryCount = Mathf.RoundToInt(Mathf.Lerp(parriesToTransform, 0, elapsed / transformDuration));

            // Mantiene a los enemigos lejos: no llegan a golpearlo en modo Freaky.
            RepelEnemies();

            // Daño de aura + regeneración de vida, por tick.
            if (Time.time >= nextAuraTick)
            {
                ApplyAuraDamage();
                if (playerHealth != null) playerHealth.Heal(healthRegenPerTick);
                nextAuraTick = Time.time + auraTickInterval;
            }

            yield return null;
        }

        DespawnAura();

        parryCount = 0;
        isTransformed = false;
        if (playerMovement != null) playerMovement.SetTransformed(false);
        if (playerCombat != null) playerCombat.IsTransforming = false;
        transformCoroutine = null;
    }

    private void SpawnAura()
    {
        if (auraPrefab == null || auraInstance != null)
            return;

        // Pegada al jugador: sigue su posición y gira con él.
        auraInstance = Instantiate(auraPrefab, transform);
        auraInstance.transform.localPosition = Vector3.up * auraYOffset;
        auraInstance.transform.localRotation = Quaternion.identity;
        auraInstance.transform.localScale = Vector3.one * auraScale;
    }

    private void DespawnAura()
    {
        if (auraInstance != null)
        {
            Destroy(auraInstance);
            auraInstance = null;
        }
    }

    private void ApplyAuraDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, auraRadius);

        foreach (var c in hits)
        {
            EnemyHealth eh = c.GetComponentInParent<EnemyHealth>();
            if (eh != null && !eh.data.isBoss)
            {
                eh.TakeDamage(auraDamagePerTick);
                continue;
            }

            BossBearHealth boss = c.GetComponentInParent<BossBearHealth>();
            if (boss != null)
                boss.TakeDamage(auraDamagePerTick);
        }
    }

    // Empuja a los enemigos que se acercan más que repelRadius, para que no lleguen
    // a la distancia de ataque. No toca al boss (su pelea es aparte).
    private void RepelEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, repelRadius);

        HashSet<Transform> moved = new HashSet<Transform>();

        foreach (var c in hits)
        {
            Transform root = null;

            EnemyAI enemy = c.GetComponentInParent<EnemyAI>();
            if (enemy != null) root = enemy.transform;

            if (root == null)
            {
                SpiderAI spider = c.GetComponentInParent<SpiderAI>();
                if (spider != null) root = spider.transform;
            }

            // Sin enemigo válido o ya movido (varios colliders por enemigo) -> saltar.
            if (root == null || !moved.Add(root))
                continue;

            Vector3 dir = root.position - transform.position;
            dir.y = 0f;

            // Si está justo encima, lo empuja en una dirección arbitraria.
            if (dir.sqrMagnitude < 0.0001f)
                dir = transform.forward;

            dir.Normalize();

            // Lo reubica en el borde del radio de repulsión, conservando su altura.
            Vector3 target = transform.position + dir * repelRadius;
            target.y = root.position.y;
            root.position = target;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isTransformed) return;

        EnemyHealth eh = other.GetComponent<EnemyHealth>();
        if (eh != null && !eh.data.isBoss)
            eh.TakeDamage(99999);
    }
}
