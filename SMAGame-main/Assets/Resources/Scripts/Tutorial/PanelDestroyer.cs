using UnityEngine;
using System.Collections;

public class FadePanelDestroyer : MonoBehaviour
{
    [SerializeField] private GameObject panelToDestroy;
    [SerializeField] private string triggerTag = "Player";
    [SerializeField] private float fadeOutDuration = 2f;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag) && panelToDestroy != null)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(triggerTag) && panelToDestroy != null)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }
    
    private IEnumerator FadeOutAndDestroy()
    {
        CanvasGroup canvasGroup = panelToDestroy.GetComponent<CanvasGroup>();
        
        if (canvasGroup != null)
        {
            float startAlpha = canvasGroup.alpha;
            float timeElapsed = 0f;
            
            while (timeElapsed < fadeOutDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timeElapsed / fadeOutDuration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }
        
        Destroy(panelToDestroy);
        Destroy(gameObject);
    }
}