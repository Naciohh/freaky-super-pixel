using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Indicador del parry: muestra el cartel "PARRY" (PARRY_cartel) cuando está listo y
/// cambia al cartel de cooldown (PARRY_cartel_cooldown) mientras recarga; al terminar
/// el cooldown vuelve al primero. Es autocontenido: se auto-instancia con
/// [RuntimeInitializeOnLoadMethod] (no hay que cablear nada en la escena). Si querés
/// ajustarlo a gusto, agregá este componente a un GameObject en la escena y todos los
/// campos quedan editables desde el inspector (posición, alto); en ese caso el
/// auto-bootstrap NO crea una copia. Lee el estado del cooldown desde PlayerCombat.
/// </summary>
public class ParryCooldownHUD : MonoBehaviour
{
    [Header("Posición / tamaño (en píxeles)")]
    [Tooltip("Posición respecto del borde INFERIOR-IZQUIERDO de la pantalla. X+ = derecha, Y+ = arriba.")]
    public Vector2 anchoredPosition = new Vector2(150f, 335f);
    [Tooltip("Alto del cartel en píxeles (el ancho se calcula manteniendo la proporción de la imagen).")]
    public float height = 110f;

    [Header("Sprites (en Resources/UI)")]
    [Tooltip("Cartel cuando el parry está listo.")]
    public Sprite readySprite;
    [Tooltip("Cartel mientras el parry está en cooldown.")]
    public Sprite cooldownSprite;

    private PlayerCombat combat;
    private Image cartel;
    private bool lastReady = true;

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
        if (readySprite == null) readySprite = Resources.Load<Sprite>("UI/PARRY_cartel");
        if (cooldownSprite == null) cooldownSprite = Resources.Load<Sprite>("UI/PARRY_cartel_cooldown");
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

        // Cartel anclado al borde inferior-izquierdo (sobre el orbe de avatar).
        var go = new GameObject("Cartel", typeof(RectTransform));
        go.transform.SetParent(canvasGO.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;

        cartel = go.AddComponent<Image>();
        cartel.raycastTarget = false;
        cartel.preserveAspect = true;
        cartel.sprite = readySprite;
        ApplySize(rt, readySprite);
    }

    private void ApplySize(RectTransform rt, Sprite sprite)
    {
        float aspect = 1f;
        if (sprite != null && sprite.rect.height > 0f)
            aspect = sprite.rect.width / sprite.rect.height;
        rt.sizeDelta = new Vector2(height * aspect, height);
    }

    void Update()
    {
        if (combat == null)
            combat = FindAnyObjectByType<PlayerCombat>();

        // Ocultar sin jugador (menú, carga) o durante una cinemática (HUD oculto).
        bool show = combat != null && !HudVisibility.GameplayHidden;

        if (cartel != null) cartel.enabled = show;

        if (!show || cartel == null)
            return;

        bool ready = combat.IsParryReady;
        if (ready != lastReady)
        {
            cartel.sprite = ready ? readySprite : cooldownSprite;
            lastReady = ready;
        }
    }
}
