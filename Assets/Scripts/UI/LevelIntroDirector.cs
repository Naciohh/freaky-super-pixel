using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Cinemática de entrada al nivel: un picado aéreo que recorre un poco el mapa y
/// baja hasta encuadrar a Emilio de frente, mientras aparece el cartel del nivel
/// (mercadito) con el mismo pop + fade que el badge del boss.
///
/// Corre solo al cargar la escena (después del portal). Congela el tiempo igual
/// que <see cref="BossIntroDirector"/> para que no te ataquen ni te muevas, y
/// pone los Animators de Emilio en Unscaled para que el idle se siga moviendo.
/// </summary>
public class LevelIntroDirector : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera cinematicCamera;

    [Header("Player")]
    [Tooltip("Si está vacío, se busca por tag 'Player' al arrancar.")]
    public Transform player;

    [Header("Badge UI (cartel del nivel)")]
    public CanvasGroup badgeCanvasGroup;
    public Image badgeImage;             // Logo mercadito.png

    [Header("Encuadre cámara")]
    [Tooltip("Altura del punto al que mira la cámara, sobre Emilio.")]
    [SerializeField] private float lookHeight = 1.5f;
    [Tooltip("Altura del picado aéreo inicial.")]
    [SerializeField] private float aerialHeight = 22f;
    [Tooltip("Desplazamiento del inicio hacia el frente de Emilio.")]
    [SerializeField] private float aerialForwardOffset = 8f;
    [Tooltip("Desplazamiento lateral del inicio, para un barrido diagonal.")]
    [SerializeField] private float aerialSideOffset = 5f;
    [Tooltip("Distancia final de la cámara frente a Emilio.")]
    [SerializeField] private float endDistance = 6f;
    [Tooltip("Altura final de la cámara (nivel de la cara).")]
    [SerializeField] private float endHeight = 2.2f;

    [Header("Tiempos cinemática")]
    [SerializeField] private float mapRevealDuration = 3.5f; // paneo aéreo mostrando el mapa
    [SerializeField] private float travelDuration = 3f;   // descenso aéreo -> Emilio
    [SerializeField] private float dwellDuration = 1.6f;  // se queda enfocando a Emilio
    [SerializeField] private float outroDuration = 1f;    // fade-out del cartel al final
    [SerializeField] private float fadeDuration = 0.5f;   // fade-in del cartel

    [Header("Logo pop")]
    [SerializeField] private float badgePopDuration = 0.45f; // crecida de chiquito a grande
    [SerializeField] private float badgeStartScale = 0.3f;

    [Header("Arranque")]
    [SerializeField] private bool playOnStart = true;

    private bool _played;

    // Ease-out-back: overshoot suave para el "pop" del logo (igual que el boss).
    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float xm1 = x - 1f;
        return 1f + c3 * xm1 * xm1 * xm1 + c1 * xm1 * xm1;
    }

    void Start()
    {
        if (playOnStart) PlayIntro();
    }

    public void PlayIntro()
    {
        if (_played) return;
        _played = true;
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) yield break; // sin jugador, no hay cinemática

        // Si no se cableó el CanvasGroup, lo deducimos del logo (su padre lo tiene).
        if (badgeCanvasGroup == null && badgeImage != null)
            badgeCanvasGroup = badgeImage.GetComponentInParent<CanvasGroup>(true);

        Time.timeScale = 0f;

        // Animators de Emilio en Unscaled para que el idle se mueva con el tiempo congelado.
        Animator[] anims = player.GetComponentsInChildren<Animator>(true);
        AnimatorUpdateMode[] prevModes = new AnimatorUpdateMode[anims.Length];
        for (int i = 0; i < anims.Length; i++)
        {
            prevModes[i] = anims[i].updateMode;
            anims[i].updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        // Dirección frontal de Emilio en el plano horizontal (para encuadrarlo de frente).
        Vector3 flatFwd = player.forward;
        flatFwd.y = 0f;
        if (flatFwd.sqrMagnitude < 0.001f) flatFwd = Vector3.forward;
        flatFwd.Normalize();
        Vector3 flatRight = Vector3.Cross(Vector3.up, flatFwd);

        // Punto al que apunta la cámara: Emilio, un poco por encima del piso.
        Vector3 lookTarget = player.position + Vector3.up * lookHeight;

        // Inicio del paneo aéreo: alto y hacia un lado/atrás del mapa.
        Vector3 aerialStart = lookTarget
                            - flatFwd * aerialForwardOffset
                            + flatRight * aerialSideOffset
                            + Vector3.up * aerialHeight;
        // Fin del paneo aéreo: alto, cruzado al otro lado, ya encarando a Emilio.
        Vector3 aerialMid = lookTarget
                            + flatFwd * aerialForwardOffset
                            - flatRight * aerialSideOffset
                            + Vector3.up * (aerialHeight * 0.85f);
        // Fin: de frente a Emilio, a la altura de la cara (como cierra la del boss).
        Vector3 endPos = lookTarget + flatFwd * endDistance + Vector3.up * endHeight;

        Quaternion startRot = Quaternion.LookRotation((lookTarget - aerialStart).normalized);

        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (cinematicCamera != null)
        {
            cinematicCamera.gameObject.SetActive(true);
            cinematicCamera.transform.SetPositionAndRotation(aerialStart, startRot);
        }

        // Cartel: empieza chiquito e invisible.
        if (badgeImage != null)
            badgeImage.transform.localScale = Vector3.one * badgeStartScale;
        if (badgeCanvasGroup != null)
        {
            badgeCanvasGroup.alpha = 0f;
            badgeCanvasGroup.gameObject.SetActive(true);
        }

        // Línea de tiempo: paneo aéreo -> descenso a Emilio -> dwell -> fade-out del cartel.
        float total = mapRevealDuration + travelDuration + dwellDuration + outroDuration;
        float t = 0f;

        while (t < total)
        {
            t += Time.unscaledDeltaTime;

            // Cámara: primero panea el mapa en alto, después baja a Emilio de frente.
            if (cinematicCamera != null)
            {
                Vector3 camPos;
                if (t < mapRevealDuration)
                {
                    // Fase 1: paneo aéreo mostrando el mapa.
                    float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / mapRevealDuration));
                    camPos = Vector3.Lerp(aerialStart, aerialMid, p);
                }
                else
                {
                    // Fase 2: desciende del aéreo hasta el encuadre frontal de Emilio.
                    float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - mapRevealDuration) / travelDuration));
                    camPos = Vector3.Lerp(aerialMid, endPos, p);
                }
                Quaternion lookRot = Quaternion.LookRotation((lookTarget - camPos).normalized);
                cinematicCamera.transform.position = camPos;
                cinematicCamera.transform.rotation = lookRot;
            }

            // Cartel: pop (de chiquito a grande) + fade-in al principio, fade-out al final.
            if (badgeImage != null)
            {
                float sp = Mathf.Clamp01(t / badgePopDuration);
                float scale = Mathf.Lerp(badgeStartScale, 1f, EaseOutBack(sp));
                badgeImage.transform.localScale = Vector3.one * scale;
            }
            if (badgeCanvasGroup != null)
            {
                float a = Mathf.Clamp01(t / fadeDuration);
                if (t > total - outroDuration)
                    a = Mathf.Clamp01((total - t) / outroDuration);
                badgeCanvasGroup.alpha = a;
            }

            yield return null;
        }

        if (badgeCanvasGroup != null)
        {
            badgeCanvasGroup.alpha = 0f;
            badgeCanvasGroup.gameObject.SetActive(false);
        }

        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
            // Evita el "salto": fija la cámara de juego sobre Emilio al instante,
            // así no se desliza desde su posición anterior al recuperar el control.
            CameraFollow follow = mainCamera.GetComponentInParent<CameraFollow>();
            if (follow != null) follow.SnapToTarget();
        }

        for (int i = 0; i < anims.Length; i++)
            if (anims[i] != null) anims[i].updateMode = prevModes[i];

        Time.timeScale = 1f;
    }
}
