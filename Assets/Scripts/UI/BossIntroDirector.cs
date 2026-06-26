using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossIntroDirector : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera cinematicCamera;

    [Header("Badge UI")]
    public CanvasGroup badgeCanvasGroup;
    public TMP_Text bossNameText;        // Opcional: si hay logo, se oculta.
    public Image bossBadgeImage;         // Logo EL OSO (jefe_titulo.png).

    [Header("Tiempos cinemática")]
    [SerializeField] private float travelDuration = 2f;   // viaje de cámara hasta el boss
    [SerializeField] private float dwellDuration = 2.8f;  // se queda enfocando al boss
    [SerializeField] private float outroDuration = 1f;    // fade-out del logo al final
    [SerializeField] private float fadeDuration = 0.5f;   // fade-in del logo

    [Header("Logo pop")]
    [SerializeField] private float badgePopDuration = 0.45f; // crecida de chiquito a grande
    [SerializeField] private float badgeStartScale = 0.3f;

    [Header("Golpe de intro")]
    [SerializeField] private float attackTime = 2.2f;     // cuándo pega el golpe (desde el inicio)

    private BossController currentBoss;

    // Se dispara cuando arranca la cinemática del boss. Lo usa el logro "Un oso wacho".
    public static event System.Action OnIntroStarted;

    // Ease-out-back: overshoot suave para el "pop" del logo.
    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float xm1 = x - 1f;
        return 1f + c3 * xm1 * xm1 * xm1 + c1 * xm1 * xm1;
    }

    public void PlayIntro(BossController boss)
    {
        currentBoss = boss;
        OnIntroStarted?.Invoke();
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        Time.timeScale = 0f;
        HudVisibility.Hide();   // ocultar TODO el HUD de gameplay; solo se ve el cartel del oso

        // Que el boss anime (idle + golpe) aunque el tiempo esté congelado.
        currentBoss.SetIntroAnimatorUnscaled(true);

        // Punto al que apunta la cámara: el centro aproximado del boss (es grande).
        Vector3 lookTarget = currentBoss.transform.position + Vector3.up * 4f;

        // Dirección frontal del boss en el plano horizontal (para encuadrarlo de frente).
        Vector3 flatFwd = currentBoss.transform.forward;
        flatFwd.y = 0f;
        if (flatFwd.sqrMagnitude < 0.001f) flatFwd = Vector3.forward;
        flatFwd.Normalize();

        // Arranca desde donde está la cámara principal y transiciona suave al encuadre del boss.
        Vector3 startPos = mainCamera != null ? mainCamera.transform.position : currentBoss.transform.position;
        Quaternion startRot = mainCamera != null ? mainCamera.transform.rotation : Quaternion.identity;
        Vector3 endPos = lookTarget + flatFwd * 13f + Vector3.up * 5f;

        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (cinematicCamera != null)
        {
            cinematicCamera.gameObject.SetActive(true);
            cinematicCamera.transform.SetPositionAndRotation(startPos, startRot);
        }

        // Con logo, ocultamos el texto del nombre; sin logo, lo seguimos usando.
        if (bossNameText != null)
        {
            if (bossBadgeImage != null)
                bossNameText.gameObject.SetActive(false);
            else
                bossNameText.text = currentBoss.bossData != null ? currentBoss.bossData.displayName : "BOSS";
        }
        if (bossBadgeImage != null)
            bossBadgeImage.transform.localScale = Vector3.one * badgeStartScale;
        if (badgeCanvasGroup != null)
        {
            badgeCanvasGroup.alpha = 0f;
            badgeCanvasGroup.gameObject.SetActive(true);
        }

        // Línea de tiempo única: viaje de cámara → dwell → fade-out.
        float total = travelDuration + dwellDuration + outroDuration;
        float t = 0f;
        bool attackTriggered = false;

        while (t < total)
        {
            t += Time.unscaledDeltaTime;

            // Cámara: viaja durante travelDuration y después se queda quieta sobre el boss.
            if (cinematicCamera != null)
            {
                float camP = Mathf.Clamp01(t / travelDuration);
                float eased = Mathf.SmoothStep(0f, 1f, camP);
                Vector3 camPos = Vector3.Lerp(startPos, endPos, eased);
                Quaternion lookRot = Quaternion.LookRotation((lookTarget - camPos).normalized);
                cinematicCamera.transform.position = camPos;
                cinematicCamera.transform.rotation = Quaternion.Slerp(startRot, lookRot, eased);
            }

            // Logo: pop (de chiquito a grande) + fade-in al principio, fade-out al final.
            if (bossBadgeImage != null)
            {
                float sp = Mathf.Clamp01(t / badgePopDuration);
                float scale = Mathf.Lerp(badgeStartScale, 1f, EaseOutBack(sp));
                bossBadgeImage.transform.localScale = Vector3.one * scale;
            }
            if (badgeCanvasGroup != null)
            {
                float a = Mathf.Clamp01(t / fadeDuration);
                if (t > total - outroDuration)
                    a = Mathf.Clamp01((total - t) / outroDuration);
                badgeCanvasGroup.alpha = a;
            }

            // Un único golpe visual durante la cinemática.
            if (!attackTriggered && t >= attackTime)
            {
                attackTriggered = true;
                currentBoss.TriggerIntroAttack();
            }

            yield return null;
        }

        if (badgeCanvasGroup != null)
        {
            badgeCanvasGroup.alpha = 0f;
            badgeCanvasGroup.gameObject.SetActive(false);
        }

        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        currentBoss.SetIntroAnimatorUnscaled(false);

        HudVisibility.Show();   // restaurar HUD; OnIntroComplete arrancará la pelea y la barra del boss
        Time.timeScale = 1f;
        currentBoss.OnIntroComplete();
    }
}
