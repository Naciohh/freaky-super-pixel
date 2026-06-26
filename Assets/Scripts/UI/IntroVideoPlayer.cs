using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// Reproduce un video de intro (el portal de dificultad FÁCIL) una sola vez al abrir
/// el juego, tapando el menú del lobby a pantalla completa hasta que el video termina.
/// Recién entonces se descubre el menú. Es autocontenido: se auto-instancia con
/// [RuntimeInitializeOnLoadMethod] (no hay que cablear nada en la escena) y corre una
/// única vez por sesión, así que al volver al lobby desde una partida NO se repite.
///
/// El video se carga desde Resources/Video/Portal_facil.
/// </summary>
public class IntroVideoPlayer : MonoBehaviour
{
    // Nombre de la escena del menú: el intro solo corre cuando arrancás ahí.
    private const string MenuSceneName = "MainMenu";
    private const string VideoResourcePath = "Video/Portal_facil";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // AfterSceneLoad corre una sola vez al inicio del juego. Si la primera escena
        // no es el menú (por ejemplo arrancaste Play dentro de un nivel), no hacemos nada.
        if (SceneManager.GetActiveScene().name != MenuSceneName)
            return;

        var go = new GameObject("IntroVideoPlayer");
        go.AddComponent<IntroVideoPlayer>();
    }

    private bool _videoFinished;

    void Start()
    {
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        VideoClip clip = Resources.Load<VideoClip>(VideoResourcePath);
        if (clip == null)
        {
            // Sin video no tapamos nada: que se vea el menú normal.
            Destroy(gameObject);
            yield break;
        }

        // 1) Overlay full-screen por encima de todo (incluido el canvas del menú).
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000; // por encima de cualquier HUD/menu
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        // Fondo negro opaco (tapa el menú aunque el video tenga bordes/alfa).
        var bg = NewFullScreenGraphic<Image>("Black", canvasGO.transform);
        bg.color = Color.black;

        // RawImage que muestra el video.
        var raw = NewFullScreenGraphic<RawImage>("Video", canvasGO.transform);

        // 2) RenderTexture + VideoPlayer.
        int w = Mathf.Max(640, Screen.width);
        int h = Mathf.Max(360, Screen.height);
        var rt = new RenderTexture(w, h, 0);
        raw.texture = rt;

        var vp = gameObject.AddComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.isLooping = false;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.targetTexture = rt;
        vp.clip = clip;

        // Audio del propio mp4.
        var audio = gameObject.AddComponent<AudioSource>();
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, audio);
        vp.EnableAudioTrack(0, true);

        vp.loopPointReached += _ => _videoFinished = true;

        // 3) Preparar y reproducir.
        bool prepared = false;
        vp.prepareCompleted += _ => prepared = true;
        vp.Prepare();

        float prepTimeout = 5f;
        while (!prepared && prepTimeout > 0f)
        {
            prepTimeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        vp.Play();

        // 4) Esperar a que termine (señal principal + respaldo por duración).
        float clipDur = 6f;
        if (clip.frameRate > 0)
            clipDur = (float)(clip.frameCount / clip.frameRate);

        float elapsed = 0f;
        while (!_videoFinished && elapsed < clipDur + 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 5) Fade-out corto del overlay y limpieza.
        var group = canvasGO.AddComponent<CanvasGroup>();
        float t = 0f;
        const float fade = 0.35f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(1f - t / fade);
            yield return null;
        }

        vp.Stop();
        rt.Release();
        Destroy(gameObject);
    }

    private static T NewFullScreenGraphic<T>(string name, Transform parent) where T : Graphic
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var g = go.AddComponent<T>();
        g.raycastTarget = true; // bloquea clicks al menú mientras corre el intro
        return g;
    }
}
