using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Panel de CONTROLES del lobby. Se abre desde la estación "Controles"
/// (<see cref="InteractionStation.onActivate"/> -> <see cref="Open"/>) y muestra
/// la imagen de controles correcta según el dispositivo en uso: teclado/mouse
/// (<see cref="keyboardSprite"/>) o joystick (<see cref="gamepadSprite"/>),
/// leyendo <see cref="InputDeviceTracker.UsingGamepad"/>.
///
/// Si cambiás de control con el panel abierto, la imagen se intercambia en vivo.
/// Se cierra con Escape o el botón Círculo (buttonEast en PS / B en Xbox), igual
/// que el resto de los "volver atrás" del juego.
/// </summary>
public class ControlsPanelUI : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Raíz del panel que se prende/apaga (fondo + imagen).")]
    [SerializeField] private GameObject panelRoot;
    [Tooltip("La Image donde se pinta la foto de controles.")]
    [SerializeField] private Image controlsImage;

    [Header("Imagenes")]
    [Tooltip("Controles de teclado y mouse (WASD).")]
    [SerializeField] private Sprite keyboardSprite;
    [Tooltip("Controles de joystick (PLAY/PlayStation).")]
    [SerializeField] private Sprite gamepadSprite;

    private bool _open;
    private bool _lastGamepad;

    void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    /// <summary>Abre el panel mostrando la imagen del dispositivo actual.</summary>
    public void Open()
    {
        if (_open) return;
        if (panelRoot != null) panelRoot.SetActive(true);
        _open = true;
        LobbyModal.Open();   // congela a Emilio y oculta la leyenda
        Refresh();
    }

    /// <summary>Cierra el panel.</summary>
    public void Close()
    {
        if (!_open) return;
        if (panelRoot != null) panelRoot.SetActive(false);
        _open = false;
        LobbyModal.Close();
    }

    public void Toggle()
    {
        if (_open) Close(); else Open();
    }

    void Update()
    {
        if (!_open) return;

        // Swap en vivo si cambió teclado <-> joystick.
        if (InputDeviceTracker.UsingGamepad != _lastGamepad)
            Refresh();

        // Cerrar con Escape o Círculo (buttonEast) / B.
        Gamepad gp = Gamepad.current;
        bool backPressed = Input.GetKeyDown(KeyCode.Escape)
                        || (gp != null && gp.buttonEast.wasPressedThisFrame);
        if (backPressed) Close();
    }

    private void Refresh()
    {
        _lastGamepad = InputDeviceTracker.UsingGamepad;
        if (controlsImage == null) return;
        Sprite s = (_lastGamepad && gamepadSprite != null) ? gamepadSprite : keyboardSprite;
        controlsImage.sprite = s;
        controlsImage.preserveAspect = true;
    }
}
