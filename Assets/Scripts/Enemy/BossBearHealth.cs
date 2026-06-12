using UnityEngine;
using System.Collections;
using System;

public class BossBearHealth : MonoBehaviour
{
    public EnemyData data;

    public static event Action<BossBearHealth> OnBossDied;

    public int currentHP;

    [Header("Boss")]
    public bool isVulnerable = true;
    public float vulnerableDuration = 5f;

    private EnemyHealthBar _healthBar;
    private EnemyAI _enemyAI;
    private Animator _anim;

    private bool isDead = false;

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[BossBearHealth] EnemyData not assigned on {gameObject.name}.", this);
            Destroy(gameObject);
            return;
        }

        currentHP = data.maxHP;

        _healthBar = GetComponentInChildren<EnemyHealthBar>(true);

        _enemyAI = GetComponent<EnemyAI>();

        _anim = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead)
            return;

        // EL BOSS SOLO RECIBE DAÑO CUANDO ES VULNERABLE
        if (!isVulnerable)
            return;

        currentHP -= amount;

        currentHP = Mathf.Max(currentHP, 0);

        _healthBar?.UpdateBar(currentHP, data.maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void BecomeVulnerable()
    {
        StartCoroutine(VulnerableRoutine());
    }

    private IEnumerator VulnerableRoutine()
    {
        isVulnerable = true;

        Debug.Log("BOSS VULNERABLE");

        yield return new WaitForSeconds(vulnerableDuration);

        isVulnerable = false;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (_enemyAI != null)
        {
            _enemyAI.isAIActive = false;
        }

        if (_anim != null)
        {
            _anim.SetBool("isWalking", false);
            _anim.SetBool("isAttacking", false);
            _anim.SetTrigger("Die");
        }

        OnBossDied?.Invoke(this);

        Destroy(gameObject, 3f);
    }
}