using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Agranda un elemento de UI cuando está resaltado: por el mouse (hover) o por la
/// selección del EventSystem (navegación con teclado/joystick). Sirve para que se
/// vea claramente qué botón está "activo" aunque no se use el mouse.
/// Usa unscaledDeltaTime → funciona aunque el juego esté con timeScale = 0.
/// </summary>
public class SelectionScaler : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private float highlightScale = 1.18f;
    [SerializeField] private float speed = 14f;

    private Vector3 _base;
    private bool _hovered;
    private bool _selected;

    void Awake() { _base = transform.localScale; }
    void OnEnable() { _hovered = false; _selected = false; transform.localScale = _base; }

    public void OnPointerEnter(PointerEventData e) { _hovered = true; }
    public void OnPointerExit(PointerEventData e)  { _hovered = false; }
    public void OnSelect(BaseEventData e)          { _selected = true; }
    public void OnDeselect(BaseEventData e)        { _selected = false; }

    void Update()
    {
        float target = (_hovered || _selected) ? highlightScale : 1f;
        transform.localScale = Vector3.Lerp(
            transform.localScale, _base * target, Time.unscaledDeltaTime * speed);
    }
}
