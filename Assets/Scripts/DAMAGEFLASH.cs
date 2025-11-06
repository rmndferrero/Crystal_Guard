using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class DamageVignette : MonoBehaviour
{
    private Image vignetteImage;
    private Coroutine flashCoroutine;

    void Awake()
    {
        vignetteImage = GetComponent<Image>();
        vignetteImage.color = new Color(1f, 1f, 1f, 0f); // Start invisible
    }

    public void Flash(Color flashColor)
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(DoFlash(flashColor));
    }

    private IEnumerator DoFlash(Color flashColor)
    {
        float duration = 0.5f;
        float fadeOutTime = 0.3f;
        float peakAlpha = 0.6f;

        // Set color and fade in
        vignetteImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        float timer = 0f;
        while (timer < (duration - fadeOutTime))
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, peakAlpha, timer / (duration - fadeOutTime));
            vignetteImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // Fade out
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(peakAlpha, 0, timer / fadeOutTime);
            vignetteImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        vignetteImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }
}