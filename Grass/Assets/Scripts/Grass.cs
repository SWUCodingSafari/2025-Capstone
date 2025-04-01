using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public float GrowSpeed = 0.5f; // 잔디 성장 속도
    public Vector3 maxScale = new Vector3(2f, 2f, 2f); // 최대 크기

    void Update()
    {
        // 최대 크기에 도달하지 않았을 때
        if (transform.localScale.x < maxScale.x ||
            transform.localScale.y < maxScale.y ||
            transform.localScale.z < maxScale.z)
        {
            // 현재 크기에 속도 만큼 더한 새 크기 계산
            Vector3 newScale = transform.localScale + Vector3.one * GrowSpeed * Time.deltaTime;

            // 최대 크기를 넘지 않도록 제한
            newScale = Vector3.Min(newScale, maxScale);

            // 새 크기 적용
            transform.localScale = newScale;
        }
    }
}
