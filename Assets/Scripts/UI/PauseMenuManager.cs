using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private RawImage   blurBackground;
    [SerializeField] private TMP_Text   saveConfirmText;

    [Header("Config")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private int    blurFactor    = 6;
    [SerializeField] private int    saveSlot      = 0;

    private bool          _isPaused;
    private RenderTexture _blurRT;

    void Start()
    {
        if (pausePanel      != null) pausePanel.SetActive(false);
        if (saveConfirmText != null) saveConfirmText.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        Gamepad gp = Gamepad.current;

        // Abrir/cerrar pausa: Escape o el botón Options/Start del joystick.
        bool togglePressed = Input.GetKeyDown(KeyCode.Escape)
                          || (gp != null && gp.startButton.wasPressedThisFrame);

        // Volver atrás con el Círculo (buttonEast = Círculo en PS, B en Xbox): SOLO
        // cierra la pausa, no la abre.
        bool backPressed = gp != null && gp.buttonEast.wasPressedThisFrame;

        if (_isPaused)
        {
            if (togglePressed || backPressed) Resume();
        }
        else if (togglePressed)
        {
            StartCoroutine(PauseRoutine());
        }
    }

    // Selecciona el primer botón del panel de pausa para poder navegar con el joystick.
    private void SelectFirstInPause()
    {
        if (pausePanel == null || EventSystem.current == null) return;
        var sel = pausePanel.GetComponentInChildren<Selectable>(false);
        if (sel != null) EventSystem.current.SetSelectedGameObject(sel.gameObject);
    }

    private IEnumerator PauseRoutine()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        int sw = screenshot.width, sh = screenshot.height;

        int bw = Mathf.Max(sw / blurFactor, 1);
        int bh = Mathf.Max(sh / blurFactor, 1);
        var rtSmall = RenderTexture.GetTemporary(bw, bh, 0, RenderTextureFormat.ARGB32);
        rtSmall.filterMode = FilterMode.Bilinear;
        Graphics.Blit(screenshot, rtSmall);
        Destroy(screenshot);

        if (_blurRT == null || _blurRT.width != sw || _blurRT.height != sh)
        {
            if (_blurRT != null) _blurRT.Release();
            _blurRT = new RenderTexture(sw, sh, 0, RenderTextureFormat.ARGB32);
            _blurRT.filterMode = FilterMode.Bilinear;
        }
        Graphics.Blit(rtSmall, _blurRT);
        RenderTexture.ReleaseTemporary(rtSmall);

        if (blurBackground != null) blurBackground.texture = _blurRT;
        if (pausePanel     != null) pausePanel.SetActive(true);
        SelectFirstInPause();
        _isPaused      = true;
        Time.timeScale = 0f;
        MusicManager.Instance?.PauseMusic();
    }

    public void Resume()
    {
        _isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        MusicManager.Instance?.ResumeMusic();
    }

    public void Guardar()
    {
        // Captura TODO el estado (jugador, oleada, stats, boss, enemigos).
        SaveManager.Save(saveSlot, GameSnapshot.Capture());
        StartCoroutine(ShowSaveConfirm());
    }

    private IEnumerator ShowSaveConfirm()
    {
        if (saveConfirmText == null) yield break;
        saveConfirmText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        saveConfirmText.gameObject.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (_blurRT != null) { _blurRT.Release(); _blurRT = null; }
        Time.timeScale = 1f;
    }
}
