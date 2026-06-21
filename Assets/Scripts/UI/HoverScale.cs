using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Agranda un poco el elemento al pasar el mouse por encima (hover) y vuelve al
/// tamaño normal al salir. Usa tiempo no escalado para funcionar con
/// Time.timeScale = 0 (p.ej. en la pantalla de Game Over).
/// </summary>
public class HoverScale : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    public float hoverScale = 1.12f;
    public float speed = 12f;

    private Vector3 baseScale = Vector3.one;
    private Vector3 target = Vector3.one;

    void OnEnable()
    {
        baseScale = transform.localScale;
        target = baseScale;
    }

    public void OnPointerEnter(PointerEventData eventData) => target = baseScale * hoverScale;
    public void OnPointerExit(PointerEventData eventData) => target = baseScale;

    // Joystick: la selección del EventSystem dispara el mismo hover que el mouse.
    public void OnSelect(BaseEventData eventData) => target = baseScale * hoverScale;
    public void OnDeselect(BaseEventData eventData) => target = baseScale;

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * speed);
    }
}
