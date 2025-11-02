using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public GameObject upgradeScreen;
    public Button upgradeButton1;
    public Button upgradeButton2;
    public Button upgradeButton3;

    private PlayerMovement playerMovement;
    private BowController bowController;
    private WaveManager waveManager;

    void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        bowController = FindFirstObjectByType<BowController>();
        waveManager = GetComponent<WaveManager>();

        upgradeScreen.SetActive(false);
    }

    public void ShowUpgradeScreen()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        upgradeButton1.onClick.RemoveAllListeners();
        upgradeButton1.GetComponentInChildren<TextMeshProUGUI>().text = "Move Speed+";
        upgradeButton1.onClick.AddListener(() => ApplyUpgrade("Speed"));

        upgradeButton2.onClick.RemoveAllListeners();
        upgradeButton2.GetComponentInChildren<TextMeshProUGUI>().text = "Fire Rate+";
        upgradeButton2.onClick.AddListener(() => ApplyUpgrade("FireRate"));

        upgradeButton3.onClick.RemoveAllListeners();
        upgradeButton3.GetComponentInChildren<TextMeshProUGUI>().text = "Dash Power+";
        upgradeButton3.onClick.AddListener(() => ApplyUpgrade("Dash"));

        upgradeScreen.SetActive(true);
    }

    void ApplyUpgrade(string upgradeType)
    {
        if (upgradeType == "Speed")
        {
            if (playerMovement != null)
                playerMovement.moveSpeed += 2f;
        }
        else if (upgradeType == "FireRate")
        {
            if (bowController != null)
                bowController.fireRate *= 0.8f;
        }
        else if (upgradeType == "Dash")
        {
            if (playerMovement != null)
                playerMovement.dashSpeed += 5f;
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