using UnityEngine;
using UnityEngine.InputSystem;

public class Clicker : MonoBehaviour
{
    [SerializeField] GameObject luigi;
    [SerializeField] float ClickRadius = 0.4f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        // get mouse position and convert to world coordinates
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f; // Ensure Z is 0 for 2D

        // try detection methods with smaller, more precise areas
        Collider2D hitCollider = null;
        
        // direct point overlap
        hitCollider = Physics2D.OverlapPoint(worldPos);
        
        // if no hit, try a very small circle (just for minor inaccuracies)
        if (hitCollider == null)
        {
            hitCollider = Physics2D.OverlapCircle(worldPos, ClickRadius);
        }

        if (hitCollider == null) return;
        // Debug.Log("Hit collider: " + hitCollider.gameObject.name);

        if (!MinigameManager.IsReady()) 
        {
            Debug.Log("MinigameManager not ready");
            return;
        }

        string targetName = luigi.name.Contains("(Clone)") ? luigi.name : luigi.name + "(Clone)";
        
        if (hitCollider.gameObject.name == targetName || hitCollider.gameObject.name == luigi.name)
        {
            // MinigameManager.SetStateToSuccess();
            // MinigameManager.EndGame();

            SpriteSpawner_NA spawner = Object.FindFirstObjectByType<SpriteSpawner_NA>();
            if (spawner != null)
            {
                spawner.OnLuigiFound(hitCollider.gameObject);
            }
        }
    }
}