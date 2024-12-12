using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonSpawner : MonoBehaviour
{
    public List<Transform> SpawnPosition;   
    public GameObject CannonPrefab;       
    private float spawnTimer = 10f;    
    private float timer = 0f;

    private int previousRandomNum=-1;
    private List<GameObject> spawnedCannons;

    void Start()
    {
        spawnedCannons = new List<GameObject>(new GameObject[SpawnPosition.Count]); 
    }

    void Update()
    {
        if (previousRandomNum != -1 && spawnedCannons[previousRandomNum] == null ) 
        { 
            SpawnCannon();
        }
        
        if (timer > spawnTimer)
        {

            if (HasAvailableSpawnPoint())
            {
                SpawnCannon();
            }

            timer = 0f;
        }

        timer += Time.deltaTime;
    }

    private bool HasAvailableSpawnPoint()
    {
        foreach (GameObject cannon in spawnedCannons)
        {
            if (cannon == null)  
            {
                return true;
            }
        }
        return false;
    }

    public void SpawnCannon()
    {
        int randSpawnPoint = -1;

        do
        {
            randSpawnPoint = Random.Range(0, SpawnPosition.Count);
            previousRandomNum = randSpawnPoint;
        }
        while (spawnedCannons[randSpawnPoint] != null);  

        GameObject newCannon = Instantiate(CannonPrefab, SpawnPosition[randSpawnPoint].position, Quaternion.identity);

        spawnedCannons[randSpawnPoint] = newCannon;
    }
}
