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

    [Header("Retrato jugador")]
    public Image facePortrait;   // Cara recortada en el orbe del HUD.
    public Sprite emilioFace;
    public Sprite freakyFace;

    private BossController bossCtrl;
    private bool faceShowsFreaky = false;

    void Awake()
    {
        // Un Image con tipo 'Filled' pero SIN Source Image ignora por completo
        // fillAmount: Unity lo dibuja siempre como un rectángulo lleno. Estas barras
        // estaban sin sprite, por eso el valor bajaba pero visualmente no se movían.
        // Les asignamos el sprite de UI por defecto de Unity para que el relleno funcione.
        EnsureFillSprite(playerHealthBar);
        EnsureFillSprite(transformBar);
        EnsureFillSprite(bossHealthBar);

        // Arranca mostrando la cara de Emilio.
        if (facePortrait != null && emilioFace != null)
            facePortrait.sprite = emilioFace;
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
        BossController.OnBossReady += OnBossReady;
    }

    void OnDisable()
    {
        WaveManager.OnWaveEnd -= OnWaveEnd;
        BossController.OnBossReady -= OnBossReady;
    }

    private void OnWaveEnd()
    {
        if (waveTimerText != null) waveTimerText.gameObject.SetActive(false);

        // Incluye inactivos: el boss aún puede estar desactivado en este instante.
        // Solo cacheamos la referencia; la barra recién se muestra en OnBossReady
        // (cuando termina la cinemática), no durante la intro.
        bossCtrl = FindAnyObjectByType<BossController>(FindObjectsInactive.Include);
    }

    // El boss terminó su cinemática (o se restauró un guardado): ahora sí mostramos la barra.
    private void OnBossReady()
    {
        if (bossCtrl == null)
            bossCtrl = FindAnyObjectByType<BossController>(FindObjectsInactive.Include);

        if (bossHealthBarGroup != null)
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

        // Cambia la cara del retrato según el estado de transformación (Emilio ↔ Freaky).
        if (facePortrait != null && transformationMode != null)
        {
            bool freaky = transformationMode.IsTransformed;
            if (freaky != faceShowsFreaky)
            {
                faceShowsFreaky = freaky;
                Sprite s = freaky ? freakyFace : emilioFace;
                if (s != null) facePortrait.sprite = s;
            }
        }
    }
}
