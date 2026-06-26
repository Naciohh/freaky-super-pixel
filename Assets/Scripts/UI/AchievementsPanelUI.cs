using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Popup de Extras (lobby): grilla scrolleable 2x3 con los 6 logros sobre el contenedor.
// Componente de escena (MainMenu). Asignar Open() al onActivate de la estación de Extras.
public class AchievementsPanelUI : MonoBehaviour
{
    [Header("Layout grilla (calibrar sobre el arte del contenedor)")]
    [SerializeField] private Vector2 cellSize = new Vector2(360f, 150f);
    [SerializeField] private Vector2 spacing  = new Vector2(40f, 30f);
    [SerializeField] private RectOffset padding;   // si queda null se usa default en Build

    private Canvas _canvas;
    private CanvasGroup _group;
    private RectTransform _content;
    private bool _open;
    private bool _built;
    private GameObject _firstSelectable;
    private float _armTime;

    private readonly List<(AchievementId id, Image img)> _cells = new List<(AchievementId, Image)>();

    void Awake()
    {
        BuildUI();
        SetVisible(false);
    }

    public void Open()
    {
        if (_open) return;
        _open = true;
        RefreshCells();
        SetVisible(true);
        LobbyModal.Open();
        _armTime = Time.unscaledTime + 0.3f;   // evita que la misma tecla la cierre al instante
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_firstSelectable);
    }

    public void Close()
    {
        if (!_open) return;
        _open = false;
        SetVisible(false);
        LobbyModal.Close();
    }

    void Update()
    {
        if (!_open || Time.unscaledTime < _armTime) return;

        Gamepad gp = Gamepad.current;
        bool cancel = Input.GetKeyDown(KeyCode.Escape)
                   || (gp != null && (gp.buttonEast.wasPressedThisFrame || gp.startButton.wasPressedThisFrame));
        if (cancel) Close();
    }

    private void SetVisible(bool v)
    {
        if (_group == null) return;
        _group.alpha = v ? 1f : 0f;
        _group.blocksRaycasts = v;
        _group.interactable = v;
        _canvas.gameObject.SetActive(true); // el canvas queda activo; controlamos por alpha/raycast
    }

    // Actualiza cada celda según el estado de desbloqueo (color vs oscurecido).
    private void RefreshCells()
    {
        var mgr = AchievementManager.Instance;
        foreach (var (id, img) in _cells)
        {
            bool unlocked = mgr != null && mgr.IsUnlocked(id);
            img.color = unlocked ? Color.white : new Color(0.18f, 0.18f, 0.18f, 0.85f);
        }
    }

    private void BuildUI()
    {
        if (_built) return;
        _built = true;
        if (padding == null) padding = new RectOffset(40, 40, 40, 40);

        var go = new GameObject("AchievementsCanvas");
        go.transform.SetParent(transform, false);
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 1150;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        _group = go.AddComponent<CanvasGroup>();

        // Backdrop oscuro a pantalla completa.
        var backdrop = NewImage("Backdrop", go.transform);
        StretchFull(backdrop.rectTransform);
        backdrop.color = new Color(0f, 0f, 0f, 0.7f);

        // Contenedor (la hoja rasgada con "LOGROS").
        var cont = NewImage("Container", go.transform);
        var contSprite = Resources.Load<Sprite>(AchievementCatalog.ContainerResource);
        var crt = cont.rectTransform;
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = Vector2.zero;
        float ch = 820f;
        float cw = contSprite != null ? ch * (contSprite.rect.width / contSprite.rect.height) : 1300f;
        crt.sizeDelta = new Vector2(cw, ch);
        if (contSprite != null) { cont.sprite = contSprite; cont.preserveAspect = true; cont.color = Color.white; }
        else cont.color = new Color(0.15f, 0.12f, 0.2f, 1f);

        // Viewport del ScrollRect (recorta el área de la grilla, dentro del contenedor,
        // dejando margen para el título "LOGROS" de arriba).
        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewportGo.transform.SetParent(cont.transform, false);
        var vrt = (RectTransform)viewportGo.transform;
        vrt.anchorMin = new Vector2(0.06f, 0.05f);
        vrt.anchorMax = new Vector2(0.94f, 0.78f);   // 0.78 deja espacio arriba para el título
        vrt.offsetMin = Vector2.zero; vrt.offsetMax = Vector2.zero;
        viewportGo.GetComponent<Image>().color = new Color(0, 0, 0, 0); // invisible, solo para raycast/mask

        var scroll = cont.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = vrt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        // Content con GridLayoutGroup (2 columnas) + ContentSizeFitter.
        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(viewportGo.transform, false);
        _content = (RectTransform)contentGo.transform;
        _content.anchorMin = new Vector2(0f, 1f);
        _content.anchorMax = new Vector2(1f, 1f);
        _content.pivot = new Vector2(0.5f, 1f);
        _content.anchoredPosition = Vector2.zero;
        scroll.content = _content;

        var grid = contentGo.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.padding = padding;
        grid.childAlignment = TextAnchor.UpperCenter;

        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Una celda por logro, en el orden del catálogo.
        foreach (var def in AchievementCatalog.All)
        {
            var cell = NewImage($"Cell_{def.id}", contentGo.transform);
            cell.sprite = def.Sprite;
            cell.preserveAspect = true;
            _cells.Add((def.id, cell));
            if (_firstSelectable == null) _firstSelectable = cell.gameObject;
        }
    }

    private static Image NewImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.AddComponent<Image>();
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
