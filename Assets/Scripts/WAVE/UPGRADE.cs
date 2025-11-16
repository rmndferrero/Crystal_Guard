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
    [Range(0, 100)]
    public int rareUpgradeChance = 25;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private BowController bowController;
    private FireballAbility fireballAbility;
    private CrystalHealth crystalHealth;
    private WaveManager waveManager;

    private List<string> upgradePool = new List<string>();
    private List<string> rareUpgradePool = new List<string>();

    private bool fireballUnlockedOnce = false;

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
        upgradePool.Clear();
        upgradePool.Add("Bow DMG+");
        upgradePool.Add("Burn Time+");
        upgradePool.Add("Player HP Regain");
        upgradePool.Add("Crystal HP Regain");
        upgradePool.Add("Move Speed+");
        upgradePool.Add("Dash CD-");
        upgradePool.Add("Player Max HP+");
        upgradePool.Add("Crystal Max HP+");

        rareUpgradePool.Clear();
        rareUpgradePool.Add("Infinite Arrows");
        rareUpgradePool.Add("Enhance 3rd Shot");
    }

    public void ShowUpgradeScreen()
    {
        if (waveManager == null) return;

        int waveIndex = waveManager.GetCurrentWaveIndex(); // Fixed getter

        if (!fireballUnlockedOnce && (waveIndex == 0 || waveIndex == 1))
        {
            fireballUnlockedOnce = true;
            StartCoroutine(ShowFireballUnlockSmooth());
            return;
        }

        ShowRandomUpgrades();
    }

    IEnumerator ShowFireballUnlockSmooth()
    {
        Time.timeScale = 1f;
        fireballUnlockScreen.SetActive(true);

        if (fireballAbility != null)
            fireballAbility.isUnlocked = true;

        yield return new WaitForSecondsRealtime(3f);

        fireballUnlockScreen.SetActive(false);

        if (waveManager != null)
            waveManager.StartNextWaveCoroutine();
    }

    void ShowRandomUpgrades()
    {
        PauseGame();

        upgradeScreen.SetActive(true);

        List<string> options = GetRandomUpgrades(3);

        SetupButton(upgradeButton1, options[0]);
        SetupButton(upgradeButton2, options[1]);
        SetupButton(upgradeButton3, options[2]);
    }

    void PauseGame()
    {
        Time.timeScale = slowMotionTimeScale;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UnpauseGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    List<string> GetRandomUpgrades(int count)
    {
        List<string> available = new List<string>(upgradePool);

        if (!fireballAbility.isUnlocked)
            available.Remove("Burn Time+");

        if (playerHealth.currentHealth >= playerHealth.maxHealth)
            available.Remove("Player HP Regain");

        if (crystalHealth.currentHealth >= crystalHealth.maxHealth)
            available.Remove("Crystal HP Regain");

        List<string> selected = new List<string>();

        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int idx = Random.Range(0, available.Count);
            selected.Add(available[idx]);
            available.RemoveAt(idx);
        }

        if (rareUpgradePool.Count > 0 && Random.Range(0, 100) < rareUpgradeChance)
        {
            int rareIdx = Random.Range(0, rareUpgradePool.Count);
            int replaceIdx = Random.Range(0, selected.Count);
            selected[replaceIdx] = rareUpgradePool[rareIdx];
        }

        return selected;
    }

    void SetupButton(Button button, string upgradeName)
    {
        button.onClick.RemoveAllListeners();

        TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = upgradeName.ToUpper();

        button.onClick.AddListener(() => ApplyUpgrade(upgradeName));
    }

    void ApplyUpgrade(string upgrade)
    {
        switch (upgrade)
        {
            case "Bow DMG+": bowController.arrowDamage += 5f; break;
            case "Burn Time+": fireballAbility.fireballBurnDuration += 1.5f; break;
            case "Player HP Regain": playerHealth.Heal(9999f); break;
            case "Crystal HP Regain": crystalHealth.Heal(9999f); break;
            case "Move Speed+": playerMovement.moveSpeed *= 1.25f; break;
            case "Dash CD-": playerMovement.dashCooldown *= 0.6f; break;
            case "Player Max HP+": playerHealth.IncreaseMaxHealth(10f); break;
            case "Crystal Max HP+": crystalHealth.IncreaseMaxHealth(25f); break;
            case "Infinite Arrows": bowController.infiniteArrows = true; rareUpgradePool.Remove("Infinite Arrows"); break;
            case "Enhance 3rd Shot": bowController.enhancedThirdShot = true; rareUpgradePool.Remove("Enhance 3rd Shot"); break;
        }

        HideUpgradeScreen();
    }

    void HideUpgradeScreen()
    {
        upgradeScreen.SetActive(false);
        UnpauseGame();

        if (waveManager != null)
            waveManager.StartNextWaveCoroutine();
    }
}
