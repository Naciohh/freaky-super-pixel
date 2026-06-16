using System;
using System.Collections.Generic;

[Serializable]
public class EnemySaveData
{
    public string dataName;   // nombre del asset EnemyData (para reconstruir el enemigo)
    public float posX, posY, posZ;
    public int currentHP;
}

[Serializable]
public class SaveData
{
    public string sceneName;

    // --- Jugador ---
    public float posX, posY, posZ;
    public float rotY;
    public int currentHealth;

    // --- Transformación (TransformationMode) ---
    public int parryCount;
    public bool isTransformed;

    // --- Estadísticas (GameStats) ---
    public int enemiesKilled;
    public int parriesPerformed;
    public float timeElapsed;

    // --- Oleada (WaveManager) ---
    public float waveTimeRemaining;
    public bool waveActive;

    // --- Boss ---
    public bool bossActive;
    public int bossHP;

    // --- Enemigos comunes vivos ---
    public List<EnemySaveData> enemies = new List<EnemySaveData>();

    // --- Meta ---
    public string timestamp;
    public string slotLabel;
}
