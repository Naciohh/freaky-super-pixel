using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData data;

    // Static event
    public static event Action<EnemyHealth> OnAnyEnemyDied;

    public int currentHP;

    // Vida máxima ya escalada por dificultad (denominador de la barra). Se calcula
    // en Start a partir de data.maxHP * GameDifficulty.EnemyHpMult (el boss no escala).
    public int MaxHPEffective { get; private set; }

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

        // El boss no escala su vida acá (lo maneja BossBearHealth); los comunes sí.
        MaxHPEffective = data.isBoss
            ? data.maxHP
            : Mathf.Max(1, Mathf.RoundToInt(data.maxHP * GameDifficulty.EnemyHpMult));

        currentHP = restoreHP >= 0 ? restoreHP : MaxHPEffective;

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

        _healthBar?.UpdateBar(currentHP, MaxHPEffective);

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

        // Apagar los colliders del cuerpo: el cadáver (queda unos segundos antes de
        // destruirse) no debe empujar ni estorbar al jugador.
        DisableColliders();

        // Si es una araña, usar su sistema propio
        if (_spiderAI != null)
        {
            _spiderAI.Die();

            OnAnyEnemyDied?.Invoke(this);

            return;
        }

        // Si es enemigo normal (esqueleto, carrito, etc): frenarlo del todo (corta la
        // embestida en curso, apaga IA y colliders) para que el cadáver no empuje.
        if (_enemyAI != null)
        {
            _enemyAI.HaltForDeath();
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

    // Desactiva todos los colliders (propios y de hijos) para que el cuerpo muerto
    // deje de colisionar/empujar al jugador. Los triggers también se apagan.
    private void DisableColliders()
    {
        foreach (var col in GetComponentsInChildren<Collider>(true))
            col.enabled = false;
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