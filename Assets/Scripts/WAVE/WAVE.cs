using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

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

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool gameIsOver = false;

    void Start()
    {
        upgradeManager = GetComponent<UpgradeManager>();
        audioSource = GetComponent<AudioSource>();

        if (winScreen) winScreen.SetActive(false);
        if (loseScreen) loseScreen.SetActive(false);
        if (waveCompleteScreen) waveCompleteScreen.SetActive(false);
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();

        Time.timeScale = 1f;
        StartCoroutine(SpawnNextWave());
    }

    void Update()
    {
        if (gameIsOver) return;

        if (crystal == null || player == null)
        {
            HandleLose();
        }
    }

    IEnumerator SpawnNextWave()
    {
        if (currentWaveIndex >= waves.Length) yield break;

        Wave wave = waves[currentWaveIndex];
        UpdateWaveUI(wave.name);

        enemiesAlive = 0;
        foreach (EnemyGroup group in wave.enemyGroups)
        {
            enemiesAlive += group.count;
        }

        if (enemiesAlive == 0)
        {
            OnEnemyDied();
            yield break;
        }

        foreach (EnemyGroup group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(1f / wave.spawnRate);
            }
        }

        currentWaveIndex++;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Length == 0) return;
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;

        if (enemiesAlive > 0) return;

        if (enemiesAlive == 0 && currentWaveIndex == waves.Length)
        {
            HandleWin();
        }
        else if (enemiesAlive == 0 && currentWaveIndex < waves.Length)
        {
            StartCoroutine(WaveCompleteSequence());
        }
    }

    IEnumerator WaveCompleteSequence()
    {
        if (waveCompleteSound != null)
        {
            audioSource.PlayOneShot(waveCompleteSound);
        }

        if (waveCompleteScreen != null)
        {
            waveCompleteScreen.SetActive(true);
        }

        yield return new WaitForSeconds(waveCompleteDisplayTime);

        if (waveCompleteScreen != null)
        {
            waveCompleteScreen.SetActive(false);
        }

        if (upgradeManager != null)
        {
            upgradeManager.ShowUpgradeScreen();
        }
        else
        {
            StartCoroutine(SpawnNextWave());
        }
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
        if (winScreen) winScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HandleLose()
    {
        if (gameIsOver) return;
        gameIsOver = true;
        if (loseScreen) loseScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    void UpdateWaveUI(string text)
    {
        if (waveText != null)
        {
            waveText.text = text;
        }
    }
}