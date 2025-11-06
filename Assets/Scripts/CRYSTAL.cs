using UnityEngine;
using UnityEngine.UI;

public class CrystalHealth : MonoBehaviour
{
    public float maxHealth = 500f;
    public float currentHealth;
    public Slider healthSlider;

    public DamageVignette damageVignette;
    public Color crystalHitColor = Color.blue;

    private WaveManager waveManager;

    void Start()
    {
        currentHealth = maxHealth;
        waveManager = FindFirstObjectByType<WaveManager>();

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

        if (damageVignette != null)
        {
            damageVignette.Flash(crystalHitColor);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
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