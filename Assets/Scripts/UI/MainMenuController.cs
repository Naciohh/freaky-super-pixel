using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Nivel01";

    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private float       fadeDuration = 0.5f;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip   hoverSfx;
    [SerializeField] private AudioClip   clickSfx;

    [Header("Pantalla de carga")]
    [SerializeField] private CanvasGroup loadingPanel;
    [SerializeField] private TMP_Text    loadingText;

    [Header("Botones")]
    [SerializeField] private Button      continuarButton;
    [SerializeField] private GameObject  firstSelected;

    [Header("Panel Cargar")]
    [SerializeField] private GameObject loadMenuPanel;

    [Header("Portal")]
    [Tooltip("Si está asignado, Nuevo Juego y Continuar entran con la animación de portal.")]
    [SerializeField] private PortalTransition portalTransition;

    private bool _isLoading;
    private DifficultySelectUI _difficultyUI;

    private void Start()
    {
        if (loadingPanel != null)
        {
            loadingPanel.alpha          = 0f;
            loadingPanel.interactable   = false;
            loadingPanel.blocksRaycasts = false;
        }

        if (continuarButton != null)
            continuarButton.interactable = SaveManager.HasAnySave();

        if (loadMenuPanel != null)
            loadMenuPanel.SetActive(false);

        if (firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    void Update()
    {
        // Volver atrás con el Círculo (buttonEast en PS, B en Xbox): cierra el panel
        // "Cargar" si está abierto. En el menú raíz no hace nada (no hay a dónde volver).
        Gamepad gp = Gamepad.current;
        if (gp != null && gp.buttonEast.wasPressedThisFrame &&
            loadMenuPanel != null && loadMenuPanel.activeSelf)
        {
            CerrarCargar();
        }
    }

    // Nuevo Juego abre primero el selector de dificultad (antes del portal).
    public void NuevoJuego()
    {
        if (_isLoading) return;
        PlayClick();
        ShowDifficultySelect();
    }

    private void ShowDifficultySelect()
    {
        if (_difficultyUI == null)
            _difficultyUI = DifficultySelectUI.Create();
        _difficultyUI.Show(OnDifficultyChosen, null);
    }

    // Confirmó una dificultad: la fijamos y entramos como partida nueva.
    private void OnDifficultyChosen(Difficulty d)
    {
        if (_isLoading) return;
        _isLoading = true;
        GameDifficulty.Current = d;
        PlayerPrefs.DeleteKey("LastSaveSlot");
        PlayerPrefs.Save();
        EnterGame(gameSceneName);
    }

    public void Continuar()
    {
        if (_isLoading) return;
        var save = SaveManager.LoadLast();
        if (save == null || string.IsNullOrEmpty(save.sceneName)) return;

        _isLoading = true;
        PlayClick();
        GameSession.PendingSave = save;
        GameDifficulty.Current = (Difficulty)save.difficulty;
        EnterGame(save.sceneName);
    }

    // Nuevo Juego / Continuar entran con el portal si está asignado; si no,
    // caen a la pantalla de carga clásica.
    private void EnterGame(string sceneName)
    {
        if (portalTransition != null)
            portalTransition.PlayThenLoad(sceneName);
        else
            StartCoroutine(LoadWithScreen(sceneName));
    }

    public void AbrirCargar()
    {
        PlayClick();
        if (loadMenuPanel != null)
            loadMenuPanel.SetActive(true);
    }

    public void CerrarCargar()
    {
        if (loadMenuPanel != null)
            loadMenuPanel.SetActive(false);
    }

    public void CargarSlot(int slot)
    {
        if (_isLoading) return;
        var save = SaveManager.Load(slot);
        if (save == null || string.IsNullOrEmpty(save.sceneName)) return;

        _isLoading = true;
        PlayClick();
        GameSession.PendingSave = save;
        GameDifficulty.Current = (Difficulty)save.difficulty;
        StartCoroutine(LoadWithScreen(save.sceneName));
    }

    public void Salir()
    {
        PlayClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void MostrarProximamente()
    {
        PlayClick();
        Debug.Log("Proximamente...");
    }

    public void PlayHover()
    {
        if (sfxSource != null && hoverSfx != null)
            sfxSource.PlayOneShot(hoverSfx);
    }

    private void PlayClick()
    {
        if (sfxSource != null && clickSfx != null)
            sfxSource.PlayOneShot(clickSfx);
    }

    private IEnumerator LoadWithScreen(string sceneName)
    {
        if (loadingPanel != null)
        {
            loadingPanel.gameObject.SetActive(true);
            loadingPanel.alpha          = 0f;
            loadingPanel.blocksRaycasts = true;
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                loadingPanel.alpha = Mathf.Clamp01(t / 0.25f);
                yield return null;
            }
            loadingPanel.alpha = 1f;
        }

        StartCoroutine(AnimateDotsLoop());
        yield return StartCoroutine(FadeBGM());

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        yield return new WaitForSecondsRealtime(0.4f);
        op.allowSceneActivation = true;
    }

    private IEnumerator AnimateDotsLoop()
    {
        if (loadingText == null) yield break;
        string[] frames = { "CARGANDO", "CARGANDO.", "CARGANDO..", "CARGANDO..." };
        int i = 0;
        while (true)
        {
            loadingText.text = frames[i % frames.Length];
            i++;
            yield return new WaitForSecondsRealtime(0.35f);
        }
    }

    private IEnumerator FadeBGM()
    {
        if (bgmSource == null) yield break;
        float startVol = bgmSource.volume;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 0f;
    }
}
