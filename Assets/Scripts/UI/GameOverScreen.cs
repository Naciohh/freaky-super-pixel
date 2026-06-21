using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Pantalla de Game Over minima y autocontenida: se construye sola en runtime
/// (Canvas + botones Reiniciar y Menu), asi no hace falta cablear nada en el
/// inspector. Mas adelante se reemplaza por la UI definitiva con la imagen.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance { get; private set; }

    [Tooltip("Nombre de la escena del menu principal (debe estar en Build Settings).")]
    public string menuSceneName = "MainMenu";

    private GameObject root;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Devuelve la instancia existente o crea una si no hay ninguna.</summary>
    public static GameOverScreen GetOrCreate()
    {
        if (Instance != null)
            return Instance;
        var go = new GameObject("GameOverScreen");
        return go.AddComponent<GameOverScreen>();
    }

    public void Show()
    {
        if (root == null)
            BuildUI();

        root.SetActive(true);
        Time.timeScale = 0f;
    }

    // Sprites del arte definitivo (Assets/Resources/UI/).
    private const string TitleSpritePath = "UI/game_over";
    private const string RestartSpritePath = "UI/reintentar_boton";
    private const string MenuSpritePath = "UI/salir_menu";

    private void BuildUI()
    {
        EnsureEventSystem();

        root = new GameObject("GameOverCanvas");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        root.AddComponent<GraphicRaycaster>();

        // Fondo oscuro semitransparente que cubre toda la pantalla.
        var panel = CreateChild("Panel", root.transform);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.75f);
        Stretch(panel.GetComponent<RectTransform>());

        // Cartel "CADUCASTE PA" (game_over.png).
        CreateTitle(panel.transform);

        CreateButton("Reiniciar", RestartSpritePath, panel.transform, new Vector2(0f, -40f), Restart);
        CreateButton("Menu", MenuSpritePath, panel.transform, new Vector2(0f, -210f), GoToMenu);
    }

    private void CreateTitle(Transform parent)
    {
        var title = CreateChild("Title", parent);
        var titleRt = title.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0f, 300f);   // header, bien arriba
        titleRt.sizeDelta = new Vector2(648f, 240f);   // sprites son 1080x400 (aspecto 2.7)

        var sprite = Resources.Load<Sprite>(TitleSpritePath);
        if (sprite != null)
        {
            var img = title.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            return;
        }

        // Fallback de texto si todavia no esta el sprite importado.
        var titleText = title.AddComponent<Text>();
        titleText.text = "GAME OVER";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 90;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleRt.sizeDelta = new Vector2(800f, 200f);
    }

    private void CreateButton(string label, string spritePath, Transform parent, Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
    {
        var buttonGo = CreateChild(label + "Button", parent);
        var img = buttonGo.AddComponent<Image>();
        var button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        buttonGo.AddComponent<HoverScale>();   // crece un poco al pasar el mouse

        var rt = buttonGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        rt.sizeDelta = new Vector2(405f, 150f);   // caja fija; preserveAspect centra el sprite

        var sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
            return;
        }

        // Fallback: boton gris con texto si falta el sprite.
        img.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        rt.sizeDelta = new Vector2(360f, 90f);

        var textGo = CreateChild("Text", buttonGo.transform);
        var text = textGo.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 40;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        Stretch(textGo.GetComponent<RectTransform>());
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
