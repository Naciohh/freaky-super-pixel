using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Indicador del cooldown del parry. Muestra un disco que se vacía a medida que se
/// recarga + los segundos que faltan para volver a parryar. Es autocontenido: se
/// auto-instancia con [RuntimeInitializeOnLoadMethod] (no hay que cablear nada en la
/// escena). Si querés ajustarlo a gusto, agregá este componente a un GameObject en la
/// escena y todos los campos quedan editables desde el inspector (posición, tamaño,
/// colores); en ese caso el auto-bootstrap NO crea una copia.
/// Lee el estado del cooldown desde PlayerCombat.
/// </summary>
public class ParryCooldownHUD : MonoBehaviour
{
    [Header("Posición / tamaño (en píxeles)")]
    [Tooltip("Posición respecto del borde INFERIOR-CENTRO de la pantalla. X+ = derecha, Y+ = arriba.")]
    public Vector2 anchoredPosition = new Vector2(0f, 170f);
    [Tooltip("Diámetro del indicador en píxeles.")]
    public float size = 96f;

    [Header("Colores")]
    [Tooltip("Color del disco mientras está en cooldown (se va vaciando).")]
    public Color cooldownColor = new Color(1f, 0.55f, 0.1f, 0.9f);
    [Tooltip("Color del fondo/pista del disco.")]
    public Color trackColor = new Color(0f, 0f, 0f, 0.45f);
    [Tooltip("Color cuando el parry ya está listo.")]
    public Color readyColor = new Color(0.3f, 1f, 0.4f, 0.9f);

    [Header("Texto")]
    [Tooltip("Mostrar los segundos restantes en el centro.")]
    public bool showSeconds = true;
    [Tooltip("Texto que aparece cuando el parry está listo (vacío = nada).")]
    public string readyLabel = "PARRY";

    private PlayerCombat combat;
    private Image track;
    private Image sweep;
    private TMP_Text label;

    private static Sprite _circleSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Si el usuario ya puso uno en la escena, no creamos otro.
        if (FindAnyObjectByType<ParryCooldownHUD>(FindObjectsInactive.Include) != null)
            return;

        var go = new GameObject("ParryCooldownHUD");
        DontDestroyOnLoad(go);
        go.AddComponent<ParryCooldownHUD>();
    }

    void Start()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        // Canvas propio (encima de todo).
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 4000;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Contenedor anclado al borde inferior-centro.
        var container = new GameObject("Container", typeof(RectTransform));
        container.transform.SetParent(canvasGO.transform, false);
        var rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = anchoredPosition;

        Sprite circle = GetCircleSprite();

        // Fondo (pista).
        track = NewImage("Track", rt, circle, trackColor);
        track.type = Image.Type.Simple;

        // Disco que se vacía (radial).
        sweep = NewImage("Sweep", rt, circle, cooldownColor);
        sweep.type = Image.Type.Filled;
        sweep.fillMethod = Image.FillMethod.Radial360;
        sweep.fillOrigin = (int)Image.Origin360.Top;
        sweep.fillClockwise = true;
        sweep.fillAmount = 0f;

        // Texto central.
        var textGO = new GameObject("Label", typeof(RectTransform));
        textGO.transform.SetParent(rt, false);
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        label = textGO.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 8f;
        label.fontSizeMax = size * 0.5f;
        label.fontStyle = FontStyles.Bold;
        label.text = "";
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    // Genera un círculo suave (anti-aliased) una sola vez.
    private static Sprite GetCircleSprite()
    {
        if (_circleSprite != null)
            return _circleSprite;

        const int res = 128;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        float c = (res - 1) * 0.5f;
        float r = c;
        var pixels = new Color32[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                // Borde suave de ~1.5px.
                float a = Mathf.Clamp01((r - d) / 1.5f);
                pixels[y * res + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        _circleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        return _circleSprite;
    }

    void Update()
    {
        if (combat == null)
            combat = FindAnyObjectByType<PlayerCombat>();

        bool hasPlayer = combat != null;

        // Sin jugador (menú, carga, etc.): ocultar.
        if (track != null) track.enabled = hasPlayer;
        if (sweep != null) sweep.enabled = hasPlayer;
        if (label != null) label.enabled = hasPlayer;

        if (!hasPlayer || sweep == null)
            return;

        if (combat.IsParryReady)
        {
            sweep.fillAmount = 0f;
            track.color = trackColor;
            label.text = showSeconds ? readyLabel : "";
            label.color = readyColor;
        }
        else
        {
            // Disco lleno al usarse y se vacía mientras recarga.
            float remaining01 = Mathf.Clamp01(1f - combat.ParryCooldownProgress);
            sweep.fillAmount = remaining01;
            sweep.color = cooldownColor;
            track.color = trackColor;

            if (showSeconds)
            {
                label.text = Mathf.CeilToInt(combat.ParryCooldownRemaining).ToString();
                label.color = Color.white;
            }
            else
            {
                label.text = "";
            }
        }
    }
}
