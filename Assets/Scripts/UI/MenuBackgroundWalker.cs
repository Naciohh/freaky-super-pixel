using UnityEngine;

/// <summary>
/// Hace que un personaje camine de ida y vuelta en el fondo del menú principal.
/// Detecta automáticamente el parámetro de movimiento del Animator en Awake.
/// Al llegar al extremo, rota suavemente 180° antes de retomar la marcha.
/// </summary>
public class MenuBackgroundWalker : MonoBehaviour
{
    [Header("Patrulla")]
    [SerializeField] private float speed         = 1.8f;
    [SerializeField] private float walkDistance  = 25f;
    [SerializeField] private float pauseAtTurn   = 1.0f;   // segundos de pausa (incluye rotación)
    [SerializeField] private float rotationSpeed = 280f;   // grados por segundo al girar

    [Header("Animacion")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string walkParam  = "IsMoving";

    private static readonly string[] WalkBoolCandidates =
        { "IsMoving", "isMoving", "IsWalking", "isWalking", "Walking", "Move" };

    private Animator   _animator;
    private string     _resolvedWalkParam;
    private float      _travelled;
    private float      _pauseTimer;
    private bool       _pausing;
    private Quaternion _targetRotation;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _targetRotation = transform.rotation;

        if (_animator == null) return;

        // Auto-detectar parámetro bool de movimiento
        foreach (var candidate in WalkBoolCandidates)
            foreach (var p in _animator.parameters)
                if (p.name == candidate && p.type == AnimatorControllerParameterType.Bool)
                { _resolvedWalkParam = candidate; break; }

        if (_resolvedWalkParam == null) _resolvedWalkParam = walkParam;
        Debug.Log($"[MenuBackgroundWalker] Param: '{_resolvedWalkParam}'");
    }

    void Start() => SetAnimWalking(true);

    void Update()
    {
        // ── Rotación suave siempre activa ────────────────────────────────────
        if (transform.rotation != _targetRotation)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime);

        // ── Pausa entre tramos ───────────────────────────────────────────────
        if (_pausing)
        {
            _pauseTimer += Time.deltaTime;
            if (_pauseTimer >= pauseAtTurn)
            {
                _pausing    = false;
                _pauseTimer = 0f;
                SetAnimWalking(true);
            }
            return;
        }

        // ── Movimiento en la dirección a la que "mira" la animación ─────────
        // Vector3.back = local -Z, que coincide con el visual "adelante" del modelo
        float step = speed * Time.deltaTime;
        transform.Translate(Vector3.back * step, Space.Self);
        _travelled += step;

        if (_travelled >= walkDistance)
        {
            _travelled = 0f;
            _pausing   = true;
            // Programar giro suave de 180°
            _targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + 180f, 0f);
            SetAnimWalking(false);
        }
    }

    private void SetAnimWalking(bool walking)
    {
        if (_animator == null) return;
        foreach (var p in _animator.parameters)
        {
            if (p.name == _resolvedWalkParam && p.type == AnimatorControllerParameterType.Bool)
                _animator.SetBool(_resolvedWalkParam, walking);
            if (p.name == speedParam && p.type == AnimatorControllerParameterType.Float)
                _animator.SetFloat(speedParam, walking ? speed : 0f);
            if (p.name == "Walk"  && p.type == AnimatorControllerParameterType.Trigger && walking)
                _animator.SetTrigger("Walk");
            if (p.name == "Idle"  && p.type == AnimatorControllerParameterType.Trigger && !walking)
                _animator.SetTrigger("Idle");
        }
    }
}
