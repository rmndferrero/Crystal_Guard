using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class CardHoverEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Hover Settings")]
    public float hoverScale = 1.1f;
    public float tiltAmount = 10f;
    public float lerpSpeed = 12f;

    [Header("Frame Highlight")]
    public Image frame;
    public float highlightAlpha = 1f;
    public float normalAlpha = 0.7f;

    // --- NEW ---
    [Header("Sound")]
    public AudioClip hoverSound;
    private AudioSource audioSource;
    // --- END NEW ---

    private RectTransform rect;
    private Vector3 originalScale;
    private bool hovered = false;
    private Vector2 pointerPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;

        if (frame != null)
        {
            Color c = frame.color;
            c.a = normalAlpha;
            frame.color = c;
        }

        // --- ADD THIS ---
        // Get the AudioSource component (the "speaker")
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0; // Make it a 2D sound
        // --- END ADD ---
    }

    void Update()
    {
        // --- ADD THIS ---
        // This forces the sound to play at normal speed
        // even when the game is in slow-motion (Time.timeScale is low)
        if (audioSource.isPlaying)
        {
            audioSource.pitch = 1f / Time.timeScale;
        }
        else
        {
            audioSource.pitch = 1f;
        }
        // --- END ADD ---


        if (!hovered)
        {
            rect.localScale = Vector3.Lerp(rect.localScale, originalScale, Time.unscaledDeltaTime * lerpSpeed);
            rect.rotation = Quaternion.Lerp(rect.rotation, Quaternion.identity, Time.unscaledDeltaTime * lerpSpeed);
            return;
        }

        rect.localScale = Vector3.Lerp(rect.localScale, originalScale * hoverScale, Time.unscaledDeltaTime * lerpSpeed);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            pointerPos,
            null,
            out Vector2 localPoint
        );

        float xTilt = (localPoint.y / rect.sizeDelta.y) * tiltAmount;
        float yTilt = -(localPoint.x / rect.sizeDelta.x) * tiltAmount;

        Quaternion targetRot = Quaternion.Euler(xTilt, yTilt, 0);
        rect.rotation = Quaternion.Lerp(rect.rotation, targetRot, Time.unscaledDeltaTime * lerpSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        pointerPos = eventData.position;

        if (frame != null)
        {
            Color c = frame.color;
            c.a = highlightAlpha;
            frame.color = c;
        }

        // --- ADD THIS ---
        // Play the sound
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
        // --- END ADD ---
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;

        if (frame != null)
        {
            Color c = frame.color;
            c.a = normalAlpha;
            frame.color = c;
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        pointerPos = eventData.position;
    }
}