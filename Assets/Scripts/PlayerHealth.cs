using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthSlider;

    public int maxHealth = 100;
    public int currentHealth;

    private SkinnedMeshRenderer meshRenderer;
    private Color originalColor;

    private Animator anim;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Barra de vida
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Renderer
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }

        // Buscar animator en hijos (Normal Form)
        anim = GetComponentInChildren<Animator>();

        if (anim != null)
        {
            Debug.Log("Animator encontrado en: " + anim.gameObject.name);
        }
        else
        {
            Debug.LogError("NO SE ENCONTRO ANIMATOR");
        }
           {
                anim = GetComponentInChildren<Animator>();

                Debug.Log(anim);
            }
    }

    void Update()
    {
        // TEST
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

        // Actualizar UI
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log("Vida actual: " + currentHealth);

        // Animación de golpe
        if (anim != null)
        {
            Debug.Log("TRIGGER HIT");
            anim.SetTrigger("Hit");
        }

        StartCoroutine(FlashRed());

        // Muerte
        if (currentHealth <= 0)
        {
            Die();
        }
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