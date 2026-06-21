using UnityEngine;
using UnityEngine.Events;

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
    [Tooltip("El cartel 'E para interactuar' que se muestra al estar cerca.")]
    [SerializeField] private GameObject promptE;

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

        // Escala tipo holograma (solo crece si es interactuable)
        if (buttonSign != null)
        {
            float target = (_inRange && interactable) ? activeScale : idleScale;
            buttonSign.localScale = Vector3.Lerp(
                buttonSign.localScale, Vector3.one * target,
                Time.unscaledDeltaTime * lerpSpeed);
        }

        // Activar con E
        if (_inRange && interactable && Input.GetKeyDown(interactKey))
            onActivate?.Invoke();
    }

    private void OnEnterRange()
    {
        if (!interactable) return;
        if (promptE != null) promptE.SetActive(true);
        if (sfxSource != null && popupClip != null) sfxSource.PlayOneShot(popupClip);
    }

    private void OnExitRange()
    {
        if (promptE != null) promptE.SetActive(false);
    }

    /// <summary>Habilita/deshabilita la estación (ej. Continuar sin save).</summary>
    public void SetInteractable(bool value)
    {
        interactable = value;
        ApplyTint();
        if (!value && promptE != null) promptE.SetActive(false);
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
