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
    private SpiderAI _spiderAI;
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
        _spiderAI = GetComponent<SpiderAI>();

        _anim = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead)
            return;

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
        if (isDead)
            return;

        isDead = true;

        // Si es una araña, usar su sistema propio
        if (_spiderAI != null)
        {
            _spiderAI.Die();

            OnAnyEnemyDied?.Invoke(this);

            return;
        }

        // Si es enemigo normal (esqueleto, carrito, etc)
        if (_enemyAI != null)
        {
            _enemyAI.isAIActive = false;
        }

        // Animación de muerte clásica
        if (_anim != null)
        {
            _anim.SetBool("isWalking", false);

            if (_anim.HasParameterOfType("isAttacking", AnimatorControllerParameterType.Bool))
            {
                _anim.SetBool("isAttacking", false);
            }

            _anim.SetTrigger("Die");
        }

        OnAnyEnemyDied?.Invoke(this);

        Destroy(gameObject, 3f);
    }
}

public static class AnimatorExtensions
{
    public static bool HasParameterOfType(
        this Animator self,
        string name,
        AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter param in self.parameters)
        {
            if (param.type == type && param.name == name)
                return true;
        }

        return false;
    }
}