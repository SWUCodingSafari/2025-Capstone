using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerSpawner : MonoBehaviour
{
    [SerializeField] private MapManager map;
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;

    [Header("Deer")]
    [SerializeField] private GameObject deer;
    [SerializeField] private int spawnCountDeer = 3;

    [Header("Wolf")]
    [SerializeField] private GameObject wolf;
    [SerializeField] private int spawnCountWolf = 2;

    private float spawnYVel = 1f;

    private void Awake()
    {
        for (int i = 0; i < spawnCountDeer; i++)
        {
            float randomX = Mathf.Round(Random.Range(bottom.position.x + 8f, top.position.x - 8f)) + 0.5f;
            float randomZ = Mathf.Round(Random.Range(bottom.position.z + 8f, top.position.z - 8f)) + 0.5f;

            Vector3 spawnPosition = new Vector3(randomX, spawnYVel, randomZ);
            GameObject d = Instantiate(deer, spawnPosition, Quaternion.identity);
        }

        for (int i = 0; i < spawnCountDeer; i++)
        {
            float randomX = Mathf.Round(Random.Range(bottom.position.x + 7f, top.position.x - 9f)) + 0.5f;
            float randomZ = Mathf.Round(Random.Range(bottom.position.z + 8f, top.position.z - 8f)) + 0.5f;

            Vector3 spawnPosition = new Vector3(randomX, spawnYVel, randomZ);
            GameObject w = Instantiate(wolf, spawnPosition, Quaternion.identity);
        }

    }
}
