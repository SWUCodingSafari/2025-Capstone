using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoBehaviour
{
    public GameObject grassPrefab; // 생성할 풀 프리팹
    public int grassCount = 2;     // 초기 생성 수
    private float y = 1f;

    void Start()
    {
        SpawnGrassRandomly();
    }

    void SpawnGrassRandomly()
    {
        for (int i = 0; i < grassCount; i++)
        {
            float randomX = Mathf.Round(Random.Range(-8.5f, 12.5f)) + 0.5f;
            float randomZ = Mathf.Round(Random.Range(-26.5f, -11.5f)) + 0.5f;

            Vector3 spawnPosition = new Vector3(randomX, y, randomZ);
            GameObject grass = Instantiate(grassPrefab, spawnPosition, Quaternion.identity);

            Vector2Int gridPos = MapManager.Instance.WorldToGrid(spawnPosition);
            MapManager.Instance.RegisterGrass(gridPos);
            MapManager.Instance.EnqueueBreedable(gridPos);
        }

        // 일정 시간 후 번식 시도
        InvokeRepeating(nameof(TryBreedingTurn), 5f, 5f);
    }

    void TryBreedingTurn()
    {
        int retryCount = 0;

        while (true)
        {
            List<Vector2Int> parents = MapManager.Instance.GetTwoBreedables();

            if (parents.Count < 2)
            {
                Debug.Log("더 이상 번식이 불가능합니다");
                return;
            }

            int successCount = 0;

            foreach (var parent in parents)
            {
                Vector2Int? targetPos = MapManager.Instance.GetRandomEmptyNeighbor(parent);
                if (targetPos != null)
                {
                    Vector3 spawnWorld = new Vector3(targetPos.Value.x - 8.5f, y, targetPos.Value.y - 26.5f);
                    GameObject newGrass = Instantiate(grassPrefab, spawnWorld, Quaternion.identity);

                    MapManager.Instance.RegisterGrass(targetPos.Value);
                    MapManager.Instance.EnqueueBreedable(targetPos.Value);
                    successCount++;
                }
            }

            // 둘 다 성공했으면 번식 완료
            if (successCount == 2) break;

            retryCount++;
            if (retryCount > 10)
            {
                Debug.Log("반복 횟수 초과로 번식 중단");
                break;
            }
        }
    }
}
