using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData data;

    // Static event — subscribers MUST unsubscribe in OnDisable/OnDestroy to avoid leaks across scene loads
    public static event Action<EnemyHealth> OnAnyEnemyDied;

    public int currentHP;

    private EnemyHealthBar _healthBar;

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyHealth] EnemyData not assigned on {gameObject.name}. Destroying.", this);
            Destroy(gameObject);
            return;
        }
        currentHP = data.maxHP;
        _healthBar = GetComponentInChildren<EnemyHealthBar>(true);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);

        _healthBar?.UpdateBar(currentHP, data.maxHP);

        if (currentHP <= 0)
            Die();
    }

    private void Die()
    {
        OnAnyEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }
}
