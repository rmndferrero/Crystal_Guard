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

    [Header("Upgrade Info Panels (Optional)")]
    public GameObject damageBoostPanel;
    public GameObject hpRegenPanel;
    public GameObject crystalHealPanel;
    public GameObject moveSpeedPanel;
    public GameObject dashCooldownPanel;
    public GameObject fireballDamagePanel;

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

        HideAllUpgradePanels();
        PopulateUpgradePool();
    }

    void PopulateUpgradePool()
    {
        upgradePool.Clear();
        upgradePool.Add("Bow Damage+");
        upgradePool.Add("Fireball Damage+");
        upgradePool.Add("Player HP Regain");
        upgradePool.Add("Crystal HP Regain");
        upgradePool.Add("Move Speed+");
        upgradePool.Add("Dash Cooldown-");
    }

    public void ShowUpgradeScreen()
    {
        int waveIndex = waveManager.GetCurrentWaveIndex();

        // Fireball unlock guaranteed on the first wave (waveIndex 0)
        if (waveIndex == 0 && !fireballUnlockedOnce)
        {
            fireballUnlockedOnce = true;
            StartCoroutine(ShowFireballUnlock());
            return; // Prevent random upgrades from showing
        }

        // All other waves: show random upgrades
        ShowRandomUpgrades();
    }

    IEnumerator ShowFireballUnlock()
    {
        PauseGame(false);
        fireballUnlockScreen.SetActive(true);

        if (fireballAbility != null)
        {
            fireballAbility.isUnlocked = true;
        }

        yield return new WaitForSecondsRealtime(3f);

        fireballUnlockScreen.SetActive(false);
        UnpauseGame();

        if (waveManager != null)
        {
            waveManager.StartNextWaveCoroutine();
        }
    }

    void ShowRandomUpgrades()
    {
        PauseGame(true);
        upgradeScreen.SetActive(true);

        List<string> options = GetRandomUpgrades(3);
        if (options.Count >= 3)
        {
            SetupButton(upgradeButton1, options[0]);
            SetupButton(upgradeButton2, options[1]);
            SetupButton(upgradeButton3, options[2]);
        }
    }

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

    List<string> GetRandomUpgrades(int count)
    {
        List<string> availableUpgrades = new List<string>(upgradePool);

        // Ensure Fireball Damage+ doesn't appear if ability not unlocked yet
        if (fireballAbility != null && !fireballAbility.isUnlocked)
        {
            availableUpgrades.Remove("Fireball Damage+");
        }

        List<string> chosenUpgrades = new List<string>();
        int iterations = Mathf.Min(count, availableUpgrades.Count);

        for (int i = 0; i < iterations; i++)
        {
            int index = Random.Range(0, availableUpgrades.Count);
            chosenUpgrades.Add(availableUpgrades[index]);
            availableUpgrades.RemoveAt(index);
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
                ShowUpgradePanel(damageBoostPanel);
                break;
            case "Fireball Damage+":
                if (fireballAbility != null) fireballAbility.fireballDamage += 10f;
                ShowUpgradePanel(fireballDamagePanel);
                break;
            case "Player HP Regain":
                if (playerHealth != null) playerHealth.Heal(25f);
                ShowUpgradePanel(hpRegenPanel);
                break;
            case "Crystal HP Regain":
                if (crystalHealth != null) crystalHealth.Heal(100f);
                ShowUpgradePanel(crystalHealPanel);
                break;
            case "Move Speed+":
                if (playerMovement != null) playerMovement.moveSpeed += 2f;
                ShowUpgradePanel(moveSpeedPanel);
                break;
            case "Dash Cooldown-":
                if (playerMovement != null) playerMovement.dashCooldown *= 0.8f;
                ShowUpgradePanel(dashCooldownPanel);
                break;
        }

        HideUpgradeScreen();
    }

    void ShowUpgradePanel(GameObject panel)
    {
        if (panel == null) return;
        StartCoroutine(ShowPanelTemporarily(panel, 2f));
    }

    IEnumerator ShowPanelTemporarily(GameObject panel, float duration)
    {
        HideAllUpgradePanels();
        panel.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        panel.SetActive(false);
    }

    void HideAllUpgradePanels()
    {
        if (damageBoostPanel != null) damageBoostPanel.SetActive(false);
        if (fireballDamagePanel != null) fireballDamagePanel.SetActive(false);
        if (hpRegenPanel != null) hpRegenPanel.SetActive(false);
        if (crystalHealPanel != null) crystalHealPanel.SetActive(false);
        if (moveSpeedPanel != null) moveSpeedPanel.SetActive(false);
        if (dashCooldownPanel != null) dashCooldownPanel.SetActive(false);
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
