using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Tracks del nivel")]
    public AudioClip explorationClip;
    public AudioClip fightClip;
    public AudioClip bossClip;

    [Header("Config")]
    [SerializeField] private float fadeDuration = 1.5f;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        sourceA.loop = true;
        sourceB.loop = true;
        activeSource = sourceA;
    }

    void OnEnable()
    {
        WaveManager.OnWaveStart += PlayFight;
    }

    void OnDisable()
    {
        WaveManager.OnWaveStart -= PlayFight;
    }

    void Start()
    {
        if (explorationClip != null) PlayClip(explorationClip);
    }

    public void PlayFight()
    {
        if (fightClip != null) CrossfadeTo(fightClip);
    }

    public void PlayBoss()
    {
        if (bossClip != null) CrossfadeTo(bossClip);
    }

    private void PlayClip(AudioClip clip)
    {
        activeSource.clip = clip;
        activeSource.volume = 1f;
        activeSource.Play();
    }

    private void CrossfadeTo(AudioClip clip)
    {
        AudioSource incoming = (activeSource == sourceA) ? sourceB : sourceA;
        incoming.clip = clip;
        incoming.volume = 0f;
        incoming.Play();
        StartCoroutine(Crossfade(activeSource, incoming));
        activeSource = incoming;
    }

    private IEnumerator Crossfade(AudioSource outgoing, AudioSource incoming)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            outgoing.volume = 1f - t;
            incoming.volume = t;
            yield return null;
        }
        outgoing.Stop();
        outgoing.volume = 1f;
    }
}
