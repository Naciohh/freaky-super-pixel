using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UImage = UnityEngine.UI.Image;

/// <summary>
/// Al morir el oso (BossBearHealth.OnBossDied) congela el juego y muestra el cartel
/// de victoria (<c>Resources/UI/Victoria/ui_contenedor_victoria</c>, con "GANASTE
/// GENIO!" horneado) con un botón SALIR adentro. Recién cuando el jugador toca SALIR
/// se reproduce el portal a pantalla completa (video + audio) que carga el lobby.
///
/// Reutiliza el mismo <see cref="PortalTransition"/> (video) que la entrada a partida.
/// La UI se arma por código (auto-bootstrap), no hace falta armarla en la escena.
/// </summary>
public class BossDefeatPortal : MonoBehaviour
{
    [Header("Portal")]
    [Tooltip("PortalTransition de la escena (instancia del prefab PortalCanvas). Si queda vacío se busca en escena.")]
    [SerializeField] private PortalTransition portal;

    [Tooltip("Escena del lobby/menú a cargar cuando termina el portal.")]
    [SerializeField] private string lobbyScene = "MainMenu";

    [Tooltip("Espera (en segundos reales) tras la muerte del oso antes de mostrar el cartel.")]
    [SerializeField] private float delay = 1.2f;

    private bool _triggered;
    private CanvasGroup _group;
    private GameObject _firstSelectable;
    private float _armTime;   // evita que un ataque masheado salte el cartel al instante

    void OnEnable()  { BossBearHealth.OnBossDied += HandleBossDied; }
    void OnDisable() { BossBearHealth.OnBossDied -= HandleBossDied; }

    private void HandleBossDied(BossBearHealth boss)
    {
        if (_triggered) return;
        _triggered = true;
        StartCoroutine(ShowVictoryAfterDelay());
    }

    private IEnumerator ShowVictoryAfterDelay()
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        BuildUI();
        Time.timeScale = 0f;
        MusicManager.Instance?.PauseMusic();

        _armTime = Time.unscaledTime + 0.4f;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void Salir()
    {
        if (Time.unscaledTime < _armTime) return;   // ignora el input heredado del combate
        Time.timeScale = 1f;
        if (portal == null) portal = FindAnyObjectByType<PortalTransition>(FindObjectsInactive.Include);

        if (_group != null) _group.gameObject.SetActive(false);

        if (portal != null)
            portal.PlayThenLoad(lobbyScene, false);   // false = portal VERDE original
        else
            SceneManager.LoadScene(lobbyScene); // fallback: sin video si falta el portal
    }

    // --- UI armada por código ---
    private void BuildUI()
    {
        var go = new GameObject("VictoryCanvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1100;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        _group = go.AddComponent<CanvasGroup>();

        var backdrop = NewImage("Backdrop", go.transform);
        StretchFull(backdrop.rectTransform);
        backdrop.color = new Color(0f, 0f, 0f, 0.82f);

        // Contenedor "GANASTE GENIO!" (landscape).
        var cont = NewImage("Container", go.transform);
        var contSprite = Resources.Load<Sprite>("UI/Victoria/ui_contenedor_victoria");
        var crt = cont.rectTransform;
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = Vector2.zero;
        float ch = 640f;
        float cw = contSprite != null ? ch * (contSprite.rect.width / contSprite.rect.height) : 900f;
        crt.sizeDelta = new Vector2(cw, ch);
        if (contSprite != null) { cont.sprite = contSprite; cont.preserveAspect = true; cont.color = Color.white; }
        else cont.color = new Color(0.98f, 0.9f, 0.7f, 1f);

        // Botón SALIR (sprite ya existente en Resources) en la zona baja del cartel.
        var salir = NewImage("Btn_Salir", cont.transform);
        var salirSprite = Resources.Load<Sprite>("UI/salir_menu");
        var srt = salir.rectTransform;
        srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.anchoredPosition = new Vector2(0f, -130f);
        srt.sizeDelta = new Vector2(320f, 120f);
        if (salirSprite != null) { salir.sprite = salirSprite; salir.preserveAspect = true; }
        else salir.color = new Color(0.7f, 0.18f, 0.18f, 1f);

        var btn = salir.gameObject.AddComponent<Button>();
        btn.targetGraphic = salir;
        var colors = btn.colors;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.fadeDuration = 0.08f;
        btn.colors = colors;
        btn.onClick.AddListener(Salir);
        _firstSelectable = salir.gameObject;
    }

    void Update()
    {
        // Permite salir también con Enter / botón Sur del joystick si el botón está seleccionado,
        // y como atajo directo si el cartel está visible.
        if (_group == null || !_group.gameObject.activeSelf) return;
        if (Time.unscaledTime < _armTime) return;

        if (_firstSelectable != null && EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(_firstSelectable);

        Gamepad gp = Gamepad.current;
        if (Input.GetKeyDown(KeyCode.Return) || (gp != null && gp.buttonSouth.wasPressedThisFrame))
            Salir();
    }

    private static UImage NewImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.AddComponent<UImage>();
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
