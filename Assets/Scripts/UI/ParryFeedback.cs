using UnityEngine;

/// <summary>
/// Feedback del parry. Escucha PlayerCombat.OnParrySuccess (que trae la posicion
/// del enemigo parryado) y muestra un cartel "PARRY" chico EN EL MUNDO, encima de
/// ese enemigo, ademas de reproducir el sonido. Es autocontenido: se auto-instancia
/// con [RuntimeInitializeOnLoadMethod], no hay que cablear nada en la escena.
/// Carga sprite y sonido desde Resources/UI/.
/// </summary>
public class ParryFeedback : MonoBehaviour
{
    private const string SpritePath = "UI/parry_hit";
    private const string AudioPath = "UI/parry_hit";

    [Tooltip("Ancho del cartel en unidades de mundo (chico).")]
    public float worldWidth = 1.5f;

    [Tooltip("Altura a la que aparece sobre el enemigo.")]
    public float yOffset = 2f;

    [Tooltip("Cuanto dura visible el cartel.")]
    public float duration = 0.5f;

    private AudioSource audioSource;
    private AudioClip clip;
    private Sprite sprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("ParryFeedback");
        DontDestroyOnLoad(go);
        go.AddComponent<ParryFeedback>();
    }

    void Awake()
    {
        sprite = Resources.Load<Sprite>(SpritePath);
        clip = Resources.Load<AudioClip>(AudioPath);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        // Sonido: una sola vez por parry.
        PlayerCombat.OnParrySuccess += HandleParrySound;
        // Cartel "PARRY": uno por cada enemigo parryado.
        PlayerCombat.OnParryHitEnemy += HandleParryPopup;
    }

    void OnDisable()
    {
        PlayerCombat.OnParrySuccess -= HandleParrySound;
        PlayerCombat.OnParryHitEnemy -= HandleParryPopup;
    }

    private void HandleParrySound(Vector3 _)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void HandleParryPopup(Vector3 worldPos)
    {
        if (sprite == null)
            return;

        var go = new GameObject("ParryPopup");
        go.transform.position = worldPos + Vector3.up * yOffset;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5000;

        // Escala para que mida ~worldWidth de ancho (el sprite es enorme por defecto).
        float spriteWidth = sprite.bounds.size.x;
        float scale = spriteWidth > 0.0001f ? worldWidth / spriteWidth : 1f;

        var anim = go.AddComponent<ParryPopupAnim>();
        anim.Init(scale, duration);
    }
}

/// <summary>Pop de escala + billboard + autodestruccion para el cartel de parry.</summary>
public class ParryPopupAnim : MonoBehaviour
{
    private float targetScale;
    private float duration;
    private float elapsed;
    private Transform cam;

    public void Init(float scale, float dur)
    {
        targetScale = scale;
        duration = dur;
        cam = Camera.main != null ? Camera.main.transform : null;
    }

    void LateUpdate()
    {
        elapsed += Time.deltaTime;

        float popIn = Mathf.Min(0.12f, duration * 0.4f);
        float s = elapsed < popIn
            ? Mathf.LerpUnclamped(targetScale * 1.5f, targetScale, elapsed / popIn)
            : targetScale;
        transform.localScale = Vector3.one * s;

        // Billboard: el cartel mira siempre a la camara.
        if (cam != null)
            transform.rotation = cam.rotation;

        if (elapsed >= duration)
            Destroy(gameObject);
    }
}
