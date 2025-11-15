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

    // --- NEW VARIABLES ---
    private RectTransform sliderRectTransform;
    private float originalSliderWidth;
    private float originalMaxHealth;
    // --- END NEW ---

    void Start()
    {
        currentHealth = maxHealth;
        waveManager = FindFirstObjectByType<WaveManager>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;

            // --- NEW CODE ---
            // Store the original size info
            sliderRectTransform = healthSlider.GetComponent<RectTransform>();
            originalSliderWidth = sliderRectTransform.sizeDelta.x;
            originalMaxHealth = maxHealth;
            // --- END NEW ---
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

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            // --- NEW CODE ---
            // Calculate and apply the new width
            float widthMultiplier = maxHealth / originalMaxHealth;
            sliderRectTransform.sizeDelta = new Vector2(originalSliderWidth * widthMultiplier, sliderRectTransform.sizeDelta.y);
            // --- END NEW ---
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