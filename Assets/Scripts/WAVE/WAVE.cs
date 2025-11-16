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
    public GameObject surviveAllWavesScreen;
    public float waveCompleteDisplayTime = 2.5f;
    public AudioClip waveCompleteSound;
    public AudioClip startGameSound;
    public AudioClip surviveAllWavesSound;

    [Header("Start Game Screen")]
    public GameObject startScreenPanel;
    public CanvasGroup startScreenCanvas;
    public CanvasGroup blackFadeCanvas;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool gameIsOver = false;
    private bool gameStarted = false;

    private readonly List<MonoBehaviour> disabledAttackScripts = new();
    private const string SCENE_NAME = "Game";

    void Start()
    {
        upgradeManager = GetComponent<UpgradeManager>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
            player = Object.FindFirstObjectByType<PlayerHealth>();

        DisableMainUI();

        Time.timeScale = 0f;
        StartCoroutine(StartFadeInSequence());
    }

    void DisableMainUI()
    {
        if (winScreen) winScreen.SetActive(false);
        if (loseScreen) loseScreen.SetActive(false);
        if (waveCompleteScreen) waveCompleteScreen.SetActive(false);
        if (surviveAllWavesScreen) surviveAllWavesScreen.SetActive(false);
    }

    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetMouseButtonDown(0))
                StartCoroutine(BeginGameAfterClick());
            return;
        }

        if (gameIsOver) return;

        if (crystal == null || player == null)
            HandleLose();
    }

    IEnumerator StartFadeInSequence()
    {
        if (blackFadeCanvas)
            yield return StartCoroutine(FadeCanvas(blackFadeCanvas, 1f, 0f, 1.5f));
    }

    IEnumerator BeginGameAfterClick()
    {
        if (startGameSound) audioSource.PlayOneShot(startGameSound);
        gameStarted = true;

        yield return StartCoroutine(FadeCanvas(startScreenCanvas, 1f, 0f, 1f));
        startScreenPanel.SetActive(false);

        Time.timeScale = 1f;
        yield return StartCoroutine(ShowSurviveAllWaves());
        StartCoroutine(SpawnNextWave());
    }

    IEnumerator ShowSurviveAllWaves()
    {
        if (surviveAllWavesSound) audioSource.PlayOneShot(surviveAllWavesSound);

        yield return StartCoroutine(FadeCanvasGroupPopup(surviveAllWavesScreen));
    }

    IEnumerator SpawnNextWave()
    {
        if (currentWaveIndex >= waves.Length)
            yield break;

        Wave wave = waves[currentWaveIndex];
        UpdateWaveUI(wave.name);

        enemiesAlive = 0;
        foreach (EnemyGroup group in wave.enemyGroups)
            enemiesAlive += group.count;

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
        if (spawnPoints == null || spawnPoints.Length == 0 || enemyPrefab == null)
            return;

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
        yield return StartCoroutine(FadeCanvasGroupPopup(waveCompleteScreen));

        if (upgradeManager != null)
            upgradeManager.ShowUpgradeScreen();
        else
            StartCoroutine(SpawnNextWave());
    }

    public void StartNextWaveCoroutine() => StartCoroutine(SpawnNextWave());
    public int GetCurrentWaveIndex() => currentWaveIndex;

    public void HandleWin()
    {
        if (gameIsOver) return;
        gameIsOver = true;
        Time.timeScale = 0f;
        StartCoroutine(FadeCanvasGroupPopup(winScreen));
    }

    public void HandleLose()
    {
        if (gameIsOver) return;
        gameIsOver = true;
        DisableCombatScripts();
        Time.timeScale = 0f;
        StartCoroutine(FadeCanvasGroupPopup(loseScreen));
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float time)
    {
        float t = 0f;
        cg.alpha = from;

        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator FadeCanvasGroupPopup(GameObject obj)
    {
        obj.SetActive(true);
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        yield return StartCoroutine(FadeCanvas(cg, 0f, 1f, 0.6f));
        yield return new WaitForSecondsRealtime(waveCompleteDisplayTime);
        yield return StartCoroutine(FadeCanvas(cg, 1f, 0f, 0.6f));
        obj.SetActive(false);
    }

    void DisableCombatScripts()
    {
        GameObject playerObj = player ?
            player.gameObject :
            Object.FindFirstObjectByType<PlayerHealth>()?.gameObject;

        if (!playerObj) return;

        MonoBehaviour[] all = playerObj.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour mb in all)
        {
            if (!mb) continue;

            string n = mb.GetType().Name.ToLower();
            if (n.Contains("camera") || n.Contains("ui") ||
                n.Contains("audio") || n.Contains("health")) continue;

            if (n.Contains("bow") || n.Contains("shoot") || n.Contains("attack") ||
                n.Contains("weapon") || n.Contains("input") || n.Contains("aim"))
            {
                if (mb.enabled)
                {
                    mb.enabled = false;
                    disabledAttackScripts.Add(mb);
                }
            }
        }
    }

    void UpdateWaveUI(string text)
    {
        if (waveText)
            waveText.text = text;
    }
}
