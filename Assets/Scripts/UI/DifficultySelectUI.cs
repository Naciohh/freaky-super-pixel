using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Panel de selección de dificultad que aparece al tocar "Nuevo Juego", ANTES
/// del video del portal. Se construye 100% por código (auto-bootstrap), así no
/// depende de armar la jerarquía a mano en la escena del menú.
///
/// Lo crea/abre <see cref="MainMenuController"/>: Show(onChosen, onCancel).
/// Al elegir un nivel llama a onChosen(Difficulty); "Volver" llama a onCancel().
/// </summary>
public class DifficultySelectUI : MonoBehaviour
{
    private Action<Difficulty> _onChosen;
    private Action _onCancel;
    private CanvasGroup _group;
    private Font _font;
    private GameObject _firstSelectable;

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

        // Selección inicial para poder navegar el panel con el joystick.
        if (_firstSelectable != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_firstSelectable);
    }

    void Update()
    {
        // Este Update solo corre cuando el panel está visible (si no, el GameObject
        // está desactivado). Volver atrás con el Círculo (buttonEast en PS, B en Xbox).
        Gamepad gp = Gamepad.current;
        if (gp != null && gp.buttonEast.wasPressedThisFrame)
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

    private void Choose(Difficulty d)
    {
        var cb = _onChosen;
        SetVisible(false);
        cb?.Invoke(d);
    }

    private void Cancel()
    {
        var cb = _onCancel;
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
        backdrop.color = new Color(0f, 0f, 0f, 0.88f);

        // Título.
        var title = CreateText("Title", transform, "ELEGÍ LA DIFICULTAD", 56, FontStyle.Bold,
            new Color(1f, 0.95f, 0.8f));
        var trt = title.rectTransform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0f, 250f);
        trt.sizeDelta = new Vector2(1000f, 120f);

        // Botones (columna centrada).
        float y = 110f;
        _firstSelectable = MakeButton("FÁCIL", new Color(0.22f, 0.55f, 0.30f), y, () => Choose(Difficulty.Facil)); y -= 95f;
        MakeButton("NORMAL",    new Color(0.22f, 0.42f, 0.68f), y, () => Choose(Difficulty.Normal));    y -= 95f;
        MakeButton("DIFÍCIL",   new Color(0.72f, 0.45f, 0.18f), y, () => Choose(Difficulty.Dificil));   y -= 95f;
        MakeButton("PESADILLA", new Color(0.70f, 0.13f, 0.13f), y, () => Choose(Difficulty.Pesadilla)); y -= 130f;
        MakeButton("VOLVER",    new Color(0.28f, 0.28f, 0.30f), y, Cancel);
    }

    private GameObject MakeButton(string label, Color color, float y, Action onClick)
    {
        var img = CreateImage("Btn_" + label, transform);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(480f, 78f);
        img.color = color;

        var btn = img.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.fadeDuration = 0.08f;
        btn.colors = colors;
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());

        var txt = CreateText("Label", img.transform, label, 34, FontStyle.Bold, Color.white);
        StretchFull(txt.rectTransform);

        return img.gameObject;
    }

    private Image CreateImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.AddComponent<Image>();
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
