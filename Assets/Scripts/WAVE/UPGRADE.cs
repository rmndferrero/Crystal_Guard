using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject upgradeScreen;
    public GameObject fireballUnlockScreen;

    [Header("Upgrade Buttons")]
    public Button upgradeButton1;
    public Button upgradeButton2;
    public Button upgradeButton3;

    [Header("Settings")]
    public float slowMotionTimeScale = 0.1f;

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
        fireballUnlockScreen.SetActive(false);

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
        if (waveManager.GetCurrentWaveIndex() == 1)
        {
            StartCoroutine(ShowFireballUnlock());
        }
        else
        {
            ShowRandomUpgrades();
        }
    }

    IEnumerator ShowFireballUnlock()
    {
        PauseGame(false); // Pause but don't show cursor
        fireballUnlockScreen.SetActive(true);

        if (fireballAbility != null)
        {
            fireballAbility.isUnlocked = true;
        }

        yield return new WaitForSecondsRealtime(3f);

        fireballUnlockScreen.SetActive(false);
        UnpauseGame();
        waveManager.StartNextWaveCoroutine();
    }

    void ShowRandomUpgrades()
    {
        PauseGame(true); // Pause and show cursor
        upgradeScreen.SetActive(true);

        List<string> options = GetRandomUpgrades(3);
        if (options.Count >= 3)
        {
            SetupButton(upgradeButton1, options[0]);
            SetupButton(upgradeButton2, options[1]);
            SetupButton(upgradeButton3, options[2]);
        }
    }

    // --- THIS IS THE FIX ---
    void PauseGame(bool showCursor)
    {
        Time.timeScale = slowMotionTimeScale;

        if (showCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void UnpauseGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    // --- END OF FIX ---

    List<string> GetRandomUpgrades(int count)
    {
        List<string> availableUpgrades = new List<string>(upgradePool);
        List<string> chosenUpgrades = new List<string>();

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
        UnpauseGame();

        if (waveManager != null)
        {
            waveManager.StartNextWaveCoroutine();
        }
    }
}