using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoBehaviour
{
    public GameObject grassPrefab;  // 풀 프리팹
    public int grassCount = 2;      // 최초 생성할 풀 개수
    private float y = 1f;           // 풀의 고정 Y 좌표

    void Start()
    {
        // 시작 시 풀 생성
        SpawnGrassRandomly();
    }

    // 지정한 수만큼 랜덤 위치에 풀 생성
    void SpawnGrassRandomly()
    {
        for (int i = 0; i < grassCount; i++)
        {
            TrySpawnGrass();
        }

        // 5초마다 번식 시도 반복
        InvokeRepeating(nameof(TryBreedingTurn), 5f, 5f);
    }

    // 랜덤한 타일에 풀 생성 시도 (타일당 최대 2개까지)
    void TrySpawnGrass()
    {
        for (int attempts = 0; attempts < 20; attempts++)
        {
            float baseX = Random.Range(-8.5f, 12.5f);
            float baseZ = Random.Range(-26.5f, -11.5f);

            Vector3 center = new Vector3(Mathf.Round(baseX) + 0.5f, y, Mathf.Round(baseZ) + 0.5f);
            Vector2Int gridPos = MapManager.Instance.WorldToGrid(center);

            if (MapManager.Instance.CanAddGrass(gridPos))
            {
                // 타일 내에서 약간 랜덤한 위치에 생성
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

    // 2개의 풀을 선택해 번식 시도
    void TryBreedingTurn()
    {
        int retryCount = 0;

        while (true)
        {
            List<Vector2Int> parents = MapManager.Instance.GetTwoBreedables();
            if (parents.Count < 2) return;

            int successCount = 0;

            foreach (var parent in parents)
            {
                Vector2Int? target = MapManager.Instance.GetRandomEmptyNeighbor(parent);
                if (target != null && MapManager.Instance.CanAddGrass(target.Value))
                {
                    // 중심 위치 + 타일 내 오프셋 적용
                    Vector3 center = new Vector3(target.Value.x - 8f, y, target.Value.y - 26f);
                    float offsetX = Random.Range(-0.3f, 0.3f);
                    float offsetZ = Random.Range(-0.3f, 0.3f);
                    Vector3 spawnPos = center + new Vector3(offsetX, 0, offsetZ);

                    Instantiate(grassPrefab, spawnPos, Quaternion.identity);
                    MapManager.Instance.RegisterGrass(target.Value);
                    MapManager.Instance.EnqueueBreedable(target.Value);
                    successCount++;
                }
            }

            // 둘 다 성공하면 종료
            if (successCount == 2) break;
            if (++retryCount > 10) break;
        }
    }
}