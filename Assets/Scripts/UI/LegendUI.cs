using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Leyenda "navegable" del lobby (abajo-izquierda). Muestra la imagen de teclado
/// (<see cref="keyboardSprite"/>, WASD) o de joystick (<see cref="gamepadSprite"/>)
/// según <see cref="InputDeviceTracker.UsingGamepad"/>, y se oculta mientras hay
/// un popup del lobby abierto (<see cref="LobbyModal"/>), ya que ahí no se navega.
/// </summary>
[RequireComponent(typeof(Image))]
public class LegendUI : MonoBehaviour
{
    [SerializeField] private Sprite keyboardSprite;
    [SerializeField] private Sprite gamepadSprite;

    private Image _img;
    private bool _lastGamepad;

    void Awake()
    {
        _img = GetComponent<Image>();
    }

    void OnEnable()
    {
        LobbyModal.Changed += OnModalChanged;
        Apply(force: true);
        SetHidden(LobbyModal.IsOpen);
    }

    void OnDisable()
    {
        LobbyModal.Changed -= OnModalChanged;
    }

    void Update()
    {
        if (InputDeviceTracker.UsingGamepad != _lastGamepad)
            Apply(force: false);
    }

    private void OnModalChanged(bool open) => SetHidden(open);

    private void SetHidden(bool hidden)
    {
        if (_img != null) _img.enabled = !hidden;
    }

    private void Apply(bool force)
    {
        _lastGamepad = InputDeviceTracker.UsingGamepad;
        if (_img == null) return;
        Sprite s = (_lastGamepad && gamepadSprite != null) ? gamepadSprite : keyboardSprite;
        if (s != null) _img.sprite = s;
        _img.preserveAspect = true;
    }
}
