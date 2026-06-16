using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData data;

    // Static event
    public static event Action<EnemyHealth> OnAnyEnemyDied;

    public int currentHP;

    // Si es >= 0, al iniciar usa este HP en vez de maxHP (para restaurar guardados).
    [HideInInspector] public int restoreHP = -1;

    private EnemyHealthBar _healthBar;
    private EnemyAI _enemyAI;
    private Animator _anim;

    private bool isDead = false;

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyHealth] EnemyData not assigned on {gameObject.name}. Destroying.", this);
            Destroy(gameObject);
            return;
        }

        currentHP = restoreHP >= 0 ? restoreHP : data.maxHP;

        _healthBar = GetComponentInChildren<EnemyHealthBar>(true);

        _enemyAI = GetComponent<EnemyAI>();

        _anim = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead) return;

        currentHP -= amount;

        currentHP = Mathf.Max(currentHP, 0);

        _healthBar?.UpdateBar(currentHP, data.maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        // detener IA
        if (_enemyAI != null)
        {
            _enemyAI.isAIActive = false;
        }

        // detener animaciones actuales
        if (_anim != null)
        {
            _anim.SetBool("isWalking", false);
            _anim.SetBool("isAttacking", false);

            // trigger muerte
            _anim.SetTrigger("Die");
        }

        // evento global
        OnAnyEnemyDied?.Invoke(this);

        // destruir enemigo después animación
        Destroy(gameObject, 3f);
    }
}