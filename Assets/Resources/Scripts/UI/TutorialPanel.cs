using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
    public CanvasGroup tutorialCanvasGroup; 
    public float fadeDuration = 2f;          
    public float displayTime = 5f;           

    void Start()
    {
        // Set the alpha to 1 (fully visible) at the start
        tutorialCanvasGroup.alpha = 1f;

        // Start fading out after the specified time
        Invoke("StartFadeOut", displayTime);
    }

    // Method to start fading out the tutorial panel
    private void StartFadeOut()
    {
        // Start the fade-out effect over the specified duration
        StartCoroutine(FadeOut());
    }

    // Coroutine to fade out the canvas group over time
    private IEnumerator FadeOut()
    {
        float startAlpha = tutorialCanvasGroup.alpha;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            // Gradually decrease alpha based on the time elapsed
            tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the alpha is set to 0 (fully transparent) at the end
        tutorialCanvasGroup.alpha = 0f;
    }
}
