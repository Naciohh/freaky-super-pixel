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

        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (cinematicCamera != null)
        {
            cinematicCamera.gameObject.SetActive(true);
            cinematicCamera.transform.LookAt(currentBoss.transform.position + Vector3.up * 1.5f);
        }

        if (bossNameText != null) bossNameText.text = currentBoss.bossData != null ? currentBoss.bossData.displayName : "BOSS";
        if (badgeCanvasGroup != null)
        {
            badgeCanvasGroup.alpha = 0f;
            badgeCanvasGroup.gameObject.SetActive(true);

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                badgeCanvasGroup.alpha = t / fadeDuration;
                yield return null;
            }
            badgeCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(introDuration);

        if (badgeCanvasGroup != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                badgeCanvasGroup.alpha = 1f - (t / fadeDuration);
                yield return null;
            }
            badgeCanvasGroup.gameObject.SetActive(false);
        }

        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        Time.timeScale = 1f;
        currentBoss.OnIntroComplete();
    }
}
