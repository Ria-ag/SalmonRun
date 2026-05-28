using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebrisSweep : MonoBehaviour
{
    public static DebrisSweep Instance;

    [Header("Debris Asset")]
    [SerializeField] private Sprite debrisSprite;
    
    [Header("Fixed Position Nodes")]
    [Tooltip("Spawns exactly in the top dirty river bend")]
    [SerializeField] private Vector3 topStationaryPosition = new Vector3(-0.5f, 3.2f, 0f); 
    
    [Tooltip("Spawns exactly in the bottom dirty river bend")]
    [SerializeField] private Vector3 bottomStationaryPosition = new Vector3(0.5f, -3.2f, 0f);

    [Header("Timing & Speed")]
    [SerializeField] private float minSpawnDelay = 2.0f;
    [SerializeField] private float maxSpawnDelay = 5.0f;
    [SerializeField] private float swipeClearSpeed = 18f;

    // Separate lists to track items currently resting at the top vs bottom positions
    private List<GameObject> activeTopDebris = new List<GameObject>();
    private List<GameObject> activeBottomDebris = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Start the spontaneous generation loop
        StartCoroutine(SpawnStaticDebrisRoutine());
    }

    // --- SPONTANEOUS GENERATION LOOP ---
    private IEnumerator SpawnStaticDebrisRoutine()
    {
        while (true)
        {
            float randomWait = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(randomWait);

            // Random choice: 50% chance Top position, 50% chance Bottom position
            bool spawnAtTop = Random.value > 0.5f;
            
            CreateStaticDebris(spawnAtTop);
        }
    }

    private void CreateStaticDebris(bool isTopLocation)
    {
        // Don't stack infinite items if one is already sitting there blocking the path
        if (isTopLocation && activeTopDebris.Count > 0) return;
        if (!isTopLocation && activeBottomDebris.Count > 0) return;

        GameObject debrisItem = new GameObject("Static_Debris_Obstacle");
        SpriteRenderer renderer = debrisItem.AddComponent<SpriteRenderer>();
        
        if (debrisSprite != null)
        {
            renderer.sprite = debrisSprite;
        }
        else
        {
            // Debug block so you can still test if the sprite asset slot unbinds
            debrisItem.AddComponent<BoxCollider2D>();
        }

        // Place it directly at its target coordinate node and leave it there (No drifting down!)
        if (isTopLocation)
        {
            debrisItem.transform.position = topStationaryPosition;
            activeTopDebris.Add(debrisItem);
        }
        else
        {
            debrisItem.transform.position = bottomStationaryPosition;
            activeBottomDebris.Add(debrisItem);
        }
    }

    // --- TRIGGERED BY PYTHON GESTURE INTERFACE ---
    
    // Left Wave = Clears TOP debris by flying UP
    public void ClearLeftPollutants()
    {
        Debug.Log("⚡ [CLEANUP] Left Wave Detected: Blasting Top Debris UP!");
        foreach (GameObject debris in activeTopDebris)
        {
            if (debris != null)
            {
                StartCoroutine(AnimateSwipeAway(debris, Vector3.up));
            }
        }
        activeTopDebris.Clear(); 
    }

    // Right Wave = Clears BOTTOM debris by flying DOWN
    public void ClearRightPollutants()
    {
        Debug.Log("⚡ [CLEANUP] Right Wave Detected: Blasting Bottom Debris DOWN!");
        foreach (GameObject debris in activeBottomDebris)
        {
            if (debris != null)
            {
                StartCoroutine(AnimateSwipeAway(debris, Vector3.down));
            }
        }
        activeBottomDebris.Clear();
    }

    private IEnumerator AnimateSwipeAway(GameObject targetDebris, Vector3 sweepDirection)
    {
        float duration = 1.0f; 
        float elapsed = 0f;

        while (elapsed < duration && targetDebris != null)
        {
            // Instantly accelerates up or down out of bounds
            targetDebris.transform.Translate(sweepDirection * swipeClearSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null; 
        }

        if (targetDebris != null)
        {
            Destroy(targetDebris);
        }
    }
}