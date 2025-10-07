using UnityEngine;

public class SpriteSpawner_NA : MonoBehaviour
{
    [Header("Distraction Sprites")]
    [SerializeField] [Tooltip("Array of regular distraction sprites that will be spawned multiple times")]
    private GameObject[] distractionSprites = new GameObject[5];
    
    [Header("Special Sprite")]
    [SerializeField] [Tooltip("The special sprite that is supposed to be hidden")]
    private GameObject specialSprite;
    
    [Header("Spawn Settings")]
    [SerializeField] [Tooltip("Margin from screen edges (in world units)")]
    private float screenMargin = 1f;
    
    [SerializeField] [Tooltip("Total number of regular sprites to spawn (distributed among the 5 regular sprites)")]
    private int totalCount = 10;
    
    [Header("Sprite Scaling")]
    [SerializeField] [Tooltip("Automatically scale sprites to match their box collider size")]
    private bool scaleToMatchCollider = true;
    
    [Header("Layer Swapping")]
    [SerializeField] [Tooltip("Enable random layer swapping to hide special sprite in crowd")]
    private bool enableLayerSwapping = true;
    
    [SerializeField] [Tooltip("Time interval between layer swaps (in seconds)")]
    private float swapInterval = 2f;
    
    [SerializeField] [Tooltip("Range of sorting order values (min to max)")]
    private int minSortingOrder = -10;
    
    [SerializeField] [Tooltip("Range of sorting order values (min to max)")]
    private int maxSortingOrder = 10;
    
    // screen size/boundaries
    private float minX, maxX, minY, maxY;
    
    private SpriteRenderer[] allSpriteRenderers;
    private float nextSwapTime;

    void Start()
    {
        CalculateScreenBounds();
        SpawnObjectsAtRandom();
        
        if (enableLayerSwapping)
        {
            InitializeLayerSwapping();
        }
    }
    
    void Update()
    {
        if (enableLayerSwapping && Time.time >= nextSwapTime)
        {
            SwapSpriteLayers();
            nextSwapTime = Time.time + swapInterval;
        }
    }
    
    private void CalculateScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("SpriteSpawner_NA: No main camera found!");
            return;
        }
        
        // get screen coordinates
        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));
        
        // screen margins
        minX = bottomLeft.x + screenMargin;
        maxX = topRight.x - screenMargin;
        minY = bottomLeft.y + screenMargin;
        maxY = topRight.y - screenMargin;
    }

    void SpawnObjectsAtRandom()
    {
        // check for real inputs
        if (distractionSprites.Length != 5)
        {
            Debug.LogError("SpriteSpawner_NA: Distraction sprites array must contain exactly 5 sprites!");
            return;
        }
        
        if (specialSprite == null)
        {
            Debug.LogError("SpriteSpawner_NA: Special sprite is not assigned!");
            return;
        }
        
        // check for null sprites in regular array
        for (int i = 0; i < distractionSprites.Length; i++)
        {
            if (distractionSprites[i] == null)
            {
                Debug.LogError($"SpriteSpawner_NA: Regular sprite at index {i} is null!");
                return;
            }
        }

        // spawn sprites
        SpawnDistractionSprites();
        SpawnSpecialSprite();
    }   

    private void SpawnDistractionSprites()
    {
        // calculate how many of each regular sprite to spawn
        int baseSpritesPerType = totalCount / distractionSprites.Length;
        int extraSprites = totalCount % distractionSprites.Length;
        
        for (int i = 0; i < distractionSprites.Length; i++)
        {
            // each sprite type gets the base amount, plus one extra if there are remainder sprites
            int spritesToSpawn = baseSpritesPerType + (i < extraSprites ? 1 : 0);
            
            for (int j = 0; j < spritesToSpawn; j++)
            {
                Vector3 randomPosition = GetRandomSpawnPosition();
                GameObject spawnedSprite = Instantiate(distractionSprites[i], randomPosition, Quaternion.identity);
                
                if (scaleToMatchCollider)
                {
                    ScaleSpriteToCollider(spawnedSprite);
                }
                
                // set initial random sorting order
                if (enableLayerSwapping)
                {
                    SetRandomSortingOrder(spawnedSprite);
                }
            }
        }
    }
    
    private void SpawnSpecialSprite()
    {
        Vector3 randomPosition = GetRandomSpawnPosition();
        GameObject spawnedSprite = Instantiate(specialSprite, randomPosition, Quaternion.identity);
        
        if (scaleToMatchCollider)
        {
            ScaleSpriteToCollider(spawnedSprite);
        }
        
        // set initial random sorting order for special sprite too
        if (enableLayerSwapping)
        {
            SetRandomSortingOrder(spawnedSprite);
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        return new Vector3(randomX, randomY, 0f);
    }
    
    private void ScaleSpriteToCollider(GameObject spriteObject)
    {
        SpriteRenderer spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        BoxCollider2D boxCollider = spriteObject.GetComponent<BoxCollider2D>();
        
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"No SpriteRenderer found on {spriteObject.name}. Cannot scale sprite.");
            return;
        }
        
        if (boxCollider == null)
        {
            Debug.LogWarning($"No BoxCollider2D found on {spriteObject.name}. Cannot scale to collider.");
            return;
        }
        
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        Vector2 colliderSize = boxCollider.size;
        Vector3 scaleFactor = new Vector3(colliderSize.x / spriteSize.x, colliderSize.y / spriteSize.y, 1f);
        
        // change the sprites local scale
        spriteObject.transform.localScale = scaleFactor;
        
        Debug.Log($"Scaled {spriteObject.name}: Sprite size {spriteSize} -> Collider size {colliderSize}, Scale factor: {scaleFactor}");
    }
    
    private void InitializeLayerSwapping()
    {
        // find all sprite renderers in the scene (spawned sprites)
        allSpriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        nextSwapTime = Time.time + swapInterval;
        
        Debug.Log($"Initialized layer swapping for {allSpriteRenderers.Length} sprites");
    }
    
    private void SetRandomSortingOrder(GameObject spriteObject)
    {
        SpriteRenderer renderer = spriteObject.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            int randomOrder = Random.Range(minSortingOrder, maxSortingOrder + 1);
            renderer.sortingOrder = randomOrder;
        }
    }
    
    private void SwapSpriteLayers()
    {
        // refresh the list to include any newly spawned sprites
        allSpriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        
        // randomize sorting orders for all sprites
        foreach (SpriteRenderer renderer in allSpriteRenderers)
        {
            if (renderer != null)
            {
                int newOrder = Random.Range(minSortingOrder, maxSortingOrder + 1);
                renderer.sortingOrder = newOrder;
            }
        }
        
        Debug.Log($"Swapped layers for {allSpriteRenderers.Length} sprites");
    }
}
