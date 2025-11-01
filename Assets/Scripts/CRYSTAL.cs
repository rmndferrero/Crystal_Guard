using UnityEngine;
using UnityEngine.UI;

public class CrystalHealth : MonoBehaviour
{
    public float maxHealth = 500f;
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

        Destroy(gameObject);
    }
}