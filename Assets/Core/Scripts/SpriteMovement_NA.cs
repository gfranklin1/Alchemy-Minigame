using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpriteMovement_NA : MonoBehaviour
{
   Rigidbody2D rb;
   Vector2 direction;
   public float force = 10f;

   void Start()
   {
       // random direction for each sprite
       float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
       direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
       
       rb = GetComponent<Rigidbody2D>();
       
    //    Debug.Log($"Sprite {gameObject.name} starting with direction: {direction}");
   }
   void FixedUpdate()
   {
       // ensure direction is normalized and not zero
       if (direction.magnitude < 0.1f)
       {
        //    Debug.LogWarning($"Sprite {gameObject.name} had invalid direction, resetting to random direction");
           float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
           direction = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
       }
       
       // Check screen boundaries every frame and bounce if needed
       CheckScreenBoundaries();
       
       rb.linearVelocity = direction * force;
   }
   
   void CheckScreenBoundaries()
   {
       Camera cam = Camera.main;
       if (cam == null) return;
       
       Vector3 spritePos = transform.position;
       Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
       Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));       
       // Check left boundary
       if (spritePos.x < bottomLeft.x)
       {
           direction.x = Mathf.Abs(direction.x); // Force positive X direction
           transform.position = new Vector3(bottomLeft.x, spritePos.y, spritePos.z);
       }
       // Check right boundary
       else if (spritePos.x > topRight.x)
       {
           direction.x = -Mathf.Abs(direction.x); // Force negative X direction
           transform.position = new Vector3(topRight.x, spritePos.y, spritePos.z);
       }
       
       // Check bottom boundary
       if (spritePos.y < bottomLeft.y)
       {
           direction.y = Mathf.Abs(direction.y); // Force positive Y direction
           transform.position = new Vector3(spritePos.x, bottomLeft.y, spritePos.z);
       }
       // Check top boundary
       else if (spritePos.y > topRight.y)
       {
           direction.y = -Mathf.Abs(direction.y); // Force negative Y direction
           transform.position = new Vector3(spritePos.x, topRight.y, spritePos.z);
       }
   }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ScreenBoundary_NA"))
        {
            // calculate bounce direction based on screen edge
            Vector3 spritePos = transform.position;
            
            // determine which edge we hit by comparing positions
            Vector2 normal = Vector2.zero;
            
            // get screen bounds for comparison
            Camera cam = Camera.main;
            Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
            Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));
            
            // find the hit edge
            if (spritePos.x <= bottomLeft.x + 0.1f) // left edge
                normal = Vector2.right;
            else if (spritePos.x >= topRight.x - 0.1f) // right edge
                normal = Vector2.left;
            else if (spritePos.y <= bottomLeft.y + 0.1f) // bottom edge
                normal = Vector2.up;
            else if (spritePos.y >= topRight.y - 0.1f) // top edge
                normal = Vector2.down;
            
            // reflect direction
            if (normal != Vector2.zero)
            {
                direction = Vector2.Reflect(direction, normal).normalized;
            }
        }
    }
}


