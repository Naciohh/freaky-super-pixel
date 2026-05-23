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

    void Start()
    {
        currentHealth = maxHealth;

        // configurar barra
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        // obtener renderer del personaje
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        // guardar color original
        originalColor = meshRenderer.material.color;
    }

    void Update()
    {
        // TEST: tocar H para recibir daño
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(25);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // evitar negativos
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // actualizar barra
        healthSlider.value = currentHealth;

        // efecto rojo
        StartCoroutine(FlashRed());

        Debug.Log("Vida actual: " + currentHealth);
    }

    IEnumerator FlashRed()
    {
        meshRenderer.material.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        meshRenderer.material.color = originalColor;
    }
}