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

    private BossController bossCtrl;

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

        // Incluye inactivos: el boss aún puede estar desactivado en este instante.
        bossCtrl = FindAnyObjectByType<BossController>(FindObjectsInactive.Include);
        if (bossCtrl != null && bossHealthBarGroup != null)
            bossHealthBarGroup.SetActive(true);
    }

    void Update()
    {
        if (playerHealth != null && playerHealthBar != null)
            playerHealthBar.fillAmount = (float)playerHealth.currentHealth / playerHealth.maxHealth;

        if (transformationMode != null && transformBar != null)
            transformBar.fillAmount = transformationMode.TransformProgress;

        if (waveManager != null && waveTimerText != null && waveManager.WaveActive)
            waveTimerText.text = Mathf.CeilToInt(waveManager.TimeRemaining).ToString();

        if (bossCtrl != null && bossHealthBar != null && bossCtrl.MaxHP > 0)
            bossHealthBar.fillAmount = (float)bossCtrl.CurrentHP / bossCtrl.MaxHP;
    }
}
