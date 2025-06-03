using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public int mapWidth = 21;   // X축 (-8.5 ~ 12.5)
    public int mapHeight = 15;  // Z축 (-26.5 ~ -11.5)

    private int[,] tileGrassCount;   // 각 타일에 존재하는 풀 개수
    private Queue<Vector2Int> breedableQueue = new Queue<Vector2Int>(); // 번식 후보 풀들

    private int maxGrassPerTile = 2; // 한 타일에 최대 2개까지 생성 가능

    void Awake()
    {
        // 싱글톤 설정 및 배열 초기화
        Instance = this;
        tileGrassCount = new int[mapWidth, mapHeight];
    }

    // 월드 좌표를 타일 좌표(Grid)로 변환
    public Vector2Int WorldToGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x + 8.5f);
        int y = Mathf.RoundToInt(position.z + 26.5f);
        return new Vector2Int(x, y);
    }

    // 타일 좌표가 유효한지 확인
    public bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    // 해당 좌표에 풀을 더 추가할 수 있는지 확인
    public bool CanAddGrass(Vector2Int pos)
    {
        return IsValid(pos) && tileGrassCount[pos.x, pos.y] < maxGrassPerTile;
    }

    // 해당 타일에 풀을 하나 등록
    public void RegisterGrass(Vector2Int pos)
    {
        if (IsValid(pos))
        {
            tileGrassCount[pos.x, pos.y]++;
        }
    }

    // 인접한 8방향에 풀을 더 심을 수 있는지 확인
    public bool HasEmptyNeighbor(Vector2Int center)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int neighbor = new Vector2Int(center.x + dx, center.y + dy);
                if (CanAddGrass(neighbor))
                    return true;
            }
        }
        return false;
    }

    // 번식 가능한 위치로 큐에 추가
    public void EnqueueBreedable(Vector2Int pos)
    {
        if (HasEmptyNeighbor(pos))
        {
            breedableQueue.Enqueue(pos);
        }
    }

    // 번식을 위해 큐에서 2개의 타일 좌표를 꺼냄
    public List<Vector2Int> GetTwoBreedables()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        while (breedableQueue.Count > 0 && result.Count < 2)
        {
            Vector2Int pos = breedableQueue.Dequeue();
            if (HasEmptyNeighbor(pos))
            {
                result.Add(pos);
            }
        }

        return result;
    }

    // 8방향 중 풀 추가 가능한 빈 타일을 랜덤으로 반환
    public Vector2Int? GetRandomEmptyNeighbor(Vector2Int center)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int neighbor = new Vector2Int(center.x + dx, center.y + dy);
                if (CanAddGrass(neighbor))
                    candidates.Add(neighbor);
            }
        }

        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        return null;
    }
}