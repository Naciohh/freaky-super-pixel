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

    void Awake()
    {
        // Un Image con tipo 'Filled' pero SIN Source Image ignora por completo
        // fillAmount: Unity lo dibuja siempre como un rectángulo lleno. Estas barras
        // estaban sin sprite, por eso el valor bajaba pero visualmente no se movían.
        // Les asignamos el sprite de UI por defecto de Unity para que el relleno funcione.
        EnsureFillSprite(playerHealthBar);
        EnsureFillSprite(transformBar);
        EnsureFillSprite(bossHealthBar);
    }

    private static Sprite _fillSprite;

    private static void EnsureFillSprite(Image img)
    {
        if (img == null || img.sprite != null)
            return;

        // Sprite blanco generado en runtime (Texture2D.whiteTexture siempre existe).
        // No dependemos de recursos built-in del editor, que pueden no estar disponibles.
        if (_fillSprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            _fillSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        img.sprite = _fillSprite;
    }

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
