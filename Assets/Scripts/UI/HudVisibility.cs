using UnityEngine;

/// <summary>
/// Permite ocultar/mostrar el HUD de gameplay (barras de vida, leyenda de misión,
/// timer de oleada, etc.) durante las cinemáticas, dejando visibles solo los carteles
/// propios de la cinemática (que NO se incluyen en <see cref="gameplayElements"/>).
///
/// Se coloca en el GameHUD_Canvas y se le asignan los elementos a ocultar. Los
/// directores de cinemática llaman <see cref="HudVisibility.Hide"/> al empezar y
/// <see cref="HudVisibility.Show"/> al terminar. Recuerda el estado previo de cada
/// elemento (para no encender, p. ej., la barra del boss si estaba apagada) y usa un
/// contador para soportar cinemáticas encadenadas.
/// </summary>
public class HudVisibility : MonoBehaviour
{
    [Tooltip("Elementos del HUD de gameplay que se ocultan durante las cinemáticas. " +
             "NO incluir los carteles de la cinemática (mercadito / oso).")]
    [SerializeField] private GameObject[] gameplayElements;

    private bool[] _saved;
    private int _hideCount;

    /// <summary>True mientras una cinemática tiene oculto el HUD de gameplay. Lo leen
    /// HUDs en canvas aparte (ej. el disco de cooldown) para ocultarse también.</summary>
    public static bool GameplayHidden { get; private set; }

    private static HudVisibility _instance;

    void Awake()  { _instance = this; }
    void OnDestroy() { if (_instance == this) _instance = null; }

    private static HudVisibility Get()
    {
        if (_instance == null)
            _instance = FindAnyObjectByType<HudVisibility>(FindObjectsInactive.Include);
        return _instance;
    }

    public static void Hide() { Get()?.HideGameplay(); }
    public static void Show() { Get()?.ShowGameplay(); }

    public void HideGameplay()
    {
        _hideCount++;
        GameplayHidden = true;
        if (_hideCount != 1) return;   // ya estaba oculto por otra cinemática

        if (_saved == null || _saved.Length != gameplayElements.Length)
            _saved = new bool[gameplayElements.Length];

        for (int i = 0; i < gameplayElements.Length; i++)
        {
            if (gameplayElements[i] == null) continue;
            _saved[i] = gameplayElements[i].activeSelf;   // recordar para restaurar
            gameplayElements[i].SetActive(false);
        }
    }

    public void ShowGameplay()
    {
        if (_hideCount == 0) return;
        _hideCount--;
        if (_hideCount != 0) return;
        GameplayHidden = false;

        for (int i = 0; i < gameplayElements.Length; i++)
        {
            if (gameplayElements[i] == null) continue;
            bool prev = (_saved != null && i < _saved.Length) ? _saved[i] : true;
            gameplayElements[i].SetActive(prev);
        }
    }
}
