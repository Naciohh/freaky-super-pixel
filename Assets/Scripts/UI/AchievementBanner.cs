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
        var rt = _raw.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -40f);
        rt.sizeDelta = new Vector2(800f, 225f);   // ajustar al aspect real del video

        _rt = new RenderTexture(1280, 360, 0);
        _raw.texture = _rt;

        _vp = gameObject.AddComponent<VideoPlayer>();
        _vp.playOnAwake = false;
        _vp.isLooping = false;
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

            _videoFinished = false;
            _group.alpha = 1f;
            _vp.Play();

            // Usar tiempo real: la cinemática del boss congela Time.timeScale.
            float dur = 4f;
            if (clip.frameRate > 0) dur = (float)(clip.frameCount / clip.frameRate);
            float elapsed = 0f;
            while (!_videoFinished && elapsed < dur + 1.5f)
            { elapsed += Time.unscaledDeltaTime; yield return null; }

            _group.alpha = 0f;
            yield return new WaitForSecondsRealtime(0.3f);
        }
        _playing = false;
    }

    private void OnVideoEnd(VideoPlayer vp) => _videoFinished = true;
}
