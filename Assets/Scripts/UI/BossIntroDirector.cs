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
    public TMP_Text bossNameText;

    [Header("Config")]
    [SerializeField] private float introDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private BossController currentBoss;

    public void PlayIntro(BossController boss)
    {
        currentBoss = boss;
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        Time.timeScale = 0f;

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

        if (bossNameText != null)
            bossNameText.text = currentBoss.bossData != null ? currentBoss.bossData.displayName : "BOSS";
        if (badgeCanvasGroup != null)
        {
            badgeCanvasGroup.alpha = 0f;
            badgeCanvasGroup.gameObject.SetActive(true);
        }

        // Travelling: la cámara se posiciona frente al boss y se va acercando mientras lo mira.
        float t = 0f;
        while (t < introDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / introDuration);
            float eased = Mathf.SmoothStep(0f, 1f, p);

            if (cinematicCamera != null)
            {
                Vector3 camPos = Vector3.Lerp(startPos, endPos, eased);
                Quaternion lookRot = Quaternion.LookRotation((lookTarget - camPos).normalized);
                cinematicCamera.transform.position = camPos;
                cinematicCamera.transform.rotation = Quaternion.Slerp(startRot, lookRot, eased);
            }

            // Badge: fade-in al principio, fade-out al final.
            if (badgeCanvasGroup != null)
            {
                float a = Mathf.Clamp01(t / fadeDuration);
                if (t > introDuration - fadeDuration)
                    a = Mathf.Clamp01((introDuration - t) / fadeDuration);
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
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        Time.timeScale = 1f;
        currentBoss.OnIntroComplete();
    }
}
