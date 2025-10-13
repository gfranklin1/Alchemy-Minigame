using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpriteMovement_NA : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] 
    [Tooltip("Minimum movement speed/speed")]
    private float minSpeed = 5f;
    
    [SerializeField] 
    [Tooltip("Maximum movement speed/speed")]
    private float maxSpeed = 15f;
    
    private Rigidbody2D rb;
    private Vector2 direction;
    private float speed; 
    
    // screen boundaries
    private Camera cam;
    private Vector2 bottomLeft;
    private Vector2 topRight;

    private Vector2 spriteHalfSize;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        
        // assign random speed to this sprite
        speed = Random.Range(minSpeed, maxSpeed);
        
        // assign random direction
        float randomAngle = Random.Range(0f, 360f);
        direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;

        CalculateScreenBounds();
        CalculateSpriteSize();
    }

    void FixedUpdate()
    {
        // check for invalid direction
        if (direction.magnitude < 0.1f)
        {
            float randomAngle = Random.Range(0f, 360f);
            direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
        }

        // set velocity directly based on speed
        rb.linearVelocity = direction * speed;
        
        CheckScreenBoundaries();
    }

    void CheckScreenBoundaries()
    {
        if (cam == null) return;

        Vector3 spritePos = transform.position;
        bool bounced = false;

        // check horizontal boundaries
        if (spritePos.x - spriteHalfSize.x < bottomLeft.x)
        {
            direction.x = Mathf.Abs(direction.x); // positive X direction
            transform.position = new Vector3(bottomLeft.x + spriteHalfSize.x, spritePos.y, spritePos.z);
            bounced = true;
        }
        else if (spritePos.x + spriteHalfSize.x > topRight.x)
        {
            direction.x = -Mathf.Abs(direction.x); // negative X direction
            transform.position = new Vector3(topRight.x - spriteHalfSize.x, spritePos.y, spritePos.z);
            bounced = true;
        }

        // check vertical boundaries
        if (spritePos.y - spriteHalfSize.y < bottomLeft.y)
        {
            direction.y = Mathf.Abs(direction.y); // positive Y direction
            transform.position = new Vector3(spritePos.x, bottomLeft.y + spriteHalfSize.y, spritePos.z);
            bounced = true;
        }
        else if (spritePos.y + spriteHalfSize.y > topRight.y)
        {
            direction.y = -Mathf.Abs(direction.y); // negative Y direction
            transform.position = new Vector3(spritePos.x, topRight.y - spriteHalfSize.y, spritePos.z);
            bounced = true;
        }

        if (bounced)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void CalculateScreenBounds()
    {
        if (cam == null) return;

        bottomLeft = cam.ScreenToWorldPoint(Vector2.zero);
        topRight = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }

    private void CalculateSpriteSize()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // get the actual rendered size of the sprite (accounts for scaling)
            Bounds spriteBounds = spriteRenderer.bounds;
            spriteHalfSize = new Vector2(spriteBounds.size.x / 2f, spriteBounds.size.y / 2f);
            
            // Debug.Log($"{gameObject.name} sprite half-size calculated: {spriteHalfSize}");
        }
        else
        {
            // fallback if no sprite renderer
            spriteHalfSize = new Vector2(0.5f, 0.5f);
        }
    }

    public void RecalculateSpriteSize()
    {
        CalculateSpriteSize();
    }

    public float GetMovementSpeed()
    {
        return speed;
    }

    public void SetMovementSpeed(float newSpeed)
    {
        speed = Mathf.Clamp(newSpeed, 0f, 50f); // Reasonable limits
        // Debug.Log($"{gameObject.name} speed changed to: {speed:F1}");
    }
}