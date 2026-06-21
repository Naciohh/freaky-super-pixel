using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthSlider;

    public int maxHealth = 200;
    public int currentHealth;

    [Tooltip("Segundos que espera tras morir antes de mostrar el Game Over (para que se vea la animacion).")]
    public float gameOverDelay = 1.5f;

    private SkinnedMeshRenderer meshRenderer;
    private Color originalColor;

    private Animator anim;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }

        anim = GetComponentInChildren<Animator>();

        if (anim != null)
        {
            Debug.Log("Animator encontrado en: " + anim.gameObject.name);
        }
        else
        {
            Debug.LogError("NO SE ENCONTRO ANIMATOR");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("SE APRETO H");
            TakeDamage(25);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log("Vida actual: " + currentHealth);

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (anim != null)
        {
            Debug.Log("TRIGGER HIT");
            anim.SetTrigger("Hit");
        }
    }

    // Cura al jugador (clamp a maxHealth). Usado por la regeneración del modo Freaky.
    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("PLAYER DEAD");

        if (anim != null)
        {
            Debug.Log("TRIGGER DIE");
            anim.SetTrigger("Die");
        }

        DisablePlayerControl();

        // Esperamos a que se reproduzca la animacion de muerte antes del Game Over.
        Invoke(nameof(ShowGameOver), gameOverDelay);
    }

    private void DisablePlayerControl()
    {
        DisableBehaviour<PlayerMovement>();
        DisableBehaviour<PlayerCombat>();
        DisableBehaviour<TransformationMode>();
    }

    private void DisableBehaviour<T>() where T : Behaviour
    {
        T behaviour = GetComponent<T>();
        if (behaviour == null) behaviour = GetComponentInParent<T>();
        if (behaviour == null) behaviour = GetComponentInChildren<T>();
        if (behaviour != null) behaviour.enabled = false;
    }

    private void ShowGameOver()
    {
        GameStats stats = GameStats.Instance;
        if (stats != null)
            stats.StopTracking();

        GameOverScreen.GetOrCreate().Show();
    }

    IEnumerator FlashRed()
    {
        if (meshRenderer == null)
            yield break;

        meshRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        meshRenderer.material.color = originalColor;
    }
}