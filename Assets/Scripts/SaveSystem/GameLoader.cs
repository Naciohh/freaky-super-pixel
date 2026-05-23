using UnityEngine;

public class GameLoader : MonoBehaviour
{
    void Start()
    {
        var save = GameSession.PendingSave;
        if (save == null) return;

        transform.position = new Vector3(save.posX, save.posY, save.posZ);

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
