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
        PauseGame(false);
        fireballUnlockScreen.SetActive(true);

        if (fireballAbility != null)
            fireballAbility.isUnlocked = true;

        yield return new WaitForSecondsRealtime(3f);

        fireballUnlockScreen.SetActive(false);
        UnpauseGame();
        waveManager.StartNextWaveCoroutine();
    }

    void ShowRandomUpgrades()
    {
        PauseGame(true);
        upgradeScreen.SetActive(true);

        List<string> options = GetRandomUpgrades(3);

        // --- THIS IS THE FIX for the "randomization" bug/crash ---

        // First, hide all buttons
        upgradeButton1.gameObject.SetActive(false);
        upgradeButton2.gameObject.SetActive(false);
        upgradeButton3.gameObject.SetActive(false);

        // Only show and set up buttons if we have an upgrade for them
        if (options.Count >= 1)
        {
            SetupButton(upgradeButton1, options[0]);
            upgradeButton1.gameObject.SetActive(true);
        }
        if (options.Count >= 2)
        {
            SetupButton(upgradeButton2, options[1]);
            upgradeButton2.gameObject.SetActive(true);
        }
        if (options.Count >= 3)
        {
            SetupButton(upgradeButton3, options[2]);
            upgradeButton3.gameObject.SetActive(true);
        }
        // --- END OF FIX ---
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
        List<string> available = new List<string>(upgradePool);

        if (fireballAbility != null && !fireballAbility.isUnlocked)
            available.Remove("Fireball Damage+");

        List<string> chosen = new List<string>();

        for (int i = 0; i < count; i++)
        {
            if (available.Count == 0) break;

            int idx = Random.Range(0, available.Count);
            chosen.Add(available[idx]);
            available.RemoveAt(idx);
        }

        return chosen;
    }

    void SetupButton(Button button, string upgradeType)
    {
        button.onClick.RemoveAllListeners();

        TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();

        // --- THIS IS THE "CAPSLOCK" FIX ---
        tmp.text = upgradeType.ToUpper();
        // --- END OF FIX ---

        button.onClick.AddListener(() => ApplyUpgrade(upgradeType));
    }

    void ApplyUpgrade(string upgradeType)
    {
        // Note: The switch checks the original (non-capslock) name
        switch (upgradeType)
        {
            case "Bow Damage+":
                bowController.arrowDamage += 5f;
                break;

            case "Fireball Damage+":
                fireballAbility.fireballDamage += 10f;
                break;

            case "Player HP Regain":
                playerHealth.Heal(25f);
                break;

            case "Crystal HP Regain":
                crystalHealth.Heal(100f);
                break;

            case "Move Speed+":
                playerMovement.moveSpeed += 2f;
                break;

            case "Dash Cooldown-":
                playerMovement.dashCooldown *= 0.8f;
                break;
        }

        HideUpgradeScreen();
    }

    void HideUpgradeScreen()
    {
        upgradeScreen.SetActive(false);
        UnpauseGame();
        waveManager.StartNextWaveCoroutine();
    }
}