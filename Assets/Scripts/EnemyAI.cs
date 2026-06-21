using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public EnemyData data;

    [Header("Config")]
    public float attackDistance = 2f;

    // Demora desde que arranca el ataque hasta que conecta el golpe.
    // Es la ventana en la que el ataque se puede parar con un parry. Ajustar para
    // que coincida con el frame del "slash" de la animación.
    [SerializeField] private float attackHitDelay = 0.5f;

    [Header("Separación (anti-encimado)")]
    public float separationRadius = 1.4f;     // radio para detectar enemigos pegados
    public float separationStrength = 1.5f;   // empuje entre sí (menor que moveSpeed para que igual avancen)

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

    // True solo durante el windup de un golpe (entre que arranca y conecta).
    // El parry SOLO debe contar contra un golpe realmente en camino.
    public bool HasPendingStrike { get; private set; }

    private bool isDead = false;

    private Animator anim;
    private PlayerHealth playerHealth;
    private AudioSource audioSource;

    private float nextAttackTime = 0f;
    private Coroutine stunCoroutine;

    [Header("Feedback stun")]
    [Tooltip("Tinte del enemigo mientras está aturdido por un parry (para que se vea claro que quedó stuneado).")]
    public Color stunTint = new Color(0.45f, 0.7f, 1f, 1f);   // celeste = aturdido
    private Renderer[] _renderers;
    private MaterialPropertyBlock _mpb;

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

        _renderers = GetComponentsInChildren<Renderer>(true);
        _mpb = new MaterialPropertyBlock();

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

        // Evitar que los enemigos se encimen unos con otros.
        ApplySeparation();

        // Gravedad / pegado al piso: ajusta la altura al terreno real bajo el enemigo.
        SnapToGround();
    }

    // Empuja al enemigo lejos de otros enemigos cercanos (separación tipo boids).
    private void ApplySeparation()
    {
        if (separationStrength <= 0f)
            return;

        Vector3 push = Vector3.zero;
        int mask = 1 << gameObject.layer;

        Collider[] hits = Physics.OverlapSphere(
            transform.position, separationRadius, mask, QueryTriggerInteraction.Ignore);

        foreach (var h in hits)
        {
            // Ignorar los propios colliders de este enemigo.
            if (h.GetComponentInParent<EnemyAI>() == this)
                continue;

            Vector3 diff = transform.position - h.transform.position;
            diff.y = 0f;
            float d = diff.magnitude;

            if (d > 0.0001f && d < separationRadius)
                push += diff.normalized * (1f - d / separationRadius);
        }

        // Topear el empuje: con muchos vecinos no debe superar al avance hacia el jugador,
        // si no los enemigos se frenan en "fila" en vez de seguir acercándose.
        if (push.sqrMagnitude > 1f)
            push.Normalize();

        if (push.sqrMagnitude > 0.0001f)
            transform.position += push * (separationStrength * Time.deltaTime);
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
        HasPendingStrike = false;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
        }

        SetStunVisual(true);

        yield return new WaitForSeconds(duration);

        SetStunVisual(false);

        if (this != null)
            isAIActive = true;

        stunCoroutine = null;
    }

    // Tiñe al enemigo mientras está aturdido, para que se vea claramente que el parry lo stuneó.
    private void SetStunVisual(bool on)
    {
        if (_renderers == null || _mpb == null)
            return;

        Color c = on ? stunTint : Color.white;

        foreach (var r in _renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", c);
            _mpb.SetColor("_Color", c);
            r.SetPropertyBlock(_mpb);
        }
    }

    private IEnumerator DealDamage()
    {
        // El golpe está "cargando": acá es cuando se puede parar con un parry.
        HasPendingStrike = true;

        // sonido de ataque
        if (audioSource != null && attackWhoosh != null)
        {
            audioSource.PlayOneShot(attackWhoosh);
        }

        yield return new WaitForSeconds(attackHitDelay);

        HasPendingStrike = false;

        if (this == null)
            yield break;

        // Si fue aturdido (parry exitoso) durante el windup, el golpe se cancela.
        if (!isAIActive)
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
        HasPendingStrike = false;

        StopAllCoroutines();
        SetStunVisual(false);

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
            anim.SetTrigger("Die");
        }

        Destroy(gameObject, 3f);
    }
}
