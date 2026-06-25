using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public EnemyData data;

    [Header("Config")]
    public float attackDistance = 2f;

    [Tooltip("Si está activo (o lo está en el EnemyData), este enemigo ataca por " +
             "embestida (retrocede y se lanza). Útil para el esqueleto del carrito.")]
    public bool lungeAttack = false;

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
    private bool _lunging = false;   // true mientras corre la embestida (carrito)

    private Animator anim;
    private PlayerHealth playerHealth;
    private AudioSource audioSource;

    // Algunos modelos (ej. el oso minion, que reusa el animator del boss) animan el
    // ataque con el trigger "Attack" en vez del bool "isAttacking". Cacheamos qué tiene.
    private bool _hasAttackBool;
    private bool _hasAttackTrigger;

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

        _hasAttackBool    = anim != null && anim.HasParameterOfType("isAttacking", AnimatorControllerParameterType.Bool);
        _hasAttackTrigger = anim != null && anim.HasParameterOfType("Attack", AnimatorControllerParameterType.Trigger);

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

        if (_lunging) return;   // la embestida controla el cuerpo; no pisar su movimiento

        float distance = Vector3.Distance(transform.position, player.position);

        // Mirar al jugador sin inclinarse cuando está a otra altura.
        Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookTarget);

        if (distance < attackDistance)
        {
            anim.SetBool("isWalking", false);
            if (_hasAttackBool) anim.SetBool("isAttacking", true);

            isAttacking = true;

            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + data.attackCooldown;

                if (lungeAttack || data.lungeAttack)
                {
                    // Carrito: ataque por embestida (retrocede y se lanza adelante).
                    StartCoroutine(LungeAttack());
                }
                else
                {
                    // El oso minion usa el trigger "Attack" del animator del boss; los
                    // esqueletos usan el bool "isAttacking". Disparamos lo que tenga.
                    if (_hasAttackTrigger) anim.SetTrigger("Attack");
                    StartCoroutine(DealDamage());
                }
            }
        }
        else
        {
            if (_hasAttackBool) anim.SetBool("isAttacking", false);
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

    // Ataque por embestida del carrito: retrocede en el windup y se lanza hacia
    // adelante; el daño se aplica al CHOCAR al jugador durante la ida. El parry lo
    // interrumpe igual que un golpe normal (mira HasPendingStrike / isAIActive).
    private IEnumerator LungeAttack()
    {
        _lunging = true;
        isAttacking = true;
        HasPendingStrike = true;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();
        transform.rotation = Quaternion.LookRotation(dir);   // encarar antes de embestir

        Vector3 start = transform.position;
        Vector3 back  = start - dir * data.lungeBackDistance;

        // 1) Windup: retroceder un poco.
        for (float t = 0f; t < data.lungeWindupTime; t += Time.deltaTime)
        {
            if (!isAIActive) { EndLunge(); yield break; }   // parry/stun cancela
            transform.position = Vector3.Lerp(start, back, t / Mathf.Max(0.0001f, data.lungeWindupTime));
            SnapToGround();
            yield return null;
        }

        if (audioSource != null && attackWhoosh != null)
            audioSource.PlayOneShot(attackWhoosh);

        // 2) Embestida hacia adelante; pega si alcanza al jugador.
        Vector3 fwdTarget = back + dir * (data.lungeBackDistance + data.lungeForwardDistance);
        bool hit = false;
        for (float t = 0f; t < data.lungeForwardTime; t += Time.deltaTime)
        {
            if (!isAIActive) { EndLunge(); yield break; }
            transform.position = Vector3.Lerp(back, fwdTarget, t / data.lungeForwardTime);
            SnapToGround();

            if (!hit && player != null &&
                Vector3.Distance(transform.position, player.position) <= data.lungeHitRange)
            {
                hit = true;
                HasPendingStrike = false;
                if (playerHealth != null) playerHealth.TakeDamage(data.damage);
            }
            yield return null;
        }

        EndLunge();
    }

    private void EndLunge()
    {
        _lunging = false;
        isAttacking = false;
        HasPendingStrike = false;
    }

    /// <summary>
    /// Frena por completo al enemigo al morir: corta cualquier corrutina (incluida la
    /// embestida en curso), apaga la IA y desactiva sus colliders. Así el cadáver no
    /// sigue moviéndose ni empuja al jugador. Lo llama EnemyHealth al morir.
    /// </summary>
    public void HaltForDeath()
    {
        StopAllCoroutines();
        _lunging = false;
        isAIActive = false;
        isAttacking = false;
        HasPendingStrike = false;
        foreach (var col in GetComponentsInChildren<Collider>(true))
            col.enabled = false;
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
