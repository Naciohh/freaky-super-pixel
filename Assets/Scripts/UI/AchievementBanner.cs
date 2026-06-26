using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

// Banner in-game: al desbloquear un logro reproduce su video (.mp4) arriba en pantalla.
// Auto-bootstrap + DontDestroyOnLoad + cola (si caen dos juntos, van de a uno).
public class AchievementBanner : MonoBehaviour
{
    private static AchievementBanner _instance;

    private VideoPlayer _vp;
    private AudioSource _audio;
    private RawImage _raw;
    private CanvasGroup _group;
    private RenderTexture _rt;

    [Header("Apariencia (16:9)")]
    [SerializeField] private Vector2 bannerSize = new Vector2(360f, 203f); // ~16:9, más chico
    [SerializeField] private float topMargin = 16f;        // pegado al borde superior
    [SerializeField] private float bannerDuration = 4f;    // cuánto se queda visible
    [SerializeField] private float slideDuration = 0.35f;  // entrada/salida deslizando

    private RectTransform _bannerRT;
    private Vector2 _restPos;     // posición visible (con margen)
    private Vector2 _hiddenPos;   // arriba, fuera de pantalla

    private readonly Queue<AchievementDef> _queue = new Queue<AchievementDef>();
    private bool _playing;
    private bool _videoFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("AchievementBanner");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<AchievementBanner>();
    }

    void Awake()
    {
        BuildUI();
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnUnlocked += Enqueue;
        else
            StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (AchievementManager.Instance == null) yield return null;
        AchievementManager.Instance.OnUnlocked += Enqueue;
    }

    void OnDestroy()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnUnlocked -= Enqueue;
        if (_rt != null) _rt.Release();
    }

    private void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1200;   // por encima del HUD

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        gameObject.AddComponent<GraphicRaycaster>();

        _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;

        var imgGo = new GameObject("BannerVideo", typeof(RectTransform));
        imgGo.transform.SetParent(transform, false);
        _raw = imgGo.AddComponent<RawImage>();
        _bannerRT = _raw.rectTransform;
        _bannerRT.anchorMin = _bannerRT.anchorMax = new Vector2(0.5f, 1f); // anclado arriba-centro
        _bannerRT.pivot = new Vector2(0.5f, 1f);
        _bannerRT.sizeDelta = bannerSize;

        _restPos   = new Vector2(0f, -topMargin);              // visible, con margen
        _hiddenPos = new Vector2(0f, bannerSize.y + 20f);      // fuera de pantalla, arriba
        _bannerRT.anchoredPosition = _hiddenPos;

        // RenderTexture en 16:9 para no estirar el video (los .mov son 1920x1080).
        _rt = new RenderTexture(1280, 720, 0);
        _raw.texture = _rt;

        _vp = gameObject.AddComponent<VideoPlayer>();
        _vp.playOnAwake = false;
        _vp.isLooping = true;   // si el clip es corto, sigue hasta cumplir bannerDuration
        _vp.renderMode = VideoRenderMode.RenderTexture;
        _vp.targetTexture = _rt;

        _audio = gameObject.AddComponent<AudioSource>();
        _vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _vp.SetTargetAudioSource(0, _audio);

        _vp.loopPointReached += OnVideoEnd;
    }

    private void Enqueue(AchievementDef def)
    {
        if (def == null) return;
        _queue.Enqueue(def);
        if (!_playing) StartCoroutine(PlayLoop());
    }

    private IEnumerator PlayLoop()
    {
        _playing = true;
        while (_queue.Count > 0)
        {
            var def = _queue.Dequeue();
            var clip = def.Video;
            if (clip == null) { Debug.LogWarning($"[Achievement] Sin video para {def.displayName}"); continue; }

            _vp.clip = clip;
            _vp.EnableAudioTrack(0, true);

            bool prepared = false;
            void OnPrep(VideoPlayer v) => prepared = true;
            _vp.prepareCompleted += OnPrep;
            _vp.Prepare();
            float to = 5f;
            while (!prepared && to > 0f) { to -= Time.unscaledDeltaTime; yield return null; }
            _vp.prepareCompleted -= OnPrep;

            _bannerRT.anchoredPosition = _hiddenPos;
            _group.alpha = 1f;
            _vp.Play();

            // Todo en tiempo real: la cinemática del boss congela Time.timeScale.
            yield return Slide(_hiddenPos, _restPos, slideDuration);   // entra deslizando

            float shown = 0f;
            while (shown < bannerDuration) { shown += Time.unscaledDeltaTime; yield return null; }

            yield return Slide(_restPos, _hiddenPos, slideDuration);   // sale deslizando

            _vp.Stop();
            _group.alpha = 0f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        _playing = false;
    }

    // Mueve el banner entre dos posiciones (unscaled, con suavizado).
    private IEnumerator Slide(Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0f, 1f, duration > 0f ? Mathf.Clamp01(t / duration) : 1f);
            _bannerRT.anchoredPosition = Vector2.Lerp(from, to, p);
            yield return null;
        }
        _bannerRT.anchoredPosition = to;
    }

    private void OnVideoEnd(VideoPlayer vp) => _videoFinished = true;
}
