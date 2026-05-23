using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public enum BossAttackType { Melee }

    [Header("Identity")]
    public string displayName = "Enemy";
    public bool isBoss = false;

    [Header("Stats")]
    [Min(0)] public int maxHP = 60;
    [Min(0)] public int damage = 25;
    [Min(0)] public float moveSpeed = 2.5f;

    [Header("Attack")]
    [Min(0)] public float attackCooldown = 1.2f;

    [Header("Visuals")]
    [Min(0)] public float modelScale = 1f;

    [Header("Boss Only")]
    [Tooltip("Only used when isBoss is true")]
    public BossAttackType bossAttackType = BossAttackType.Melee;
}
