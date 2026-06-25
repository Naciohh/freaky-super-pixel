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

    [Header("Ataque por embestida (carrito)")]
    [Tooltip("Si está activo, en vez de pegar quieto el enemigo RETROCEDE y se lanza hacia " +
             "adelante (embestida). El daño se aplica al chocar al jugador durante la ida.")]
    public bool lungeAttack = false;
    [Tooltip("Cuánto retrocede en el windup (metros).")]
    [Min(0)] public float lungeBackDistance = 0.3f;
    [Tooltip("Cuánto se lanza hacia adelante en la embestida (metros).")]
    [Min(0)] public float lungeForwardDistance = 2.4f;
    [Tooltip("Duración del retroceso (s).")]
    [Min(0)] public float lungeWindupTime = 0.25f;
    [Tooltip("Duración de la embestida hacia adelante (s).")]
    [Min(0.01f)] public float lungeForwardTime = 0.16f;
    [Tooltip("Distancia a la que la embestida conecta el golpe.")]
    [Min(0)] public float lungeHitRange = 1.3f;

    [Header("Visuals")]
    [Min(0)] public float modelScale = 1f;

    [Header("Boss Only")]
    [Tooltip("Only used when isBoss is true")]
    public BossAttackType bossAttackType = BossAttackType.Melee;
}
