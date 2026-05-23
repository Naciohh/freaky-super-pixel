using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public EnemyData data;

    [Header("Config")]
    public float attackDistance = 2f;

    public bool isAttacking { get; private set; }
    public bool isAIActive = true;
    private bool isDead = false;

    private Animator anim;
    private PlayerHealth playerHealth;
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

        playerHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
        transform.localScale = Vector3.one * data.modelScale;
    }

    void Update()
    {
        if (!isAIActive || player == null || anim == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        transform.LookAt(player);

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

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                data.moveSpeed * Time.deltaTime
            );
        }
    }

    public void Stun(float duration)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
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
        if (this != null) isAIActive = true;
        stunCoroutine = null;
    }

    private IEnumerator DealDamage()
    {
        yield return new WaitForSeconds(0.5f);
        if (this == null) yield break;
        if (playerHealth != null)
            playerHealth.TakeDamage(data.damage);
    }
public void Die()
{
    if (isDead) return;

    isDead = true;

    isAIActive = false;
    isAttacking = false;

    StopAllCoroutines();

    if (anim != null)
    {
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", false);

        // animación de muerte
        anim.SetTrigger("Die");
    }

    // destruir enemigo después de 3 segundos
    Destroy(gameObject, 3f);
}


}
