using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreen : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Stats")]
    public TMP_Text enemiesKilledText;
    public TMP_Text parriesText;
    public TMP_Text timeText;

    [Header("Boton")]
    public Button nextLevelButton;
    public string nextLevelSceneName = "Nivel 2";

    [Header("Audio")]
    [Tooltip("Sonido de victoria. Si está vacío se carga desde Resources/Audio/victoria_sound.")]
    public AudioClip victorySound;
    private AudioSource victoryAudio;

    void OnEnable()
    {
        EnemyHealth.OnAnyEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(EnemyHealth eh)
    {
        if (eh != null && eh.data != null && eh.data.isBoss)
            ShowVictory();
    }

    private void ShowVictory()
    {
        GameStats stats = GameStats.Instance;
        if (stats != null)
        {
            stats.StopTracking();
            if (enemiesKilledText != null)
                enemiesKilledText.text = "Enemigos eliminados: " + stats.EnemiesKilled;
            if (parriesText != null)
                parriesText.text = "Parries realizados: " + stats.ParriesPerformed;
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(stats.TimeElapsed / 60f);
                int seconds = Mathf.FloorToInt(stats.TimeElapsed % 60f);
                timeText.text = string.Format("Tiempo: {0:00}:{1:00}", minutes, seconds);
            }
        }

        Time.timeScale = 0f;
        if (panel != null) panel.SetActive(true);

        PlayVictorySound();

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(() =>
            {
                if (victoryAudio != null) victoryAudio.Stop();
                Time.timeScale = 1f;
                SceneManager.LoadScene(nextLevelSceneName);
            });
        }
    }

    private void PlayVictorySound()
    {
        if (victorySound == null)
            victorySound = Resources.Load<AudioClip>("Audio/victoria_sound");
        if (victorySound == null)
            return;

        if (victoryAudio == null)
        {
            victoryAudio = gameObject.AddComponent<AudioSource>();
            victoryAudio.playOnAwake = false;
            victoryAudio.loop = true;          // suena hasta que cambiás de escena (salir)
            victoryAudio.ignoreListenerPause = true; // suena aunque timeScale = 0
            victoryAudio.spatialBlend = 0f;    // 2D
        }
        victoryAudio.clip = victorySound;
        victoryAudio.Play();
    }
}
