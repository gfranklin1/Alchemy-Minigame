using System.Collections;
using UnityEngine;

public class SpriteSpawner_NA : MonoBehaviour, MinigameSubscriber_NA
{
    [Header("Distraction Sprites")]
    [SerializeField]
    [Tooltip("Array of regular distraction sprites that will be spawned multiple times")]
    private GameObject[] distractionSprites = new GameObject[5];

    [Header("Special Sprite")]
    [SerializeField]
    [Tooltip("The special sprite that is supposed to be hidden")]
    private GameObject specialSprite;

    [Header("Audio")]
    [SerializeField]
    [Tooltip("Audio controller for win sound effect")]
    private AudioController_NA winSFXController;


    [Header("Spawn Settings")]
    [SerializeField]
    [Tooltip("Margin from screen edges (in world units)")]
    private float screenMargin = 1f;

    [Header("Progressive Difficulty")]
    [SerializeField]
    [Tooltip("Score increase required to unlock next distraction sprite type")]
    private int scoreThreshold = 2;

    [SerializeField]
    [Tooltip("Multiplier for total sprites spawned (totalSprites = multiplier * score)")]
    private int spawnMultiplier = 5;

    [Header("Sprite Scaling")]
    [SerializeField]
    [Tooltip("Automatically scale sprites to match their box collider size")]
    private bool scaleToMatchCollider = true;

    [Header("Layer Swapping")]
    [SerializeField]
    [Tooltip("Enable random layer swapping to hide special sprite in crowd")]
    private bool enableLayerSwapping = true;

    [SerializeField]
    [Tooltip("Time interval between layer swaps (in seconds)")]
    private float swapInterval = 2f;

    [SerializeField]
    [Tooltip("Range of sorting order values (min to max)")]
    private int minSortingOrder = -10;

    [SerializeField]
    [Tooltip("Range of sorting order values (min to max)")]
    private int maxSortingOrder = 10;

    [Header("UI")]
    [SerializeField]
    [Tooltip("UI Manager for displaying final score")]
    private GameUIManager_NA uiManager;


    [Header("Game Loop Settings")]
    [SerializeField]
    [Tooltip("Time to wait before deleting the special sprite after being found")]    
    private float luigiDeleteDelay = 1f;

    private int score = 0;


    // screen size/boundaries
    private float minX, maxX, minY, maxY;

    private SpriteRenderer[] allSpriteRenderers;
    private float nextSwapTime;
    private bool gameLoopInProgress = false;

    public void OnMinigameStart()
    {
        SpawnObjectsAtRandom();
    }

    public void OnTimerEnd()
    {
        if (uiManager != null)
        {
            uiManager.ShowFinalScore(score);
        }
    
        StartCoroutine(EndGame());
        
    }

    private IEnumerator EndGame()
    {
        // Debug.Log("Final Score: " + score);

        DeleteAllSprites();

        yield return new WaitForSeconds(uiManager.getDuration() + 3);

        MinigameManager_NA.SetStateToSuccess();
        MinigameManager_NA.EndGame();
    }

    private void DeleteAllSprites()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int deletedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("(Clone)"))
            {
                bool isOurSprite = false;
                
                // check if it's a distraction sprite
                foreach (GameObject distractionPrefab in distractionSprites)
                {
                    if (distractionPrefab != null && obj.name.StartsWith(distractionPrefab.name))
                    {
                        isOurSprite = true;
                        break;
                    }
                }
                
                // check if it's the special sprite
                if (!isOurSprite && specialSprite != null && obj.name.StartsWith(specialSprite.name))
                {
                    isOurSprite = true;
                }
                
                if (isOurSprite)
                {
                    Destroy(obj);
                    deletedCount++;
                }
            }
        }
    }

    void Start()
    {
        MinigameManager_NA.Subscribe(this);

        CalculateScreenBounds();

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

    private int GetTotalSpritesToSpawn()
    {
        int totalSprites = score * (spawnMultiplier + (score * spawnMultiplier)) / 2;
        return totalSprites;
    }

    private int GetActiveDistractionTypes()
    {
        int activeTypes = 1 + (score / scoreThreshold);

        return Mathf.Min(activeTypes, distractionSprites.Length);
    }

    private void CalculateScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            //Debug.LogError("SpriteSpawner_NA: No main camera found!");
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
        int activeTypes = GetActiveDistractionTypes();
        int totalSprites = GetTotalSpritesToSpawn();

        // check for null sprites in regular array
        for (int i = 0; i < activeTypes; i++)
        {
            if (distractionSprites[i] == null)
            {
                //Debug.LogError($"SpriteSpawner_NA: Regular sprite at index {i} is null!");
                return;
            }
        }

        // spawn sprites
        SpawnDistractionSprites(activeTypes, totalSprites);
        SpawnSpecialSprite();
    }

    private void SpawnDistractionSprites(int activeTypes, int totalSprites)
    {
        // calculate how many of each regular sprite to spawn
        int baseSpritesPerType = totalSprites / activeTypes;
        int extraSprites = totalSprites % activeTypes;

        for (int i = 0; i < activeTypes; i++)
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
            //Debug.LogWarning($"No SpriteRenderer found on {spriteObject.name}. Cannot scale sprite.");
            return;
        }

        if (boxCollider == null)
        {
            //Debug.LogWarning($"No BoxCollider2D found on {spriteObject.name}. Cannot scale to collider.");
            return;
        }

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        Vector2 colliderSize = boxCollider.size;
        Vector3 scaleFactor = new Vector3(colliderSize.x / spriteSize.x, colliderSize.y / spriteSize.y, 1f);

        // change the sprites local scale
        spriteObject.transform.localScale = scaleFactor;

        //Debug.Log($"Scaled {spriteObject.name}: Sprite size {spriteSize} -> Collider size {colliderSize}, Scale factor: {scaleFactor}");
    }

    public void OnLuigiFound(GameObject luigiObject)
    {
        if (gameLoopInProgress) return;

        if (winSFXController != null)
        {
            winSFXController.PlayAudio();
        }

        int prevActiveTypes = GetActiveDistractionTypes();
        score++;
        int newActiveTypes = GetActiveDistractionTypes();

        if (newActiveTypes > prevActiveTypes)
        {
            // new distraction type unlocked
        }

        StartCoroutine(GameLoopSequence(luigiObject));
    }

    private IEnumerator GameLoopSequence(GameObject luigiObject)
    {
        gameLoopInProgress = true;

        SpriteMovement_NA luigiMovement = luigiObject.GetComponent<SpriteMovement_NA>();
        if (luigiMovement != null)
        {
            luigiMovement.enabled = false;
        }

        Rigidbody2D luigiRigidbody = luigiObject.GetComponent<Rigidbody2D>();
        if (luigiRigidbody != null)
        {
            luigiRigidbody.linearVelocity = Vector2.zero;
            luigiRigidbody.angularVelocity = 0f;
            luigiRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        DeleteDistractionSprites(luigiObject);

        yield return new WaitForSeconds(luigiDeleteDelay);

        if (luigiObject != null)
        {
            Destroy(luigiObject);
        }

        SpawnObjectsAtRandom();

        if (enableLayerSwapping)
        {
            InitializeLayerSwapping();
        }

        gameLoopInProgress = false;
    }

    private void DeleteDistractionSprites(GameObject luigiObject)
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int deletedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj == luigiObject)
            {
                continue;
            }

            if (obj.name.Contains("(Clone)"))
            {
                bool isDistractionSprite = false;
                foreach (GameObject distractionPrefab in distractionSprites)
                {
                    if (distractionPrefab != null && obj.name.StartsWith(distractionPrefab.name))
                    {
                        isDistractionSprite = true;
                        break;
                    }
                }

                if (isDistractionSprite)
                {
                    Destroy(obj);
                    deletedCount++;
                }
            }
        }

        // Debug.Log($"Deleted {deletedCount} sprites");
    }

    private void InitializeLayerSwapping()
    {
        // find all sprite renderers in the scene (spawned sprites)
        allSpriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        nextSwapTime = Time.time + swapInterval;

        //Debug.Log($"Initialized layer swapping for {allSpriteRenderers.Length} sprites");
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

        //Debug.Log($"Swapped layers for {allSpriteRenderers.Length} sprites");
    }

    public int getScore()
    {
        return score;
    }

}
