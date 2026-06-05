using UnityEngine;

public class GameLoader : MonoBehaviour
{
    void Start()
    {
        var save = GameSession.PendingSave;
        if (save == null) return;

        // Ignorar posY guardada: dejar que la gravedad lo apoye en el piso.
        transform.position = new Vector3(save.posX, transform.position.y, save.posZ);

        var ph = GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.currentHealth      = save.currentHealth;
            if (ph.healthSlider != null)
                ph.healthSlider.value = save.currentHealth;
        }

        GameSession.PendingSave = null;
    }
}
