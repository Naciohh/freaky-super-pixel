# Save System + Menús Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Sistema de guardado por slots con Guardar desde pausa, Continuar y Cargar desde el MainMenu.

**Architecture:** Un `SaveManager` estático escribe JSON a disco (`Application.persistentDataPath`). `PauseMenuManager` llama a `SaveManager.Save(slot)`. El MainMenu lee los slots disponibles para mostrar Continuar (último guardado) y el panel Cargar (todos los slots). Los datos que se persisten son: escena, posición del jugador, vida actual y timestamp.

**Tech Stack:** Unity C#, JSON (`JsonUtility`), `Application.persistentDataPath`, Unity MCP para setup de escenas.

---

## Archivos que se crean / modifican

| Archivo | Acción | Responsabilidad |
|---|---|---|
| `Assets/Scripts/SaveSystem/SaveData.cs` | Crear | Modelo serializable de un guardado |
| `Assets/Scripts/SaveSystem/SaveManager.cs` | Crear | Leer/escribir JSON a disco, listar slots |
| `Assets/Scripts/UI/PauseMenuManager.cs` | Modificar | Agregar botón Guardar + feedback |
| `Assets/Scripts/UI/MainMenuController.cs` | Modificar | Continuar carga último save, abre panel Cargar |
| `Assets/Scripts/UI/SaveSlotUI.cs` | Crear | Un botón de slot en el panel Cargar |
| `Assets/Scripts/UI/LoadMenuController.cs` | Crear | Panel Cargar: instancia y puebla los slots |

---

## Task 1: SaveData + SaveManager

**Files:**
- Create: `Assets/Scripts/SaveSystem/SaveData.cs`
- Create: `Assets/Scripts/SaveSystem/SaveManager.cs`

- [ ] **Step 1: Crear la carpeta SaveSystem**

En Unity o en el filesystem, crear `Assets/Scripts/SaveSystem/`.

- [ ] **Step 2: Crear SaveData.cs**

```csharp
// Assets/Scripts/SaveSystem/SaveData.cs
using System;

[Serializable]
public class SaveData
{
    public string sceneName;
    public float  posX, posY, posZ;
    public int    currentHealth;
    public string timestamp;   // "2026-05-22 14:30"
    public string slotLabel;   // "Ranura 1"
}
```

- [ ] **Step 3: Crear SaveManager.cs**

```csharp
// Assets/Scripts/SaveSystem/SaveManager.cs
using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public const int SlotCount = 3;
    private const string LastSlotKey = "LastSaveSlot";

    // Devuelve la ruta en disco para un slot (0-based)
    public static string SlotPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot{slot}.json");

    // Guarda en el slot indicado (0-based). Llama desde PauseMenu.
    public static void Save(int slot, SaveData data)
    {
        data.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        data.slotLabel  = $"Ranura {slot + 1}";

        File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data, true));
        PlayerPrefs.SetInt(LastSlotKey, slot);
        PlayerPrefs.Save();
    }

    // Carga un slot. Devuelve null si no existe.
    public static SaveData Load(int slot)
    {
        string path = SlotPath(slot);
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    // Devuelve el último slot guardado, o null si nunca se guardó.
    public static SaveData LoadLast()
    {
        if (!PlayerPrefs.HasKey(LastSlotKey)) return null;
        return Load(PlayerPrefs.GetInt(LastSlotKey));
    }

    // Devuelve true si existe al menos un guardado.
    public static bool HasAnySave()
    {
        for (int i = 0; i < SlotCount; i++)
            if (File.Exists(SlotPath(i))) return true;
        return false;
    }

    // Borra un slot.
    public static void Delete(int slot)
    {
        string path = SlotPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }
}
```

- [ ] **Step 4: Verificar compilación**

Abrir Unity. En la consola no deben aparecer errores de `SaveManager` ni `SaveData`. Si hay errores, corregirlos antes de continuar.

- [ ] **Step 5: Commit**

```
git add Assets/Scripts/SaveSystem/
git commit -m "feat: add SaveManager and SaveData for JSON save slots"
```

---

## Task 2: Guardar desde el menú de pausa

**Files:**
- Modify: `Assets/Scripts/UI/PauseMenuManager.cs`

- [ ] **Step 1: Reemplazar PauseMenuManager.cs completo**

```csharp
// Assets/Scripts/UI/PauseMenuManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private RawImage   blurBackground;
    [SerializeField] private TMP_Text   saveConfirmText; // texto "¡Guardado!" (opcional)

    [Header("Config")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private int    blurFactor    = 6;
    [SerializeField] private int    saveSlot      = 0; // slot que usa este nivel

    private bool          _isPaused;
    private RenderTexture _blurRT;

    void Start()
    {
        if (pausePanel    != null) pausePanel.SetActive(false);
        if (saveConfirmText != null) saveConfirmText.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) Resume();
            else           StartCoroutine(PauseRoutine());
        }
    }

    // ── Pausa ─────────────────────────────────────────────────────────────────

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
        _isPaused      = true;
        Time.timeScale = 0f;
    }

    // ── Botones ───────────────────────────────────────────────────────────────

    public void Resume()
    {
        _isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Guardar()
    {
        // Recopilar datos del jugador
        var player = GameObject.FindWithTag("Player");
        var ph     = player != null ? player.GetComponent<PlayerHealth>() : null;

        var data = new SaveData
        {
            sceneName     = SceneManager.GetActiveScene().name,
            posX          = player != null ? player.transform.position.x : 0f,
            posY          = player != null ? player.transform.position.y : 0f,
            posZ          = player != null ? player.transform.position.z : 0f,
            currentHealth = ph     != null ? ph.currentHealth : 100,
        };

        SaveManager.Save(saveSlot, data);
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
```

- [ ] **Step 2: Agregar botón "Guardar" en el PausePanel (Unity MCP)**

Ejecutar en Unity via MCP RunCommand:

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        // Cargar Nivel01
        EditorSceneManager.OpenScene("Assets/Scenes/Nivel01.unity");

        // Buscar PauseMenuBox dentro de PausePanel
        var pauseBox = GameObject.Find("PauseMenuBox");
        if (pauseBox == null) { result.LogError("No se encontro PauseMenuBox"); return; }

        // Crear botón Guardar
        var btnGO = new GameObject("GuardarButton");
        result.RegisterObjectCreation(btnGO);
        btnGO.transform.SetParent(pauseBox.transform, false);

        var rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 50);

        var img = btnGO.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.2f, 0.6f, 0.2f, 1f);

        var btn = btnGO.AddComponent<Button>();

        var txtGO = new GameObject("Text");
        result.RegisterObjectCreation(txtGO);
        txtGO.transform.SetParent(btnGO.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "GUARDAR";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = 20;
        var txtRt = txtGO.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;

        // Conectar al PauseMenuManager
        var pmm = Object.FindFirstObjectByType<PauseMenuManager>();
        if (pmm != null)
        {
            var entry = new UnityEngine.Events.UnityAction(pmm.Guardar);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, entry);
            result.Log("Boton Guardar conectado a PauseMenuManager.Guardar()");
        }

        EditorSceneManager.SaveOpenScenes();
        result.Log("Boton Guardar creado y escena guardada");
    }
}
```

- [ ] **Step 3: Verificar en Play mode**

Entrar a Nivel01 → presionar ESC → debe aparecer el botón "GUARDAR" → presionarlo → verificar que se crea el archivo `save_slot0.json` en `Application.persistentDataPath` (ruta en Windows: `%APPDATA%/../LocalLow/<CompanyName>/<ProductName>/`).

- [ ] **Step 4: Commit**

```
git add Assets/Scripts/UI/PauseMenuManager.cs Assets/Scenes/Nivel01.unity
git commit -m "feat: add Guardar button to pause menu, writes save_slot JSON"
```

---

## Task 3: MainMenu — Continuar carga el último guardado

**Files:**
- Modify: `Assets/Scripts/UI/MainMenuController.cs`

- [ ] **Step 1: Reemplazar MainMenuController.cs completo**

```csharp
// Assets/Scripts/UI/MainMenuController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
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
    [SerializeField] private Button      continuarButton; // se desactiva si no hay save
    [SerializeField] private GameObject  firstSelected;

    [Header("Panel Cargar")]
    [SerializeField] private GameObject loadMenuPanel; // asignado en Task 4

    private bool _isLoading;

    private void Start()
    {
        if (loadingPanel != null)
        {
            loadingPanel.alpha          = 0f;
            loadingPanel.interactable   = false;
            loadingPanel.blocksRaycasts = false;
        }

        // Habilitar/deshabilitar Continuar según si hay guardados
        if (continuarButton != null)
            continuarButton.interactable = SaveManager.HasAnySave();

        if (loadMenuPanel != null)
            loadMenuPanel.SetActive(false);

        if (firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    // ── Botones ──────────────────────────────────────────────────────────────

    public void NuevoJuego()
    {
        if (_isLoading) return;
        _isLoading = true;
        PlayClick();
        // Borrar referencia de último slot para que no cargue datos viejos
        PlayerPrefs.DeleteKey("LastSaveSlot");
        PlayerPrefs.Save();
        StartCoroutine(LoadWithScreen(gameSceneName));
    }

    public void Continuar()
    {
        if (_isLoading) return;
        var save = SaveManager.LoadLast();
        if (save == null) return;

        _isLoading = true;
        PlayClick();
        // Guardar la posición a restaurar antes de cargar
        GameSession.PendingSave = save;
        StartCoroutine(LoadWithScreen(save.sceneName));
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
        if (save == null) return;

        _isLoading = true;
        PlayClick();
        GameSession.PendingSave = save;
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

    // ── Audio ─────────────────────────────────────────────────────────────────

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

    // ── Pantalla de carga ─────────────────────────────────────────────────────

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
```

- [ ] **Step 2: Crear GameSession.cs (puente entre escenas)**

```csharp
// Assets/Scripts/SaveSystem/GameSession.cs
// Clase estática que transporta datos entre escenas sin DontDestroyOnLoad.
public static class GameSession
{
    // Se setea antes de cargar la escena del juego. Null = nueva partida.
    public static SaveData PendingSave;
}
```

- [ ] **Step 3: Crear GameLoader.cs (restaura estado al entrar a Nivel01)**

```csharp
// Assets/Scripts/SaveSystem/GameLoader.cs
using UnityEngine;

// Adjuntar al Player en Nivel01. Al Start(), si hay PendingSave, restaura posición y vida.
public class GameLoader : MonoBehaviour
{
    void Start()
    {
        var save = GameSession.PendingSave;
        if (save == null) return;

        // Restaurar posición
        transform.position = new Vector3(save.posX, save.posY, save.posZ);

        // Restaurar vida
        var ph = GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.currentHealth      = save.currentHealth;
            ph.healthSlider.value = save.currentHealth;
        }

        // Limpiar para que no se restaure dos veces
        GameSession.PendingSave = null;
    }
}
```

- [ ] **Step 4: Agregar GameLoader al Player en Nivel01 (Unity MCP)**

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Nivel01.unity");
        var player = GameObject.Find("Player");
        if (player == null) { result.LogError("No se encontro Player"); return; }

        var glType = System.Type.GetType("GameLoader")
                  ?? System.Type.GetType("GameLoader, Assembly-CSharp");
        if (glType == null) { result.LogError("Tipo GameLoader no encontrado"); return; }

        result.RegisterObjectModification(player);
        if (player.GetComponent(glType) == null)
            player.AddComponent(glType);

        EditorSceneManager.SaveOpenScenes();
        result.Log("GameLoader agregado al Player y escena guardada");
    }
}
```

- [ ] **Step 5: Verificar flujo Continuar**

1. Entrar a Nivel01 → ESC → Guardar
2. Volver al MainMenu → el botón Continuar debe estar interactable
3. Presionar Continuar → debe cargar Nivel01 con el jugador en la posición guardada

- [ ] **Step 6: Commit**

```
git add Assets/Scripts/UI/MainMenuController.cs Assets/Scripts/SaveSystem/
git commit -m "feat: Continuar loads last save, GameLoader restores player state"
```

---

## Task 4: MainMenu — Panel Cargar con slots

**Files:**
- Create: `Assets/Scripts/UI/SaveSlotUI.cs`
- Create: `Assets/Scripts/UI/LoadMenuController.cs`

- [ ] **Step 1: Crear SaveSlotUI.cs**

```csharp
// Assets/Scripts/UI/SaveSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Adjuntar a cada botón de slot en el panel Cargar.
public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text slotLabel;
    [SerializeField] private TMP_Text timestampLabel;
    [SerializeField] private Button   loadButton;
    [SerializeField] private Button   deleteButton;

    private int                  _slot;
    private LoadMenuController   _controller;

    public void Setup(int slot, SaveData data, LoadMenuController controller)
    {
        _slot       = slot;
        _controller = controller;

        bool hasData = data != null;
        loadButton.interactable   = hasData;
        deleteButton.interactable = hasData;

        slotLabel.text     = $"Ranura {slot + 1}";
        timestampLabel.text = hasData ? data.timestamp : "— vacío —";
    }

    public void OnLoad()   => _controller.LoadSlot(_slot);
    public void OnDelete() => _controller.DeleteSlot(_slot);
}
```

- [ ] **Step 2: Crear LoadMenuController.cs**

```csharp
// Assets/Scripts/UI/LoadMenuController.cs
using UnityEngine;

// Adjuntar al panel Cargar en el MainMenu.
public class LoadMenuController : MonoBehaviour
{
    [SerializeField] private SaveSlotUI[] slots; // 3 elementos, uno por slot

    private MainMenuController _mainMenu;

    void OnEnable()
    {
        _mainMenu = FindFirstObjectByType<MainMenuController>();
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].Setup(i, SaveManager.Load(i), this);
    }

    public void LoadSlot(int slot)  => _mainMenu.CargarSlot(slot);

    public void DeleteSlot(int slot)
    {
        SaveManager.Delete(slot);
        RefreshSlots();
    }
}
```

- [ ] **Step 3: Crear el panel Cargar en MainMenu (Unity MCP)**

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { result.LogError("No hay Canvas en MainMenu"); return; }

        // Panel raíz Cargar
        var panel = new GameObject("LoadMenuPanel");
        result.RegisterObjectCreation(panel);
        panel.transform.SetParent(canvas.transform, false);
        panel.SetActive(false);

        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
        var panelImg = panel.AddComponent<UnityEngine.UI.Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.85f);

        // Título
        var title = new GameObject("Title");
        result.RegisterObjectCreation(title);
        title.transform.SetParent(panel.transform, false);
        var titleTmp = title.AddComponent<TextMeshProUGUI>();
        titleTmp.text      = "CARGAR PARTIDA";
        titleTmp.fontSize  = 36;
        titleTmp.alignment = TextAlignmentOptions.Center;
        var titleRt = title.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.2f, 0.75f); titleRt.anchorMax = new Vector2(0.8f, 0.9f);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // Crear 3 slots
        var slotUIs = new SaveSlotUI[SaveManager.SlotCount];
        for (int i = 0; i < SaveManager.SlotCount; i++)
        {
            float yMin = 0.55f - i * 0.2f;
            float yMax = yMin + 0.18f;

            var slotGO = new GameObject($"Slot{i}");
            result.RegisterObjectCreation(slotGO);
            slotGO.transform.SetParent(panel.transform, false);
            var slotRt = slotGO.AddComponent<RectTransform>();
            slotRt.anchorMin = new Vector2(0.2f, yMin);
            slotRt.anchorMax = new Vector2(0.8f, yMax);
            slotRt.offsetMin = Vector2.zero; slotRt.offsetMax = Vector2.zero;
            var slotImg = slotGO.AddComponent<UnityEngine.UI.Image>();
            slotImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            var sui = slotGO.AddComponent<SaveSlotUI>();
            slotUIs[i] = sui;

            // Etiqueta ranura
            var lbl = new GameObject("SlotLabel");
            result.RegisterObjectCreation(lbl);
            lbl.transform.SetParent(slotGO.transform, false);
            var lblTmp = lbl.AddComponent<TextMeshProUGUI>();
            lblTmp.text     = $"Ranura {i + 1}";
            lblTmp.fontSize = 22;
            var lblRt = lbl.GetComponent<RectTransform>();
            lblRt.anchorMin = new Vector2(0.02f, 0.5f); lblRt.anchorMax = new Vector2(0.4f, 1f);
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;

            // Timestamp
            var ts = new GameObject("Timestamp");
            result.RegisterObjectCreation(ts);
            ts.transform.SetParent(slotGO.transform, false);
            var tsTmp = ts.AddComponent<TextMeshProUGUI>();
            tsTmp.text      = "— vacío —";
            tsTmp.fontSize  = 16;
            tsTmp.color     = Color.gray;
            var tsRt = ts.GetComponent<RectTransform>();
            tsRt.anchorMin = new Vector2(0.02f, 0f); tsRt.anchorMax = new Vector2(0.4f, 0.5f);
            tsRt.offsetMin = Vector2.zero; tsRt.offsetMax = Vector2.zero;

            // Botón Cargar
            var loadBtn = MakeButton(slotGO, "CargarBtn", "CARGAR",
                new Vector2(0.55f, 0.1f), new Vector2(0.74f, 0.9f), new Color(0.2f, 0.5f, 0.2f));
            result.RegisterObjectCreation(loadBtn);

            // Botón Borrar
            var delBtn = MakeButton(slotGO, "BorrarBtn", "BORRAR",
                new Vector2(0.76f, 0.1f), new Vector2(0.98f, 0.9f), new Color(0.5f, 0.1f, 0.1f));
            result.RegisterObjectCreation(delBtn);

            // Conectar referencias via SerializedObject
            var soSlot = new SerializedObject(sui);
            soSlot.FindProperty("slotLabel").objectReferenceValue      = lblTmp;
            soSlot.FindProperty("timestampLabel").objectReferenceValue = tsTmp;
            soSlot.FindProperty("loadButton").objectReferenceValue     = loadBtn.GetComponent<Button>();
            soSlot.FindProperty("deleteButton").objectReferenceValue   = delBtn.GetComponent<Button>();
            soSlot.ApplyModifiedProperties();

            // Conectar onClick
            var loadBtnComp = loadBtn.GetComponent<Button>();
            var delBtnComp  = delBtn.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(loadBtnComp.onClick, sui.OnLoad);
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(delBtnComp.onClick,  sui.OnDelete);
        }

        // LoadMenuController en el panel
        var lmc = panel.AddComponent<LoadMenuController>();
        var soLmc = new SerializedObject(lmc);
        var arr   = soLmc.FindProperty("slots");
        arr.arraySize = SaveManager.SlotCount;
        for (int i = 0; i < SaveManager.SlotCount; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = slotUIs[i];
        soLmc.ApplyModifiedProperties();

        // Botón Volver
        var backBtn = MakeButton(panel, "VolverBtn", "VOLVER",
            new Vector2(0.35f, 0.02f), new Vector2(0.65f, 0.1f), new Color(0.3f, 0.3f, 0.3f));
        result.RegisterObjectCreation(backBtn);

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        result.Log("Panel Cargar creado con 3 slots en MainMenu");
    }

    static GameObject MakeButton(GameObject parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        go.AddComponent<Button>();

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        var tRt = txtGO.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
        tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;

        return go;
    }
}
```

- [ ] **Step 4: Conectar botón "Cargar" del MainMenu al panel (Unity MCP)**

```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        var mmc       = Object.FindFirstObjectByType<MainMenuController>();
        var loadPanel = GameObject.Find("LoadMenuPanel");
        if (mmc == null || loadPanel == null)
        { result.LogError("Faltan MainMenuController o LoadMenuPanel"); return; }

        // Asignar loadMenuPanel al MainMenuController
        var soMmc = new SerializedObject(mmc);
        soMmc.FindProperty("loadMenuPanel").objectReferenceValue = loadPanel;
        soMmc.ApplyModifiedProperties();

        // Conectar botón Volver del panel
        var backBtn = GameObject.Find("VolverBtn")?.GetComponent<Button>();
        if (backBtn != null)
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                backBtn.onClick, mmc.CerrarCargar);

        EditorSceneManager.SaveOpenScenes();
        result.Log("LoadMenuPanel conectado al MainMenuController");
    }
}
```

- [ ] **Step 5: Verificar flujo completo**

1. Guardar desde pausa en Nivel01
2. Volver al MainMenu
3. Presionar "CARGAR" → panel muestra Ranura 1 con el timestamp
4. Presionar CARGAR en la ranura → carga Nivel01 con posición restaurada
5. Presionar BORRAR → la ranura queda vacía

- [ ] **Step 6: Commit**

```
git add Assets/Scripts/UI/ Assets/Scenes/MainMenu.unity
git commit -m "feat: load menu panel with 3 save slots, delete and load working"
```

---

## Resumen de flujo completo

```
MainMenu
  ├── Nueva Partida  → borra PendingSave → carga Nivel01 desde cero
  ├── Continuar      → PendingSave = último save → carga escena del save
  ├── Cargar         → abre panel con 3 ranuras
  │     ├── [CARGAR] → PendingSave = save del slot → carga escena
  │     └── [BORRAR] → elimina JSON del slot
  └── Salir

Nivel01
  ├── Al Start: GameLoader restaura posición y vida si hay PendingSave
  └── Pausa (ESC)
        └── [GUARDAR] → SaveManager.Save(slot) → crea/sobreescribe JSON
```
