using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

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

    [Header("Wave Settings")]
    public Wave[] waves;
    public Transform[] spawnPoints;

    [Header("References")]
    public CrystalHealth crystal;
    public PlayerHealth player;
    public TextMeshProUGUI waveText;
    public GameObject winScreen;
    public GameObject loseScreen;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;
    private bool gameIsOver = false;
    private UpgradeManager upgradeManager;

    void Start()
    {
        upgradeManager = GetComponent<UpgradeManager>();

        if (winScreen) winScreen.SetActive(false);
        if (loseScreen) loseScreen.SetActive(false);
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();

        Time.timeScale = 1f;

        // ✅ Always start at wave 0 explicitly (so indexing is reliable)
        currentWaveIndex = 0;
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
        // ✅ Prevent index out of range
        if (currentWaveIndex >= waves.Length)
        {
            HandleWin();
            yield break;
        }

        Wave wave = waves[currentWaveIndex];
        UpdateWaveUI(wave.name);

        // Count total enemies for this wave
        enemiesAlive = 0;
        foreach (EnemyGroup group in wave.enemyGroups)
            enemiesAlive += group.count;

        // Failsafe for empty wave
        if (enemiesAlive <= 0)
        {
            OnEnemyDied();
            yield break;
        }

        // ✅ Randomize spawn order
        List<GameObject> spawnList = new List<GameObject>();
        foreach (EnemyGroup group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
                spawnList.Add(group.enemyPrefab);
        }

        for (int i = 0; i < spawnList.Count; i++)
        {
            int rand = Random.Range(i, spawnList.Count);
            (spawnList[i], spawnList[rand]) = (spawnList[rand], spawnList[i]);
        }

        // ✅ Spawn enemies in randomized order
        foreach (GameObject enemyPrefab in spawnList)
        {
            SpawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(1f / wave.spawnRate);
        }

        // ✅ Wave complete: increment AFTER spawn finishes
        currentWaveIndex++;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Length == 0) return;
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;

        if (enemiesAlive > 0) return;

        // ✅ All enemies defeated
        if (currentWaveIndex >= waves.Length)
        {
            HandleWin();
        }
        else
        {
            // ✅ Wait a brief moment for clarity before showing upgrades
            StartCoroutine(HandleWaveCompletion());
        }
    }

    IEnumerator HandleWaveCompletion()
    {
        yield return new WaitForSeconds(0.5f);

        if (upgradeManager != null)
        {
            upgradeManager.ShowUpgradeScreen();
        }
        else
        {
            StartCoroutine(SpawnNextWave());
        }
    }

    // Called by UpgradeManager after choosing an upgrade
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

        AudioManager.Instance?.PlayWinMusic();
        if (winScreen) winScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HandleLose()
    {
        if (gameIsOver) return;
        gameIsOver = true;

        AudioManager.Instance?.PlayLoseMusic();
        if (loseScreen) loseScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    void UpdateWaveUI(string text)
    {
        if (waveText != null)
            waveText.text = text;
    }
}
