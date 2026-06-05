using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public EnemyData data;

    [Header("Config")]
    public float attackDistance = 2f;

    [Header("Suelo / Gravedad")]
    public LayerMask groundMask = ~0;          // capas consideradas "piso"
    public float groundRayHeight = 2f;          // desde cuánto arriba se lanza el rayo
    public float groundCheckDistance = 5f;      // cuánto se busca hacia abajo
    public float groundOffset = 0f;             // ajuste fino sobre el piso

    private float feetOffset;                    // distancia pies-pivote (se calcula solo)

    [Header("Audio")]
    public AudioClip attackWhoosh;

    public bool isAttacking { get; private set; }
    public bool isAIActive = true;

    private bool isDead = false;

    private Animator anim;
    private PlayerHealth playerHealth;
    private AudioSource audioSource;

    private float nextAttackTime = 0f;
    private Coroutine stunCoroutine;

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyAI] EnemyData not assigned on {gameObject.name}.", this);
            enabled = false;
            return;
        }

        anim = GetComponent<Animator>();

        if (anim == null)
            Debug.LogWarning($"[EnemyAI] No Animator found on {gameObject.name}.", this);

        audioSource = GetComponent<AudioSource>();

        playerHealth = player != null ? player.GetComponent<PlayerHealth>() : null;

        transform.localScale = Vector3.one * data.modelScale;

        ComputeFeetOffset();
    }

    // Calcula la distancia entre el pivote y los pies visuales del modelo,
    // para poder apoyarlo exactamente sobre el piso sin que se hunda ni flote.
    private void ComputeFeetOffset()
    {
        var rends = GetComponentsInChildren<Renderer>(true);
        bool found = false;
        Bounds b = new Bounds();
        foreach (var r in rends)
        {
            if (!(r is MeshRenderer || r is SkinnedMeshRenderer)) continue;
            if (!found) { b = r.bounds; found = true; }
            else b.Encapsulate(r.bounds);
        }
        feetOffset = found ? (b.min.y - transform.position.y) : 0f;
    }

    void Update()
    {
        if (!isAIActive || player == null || anim == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Mirar al jugador sin inclinarse cuando está a otra altura.
        Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookTarget);

        if (distance < attackDistance)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", true);

            isAttacking = true;

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(DealDamage());
                nextAttackTime = Time.time + data.attackCooldown;
            }
        }
        else
        {
            anim.SetBool("isAttacking", false);
            isAttacking = false;

            anim.SetBool("isWalking", true);

            // Moverse solo en el plano horizontal hacia el jugador (sin seguir su altura).
            Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                data.moveSpeed * Time.deltaTime
            );
        }

        // Gravedad / pegado al piso: ajusta la altura al terreno real bajo el enemigo.
        SnapToGround();
    }

    private void SnapToGround()
    {
        Vector3 origin = transform.position + Vector3.up * groundRayHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                groundRayHeight + groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 pos = transform.position;
            // Apoyar los pies visuales sobre el piso (restando el offset pies-pivote).
            pos.y = hit.point.y - feetOffset + groundOffset;
            transform.position = pos;
        }
    }

    public void Stun(float duration)
    {
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isAIActive = false;
        isAttacking = false;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
        }

        yield return new WaitForSeconds(duration);

        if (this != null)
            isAIActive = true;

        stunCoroutine = null;
    }

    private IEnumerator DealDamage()
    {
        // sonido de ataque
        if (audioSource != null && attackWhoosh != null)
        {
            audioSource.PlayOneShot(attackWhoosh);
        }

        yield return new WaitForSeconds(0.5f);

        if (this == null)
            yield break;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(data.damage);
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        isAIActive = false;
        isAttacking = false;

        StopAllCoroutines();

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
            anim.SetTrigger("Die");
        }

        Destroy(gameObject, 3f);
    }
}
