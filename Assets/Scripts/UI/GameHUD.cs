using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Vida jugador")]
    public Image playerHealthBar;
    public PlayerHealth playerHealth;

    [Header("Barra transformacion")]
    public Image transformBar;
    public TransformationMode transformationMode;

    [Header("Timer oleada")]
    public TMP_Text waveTimerText;
    public WaveManager waveManager;

    [Header("Boss")]
    public GameObject bossHealthBarGroup;
    public Image bossHealthBar;

    private EnemyHealth bossEnemyHealth;

    void OnEnable()
    {
        WaveManager.OnWaveEnd += OnWaveEnd;
    }

    void OnDisable()
    {
        WaveManager.OnWaveEnd -= OnWaveEnd;
    }

    private void OnWaveEnd()
    {
        if (waveTimerText != null) waveTimerText.gameObject.SetActive(false);

        BossController boss = FindAnyObjectByType<BossController>();
        if (boss != null)
        {
            bossEnemyHealth = boss.GetComponent<EnemyHealth>();
            if (bossHealthBarGroup != null) bossHealthBarGroup.SetActive(true);
        }
    }

    void Update()
    {
        if (playerHealth != null && playerHealthBar != null)
            playerHealthBar.fillAmount = (float)playerHealth.currentHealth / playerHealth.maxHealth;

        if (transformationMode != null && transformBar != null)
            transformBar.fillAmount = transformationMode.TransformProgress;

        if (waveManager != null && waveTimerText != null && waveManager.WaveActive)
            waveTimerText.text = Mathf.CeilToInt(waveManager.TimeRemaining).ToString();

        if (bossEnemyHealth != null && bossHealthBar != null && bossEnemyHealth.data != null)
            bossHealthBar.fillAmount = (float)bossEnemyHealth.currentHP / bossEnemyHealth.data.maxHP;
    }
}
