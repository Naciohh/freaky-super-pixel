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

    private PlayerHealth playerHealth;

    private bool isDead = false;
    private bool isAttacking = false;

    private float nextAttackTime;

    void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (isDead)
            return;

        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // mirar al jugador
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;

        transform.LookAt(lookPos);

        // atacar
        if (distance <= attackDistance)
        {
            anim.SetBool("isWalking", false);

            if (!isAttacking && Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            anim.SetBool("isWalking", true);

            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f);

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        nextAttackTime = Time.time + attackCooldown;

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        StopAllCoroutines();

        anim.SetBool("isWalking", false);
        anim.SetTrigger("Die");

        Destroy(gameObject, 3f);
    }
}