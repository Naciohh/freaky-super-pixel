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
    [Tooltip("Daño por tick en el BORDE del aura (enemigo lejos). Cuanto más cerca está, más daño recibe.")]
    [SerializeField] private int auraDamagePerTick = 25;
    [Tooltip("Multiplicador de daño cuando el enemigo está pegado a Freaky (centro del aura). Daño en el centro = auraDamagePerTick x esto. En el borde es x1.")]
    [SerializeField] private float auraDamageCloseMultiplier = 4f;
    [Tooltip("Cada cuánto (seg) el aura aplica daño.")]
    [SerializeField] private float auraTickInterval = 0.5f;

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
        // La dificultad escala cuántos parries hacen falta para transformarse.
        parriesToTransform = Mathf.Max(1, Mathf.RoundToInt(parriesToTransform * GameDifficulty.ParriesToTransformMult));

        // En dificultades altas el aura pega más fuerte y llega más lejos (x1 en Fácil/Normal).
        auraDamagePerTick = Mathf.Max(1, Mathf.RoundToInt(auraDamagePerTick * GameDifficulty.AuraDamageMult));
        auraRadius *= GameDifficulty.AuraRadiusMult;

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

        // Durante la pelea con el boss, el parry NO acumula transformación: queda bloqueada.
        // (El parry sigue funcionando para volver vulnerable al oso, eso se resuelve en PlayerCombat.)
        if (BossBearHealth.IsFightActive) return;

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

    // Daño por tick a todo lo que esté dentro del aura. Cuanto más cerca del centro,
    // más daño: de auraDamagePerTick en el borde a auraDamagePerTick*close pegado.
    // (Ya no se repele a los enemigos: pueden acercarse, pero cerca duele más.)
    private void ApplyAuraDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, auraRadius);

        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();
        HashSet<BossBearHealth> hitBosses = new HashSet<BossBearHealth>();

        foreach (var c in hits)
        {
            EnemyHealth eh = c.GetComponentInParent<EnemyHealth>();
            if (eh != null && eh.data != null && !eh.data.isBoss)
            {
                if (hitEnemies.Add(eh))
                    eh.TakeDamage(AuraDamageAt(eh.transform.position));
                continue;
            }

            BossBearHealth boss = c.GetComponentInParent<BossBearHealth>();
            if (boss != null && hitBosses.Add(boss))
                boss.TakeDamage(AuraDamageAt(boss.transform.position));
        }
    }

    // Daño del aura según la cercanía: x1 en el borde, xCloseMult pegado a Freaky.
    private int AuraDamageAt(Vector3 enemyPos)
    {
        Vector3 d = enemyPos - transform.position;
        d.y = 0f;
        float t = auraRadius > 0f ? Mathf.Clamp01(1f - d.magnitude / auraRadius) : 1f;
        float mult = Mathf.Lerp(1f, Mathf.Max(1f, auraDamageCloseMultiplier), t);
        return Mathf.Max(1, Mathf.RoundToInt(auraDamagePerTick * mult));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }

}
