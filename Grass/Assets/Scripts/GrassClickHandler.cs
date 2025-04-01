using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassClickHandler : MonoBehaviour
{
    public GrassSpawner spawner; // GrassSpawner ��ũ��Ʈ ����

    private bool clicked = false; // Ŭ�� ����
    private bool isGrown = false; // ���� �Ϸ� ����
    private bool hasSpread = false; // Ȯ�� ���� (�� ���� Ȯ��)

    void Start()
    {
        // 2�� �� ���� �Ϸ�
        Invoke("OnGrown", 2f);
    }

    // ���� �Ϸ� �� ȣ��
    void OnGrown()
    {
        isGrown = true;

        // ���� �Ϸ� �� 2�� �� Ȯ�� �õ� (�� 4�� �� Ȯ��)
        Invoke("TrySpread", 2f);
    }

    void OnMouseDown()
    {
        if (clicked) return; // �ߺ� ����

        clicked = true;

        // �����ʿ��� �����ϰ� 2�� �ڿ� ���� ����
        if (spawner != null)
        {
            spawner.Invoke("SpawnNewGrass", 2f);
        }
        else
        {
            Debug.LogError("Spawner is NULL in GrassClickHandler");
        }

        Destroy(gameObject); // ���� Grass ����
    }

    void SpawnAfterClick()
    {
        if (spawner != null)
        {
            spawner.SpawnNewGrass();
        }
        else
        {
            Debug.LogError("Spawner is NULL in GrassClickHandler.");
        }
    }

    // Ȯ�� ó��
    void TrySpread()
    {
        if (clicked || hasSpread) return; // Ŭ���Ǿ��ų� �̹� Ȯ���ߴٸ� ����

        Vector3 center = transform.position;
        int spawned = 0;
        int attempts = 0;

        // �ִ� 10�� �õ��ؼ� �ֺ��� 2���� Grass ����
        while (spawned < 2 && attempts < 10)
        {
            attempts++;
            Vector3 offset = new Vector3(
                Random.Range(-3f, 3f), // �¿� �Ÿ�
                0,
                Random.Range(-3f, 3f) // �յ� �Ÿ�
            );

            Vector3 spawnPos = center + offset;

            if (spawner.IsPositionValid(spawnPos))
            {
                spawner.SpawnGrassAt(spawnPos);
                spawned++;
            }
        }

        hasSpread = true; // Ȯ�� �Ϸ� ó��
    }
}

