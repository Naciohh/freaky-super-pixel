using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hover visual completo para botones de menú: escala suave + fondo violeta.
/// Maneja todo el feedback visual para no pelear con el ColorTint del Button.
/// Usa unscaledDeltaTime → funciona con timeScale = 0 (menú de pausa).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MenuButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler,  IPointerUpHandler
{
    [Header("Escala")]
    [SerializeField] private float hoverScale = 1.07f;
    [SerializeField] private float animSpeed  = 12f;

    [Header("Color de fondo")]
    [SerializeField] private Color colorNormal  = new Color(0.48f, 0.18f, 0.75f, 0.00f); // transparente
    [SerializeField] private Color colorHover   = new Color(0.48f, 0.18f, 0.75f, 1.00f); // violeta sólido
    [SerializeField] private Color colorPressed = new Color(0.30f, 0.10f, 0.52f, 1.00f); // violeta oscuro

    private RectTransform      _rt;
    private Image              _bg;
    private Vector3            _baseScale;
    private Vector3            _targetScale;
    private Color              _targetColor;
    private MainMenuController _controller;
    private bool               _isHovered;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();

        // Usar Image existente o crear una nueva para el fondo del botón
        _bg = GetComponent<Image>();
        if (_bg == null) _bg = gameObject.AddComponent<Image>();
        _bg.color = colorNormal;
        _bg.raycastTarget = true;

        _baseScale   = _rt.localScale;
        _targetScale = _baseScale;
        _targetColor = colorNormal;

        _controller = GetComponentInParent<MainMenuController>();

        // Desactivar el sistema de color interno del Button para que no interfiera
        var btn = GetComponent<Button>();
        if (btn != null) btn.transition = Selectable.Transition.None;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        // Escala suave
        if (_rt.localScale != _targetScale)
            _rt.localScale = Vector3.Lerp(_rt.localScale, _targetScale, dt * animSpeed);

        // Color suave
        if (_bg != null)
            _bg.color = Color.Lerp(_bg.color, _targetColor, dt * animSpeed);
    }

    public void OnPointerEnter(PointerEventData _)
    {
        _isHovered   = true;
        _targetScale = _baseScale * hoverScale;
        _targetColor = colorHover;
        _controller?.PlayHover();
    }

    public void OnPointerExit(PointerEventData _)
    {
        _isHovered   = false;
        _targetScale = _baseScale;
        _targetColor = colorNormal;
    }

    public void OnPointerDown(PointerEventData _) => _targetColor = colorPressed;

    public void OnPointerUp(PointerEventData _) =>
        _targetColor = _isHovered ? colorHover : colorNormal;
}
