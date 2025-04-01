using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public Terrain terrain;                  // Terrain ����
    public GameObject grassPrefab;           // Grass ������

    private List<GameObject> allGrass = new List<GameObject>(); // ���� �����ϴ� ��� Grass ����� ����Ʈ

    void Start()
    {
        // ù ��° �ܵ� ����
        SpawnNewGrass();
    }

    // ������ ��ġ�� Ǯ ���� ��û
    public void SpawnNewGrass()
    {
        Vector3 position = GetRandomPositionOnTerrain();
        SpawnGrassAt(position);
    }

    // Ư�� ��ġ�� Ǯ ���� �� �ʱ�ȭ
    public void SpawnGrassAt(Vector3 position)
    {
        // y��ǥ�� Terrain�� ���̿� ����
        float y = terrain.SampleHeight(position) + terrain.GetPosition().y;
        Vector3 spawnPos = new Vector3(position.x, y + 0.5f, position.z); // ��¦ ���

        // �ܵ� ����
        GameObject grass = Instantiate(grassPrefab, spawnPos, Quaternion.identity);
        allGrass.Add(grass); // ����Ʈ�� �߰�

        // Ŭ�� ���� ��ũ��Ʈ + ������ ���� ����
        GrassClickHandler clickHandler = grass.AddComponent<GrassClickHandler>();
        clickHandler.spawner = this;
    }

    // �ٸ� Grass��� ��ġ�� �ʰ� �Ÿ� �˻�
    public bool IsPositionValid(Vector3 pos)
    {
        float minDistance = 1.5f; // �ּ� �Ÿ�

        foreach (GameObject g in allGrass)
        {
            if (g == null) continue; // �̹� �ı��� ��� �ǳʶ�
            float dist = Vector3.Distance(g.transform.position, pos);
            if (dist < minDistance)
                return false;
        }
        return true; // ����� �ָ� ����
    }

    // Terrain ���ο��� ���� ��ġ ��ȯ
    Vector3 GetRandomPositionOnTerrain()
    {
        float terrainWidth = terrain.terrainData.size.x;
        float terrainLength = terrain.terrainData.size.z;

        float originX = terrain.GetPosition().x;
        float originZ = terrain.GetPosition().z;

        // ���� ��ġ ����
        float randomX = Random.Range(originX, originX + terrainWidth);
        float randomZ = Random.Range(originZ, originZ + terrainLength);

        // ���� ���缭 ���� ��ġ ��ȯ
        float y = terrain.SampleHeight(new Vector3(randomX, 0, randomZ)) + terrain.GetPosition().y;

        return new Vector3(randomX, y + 0.5f, randomZ);
    }
}
