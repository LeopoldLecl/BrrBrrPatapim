using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupsManager : MonoBehaviour
{
    public static PowerupsManager Instance;

    [SerializeField] private List<GameObject> powerupPrefabs;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistanceAhead = 20f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 2f;
    [SerializeField] private float despawnDistanceBehind = 10f;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float spawnInterval = 2f;

    private List<GameObject> powerupPool = new List<GameObject>();
    private List<GameObject> activePowerups = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializePool();
        StartGame();
    }

    public void StartGame()
    {
        StartCoroutine(SpawnPowerupsCoroutine());
    }

    public void StopGame()
    {
        StopAllCoroutines();
        foreach (var powerup in activePowerups)
        {
            powerup.SetActive(false);
            powerupPool.Add(powerup);
        }
        activePowerups.Clear();
    }

    private IEnumerator SpawnPowerupsCoroutine()
    {
        while (true)
        {
            SpawnPowerup();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void Update()
    {
        for (int i = activePowerups.Count - 1; i >= 0; i--)
        {
            if (activePowerups[i].transform.position.x < player.position.x - despawnDistanceBehind)
            {
                activePowerups[i].SetActive(false);
                powerupPool.Add(activePowerups[i]);
                activePowerups.RemoveAt(i);
            }
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = powerupPrefabs[Random.Range(0, powerupPrefabs.Count)];
            GameObject powerup = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            powerup.SetActive(false);
            powerupPool.Add(powerup);
        }
    }

    private void SpawnPowerup()
    {
        if (powerupPool.Count == 0) return;

        GameObject powerup = powerupPool[0];
        powerupPool.RemoveAt(0);

        float y = Random.Range(player.position.y + minY, player.position.y + maxY);
        Vector3 spawnPos = new Vector3(player.position.x + spawnDistanceAhead, y, player.position.z);

        powerup.transform.position = spawnPos;
        powerup.SetActive(true);
        activePowerups.Add(powerup);
    }
}
