using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string name;
        public GameObject enemyPrefab;
        public int count;
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

    void Start()
    {
        if (winScreen) winScreen.SetActive(false);
        if (loseScreen) loseScreen.SetActive(false);
        if (player == null) player = FindObjectOfType<PlayerHealth>();

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
        enemiesAlive = wave.count;

        for (int i = 0; i < wave.count; i++)
        {
            SpawnEnemy(wave.enemyPrefab);
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

        if (enemiesAlive == 0 && currentWaveIndex == waves.Length)
        {
            HandleWin();
        }
        else if (enemiesAlive == 0 && currentWaveIndex < waves.Length)
        {
            StartCoroutine(SpawnNextWave());
        }
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