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

        if (anim != null)
        {
            Debug.Log("TRIGGER HIT");
            anim.SetTrigger("Hit");
        }

        StartCoroutine(FlashRed());

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