using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerSpawner : MonoBehaviour
{
    [SerializeField] private MapManager map;
    [SerializeField] private GameObject deer;
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;
    [SerializeField] private int spawnCount = 5;

    private float spawnYVel = 1f;

    private void Awake()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            float randomX = Mathf.Round(Random.Range(bottom.position.x + 8f, top.position.x - 8f)) + 0.5f;
            float randomZ = Mathf.Round(Random.Range(bottom.position.z + 8f, top.position.z - 8f)) + 0.5f;

            Vector3 spawnPosition = new Vector3(randomX, spawnYVel, randomZ);
            GameObject grass = Instantiate(deer, spawnPosition, Quaternion.identity);
        }
    }
}
