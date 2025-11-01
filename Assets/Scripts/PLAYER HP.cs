using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;

    private WaveManager waveManager;

    void Start()
    {
        currentHealth = maxHealth;
        waveManager = FindObjectOfType<WaveManager>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (waveManager != null)
        {
            waveManager.HandleLose();
        }

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerLook>().enabled = false;
    }
}