using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public int mapWidth = 21;   // X축 (-8.5 ~ 12.5)
    public int mapHeight = 15;  // Z축 (-26.5 ~ -11.5)
    private bool[,] tileOccupied;  // 풀의 존재 여부

    private Queue<Vector2Int> breedableQueue = new Queue<Vector2Int>(); // 번식 가능한 큐브 위치

    void Awake()
    {
        Instance = this;
        tileOccupied = new bool[mapWidth, mapHeight];
    }

    // Position을 좌표 인덱스로 변환
    public Vector2Int WorldToGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x + 8.5f);
        int y = Mathf.RoundToInt(position.z + 26.5f);
        return new Vector2Int(x, y);
    }

    // 좌표가 맵 범위 안에 있는지 확인
    public bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    // 좌표에 풀이 존재하는지 확인
    public bool IsOccupied(Vector2Int pos)
    {
        return IsValid(pos) && tileOccupied[pos.x, pos.y];
    }

    // 해당 좌표에 풀 등록
    public void RegisterGrass(Vector2Int pos)
    {
        if (IsValid(pos))
        {
            tileOccupied[pos.x, pos.y] = true;
        }
    }

    // 주변 8방향 중 비어있는 곳이 있으면 번식 가능
    public bool HasEmptyNeighbor(Vector2Int center)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // 중심 좌표 제외

                Vector2Int neighbor = new Vector2Int(center.x + dx, center.y + dy);
                if (IsValid(neighbor) && !IsOccupied(neighbor))
                    return true;
            }
        }
        return false;
    }

    // 번식 가능한 풀을 큐에 등록
    public void EnqueueBreedable(Vector2Int pos)
    {
        if (HasEmptyNeighbor(pos))
        {
            breedableQueue.Enqueue(pos);
        }
    }

    // 큐에서 번식 후보 2개 꺼내기
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

    public Vector2Int? GetRandomEmptyNeighbor(Vector2Int center)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // 중심 좌표 제외

                Vector2Int neighbor = new Vector2Int(center.x + dx, center.y + dy);
                if (IsValid(neighbor) && !IsOccupied(neighbor))
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
