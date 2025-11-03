using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public GameObject upgradeScreen;
    public Button upgradeButton1;
    public Button upgradeButton2;
    public Button upgradeButton3;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private BowController bowController;
    private FireballAbility fireballAbility;
    private CrystalHealth crystalHealth;
    private WaveManager waveManager;

    private List<string> upgradePool = new List<string>();

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        bowController = FindFirstObjectByType<BowController>();
        fireballAbility = FindFirstObjectByType<FireballAbility>();
        crystalHealth = FindFirstObjectByType<CrystalHealth>();
        waveManager = GetComponent<WaveManager>();

        upgradeScreen.SetActive(false);

        PopulateUpgradePool();
    }

    void PopulateUpgradePool()
    {
        upgradePool.Add("Bow Damage+");
        upgradePool.Add("Fireball Damage+");
        upgradePool.Add("Player HP Regain");
        upgradePool.Add("Crystal HP Regain");
        upgradePool.Add("Move Speed+");
        upgradePool.Add("Dash Cooldown-");
    }

    public void ShowUpgradeScreen()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Check if this is the first wave (Wave index 0 just finished)
        if (waveManager.GetCurrentWaveIndex() == 1)
        {
            // Special case: Unlock Fireball
            upgradeButton1.onClick.RemoveAllListeners();
            upgradeButton1.GetComponentInChildren<TextMeshProUGUI>().text = "Unlock Fireball! (E Key)";
            upgradeButton1.onClick.AddListener(() => ApplyUpgrade("UnlockFireball"));

            upgradeButton2.gameObject.SetActive(false);
            upgradeButton3.gameObject.SetActive(false);
        }
        else
        {
            // Normal wave: Show 3 random upgrades
            upgradeButton2.gameObject.SetActive(true);
            upgradeButton3.gameObject.SetActive(true);

            List<string> options = GetRandomUpgrades(3);

            SetupButton(upgradeButton1, options[0]);
            SetupButton(upgradeButton2, options[1]);
            SetupButton(upgradeButton3, options[2]);
        }

        upgradeScreen.SetActive(true);
    }

    List<string> GetRandomUpgrades(int count)
    {
        List<string> availableUpgrades = new List<string>(upgradePool);
        List<string> chosenUpgrades = new List<string>();

        // Remove fireball damage if it's not unlocked yet
        if (fireballAbility != null && !fireballAbility.isUnlocked)
        {
            availableUpgrades.Remove("Fireball Damage+");
        }

        for (int i = 0; i < count; i++)
        {
            if (availableUpgrades.Count == 0) break;

            string randomUpgrade = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
            chosenUpgrades.Add(randomUpgrade);
            availableUpgrades.Remove(randomUpgrade);
        }
        return chosenUpgrades;
    }

    void SetupButton(Button button, string upgradeType)
    {
        button.onClick.RemoveAllListeners();
        button.GetComponentInChildren<TextMeshProUGUI>().text = upgradeType;
        button.onClick.AddListener(() => ApplyUpgrade(upgradeType));
    }

    void ApplyUpgrade(string upgradeType)
    {
        switch (upgradeType)
        {
            case "UnlockFireball":
                if (fireballAbility != null) fireballAbility.isUnlocked = true;
                break;
            case "Bow Damage+":
                if (bowController != null) bowController.arrowDamage += 5f;
                break;
            case "Fireball Damage+":
                if (fireballAbility != null) fireballAbility.fireballDamage += 10f;
                break;
            case "Player HP Regain":
                if (playerHealth != null) playerHealth.Heal(25f);
                break;
            case "Crystal HP Regain":
                if (crystalHealth != null) crystalHealth.Heal(100f);
                break;
            case "Move Speed+":
                if (playerMovement != null) playerMovement.moveSpeed += 2f;
                break;
            case "Dash Cooldown-":
                if (playerMovement != null) playerMovement.dashCooldown *= 0.8f;
                break;
        }

        HideUpgradeScreen();
    }

    void HideUpgradeScreen()
    {
        upgradeScreen.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (waveManager != null)
        {
            waveManager.StartNextWaveCoroutine();
        }
    }
}