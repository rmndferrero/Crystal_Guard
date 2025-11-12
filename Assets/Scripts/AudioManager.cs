using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip winMusic;
    public AudioClip loseMusic;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    public float fadeDuration = 1f;

    private void Awake()
    {
        // Singleton pattern (make sure only one AudioManager exists)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Play menu music if the first scene is the Menu
        if (SceneManager.GetActiveScene().name.Contains("Menu"))
            PlayMusic(menuMusic);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Menu"))
            PlayMusic(menuMusic);
        else if (scene.name.Contains("Game"))
            PlayMusic(gameMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        StartCoroutine(FadeMusic(clip));
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        if (musicSource.isPlaying)
        {
            // Fade out
            for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(musicVolume, 0, t / fadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        // Switch clip and fade in
        musicSource.clip = newClip;
        musicSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, musicVolume, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = musicVolume;
    }

    // These can be called manually when game state changes
    public void PlayWinMusic() => PlayMusic(winMusic);
    public void PlayLoseMusic() => PlayMusic(loseMusic);
}
