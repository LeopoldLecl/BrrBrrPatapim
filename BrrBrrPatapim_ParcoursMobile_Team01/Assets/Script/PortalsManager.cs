using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PortalsManager : MonoBehaviour
{
    public static PortalsManager Instance;
    
    [SerializeField] private List<GameObject> portalsList = new List<GameObject>();
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistanceAhead = 20f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 2f;
    [SerializeField] private float despawnDistanceBehind = 10f;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float spawnInterval = 2f; // Time between spawns

    private List<GameObject> portalPool = new List<GameObject>();
    private List<GameObject> activePortals = new List<GameObject>();
    private List<GameObject> weightedPortalsList = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("PortalsManager started");
        // Build weighted list
        weightedPortalsList.Clear();
        if (portalsList.Count > 0)
        {
            // Add first prefab 10 times
            for (int i = 0; i < 10; i++)
                weightedPortalsList.Add(portalsList[0]);
            // Add the rest once
            for (int i = 1; i < portalsList.Count; i++)
                weightedPortalsList.Add(portalsList[i]);
        }


    }

    public void StartGame()
    {
        StartCoroutine(SpawnPortalsCoroutine());
        
        // Initialize pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = weightedPortalsList[Random.Range(0, weightedPortalsList.Count)];
            GameObject portal = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            portal.SetActive(false);
            portalPool.Add(portal);
        }
    }
    
    public void StopGame()
    {
        StopAllCoroutines();
        foreach (var portal in activePortals)
        {
            portal.SetActive(false);
            portalPool.Add(portal);
        }
        activePortals.Clear();
    }

    private IEnumerator SpawnPortalsCoroutine()
    {
        while (true)
        {
            SpawnPortal();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void Update()
    {
        // Despawn portals behind player
        for (int i = activePortals.Count - 1; i >= 0; i--)
        {
            if (activePortals[i].transform.position.x < player.position.x - despawnDistanceBehind)
            {
                activePortals[i].SetActive(false);
                portalPool.Add(activePortals[i]);
                activePortals.RemoveAt(i);
            }
        }
    }

    private void SpawnPortal()
    {
        if (portalPool.Count == 0) return;

        GameObject portal = portalPool[0];
        portalPool.RemoveAt(0);

        float y = Random.Range(player.transform.position.y + minY,player.transform.position.y + maxY);
        Vector3 spawnPos = new Vector3(player.position.x + spawnDistanceAhead, y, player.position.z);

        // Pick prefab for appearance (if you want to randomize type on spawn)
        // GameObject prefab = weightedPortalsList[Random.Range(0, weightedPortalsList.Count)];
        // Optionally, set up portal here if needed

        portal.transform.position = spawnPos;
        portal.SetActive(true);
        activePortals.Add(portal);
    }
}