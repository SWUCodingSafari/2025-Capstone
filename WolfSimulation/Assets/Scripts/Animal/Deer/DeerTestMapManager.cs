using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerTestMapManager : MonoBehaviour
{
    private static DeerTestMapManager instance;
    public static DeerTestMapManager Instance
    {
        get { return instance; }
    }

    [SerializeField] private GameObject grass;
    [SerializeField] private int grassSpawnCount = 20;

    [SerializeField] private GameObject deer;
    [SerializeField] private int deerSpawnCount = 10;
    [SerializeField] private bool spawnDeer = true;

    [SerializeField] private GameObject wolf;
    [SerializeField] private int wolfSpawnCount = 5;
    [SerializeField] private bool spawnWolf = true;

    [SerializeField] private float grassSpawnTime = 0.5f;
    [SerializeField] private float mapSize = 100f;
    private float elapsedTime = 0f;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < grassSpawnCount; ++i)
        {
            GameObject g = Instantiate(grass);
            g.transform.position = new Vector3(Random.Range(-mapSize, mapSize), 0.01f, Random.Range(-mapSize, mapSize));
        }

        if (spawnDeer)
        {
            for (int i = 0; i < deerSpawnCount; ++i)
            {
                GameObject g = Instantiate(deer);
                g.transform.position = new Vector3(Random.Range(-mapSize, mapSize), 0.01f, Random.Range(-mapSize, mapSize));
            }
        }

        if (spawnWolf)
        {
            for (int i = 0; i < wolfSpawnCount; ++i)
            {
                GameObject w = Instantiate(wolf);
                w.transform.position = new Vector3(Random.Range(-mapSize, mapSize), 0.01f, Random.Range(-mapSize, mapSize));
            }
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= grassSpawnTime)
        {
            elapsedTime -= grassSpawnTime;

            int count = Random.Range(5, 10);

            for (int i = 0; i < count; ++i)
            {
                GameObject g = Instantiate(grass);
                g.transform.position = new Vector3(Random.Range(-mapSize, mapSize), 0.01f, Random.Range(-mapSize, mapSize));
            }
        }
    }

    public void SpawnDeer()
    {
        StartCoroutine(CoSpawnDeer());
    }

    private IEnumerator CoSpawnDeer()
    {
        yield return new WaitForSeconds(1f);

        GameObject g = Instantiate(deer);
        g.transform.position = new Vector3(Random.Range(-mapSize, mapSize), 0.01f, Random.Range(-mapSize, mapSize));
    }

    public void SpawnWolf()
    {
        StartCoroutine(CoSpawnWolf());
    }

    private IEnumerator CoSpawnWolf()
    {
        yield return new WaitForSeconds(1f);

        GameObject g = Instantiate(wolf);
        g.transform.position = new Vector3(Random.Range(-mapSize, mapSize), 0.01f, Random.Range(-mapSize, mapSize));
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
