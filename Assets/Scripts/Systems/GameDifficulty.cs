using UnityEngine;

/// <summary>
/// Holder estático de la dificultad elegida + tabla de multiplicadores.
///
/// Se fija en el menú (antes de cargar la escena de gameplay) y cada sistema lee
/// su multiplicador en su Start/Awake, aplicándolo sobre su valor base del
/// Inspector. NO se mutan los ScriptableObject (EnemyData) ni los valores
/// serializados de forma permanente.
///
/// Normal = todo x1 (el juego se comporta exactamente como la línea base).
/// Spread "Marcado": las diferencias entre niveles son notorias y Pesadilla es
/// un reto real.
/// </summary>
public enum Difficulty { Facil = 0, Normal = 1, Dificil = 2, Pesadilla = 3 }

public static class GameDifficulty
{
    // Se setea en el menú antes de cargar la escena. Default = Normal por si se
    // entra al nivel directo desde el editor sin pasar por el menú.
    public static Difficulty Current = Difficulty.Normal;

    private struct Mods
    {
        public float enemyDamage;
        public float enemyHp;
        public float playerMaxHp;
        public float parryCooldown;
        public float parriesToTransform;
        public float spawnCount;
        public float spawnInterval;
        public float bossHp;
        public float bossVulnerableDuration;
        public float bossVulnerableDamage;
        // Buffs a Emilio en dificultades altas (compensan la horda): >1 = más fuerte.
        public float playerDamage;
        public float playerAttackRange;
        public float auraDamage;
        public float auraRadius;
    }

    // Índice = (int)Difficulty. Ver docs/superpowers/specs/2026-06-21-dificultad-design.md
    private static readonly Mods[] table =
    {
        // Fácil
        new Mods { enemyDamage = 0.55f, enemyHp = 0.7f, playerMaxHp = 1.4f, parryCooldown = 0.7f,
                   parriesToTransform = 0.6f, spawnCount = 0.6f, spawnInterval = 1.4f,
                   bossHp = 0.7f, bossVulnerableDuration = 1.3f, bossVulnerableDamage = 1.2f,
                   playerDamage = 1f, playerAttackRange = 1f, auraDamage = 1f, auraRadius = 1f },
        // Normal (línea base: todo x1)
        new Mods { enemyDamage = 1f, enemyHp = 1f, playerMaxHp = 1f, parryCooldown = 1f,
                   parriesToTransform = 1f, spawnCount = 1f, spawnInterval = 1f,
                   bossHp = 1f, bossVulnerableDuration = 1f, bossVulnerableDamage = 1f,
                   playerDamage = 1f, playerAttackRange = 1f, auraDamage = 1f, auraRadius = 1f },
        // Difícil
        new Mods { enemyDamage = 1.8f, enemyHp = 1.5f, playerMaxHp = 0.7f, parryCooldown = 1.4f,
                   parriesToTransform = 1.6f, spawnCount = 1.8f, spawnInterval = 0.6f,
                   bossHp = 1.4f, bossVulnerableDuration = 0.75f, bossVulnerableDamage = 0.8f,
                   playerDamage = 1.2f, playerAttackRange = 1.15f, auraDamage = 1.3f, auraRadius = 1.15f },
        // Pesadilla
        new Mods { enemyDamage = 3f, enemyHp = 2.2f, playerMaxHp = 0.5f, parryCooldown = 1.8f,
                   parriesToTransform = 2.2f, spawnCount = 2.8f, spawnInterval = 0.4f,
                   bossHp = 1.9f, bossVulnerableDuration = 0.55f, bossVulnerableDamage = 0.65f,
                   playerDamage = 1.4f, playerAttackRange = 1.3f, auraDamage = 1.6f, auraRadius = 1.3f },
    };

    private static Mods M => table[Mathf.Clamp((int)Current, 0, table.Length - 1)];

    public static float EnemyDamageMult            => M.enemyDamage;
    public static float EnemyHpMult                => M.enemyHp;
    public static float PlayerMaxHpMult            => M.playerMaxHp;
    public static float ParryCooldownMult          => M.parryCooldown;
    public static float ParriesToTransformMult     => M.parriesToTransform;
    public static float SpawnCountMult             => M.spawnCount;
    public static float SpawnIntervalMult          => M.spawnInterval;
    public static float BossHpMult                 => M.bossHp;
    public static float BossVulnerableDurationMult => M.bossVulnerableDuration;
    public static float BossVulnerableDamageMult   => M.bossVulnerableDamage;
    public static float PlayerDamageMult           => M.playerDamage;
    public static float PlayerAttackRangeMult      => M.playerAttackRange;
    public static float AuraDamageMult             => M.auraDamage;
    public static float AuraRadiusMult             => M.auraRadius;

    // Nombre para mostrar en la UI (ES, en mayúsculas como el resto del menú).
    public static string DisplayName(Difficulty d)
    {
        switch (d)
        {
            case Difficulty.Facil:     return "FÁCIL";
            case Difficulty.Normal:    return "NORMAL";
            case Difficulty.Dificil:   return "DIFÍCIL";
            case Difficulty.Pesadilla: return "PESADILLA";
            default:                   return d.ToString();
        }
    }
}
