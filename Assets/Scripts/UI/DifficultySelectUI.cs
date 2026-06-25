using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UImage = UnityEngine.UI.Image;

/// <summary>
/// Panel de selección de dificultad que aparece al tocar "Nuevo Juego", ANTES
/// del video del portal. Se construye 100% por código (auto-bootstrap) y se skinea
/// con el arte de <c>Assets/Resources/UI/Dificultad/</c>: el contenedor
/// (CONTENEDOR_dificultad, con el título "DIFICULTAD" horneado) y un botón-imagen
/// por nivel (FACIL/MEDIA/DIFICIL/PESADILLA).
///
/// Lo crea/abre <see cref="MainMenuController"/>: Show(onChosen, onCancel).
/// Mientras está abierto congela a Emilio y oculta la leyenda vía <see cref="LobbyModal"/>.
/// </summary>
public class DifficultySelectUI : MonoBehaviour
{
    private Action<Difficulty> _onChosen;
    private Action _onCancel;
    private CanvasGroup _group;
    private Font _font;
    private GameObject _firstSelectable;
    private bool _isOpen;

    // El botón con el que se abre el panel (E / X del joystick) puede "confirmar" en
    // el mismo frame el botón auto-seleccionado y elegir FÁCIL al instante (saltando
    // directo al portal). Por eso desarmamos los botones unos ms y recién ahí
    // seleccionamos el primero.
    private float _armTime;
    private bool _selected;
    private bool _decided;   // evita doble confirmación (pad + EventSystem)

    /// <summary>Crea un panel nuevo (oculto) listo para Show().</summary>
    public static DifficultySelectUI Create()
    {
        var go = new GameObject("DifficultySelectUI");
        return go.AddComponent<DifficultySelectUI>();
    }

    void Awake()
    {
        BuildUI();
        SetVisible(false);
    }

    public void Show(Action<Difficulty> onChosen, Action onCancel)
    {
        _onChosen = onChosen;
        _onCancel = onCancel;
        SetVisible(true);
        if (!_isOpen) { _isOpen = true; LobbyModal.Open(); }

        // Desarmar ~0.3s y limpiar la selección para que el press que abrió el panel
        // no confirme nada. La selección real se hace en Update al terminar la ventana.
        _armTime = Time.unscaledTime + 0.3f;
        _selected = false;
        _decided = false;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private bool Armed => Time.unscaledTime >= _armTime;

    void Update()
    {
        if (!Armed) return;   // ignora todo input hasta terminar la ventana de desarme

        // Una vez armado, recién seleccionamos el primer botón (para navegar con joystick).
        if (!_selected && _firstSelectable != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(_firstSelectable);
            _selected = true;
        }

        Gamepad gp = Gamepad.current;

        // Confirmar el botón seleccionado con X/A del joystick o Enter. En el lobby los
        // menús son estaciones (input directo), así que el submit del gamepad por
        // EventSystem no está cableado: lo resolvemos a mano invocando el botón activo.
        if ((gp != null && gp.buttonSouth.wasPressedThisFrame) ||
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            var sel = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            var btn = sel != null ? sel.GetComponent<Button>() : null;
            if (btn != null) { btn.onClick.Invoke(); return; }
        }

        // Volver con Círculo (buttonEast) o Escape.
        if ((gp != null && gp.buttonEast.wasPressedThisFrame) || Input.GetKeyDown(KeyCode.Escape))
            Cancel();
    }

    private void SetVisible(bool on)
    {
        gameObject.SetActive(on);
        if (_group != null)
        {
            _group.alpha = on ? 1f : 0f;
            _group.blocksRaycasts = on;
            _group.interactable = on;
        }
    }

    private void CloseModal()
    {
        if (_isOpen) { _isOpen = false; LobbyModal.Close(); }
    }

    private void Choose(Difficulty d)
    {
        if (!Armed || _decided) return;   // ignora el press heredado y el doble-disparo
        _decided = true;
        var cb = _onChosen;
        CloseModal();
        SetVisible(false);
        cb?.Invoke(d);
    }

    private void Cancel()
    {
        if (!Armed || _decided) return;
        _decided = true;
        var cb = _onCancel;
        CloseModal();
        SetVisible(false);
        cb?.Invoke();
    }

    // --- Construcción de la UI ---

    private void BuildUI()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;   // por encima del menú

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        _group = gameObject.AddComponent<CanvasGroup>();

        // Fondo oscuro que tapa el menú y bloquea clicks por detrás.
        var backdrop = CreateImage("Backdrop", transform);
        StretchFull(backdrop.rectTransform);
        backdrop.color = new Color(0f, 0f, 0f, 0.86f);

        // Contenedor con el título "DIFICULTAD" horneado.
        var container = CreateImage("Container", transform);
        var contSprite = LoadSprite("CONTENEDOR_dificultad");
        var crt = container.rectTransform;
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = Vector2.zero;
        float ch = 940f;
        float cw = contSprite != null ? ch * (contSprite.rect.width / contSprite.rect.height) : 760f;
        crt.sizeDelta = new Vector2(cw, ch);
        if (contSprite != null) { container.sprite = contSprite; container.preserveAspect = true; container.color = Color.white; }
        else container.color = new Color(0.16f, 0.12f, 0.22f, 1f);

        // Botones-imagen repartidos parejo en una banda debajo del título horneado.
        // 'y' es la posición vertical del centro respecto al centro del contenedor.
        var opciones = new[]
        {
            ("FACIL_dificultad",     Difficulty.Facil),
            ("MEDIA_dificultad",     Difficulty.Normal),
            ("DIFICIL_dificultad",   Difficulty.Dificil),
            ("PESADILLA_dificultad", Difficulty.Pesadilla),
        };

        const float btnH      = 104f;   // alto fijo; el ancho sale del aspecto de cada sprite
        const float bandTop   = 160f;
        const float bandBottom = -320f;
        int n = opciones.Length;
        float gap = (bandTop - bandBottom - n * btnH) / (n - 1);

        for (int i = 0; i < n; i++)
        {
            float cy = bandTop - btnH * 0.5f - i * (btnH + gap);
            Difficulty d = opciones[i].Item2;
            var go = MakeImageButton(container.transform, opciones[i].Item1, cy, btnH, () => Choose(d));
            if (i == 0) _firstSelectable = go;
        }

        // VOLVER: no tiene arte, se arma con un botón de texto sobrio.
        MakeTextButton(container.transform, "VOLVER", -390f, Cancel);
    }

    // Botón cuyo gráfico ES el sprite (la etiqueta viene horneada en la imagen).
    // El ancho se calcula desde el ALTO + el aspecto del sprite, así no queda
    // letterboxeado (chico y centrado) dentro de un rect ancho.
    private GameObject MakeImageButton(Transform parent, string spriteName, float y, float h, Action onClick)
    {
        var img = CreateImage("Btn_" + spriteName, parent);
        var sprite = LoadSprite(spriteName);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
        float bw = sprite != null ? h * (sprite.rect.width / sprite.rect.height) : 300f;
        rt.sizeDelta = new Vector2(bw, h);
        if (sprite != null) { img.sprite = sprite; img.preserveAspect = true; }
        img.color = Color.white;

        AddButton(img);
        img.GetComponent<Button>().onClick.AddListener(() => onClick());
        img.gameObject.AddComponent<SelectionScaler>();   // resalta el botón activo
        return img.gameObject;
    }

    private GameObject MakeTextButton(Transform parent, string label, float y, Action onClick)
    {
        var img = CreateImage("Btn_" + label, parent);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(240f, 64f);
        img.color = new Color(0.30f, 0.28f, 0.34f, 0.95f);

        AddButton(img);
        img.GetComponent<Button>().onClick.AddListener(() => onClick());
        img.gameObject.AddComponent<SelectionScaler>();

        var txt = CreateText("Label", img.transform, label, 30, FontStyle.Bold, new Color(0.92f, 0.96f, 0.78f));
        StretchFull(txt.rectTransform);
        return img.gameObject;
    }

    private void AddButton(UImage img)
    {
        var btn = img.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        colors.fadeDuration = 0.08f;
        btn.colors = colors;
        btn.targetGraphic = img;
    }

    private static Sprite LoadSprite(string name)
    {
        return Resources.Load<Sprite>("UI/Dificultad/" + name);
    }

    private UImage CreateImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.AddComponent<UImage>();
    }

    private Text CreateText(string name, Transform parent, string content, int size,
        FontStyle style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = _font;
        t.text = content;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
