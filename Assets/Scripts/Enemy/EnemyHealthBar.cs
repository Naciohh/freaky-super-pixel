using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de salud flotante sobre el enemigo.
/// Se desvincula del padre en Start para ignorar la escala heredada.
/// Se muestra solo cuando el enemigo recibió daño (HP < máx).
/// Usa anchorMax.x del fillRect en lugar de fillAmount para máxima compatibilidad.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fillImage;

    [Header("Config")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private Vector2 worldSize = new Vector2(0.8f, 0.08f);

    private Transform _enemyRoot;
    private Transform _canvasTransform;
    private Transform _cam;
    private RectTransform _fillRect;
    private bool _isDamaged = false;

    void Awake()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    void Start()
    {
        _cam = Camera.main != null ? Camera.main.transform : null;
        _enemyRoot = transform;

        if (canvas == null) return;

        _canvasTransform = canvas.transform;

        // Desvincular canvas del padre para que NO herede escala del enemigo
        _canvasTransform.SetParent(null, false);

        // Tamaño directo en unidades de mundo (sizeDelta = tamaño real, scale = 1)
        _canvasTransform.localScale = Vector3.one;
        var rt = canvas.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = worldSize;

        // Configurar fillRect: ancla izquierda→derecha controlada por anchorMax.x
        if (fillImage != null)
        {
            _fillRect = fillImage.rectTransform;
            _fillRect.anchorMin = new Vector2(0f, 0f);
            _fillRect.anchorMax = new Vector2(1f, 1f);
            _fillRect.sizeDelta = Vector2.zero;
            _fillRect.anchoredPosition = Vector2.zero;
            _fillRect.pivot = new Vector2(0f, 0.5f); // pivot izquierdo
        }

        // Posición inicial
        _canvasTransform.position = _enemyRoot.position + worldOffset;
    }

    void LateUpdate()
    {
        if (!_isDamaged || _canvasTransform == null || _enemyRoot == null) return;

        // Seguir al enemigo
        _canvasTransform.position = _enemyRoot.position + worldOffset;

        // Billboard: siempre mira hacia la cámara
        if (_cam != null)
            _canvasTransform.LookAt(_canvasTransform.position + _cam.forward);
    }

    void OnDestroy()
    {
        // Destruir el canvas flotante cuando el enemigo muere
        if (_canvasTransform != null)
            Destroy(_canvasTransform.gameObject);
    }

    /// <summary>Llamado por EnemyHealth al recibir daño.</summary>
    public void UpdateBar(int current, int max)
    {
        if (max <= 0) return;

        float ratio = Mathf.Clamp01((float)current / max);

        if (!_isDamaged && ratio < 1f)
        {
            _isDamaged = true;
            if (canvas != null) canvas.gameObject.SetActive(true);
        }

        // Controlar el ancho via anchorMax.x — más confiable que fillAmount en WS canvas
        if (_fillRect != null)
            _fillRect.anchorMax = new Vector2(ratio, 1f);
    }
}
