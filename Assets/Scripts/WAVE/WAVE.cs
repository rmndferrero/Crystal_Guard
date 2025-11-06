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

    public Wave[] waves;
    public Transform[] spawnPoints;
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
        Wave wave = waves[currentWaveIndex];
        UpdateWaveUI(wave.name);

        // Count all enemies
        enemiesAlive = 0;
        foreach (EnemyGroup group in wave.enemyGroups)
            enemiesAlive += group.count;

        // Failsafe for empty waves
        if (enemiesAlive == 0)
        {
            OnEnemyDied();
            yield break;
        }

        // ✅ Build a randomized spawn list of all enemies from all groups
        List<GameObject> spawnList = new List<GameObject>();
        foreach (EnemyGroup group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
                spawnList.Add(group.enemyPrefab);
        }

        // ✅ Shuffle the list (Fisher-Yates)
        for (int i = 0; i < spawnList.Count; i++)
        {
            int rand = Random.Range(i, spawnList.Count);
            (spawnList[i], spawnList[rand]) = (spawnList[rand], spawnList[i]);
        }

        // ✅ Spawn in randomized order
        foreach (GameObject enemyPrefab in spawnList)
        {
            SpawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(1f / wave.spawnRate);
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
            // --- THIS IS THE FIX ---
            // We show the upgrade screen INSTEAD of starting the next wave.
            if (upgradeManager != null)
            {
                upgradeManager.ShowUpgradeScreen();
            }
            else
            {
                // Failsafe if you forgot the upgrade manager
                StartCoroutine(SpawnNextWave());
            }
        }
    }

    // This is called by the UpgradeManager after an upgrade is chosen
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