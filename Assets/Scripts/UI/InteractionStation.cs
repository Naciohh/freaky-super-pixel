using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Estación de interacción del menú jugable.
///
/// Cuando el jugador (tag <see cref="playerTag"/>) entra en el radio, el cartel
/// (<see cref="buttonSign"/>) crece tipo holograma, aparece el prompt "E para
/// interactuar" y suena el popup una vez. Si el jugador está cerca y aprieta E,
/// se dispara <see cref="onActivate"/> (mapeado en el Inspector a un método de
/// MainMenuController, ej. NuevoJuego()).
///
/// Si la estación no es interactuable (<see cref="interactable"/> = false, ej.
/// Continuar sin partida guardada), el cartel se ve apagado/gris, no crece a full,
/// no suena y la E no hace nada.
/// </summary>
public class InteractionStation : MonoBehaviour
{
    [Header("Carteles")]
    [Tooltip("El transform del cartel sprite que crece/decrece.")]
    [SerializeField] private Transform buttonSign;
    [Tooltip("El cartel 'E para interactuar' (teclado) que se muestra al estar cerca.")]
    [SerializeField] private GameObject promptE;
    [Tooltip("Versión del cartel para joystick (ej. 'X para interactuar'). Si está asignado, " +
             "se muestra automáticamente cuando se detecta que estás usando un joystick.")]
    [SerializeField] private GameObject promptGamepad;

    [Header("Escala holograma")]
    [SerializeField] private float idleScale   = 0.45f;
    [SerializeField] private float activeScale  = 1.0f;
    [SerializeField] private float lerpSpeed    = 8f;

    [Header("Interaccion")]
    [SerializeField] private float   interactRadius = 3.5f;
    [SerializeField] private KeyCode interactKey    = KeyCode.E;
    [SerializeField] private string  playerTag      = "Player";

    [Header("Disponibilidad")]
    [SerializeField] private bool  interactable = true;
    [Tooltip("Si está activo, la estación se apaga automáticamente cuando NO hay " +
             "partida guardada (usar en Continuar y Cargar).")]
    [SerializeField] private bool  requiresSave = false;
    [Tooltip("Color del cartel cuando NO es interactuable (apagado/gris).")]
    [SerializeField] private Color disabledTint = new Color(0.45f, 0.45f, 0.45f, 0.55f);

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip   popupClip;

    [Header("Accion")]
    public UnityEvent onActivate;

    private Transform        _player;
    private bool             _inRange;
    private bool             _promptShown;
    private bool             _lastGamepad;
    private SpriteRenderer[] _signRenderers;
    private Color[]          _baseColors;

    void Awake()
    {
        Transform source = buttonSign != null ? buttonSign : transform;
        _signRenderers = source.GetComponentsInChildren<SpriteRenderer>(true);
        _baseColors = new Color[_signRenderers.Length];
        for (int i = 0; i < _signRenderers.Length; i++)
            _baseColors[i] = _signRenderers[i].color;

        if (promptE != null) promptE.SetActive(false);
        if (promptGamepad != null) promptGamepad.SetActive(false);
        if (buttonSign != null) buttonSign.localScale = Vector3.one * idleScale;
        ApplyTint();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) _player = p.transform;

        // Continuar / Cargar: apagar si no hay partida guardada.
        if (requiresSave && !SaveManager.HasAnySave())
            SetInteractable(false);
    }

    void Update()
    {
        if (_player == null) return;

        // Distancia horizontal (ignora la altura) → el radio es distancia en el piso.
        Vector3 a = _player.position;  a.y = 0f;
        Vector3 b = transform.position; b.y = 0f;
        float dist        = Vector3.Distance(a, b);
        bool  nowInRange  = dist <= interactRadius;

        if (nowInRange && !_inRange)      OnEnterRange();
        else if (!nowInRange && _inRange) OnExitRange();
        _inRange = nowInRange;

        // Si cambió el dispositivo (teclado <-> joystick) mientras el cartel está
        // visible, hacemos el swap de la foto en vivo.
        if (_promptShown && InputDeviceTracker.UsingGamepad != _lastGamepad)
            ShowPrompt(true);

        // Escala tipo holograma (solo crece si es interactuable)
        if (buttonSign != null)
        {
            float target = (_inRange && interactable) ? activeScale : idleScale;
            buttonSign.localScale = Vector3.Lerp(
                buttonSign.localScale, Vector3.one * target,
                Time.unscaledDeltaTime * lerpSpeed);
        }

        // Activar con E (teclado) o X / R1 del joystick.
        if (_inRange && interactable && InteractPressed())
            onActivate?.Invoke();
    }

    // True el frame en que se aprieta el botón de interactuar: tecla E, o X
    // (buttonSouth) o R1 (rightShoulder) del joystick.
    private bool InteractPressed()
    {
        if (Input.GetKeyDown(interactKey)) return true;
        Gamepad gp = Gamepad.current;
        return gp != null &&
               (gp.buttonSouth.wasPressedThisFrame || gp.rightShoulder.wasPressedThisFrame);
    }

    private void OnEnterRange()
    {
        if (!interactable) return;
        ShowPrompt(true);
        if (sfxSource != null && popupClip != null) sfxSource.PlayOneShot(popupClip);
    }

    private void OnExitRange()
    {
        ShowPrompt(false);
    }

    // Muestra el cartel correcto (joystick vs teclado) según el dispositivo en uso,
    // u oculta ambos. Si no hay foto de joystick asignada, siempre cae al de teclado.
    private void ShowPrompt(bool show)
    {
        if (!show)
        {
            if (promptE != null) promptE.SetActive(false);
            if (promptGamepad != null) promptGamepad.SetActive(false);
            _promptShown = false;
            return;
        }

        bool useGamepad = InputDeviceTracker.UsingGamepad && promptGamepad != null;
        if (promptGamepad != null) promptGamepad.SetActive(useGamepad);
        if (promptE != null) promptE.SetActive(!useGamepad);

        _promptShown = true;
        _lastGamepad = InputDeviceTracker.UsingGamepad;
    }

    /// <summary>Habilita/deshabilita la estación (ej. Continuar sin save).</summary>
    public void SetInteractable(bool value)
    {
        interactable = value;
        ApplyTint();
        if (!value) ShowPrompt(false);
    }

    private void ApplyTint()
    {
        if (_signRenderers == null) return;
        for (int i = 0; i < _signRenderers.Length; i++)
        {
            if (_signRenderers[i] == null) continue;
            _signRenderers[i].color = interactable ? _baseColors[i] : disabledTint;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
