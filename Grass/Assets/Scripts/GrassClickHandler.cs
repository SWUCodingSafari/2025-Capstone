using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassClickHandler : MonoBehaviour
{
    public GrassSpawner spawner; // GrassSpawner 스크립트 참조

    private bool clicked = false; // 클릭 여부
    private bool isGrown = false; // 성장 완료 여부
    private bool hasSpread = false; // 확산 여부 (한 번만 확산)

    void Start()
    {
        // 2초 후 성장 완료
        Invoke("OnGrown", 2f);
    }

    // 성장 완료 시 호출
    void OnGrown()
    {
        isGrown = true;

        // 성장 완료 후 2초 뒤 확산 시도 (총 4초 뒤 확산)
        Invoke("TrySpread", 2f);
    }

    void OnMouseDown()
    {
        if (clicked) return; // 중복 방지

        clicked = true;

        // 스포너에서 안전하게 2초 뒤에 생성 예약
        if (spawner != null)
        {
            spawner.Invoke("SpawnNewGrass", 2f);
        }
        else
        {
            Debug.LogError("Spawner is NULL in GrassClickHandler");
        }

        Destroy(gameObject); // 현재 Grass 제거
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

    // 확산 처리
    void TrySpread()
    {
        if (clicked || hasSpread) return; // 클릭되었거나 이미 확산했다면 종료

        Vector3 center = transform.position;
        int spawned = 0;
        int attempts = 0;

        // 최대 10번 시도해서 주변에 2개의 Grass 생성
        while (spawned < 2 && attempts < 10)
        {
            attempts++;
            Vector3 offset = new Vector3(
                Random.Range(-3f, 3f), // 좌우 거리
                0,
                Random.Range(-3f, 3f) // 앞뒤 거리
            );

            Vector3 spawnPos = center + offset;

            if (spawner.IsPositionValid(spawnPos))
            {
                spawner.SpawnGrassAt(spawnPos);
                spawned++;
            }
        }

        hasSpread = true; // 확산 완료 처리
    }
}

