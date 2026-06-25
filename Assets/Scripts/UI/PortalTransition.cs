using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// Transición de "portal" al entrar a una partida desde el menú jugable.
///
/// Muestra un overlay a pantalla completa, reproduce el video del portal junto
/// con su audio y espera a que el video termine por completo. Recién entonces
/// carga y activa la escena del nivel, para que la partida NO arranque (ni te
/// ataquen) mientras el portal todavía está corriendo.
///
/// Lo llaman <see cref="MainMenuController"/>.NuevoJuego() y .Continuar() pasando
/// el nombre de la escena a cargar.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class PortalTransition : MonoBehaviour
{
    [Header("Overlay")]
    [Tooltip("CanvasGroup full-screen que tapa el menú mientras corre el portal.")]
    [SerializeField] private CanvasGroup overlay;
    [Tooltip("RawImage full-screen que muestra el RenderTexture del video.")]
    [SerializeField] private RawImage videoImage;

    [Header("Video / Audio")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Audio del portal VERDE original (victoria -> lobby). Los portales de " +
             "dificultad NO lo usan: traen su propio audio en el mp4.")]
    [SerializeField] private AudioClip   playClip;

    [Header("Portales por dificultad (inicio de partida)")]
    [Tooltip("Si están asignados, el portal de INICIO cambia según la dificultad elegida. " +
             "La victoria siempre usa el clip por defecto (verde) del VideoPlayer.")]
    [SerializeField] private VideoClip facilClip;
    [SerializeField] private VideoClip medioClip;
    [SerializeField] private VideoClip dificilClip;
    [SerializeField] private VideoClip pesadillaClip;

    private VideoClip _defaultClip;       // el verde original (para la victoria)
    private bool _useDifficultyPortal;

    [Header("Tiempos")]
    [Tooltip("Fade-in del overlay para tapar el menú antes de que arranque el video.")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [Tooltip("Tope de seguridad por si el video no avisa que terminó (segundos extra).")]
    [SerializeField] private float maxExtraWait = 2f;

    private bool _playing;
    private bool _videoFinished;

    void Awake()
    {
        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();

        // El clip que venga asignado en el inspector es el portal VERDE original.
        _defaultClip = videoPlayer != null ? videoPlayer.clip : null;

        // Audio por la pista del propio video (los mp4 de dificultad traen sonido).
        if (videoPlayer != null && audioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        if (overlay != null)
        {
            overlay.alpha          = 0f;
            overlay.blocksRaycasts = false;
            overlay.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Reproduce el portal de INICIO de partida (cambia según la dificultad elegida)
    /// y, al terminar, carga la escena.
    /// </summary>
    public void PlayThenLoad(string sceneName) => PlayThenLoad(sceneName, true);

    /// <summary>
    /// <paramref name="useDifficultyPortal"/> = true: portal de inicio según dificultad.
    /// false: portal VERDE original (victoria -> lobby).
    /// </summary>
    public void PlayThenLoad(string sceneName, bool useDifficultyPortal)
    {
        if (_playing || string.IsNullOrEmpty(sceneName)) return;
        _playing = true;
        _useDifficultyPortal = useDifficultyPortal;
        StartCoroutine(Routine(sceneName));
    }

    private VideoClip ClipForDifficulty()
    {
        switch (GameDifficulty.Current)
        {
            case Difficulty.Facil:     return facilClip;
            case Difficulty.Normal:    return medioClip;
            case Difficulty.Dificil:   return dificilClip;
            case Difficulty.Pesadilla: return pesadillaClip;
            default:                   return null;
        }
    }

    private IEnumerator Routine(string sceneName)
    {
        // 1) Mostrar overlay (fade-in corto para tapar el menú).
        if (overlay != null)
        {
            overlay.gameObject.SetActive(true);
            overlay.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                overlay.alpha = fadeInDuration > 0f ? Mathf.Clamp01(t / fadeInDuration) : 1f;
                yield return null;
            }
            overlay.alpha = 1f;
        }

        // 2) Preparar el video.
        _videoFinished = false;
        if (videoPlayer != null)
        {
            // Elegir el clip: inicio -> según dificultad (con fallback al verde);
            // victoria -> verde original.
            VideoClip clip = _useDifficultyPortal ? (ClipForDifficulty() ?? _defaultClip) : _defaultClip;
            if (clip != null) videoPlayer.clip = clip;
            videoPlayer.EnableAudioTrack(0, true);

            videoPlayer.isLooping = false;
            videoPlayer.loopPointReached += OnVideoEnd;

            bool prepared = false;
            videoPlayer.prepareCompleted += _ => prepared = true;
            videoPlayer.Prepare();

            float prepTimeout = 5f;
            while (!prepared && prepTimeout > 0f)
            {
                prepTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            videoPlayer.Play();
        }
        else
        {
            _videoFinished = true;
        }

        // 3) Audio: los portales de dificultad traen su propio audio (pista del mp4).
        //    Solo el portal VERDE original (victoria) usa el playClip suelto.
        if (!_useDifficultyPortal && audioSource != null && playClip != null)
            audioSource.PlayOneShot(playClip);

        // 4) Esperar a que el video termine COMPLETO antes de tocar la escena.
        //    Señal principal: loopPointReached. Respaldo: la duración real del clip
        //    (frames/fps) + un margen, por si el evento no dispara.
        float clipDur = 4f;
        if (videoPlayer != null && videoPlayer.clip != null && videoPlayer.clip.frameRate > 0)
            clipDur = (float)(videoPlayer.clip.frameCount / videoPlayer.clip.frameRate);

        float elapsed = 0f;
        while (!_videoFinished && elapsed < clipDur + maxExtraWait)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 5) Recién ahora cargamos y activamos el nivel. NO se precarga durante el
        //    video, así la partida no arranca (ni te atacan) mientras el portal corre.
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        _videoFinished = true;
    }

    void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }
}
