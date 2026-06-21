using UnityEngine;
using System.Collections;

public class SpiderAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator anim;

    [Header("Stats")]
    public float moveSpeed = 8f;
    public float attackDistance = 1.2f;
    public int damage = 3;
    public float attackCooldown = 1.5f;

    [Header("Windup (ventana de parry)")]
    [Tooltip("Demora entre que arranca el ataque y conecta el golpe: ventana para pararlo con un parry.")]
    [SerializeField] private float attackHitDelay = 0.4f;

    [Header("Separación (anti-encimado)")]
    public float separationRadius = 0.9f;     // radio para detectar arañas/enemigos pegados
    public float separationStrength = 3f;     // fuerza de empuje entre sí

    private PlayerHealth playerHealth;

    private bool isDead = false;
    private bool isAttacking = false;
    private float nextAttackTime;

    // El parry necesita IA activa + un golpe realmente en camino.
    public bool isAIActive = true;
    public bool HasPendingStrike { get; private set; }

    private Coroutine stunCoroutine;

    void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (isDead || !isAIActive || player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // mirar al jugador
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        // atacar
        if (distance <= attackDistance)
        {
            if (anim != null) anim.SetBool("isWalking", false);

            if (!isAttacking && Time.time >= nextAttackTime)
                StartCoroutine(AttackRoutine());
        }
        else
        {
            if (anim != null) anim.SetBool("isWalking", true);

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

        // Evitar que las arañas se encimen entre sí (y con otros enemigos).
        ApplySeparation();
    }

    // Empuja a la araña lejos de otros enemigos cercanos del mismo layer (tipo boids).
    private void ApplySeparation()
    {
        if (separationStrength <= 0f)
            return;

        int mask = 1 << gameObject.layer;
        Collider[] hits = Physics.OverlapSphere(
            transform.position, separationRadius, mask, QueryTriggerInteraction.Ignore);

        Vector3 push = Vector3.zero;
        foreach (var h in hits)
        {
            // Ignorar los propios colliders de esta araña.
            if (h.transform == transform || h.GetComponentInParent<SpiderAI>() == this)
                continue;

            Vector3 diff = transform.position - h.transform.position;
            diff.y = 0f;
            float d = diff.magnitude;

            if (d > 0.0001f && d < separationRadius)
                push += diff.normalized * (1f - d / separationRadius);
        }

        if (push.sqrMagnitude > 1f)
            push.Normalize();

        if (push.sqrMagnitude > 0.0001f)
            transform.position += push * (separationStrength * Time.deltaTime);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (anim != null) anim.SetTrigger("Attack");

        // El golpe está "cargando": acá es cuando se puede parar con un parry.
        HasPendingStrike = true;
        yield return new WaitForSeconds(attackHitDelay);
        HasPendingStrike = false;

        // Si fue aturdida (parry exitoso) durante el windup, el golpe se cancela.
        if (!isDead && isAIActive && playerHealth != null)
            playerHealth.TakeDamage(damage);

        nextAttackTime = Time.time + attackCooldown;

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }

    // Aturde a la araña (lo llama PlayerCombat al parryar).
    public void Stun(float duration)
    {
        if (isDead) return;

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isAIActive = false;
        isAttacking = false;
        HasPendingStrike = false;

        if (anim != null) anim.SetBool("isWalking", false);

        yield return new WaitForSeconds(duration);

        if (this != null && !isDead)
            isAIActive = true;

        stunCoroutine = null;
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isAIActive = false;
        HasPendingStrike = false;

        StopAllCoroutines();

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetTrigger("Die");
        }

        Destroy(gameObject, 3f);
    }
}
