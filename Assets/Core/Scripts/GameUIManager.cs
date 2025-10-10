using UnityEngine;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        // hide UI elements at start
        if (finalScoreText != null)
            finalScoreText.gameObject.SetActive(false);
    }

    public void ShowFinalScore(int score)
    {
        StartCoroutine(DisplayFinalScoreSequence(score));
    }

    private IEnumerator DisplayFinalScoreSequence(int score)
    {            
        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(true);
            
            int totalSpritesSpawned = score * (5 + (score * 5)) / 2;
            string scoreText = $"<color=red>GAME OVER!</color>\n\n" +
                            $"Potion Found: {score} times\n" +
                            $"Total Sprites Spawned: {totalSpritesSpawned}";
            
            finalScoreText.text = scoreText;
            
            // fade in animation
            CanvasGroup canvasGroup = finalScoreText.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = finalScoreText.gameObject.AddComponent<CanvasGroup>();
                
            // fade in
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // keep it displayed for a while
        yield return new WaitForSeconds(displayDuration);
    }

    public void HideScoreDisplay()
    {
        if (finalScoreText != null)
            finalScoreText.gameObject.SetActive(false);
    }

    public float getDuration()
    {
        return displayDuration;
    }
}