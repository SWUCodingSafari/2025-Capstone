using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoBehaviour
{
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;

    public GameObject grassPrefab;
    public int grassCount = 2;
    private float y = 1f;
    public WindZone windZone; // WindZone은 WeatherSystem이 제어

    private Vector3 GetWindDirection()
    {
        if (windZone == null) return Vector3.zero;

        // 바람 방향 (forward 방향) * 세기
        return windZone.transform.forward.normalized * windZone.windMain;
    }

    void Start()
    {
        SpawnGrassRandomly();
    }

    void SpawnGrassRandomly()
    {
        for (int i = 0; i < grassCount; i++)
        {
            TrySpawnGrass();
        }

        // 주기적으로 번식 시도
        InvokeRepeating(nameof(TryBreedingTurn), 5f, 5f);
    }

    void TrySpawnGrass()
    {
        for (int attempts = 0; attempts < 20; attempts++)
        {
            float baseX = Random.Range(top.position.x, bottom.position.x);
            float baseZ = Random.Range(top.position.z, bottom.position.z);

            Vector3 center = new Vector3(Mathf.Round(baseX) + 0.5f, y, Mathf.Round(baseZ) + 0.5f);
            Vector2Int gridPos = MapManager.Instance.WorldToGrid(center);

            if (MapManager.Instance.CanAddGrass(gridPos))
            {
                float offsetX = Random.Range(-0.3f, 0.3f);
                float offsetZ = Random.Range(-0.3f, 0.3f);
                Vector3 spawnPos = center + new Vector3(offsetX, 0, offsetZ);

                Instantiate(grassPrefab, spawnPos, Quaternion.identity);
                MapManager.Instance.RegisterGrass(gridPos);
                MapManager.Instance.EnqueueBreedable(gridPos);
                break;
            }
        }
    }

    void TryBreedingTurn()
    {
        int retryCount = 0;

        while (true)
        {
            List<Vector2Int> parents = MapManager.Instance.GetTwoBreedables();
            if (parents.Count < 2) return;

            int successCount = 0;

            Vector3 windDir = GetWindDirection();
            bool useWind = windDir.magnitude > 0.1f; // 바람 세기가 충분할 때만 방향 기반

            foreach (var parent in parents)
            {
                Vector2Int? target = useWind
                    ? MapManager.Instance.GetDirectionalEmptyNeighbor(parent, windDir)
                    : MapManager.Instance.GetRandomEmptyNeighbor(parent);

                if (target != null && MapManager.Instance.CanAddGrass(target.Value))
                {
                    Vector3 center = new Vector3(target.Value.x - 8.5f + 0.5f, y, target.Value.y - 26.5f + 0.5f);
                    float offsetX = Random.Range(-0.3f, 0.3f);
                    float offsetZ = Random.Range(-0.3f, 0.3f);
                    Vector3 spawnPos = center + new Vector3(offsetX, 0, offsetZ);

                    Instantiate(grassPrefab, spawnPos, Quaternion.identity);
                    MapManager.Instance.RegisterGrass(target.Value);
                    MapManager.Instance.EnqueueBreedable(target.Value);
                    successCount++;
                }
            }

            if (successCount == 2) break;
            if (++retryCount > 10) break;
        }
    }
}