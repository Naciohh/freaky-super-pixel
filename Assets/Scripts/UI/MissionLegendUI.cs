using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Leyenda de misión (arriba-derecha del nivel). Muestra el objetivo actual:
///   - <see cref="mercadito"/> mientras peleás las oleadas.
///   - <see cref="oso"/> cuando arranca la pelea con el boss (aparece el oso).
///   - <see cref="terminadas"/> cuando matás al boss.
///
/// Se apoya en <see cref="BossBearHealth.IsFightActive"/> (hay un oso vivo) y en
/// <see cref="BossBearHealth.OnBossDied"/>, así no depende del gestor de oleadas.
/// </summary>
[RequireComponent(typeof(Image))]
public class MissionLegendUI : MonoBehaviour
{
    [SerializeField] private Sprite mercadito;   // oleadas
    [SerializeField] private Sprite oso;         // pelea de boss
    [SerializeField] private Sprite terminadas;  // boss derrotado

    [Tooltip("Objeto que se activa cuando arranca la pelea del boss (ej. la barra de " +
             "vida del oso, BossHealthBarGroup). Si está asignado, se usa para pasar a " +
             "la misión del oso; si no, se cae a BossBearHealth.IsFightActive.")]
    [SerializeField] private GameObject bossFightIndicator;

    private Image _img;
    private bool _bossFightSeen;
    private bool _completed;

    void Awake()
    {
        _img = GetComponent<Image>();
        _img.preserveAspect = true;
    }

    void OnEnable()
    {
        BossBearHealth.OnBossDied += OnBossDied;
        SetSprite(mercadito);
    }

    void OnDisable()
    {
        BossBearHealth.OnBossDied -= OnBossDied;
    }

    void Update()
    {
        if (_completed) return;

        // Pasamos a la misión del oso cuando arranca la pelea: preferimos el indicador
        // (barra de vida del boss); si no está, caemos a IsFightActive.
        bool bossFight = bossFightIndicator != null
            ? bossFightIndicator.activeInHierarchy
            : BossBearHealth.IsFightActive;

        if (!_bossFightSeen && bossFight)
        {
            _bossFightSeen = true;
            SetSprite(oso);
        }
    }

    private void OnBossDied(BossBearHealth boss)
    {
        _completed = true;
        SetSprite(terminadas);
    }

    private void SetSprite(Sprite s)
    {
        if (s != null && _img != null) _img.sprite = s;
    }
}
