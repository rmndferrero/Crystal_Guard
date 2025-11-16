using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyGroup
    {
        public GameObject enemyPrefab;
        public int count;
    }

    [System.Serializable]
    public class Wave
    {
        public string name;
        public EnemyGroup[] enemyGroups;
        public float spawnRate;
    }

    [Header("Wave Setup")]
    public Wave[] waves;
    public Transform[] spawnPoints;

    [Header("Core Components")]
    public CrystalHealth crystal;
    public PlayerHealth player;
    private UpgradeManager upgradeManager;
    private AudioSource audioSource;

    [Header("UI & SFX")]
    public TextMeshProUGUI waveText;
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject waveCompleteScreen;
    public AudioClip waveCompleteSound;
    public float waveCompleteDisplayTime = 2.5f;

    [Header("Start Sequence UI")]
    public CanvasGroup blackFade;
    public CanvasGroup controlsPanel;
    public CanvasGroup survivePanel;
    public AudioClip survivePanelSound; // Sound for survive panel

    private bool waitingForStartClick = true;
    private bool startSequenceStarted = false;
    private bool controlsPanelShownOnce = false; // Prevent showing controls panel again

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool gameIsOver = false;
    public bool GameStarted { get; private set; } = false;

    private readonly List<MonoBehaviour> disabledAttackScripts = new();
    private const string SCENE_NAME = "Game"; // Change to your scene name

    void Start()
    {
        upgradeManager = GetComponent<UpgradeManager>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
            player = Object.FindFirstObjectByType<PlayerHealth>();

        winScreen?.SetActive(false);
        loseScreen?.SetActive(false);
        waveCompleteScreen?.SetActive(false);

        Time.timeScale = 1f;

        DisableCombatScripts();

        // Initial UI setup
        blackFade.alpha = 1f;
        controlsPanel.alpha = 0f;
        controlsPanel.gameObject.SetActive(false);
        survivePanel.alpha = 0f;
        survivePanel.gameObject.SetActive(false);

        if (!controlsPanelShownOnce)
        {
            StartCoroutine(InitialFadeSequence());
            controlsPanelShownOnce = true;
        }
    }

    IEnumerator InitialFadeSequence()
    {
        // Fade black to transparent
        yield return FadeCanvas(blackFade, 1f, 0f, 1f);

        // Show controls panel only once
        controlsPanel.gameObject.SetActive(true);
        yield return FadeCanvas(controlsPanel, 0f, 1f, 0.7f);
    }

    void Update()
    {
        if (gameIsOver) return;

        // Wait for left click while controls panel is shown
        if (waitingForStartClick && controlsPanel.alpha >= 0.99f)
        {
            if (Input.GetMouseButtonDown(0))
            {
                waitingForStartClick = false;
                StartCoroutine(StartGameSequence());
            }
            return;
        }

        if (crystal == null || player == null)
            HandleLose();
    }

    IEnumerator StartGameSequence()
    {
        if (startSequenceStarted) yield break;
        startSequenceStarted = true;

        // Fade out controls panel
        if (controlsPanel.gameObject.activeInHierarchy)
        {
            yield return FadeCanvas(controlsPanel, 1f, 0f, 0.6f);
            controlsPanel.gameObject.SetActive(false);
        }

        // Show "Survive All Waves"
        survivePanel.gameObject.SetActive(true);

        // Play survive panel sound
        if (survivePanelSound != null)
            audioSource.PlayOneShot(survivePanelSound);

        // Allow movement while survive panel is visible
        EnableCombatScripts();

        yield return FadeCanvas(survivePanel, 0f, 1f, 0.6f);
        yield return new WaitForSeconds(1.3f);
        yield return FadeCanvas(survivePanel, 1f, 0f, 0.6f);
        survivePanel.gameObject.SetActive(false);

        GameStarted = true;
        StartCoroutine(SpawnNextWave());
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float time)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < time)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator SpawnNextWave()
    {
        if (currentWaveIndex >= waves.Length) yield break;

        Wave wave = waves[currentWaveIndex];
        UpdateWaveUI(wave.name);

        enemiesAlive = 0;
        foreach (EnemyGroup group in wave.enemyGroups)
            enemiesAlive += group.count;

        if (enemiesAlive <= 0)
        {
            OnEnemyDied();
            yield break;
        }

        foreach (EnemyGroup group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(1f / Mathf.Max(0.01f, wave.spawnRate));
            }
        }

        currentWaveIndex++;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || enemyPrefab == null) return;

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemyPrefab, spawn.position, spawn.rotation);
    }

    public void OnEnemyDied()
    {
        if (gameIsOver) return;

        enemiesAlive--;

        if (enemiesAlive > 0) return;

        if (currentWaveIndex >= waves.Length)
            HandleWin();
        else
            StartCoroutine(WaveCompleteSequence());
    }

    IEnumerator WaveCompleteSequence()
    {
        if (waveCompleteSound) audioSource.PlayOneShot(waveCompleteSound);
        waveCompleteScreen?.SetActive(true);

        float t = 0f;
        while (t < waveCompleteDisplayTime)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        waveCompleteScreen?.SetActive(false);

        if (upgradeManager != null)
            upgradeManager.ShowUpgradeScreen();
        else
            StartCoroutine(SpawnNextWave());
    }

    public void StartNextWaveCoroutine()
    {
        StartCoroutine(SpawnNextWave());
    }

    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    public void HandleWin()
    {
        if (gameIsOver) return;
        gameIsOver = true;

        winScreen?.SetActive(true);
        SetScreenInteractable(winScreen, true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void HandleLose()
    {
        if (gameIsOver) return;
        gameIsOver = true;

        loseScreen?.SetActive(true);
        SetScreenInteractable(loseScreen, true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisableCombatScripts();

        Time.timeScale = 0f;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void SetScreenInteractable(GameObject screen, bool state)
    {
        if (!screen) return;

        CanvasGroup cg = screen.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = state;
            cg.blocksRaycasts = state;
        }

        Button[] buttons = screen.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
            btn.interactable = state;
    }

    void DisableCombatScripts()
    {
        GameObject playerObj = player ? player.gameObject : Object.FindFirstObjectByType<PlayerHealth>()?.gameObject;
        if (!playerObj) return;

        MonoBehaviour[] all = playerObj.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour mb in all)
        {
            if (!mb) continue;
            string n = mb.GetType().Name.ToLower();

            if (n.Contains("camera") || n.Contains("audiolistener") || n.Contains("canvas") ||
                n.Contains("ui") || n.Contains("render") || n.Contains("animator") ||
                n.Contains("audio") || n.Contains("health") || n.Contains("crystal"))
                continue;

            if (n.Contains("bow") || n.Contains("shoot") || n.Contains("attack") ||
                n.Contains("weapon") || n.Contains("playerinput") || n.Contains("input") ||
                n.Contains("aim") || n.Contains("fire"))
            {
                if (mb.enabled)
                {
                    mb.enabled = false;
                    disabledAttackScripts.Add(mb);
                }
            }
        }
    }

    void EnableCombatScripts()
    {
        foreach (var script in disabledAttackScripts)
        {
            if (script != null)
                script.enabled = true;
        }
        disabledAttackScripts.Clear();
    }

    void UpdateWaveUI(string text)
    {
        if (waveText) waveText.text = text;
    }

    public void RetryGame()
    {
        StartCoroutine(RetryAndReload());
    }

    IEnumerator RetryAndReload()
    {
        Time.timeScale = 1f;

        foreach (var mb in disabledAttackScripts)
        {
            if (mb != null)
            {
                try { mb.enabled = true; } catch { }
            }
        }
        disabledAttackScripts.Clear();
        gameIsOver = false;

        // Ensure controls panel never appears on retry
        controlsPanel.gameObject.SetActive(false);

        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(SCENE_NAME);
        while (!op.isDone)
            yield return null;
    }
}
