using UnityEngine;
using System;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float snapRotationRange = 6f;   // radio para buscar enemigo más cercano al atacar
    [SerializeField] private int playerDamage = 34;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Parry")]
    [SerializeField] private float parryWindow = 0.4f;
    [SerializeField] private float parryRange = 2.5f;
    [SerializeField] private float stunDuration = 2f;

    public static event Action OnParrySuccess;

    private float nextAttackTime = 0f;
    private bool isParrying = false;
    private PlayerMovement playerMovement;

    public bool IsTransforming { get; set; } = false;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (IsTransforming) return;

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(ParryWindow());
        }
    }

    private void Attack()
    {
        SnapToNearestEnemy();
        playerMovement?.TriggerAnimation("Slash");
        Vector3 origin = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(origin, attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            if (eh != null)
                eh.TakeDamage(playerDamage);
        }
    }

    /// <summary>Rota instantáneamente hacia el enemigo más cercano dentro de snapRotationRange.</summary>
    private void SnapToNearestEnemy()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, snapRotationRange, enemyLayer);
        if (nearby.Length == 0) return;

        Transform closest = null;
        float minDist = float.MaxValue;
        foreach (var col in nearby)
        {
            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < minDist) { minDist = d; closest = col.transform; }
        }

        if (closest != null)
        {
            Vector3 dir = (closest.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
    }

    private IEnumerator ParryWindow()
    {
        if (isParrying) yield break;
        isParrying = true;
        float elapsed = 0f;

        while (elapsed < parryWindow)
        {
            elapsed += Time.deltaTime;

            Collider[] nearby = Physics.OverlapSphere(transform.position, parryRange, enemyLayer);
            foreach (var col in nearby)
            {
                EnemyAI ai = col.GetComponent<EnemyAI>();
                if (ai != null && ai.isAttacking)
                {
                    // AOE stun — stun every enemy in parryRange, not just the attacker
                    foreach (var c in nearby)
                    {
                        EnemyAI other = c.GetComponent<EnemyAI>();
                        if (other != null) other.Stun(stunDuration);
                    }
                    playerMovement?.TriggerAnimation("Parry");
                    isParrying = false;
                    OnParrySuccess?.Invoke();
                    yield break;
                }
            }
            yield return null;
        }

        isParrying = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, parryRange);
    }
}
