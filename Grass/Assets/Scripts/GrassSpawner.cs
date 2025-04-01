using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public Terrain terrain;                  // Terrain 참조
    public GameObject grassPrefab;           // Grass 프리팹

    private List<GameObject> allGrass = new List<GameObject>(); // 현재 존재하는 모든 Grass 저장용 리스트

    void Start()
    {
        // 첫 번째 잔디 생성
        SpawnNewGrass();
    }

    // 무작위 위치에 풀 생성 요청
    public void SpawnNewGrass()
    {
        Vector3 position = GetRandomPositionOnTerrain();
        SpawnGrassAt(position);
    }

    // 특정 위치에 풀 생성 및 초기화
    public void SpawnGrassAt(Vector3 position)
    {
        // y좌표는 Terrain의 높이에 맞춤
        float y = terrain.SampleHeight(position) + terrain.GetPosition().y;
        Vector3 spawnPos = new Vector3(position.x, y + 0.5f, position.z); // 살짝 띄움

        // 잔디 생성
        GameObject grass = Instantiate(grassPrefab, spawnPos, Quaternion.identity);
        allGrass.Add(grass); // 리스트에 추가

        // 클릭 반응 스크립트 + 스포너 참조 연결
        GrassClickHandler clickHandler = grass.AddComponent<GrassClickHandler>();
        clickHandler.spawner = this;
    }

    // 다른 Grass들과 겹치지 않게 거리 검사
    public bool IsPositionValid(Vector3 pos)
    {
        float minDistance = 1.5f; // 최소 거리

        foreach (GameObject g in allGrass)
        {
            if (g == null) continue; // 이미 파괴된 경우 건너뜀
            float dist = Vector3.Distance(g.transform.position, pos);
            if (dist < minDistance)
                return false;
        }
        return true; // 충분히 멀리 있음
    }

    // Terrain 내부에서 랜덤 위치 반환
    Vector3 GetRandomPositionOnTerrain()
    {
        float terrainWidth = terrain.terrainData.size.x;
        float terrainLength = terrain.terrainData.size.z;

        float originX = terrain.GetPosition().x;
        float originZ = terrain.GetPosition().z;

        // 랜덤 위치 생성
        float randomX = Random.Range(originX, originX + terrainWidth);
        float randomZ = Random.Range(originZ, originZ + terrainLength);

        // 높이 맞춰서 최종 위치 반환
        float y = terrain.SampleHeight(new Vector3(randomX, 0, randomZ)) + terrain.GetPosition().y;

        return new Vector3(randomX, y + 0.5f, randomZ);
    }
}
