using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cuando muere el oso (BossBearHealth.OnBossDied), espera un instante para que se
/// vea la animación de muerte y luego reproduce el portal a pantalla completa.
/// Al terminar el video, PortalTransition carga la escena del lobby.
///
/// Reutiliza el mismo PortalTransition (video + audio + overlay) que la entrada
/// a partida desde el menú; solo cambia la escena destino al lobby.
/// </summary>
public class BossDefeatPortal : MonoBehaviour
{
    [Header("Portal")]
    [Tooltip("PortalTransition de la escena (instancia del prefab PortalCanvas). Si queda vacío se busca en escena.")]
    [SerializeField] private PortalTransition portal;

    [Tooltip("Escena del lobby/menú a cargar cuando termina el portal.")]
    [SerializeField] private string lobbyScene = "MainMenu";

    [Tooltip("Espera (en segundos reales) tras la muerte del oso antes de abrir el portal.")]
    [SerializeField] private float delay = 1.5f;

    private bool _triggered;

    void OnEnable()  { BossBearHealth.OnBossDied += HandleBossDied; }
    void OnDisable() { BossBearHealth.OnBossDied -= HandleBossDied; }

    private void HandleBossDied(BossBearHealth boss)
    {
        if (_triggered) return;
        _triggered = true;
        StartCoroutine(PlayPortalAfterDelay());
    }

    private IEnumerator PlayPortalAfterDelay()
    {
        if (portal == null) portal = FindObjectOfType<PortalTransition>(true);

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        if (portal != null)
            portal.PlayThenLoad(lobbyScene);
        else
            SceneManager.LoadScene(lobbyScene); // fallback: sin video si falta el portal
    }
}
