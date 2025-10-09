using UnityEngine;
using UnityEngine.InputSystem;

public class Clicker : MonoBehaviour
{
    [SerializeField] GameObject luigi;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Use simple mouse detection instead of Input System
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        // Get mouse position and convert to world coordinates
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f; // Ensure Z is 0 for 2D

        // Try detection methods with smaller, more precise areas
        Collider2D hitCollider = null;
        
        // Method 1: Direct point overlap
        hitCollider = Physics2D.OverlapPoint(worldPos);
        
        // Method 2: If no hit, try a very small circle (just for minor inaccuracies)
        if (hitCollider == null)
        {
            hitCollider = Physics2D.OverlapCircle(worldPos, .3f);
            if (hitCollider != null)
            {
                Debug.Log("Found collider using small circle detection (radius 0.1)");
            }
        }
        
        if (hitCollider == null) 
        {
            Debug.Log("No collider hit - click more precisely on the sprite");
            return;
        }

        Debug.Log("Hit collider: " + hitCollider.gameObject.name);

        if (!MinigameManager.IsReady()) 
        {
            Debug.Log("MinigameManager not ready");
            return;
        }

        string targetName = luigi.name.Contains("(Clone)") ? luigi.name : luigi.name + "(Clone)";
        
        if (hitCollider.gameObject.name == targetName || hitCollider.gameObject.name == luigi.name)
        {
            MinigameManager.SetStateToSuccess();
            MinigameManager.EndGame();
        }
    }
}